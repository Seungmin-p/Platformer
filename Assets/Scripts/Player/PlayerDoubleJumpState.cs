using UnityEngine;

namespace FSM
{
    public class PlayerDoubleJumpState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Double_Jump");
        
        public PlayerDoubleJumpState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            owner.Animator.Play(animHash);

            //점프력만큼 점프 실행
            controller.ExecuteJump(controller.JumpForce);
            
            //점프 파티클 출력
            owner.JumpDust.Play();
            
            //더블점프 이후 상태처리
            controller.CanDoubleJump = false;
        }
        public override void OnUpdate()
        {
            //상태 변환 처리(추락)
            
            //대시 입력 체크 후 대시처리
            if(CheckDashAction()) return;
            
            //추락 판정
            if (owner.Rb.linearVelocity.y <= -0.1f && !controller.IsGrounded)
            {
                stateMachine.ChangeState(owner.FallState);
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
        }
        public override void OnExit()
        {
            
        }
    }
}