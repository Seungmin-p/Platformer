using UnityEngine;

namespace FSM
{
    public class PlayerWallJumpState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Jump");
        private float wallJumpTimer;
        
        public PlayerWallJumpState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            owner.Animator.Play(animHash);

            //벽 점프 실행
            WallJump();
            
            //점프 파티클 출력
            owner.WallJumpDust.Play();
            
            //점프 이후 다시 기본점프 불가능, 더블점프는 굳이 여기서 초기화 할 필요 없음
            controller.CanJump = false;
        }
        public override void OnUpdate()
        {
            //벽 점프 타이머 감소, 어차피 몇초 내로 상태가 변경되니 if 없이 열어놔도 무방
            wallJumpTimer -= Time.deltaTime;
            
            //상태 변환 처리(더블점프, 추락)
            
            //대시 입력 체크 후 대시처리
            if(CheckDashAction()) return;
            
            //더블점프가 가능한 상태면 더블점프
            if (controller.JumpInput && controller.CanDoubleJump)
            {
                stateMachine.ChangeState(owner.DoubleJumpState);
                return;
            }
            
            //추락 판정
            if (owner.Rb.linearVelocity.y <= -0.1f && !controller.IsGrounded)
            {
                stateMachine.ChangeState(owner.FallState);
                return;
            }
        }
        public override void OnFixedUpdate()
        {
            //벽 점프 직후 0.1초동안 이동 조작 불가, 타이머가 지난 후 이동 가능
            if (wallJumpTimer <= 0 && !controller.IsWall)
            {
                //공중에서도 좌/우 이동은 가능해야함
                controller.ExecuteMove(controller.XInput);
            }
        }
        public override void OnExit()
        {
        }

        private void WallJump()
        {
            //벽의 반대 방향으로 밀리는 점프 실행해야함
            //상태 및 타이머 설정
            wallJumpTimer = 0.1f;
                    
            //방향 전환처리
            controller.PlayerDirection *= -1; 
            owner.transform.localScale = new Vector3( owner.transform.localScale.x * -1, 1, 1);

            //붙은 벽의 반대방향으로 밀려야함
            owner.Rb.linearVelocity = new Vector2((controller.JumpForce / 3 ) * controller.PlayerDirection, controller.JumpForce);
        }
    }
}