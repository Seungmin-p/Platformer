using UnityEngine;

namespace FSM
{
    public class PlayerIdleState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Idle");
        
        public PlayerIdleState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            owner.Animator.Play(animHash);
            
            //점프 가능 여부 초기화
            controller.CanJump = true;
            controller.CanDoubleJump = true;
        }
        public override void OnUpdate()
        {
            //상태 변환 처리
            //추락상태 변환
            if (owner.Rb.linearVelocity.y <= -0.1f && !controller.IsGrounded)
            {
                stateMachine.ChangeState(owner.FallState);
                return;
            }
            
            //점프 상태 변환
            if (Input.GetButtonDown("Jump") && (controller.CanJump || controller.IsWall) )
            {
                stateMachine.ChangeState(owner.JumpState);
                return;
            }
            
            //이동상태 변환
            if (Mathf.Abs(controller.XInput) > 0.1f)
            {
                stateMachine.ChangeState(owner.RunState);
                return;
            }
        }
        public override void OnFixedUpdate()
        {
            
        }
        public override void OnExit()
        {
            
        }
    }
}