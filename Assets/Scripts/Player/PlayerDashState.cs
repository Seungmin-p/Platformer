using UnityEngine;

namespace FSM
{
    public class PlayerDashState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Jump");
        
        //대시상태 유지 타이머, 기존 중력값 변수
        private float dashTimer;
        private float defaultGravity;
        
        public PlayerDashState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            owner.Animator.Play(animHash);
            owner.DashDust.Play();
            
            //대시 상태에서는 중력 임시로 제거
            defaultGravity = owner.Rb.gravityScale;
            owner.Rb.gravityScale = 0f;
            
            //대시 방향 계산 후, 속도 지정
            owner.Rb.linearVelocity = DashDirection() * 30f;

            //대시 상태 유지 타이머 초기화
            dashTimer = 0f;

            //대시 쿨타임 초기화
            controller.ExecuteResetDashCooltime();
        }
        
        public override void OnUpdate()
        {            
            //대시 유지 시간 타이머 이후 Fall 전환
            dashTimer += Time.deltaTime;

            if (dashTimer > 0.2f)
            {
                stateMachine.ChangeState(owner.FallState);
            }
        }
        public override void OnFixedUpdate()
        {
        }
        public override void OnExit()
        {
            //대시 상태가 끝날때 돌려놔야 하는 데이터 처리
            owner.Rb.gravityScale = defaultGravity; //중력값 복구
            owner.Rb.linearVelocity = Vector2.zero; //속도 초기화
            owner.DashDust.Stop();
        }

        private Vector2 DashDirection()
        {
            //방향 계산에 사용할 변수
            float snapX = 0f;
            float snapY = 0f;

            //입력값이 0이 아닌경우 sign 처리
            //sign은 양수면 1, 음수면 -1을 반환함
            if (Mathf.Abs(controller.XInput) > 0.1f) snapX = Mathf.Sign(controller.XInput);
            if (Mathf.Abs(controller.YInput) > 0.1f) snapY = Mathf.Sign(controller.YInput);

            Vector2 inputDir = new Vector2(snapX, snapY);

            //방향키 입력이 아예 없어서 초기 0 값이 유지된 경우
            if (inputDir == Vector2.zero)
            {
                //플레이어 방향 반환
                return new Vector2(controller.PlayerDirection, 0f);
            }
            else
            {
                //만약 대각선 입력이라면 벡터의 길이가 길어지기 때문에 정규화 진행 후 반환
                return inputDir.normalized;
            }
        }
    }
}
