using UnityEngine;

namespace FSM
{
    public class PlayerJumpState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Jump");
        
        //점프 직후 타이머
        private float jumpTimer;
        
        public PlayerJumpState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            owner.Animator.Play(animHash);

            //점프력만큼 점프 실행
            controller.ExecuteJump(controller.JumpForce);
            
            //점프 파티클 출력
            owner.JumpDust.Play();
            
            //타이머 초기화
            jumpTimer = 0f;
            
            //점프 이후 다시 기본점프 불가능, 더블점프는 굳이 여기서 초기화 할 필요 없음
            controller.CanJump = false;
        }
        public override void OnUpdate()
        {
            //프레임마다 타이머 증가
            jumpTimer += Time.deltaTime;
            
            //상태 변환 처리(더블점프, 추락, 바닥판정)
            
            //대시 입력 체크 후 대시처리
            if(CheckDashAction()) return;
            
            //더블점프가 가능한 상태면 더블점프
            if (controller.JumpInput && controller.CanDoubleJump)
            {
                stateMachine.ChangeState(owner.DoubleJumpState);
                return;
            }
            
            //추락 판정
            //y축 변환 속도가 0보다 작음, 땅이 아님
            if (owner.Rb.linearVelocity.y < 0.0f && !controller.IsGrounded)
            {
                stateMachine.ChangeState(owner.FallState);
                return;
            }

            //점프 직후 벽에 껴서 상태전환되는걸 방지함
            //간혹 점프 이후 즉시 바닥에 닿는 경우를 위한 착지판정
            if (jumpTimer > 0.1f && controller.IsGrounded)
            {
                //착지 파티클 재생
                owner.LandingDust.Play();
                
                //착지 타이밍에 방향키를 누르고 있다면 Run, 아니면 Idle로 직행
                if (Mathf.Abs(controller.XInput) > 0.1f)
                {
                    stateMachine.ChangeState(owner.RunState);
                }
                else
                {
                    stateMachine.ChangeState(owner.IdleState);
                }
                return;
            }
        }
        
        public override void OnFixedUpdate()
        {
            //공중에서도 좌/우 이동은 가능해야함
            if (!controller.IsWall)
            {
                controller.ExecuteMove(controller.XInput);
            }
            else
            {
                //TODO : 만약 앞에있는게 벽이라면, 반대방향키 눌렀을 때 바로 빠져나올 수 있어야함
            }
        }
        
        public override void OnExit()
        {
            
        }
    }

}