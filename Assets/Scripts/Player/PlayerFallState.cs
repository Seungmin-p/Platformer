using UnityEngine;

namespace FSM
{
    public class PlayerFallState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Fall");
        
        public PlayerFallState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            owner.Animator.Play(animHash);
            
            //떨어지는 도중엔 기본 점프 불가능(더블점프는 아직 안한 경우 가능)
            controller.CanJump = false;
        }
        public override void OnUpdate()
        {
            //상태 변환 처리(착지(idle,run), 더블점프, 벽)
            
            //착지 판정
            if (controller.IsGrounded)
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
            
            //더블점프가 가능한 상태면 더블점프
            if (Input.GetButtonDown("Jump") && controller.CanDoubleJump)
            {
                stateMachine.ChangeState(owner.DoubleJumpState);
                return;
            }

            //벽 판정
            if (controller.IsWall)
            {
                stateMachine.ChangeState(owner.WallSlipState);
                return;
            }
        }
        public override void OnFixedUpdate()
        {
            //공중에서도 좌/우 이동은 가능해야함
            controller.ExecuteMove(controller.XInput);
        }
        public override void OnExit()
        {
            
        }
    }

}