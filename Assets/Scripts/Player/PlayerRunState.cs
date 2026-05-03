using UnityEngine;

namespace FSM
{
    public class PlayerRunState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Run");
        
        public PlayerRunState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

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
            if ( !controller.IsGrounded )
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
            
            //이동 입력이 멈춘 경우
            if (Mathf.Abs(controller.XInput) < 0.1f)
            {
                stateMachine.ChangeState(owner.IdleState);
                return;
            }
        }
        public override void OnFixedUpdate()
        {
            controller.ExecuteMove(controller.XInput);
        }
        public override void OnExit()
        {
            
        }
    }
}
