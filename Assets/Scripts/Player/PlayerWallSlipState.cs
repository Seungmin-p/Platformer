using UnityEngine;

namespace FSM
{
    public class PlayerWallSlipState : PlayerState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Wall");
        
        public PlayerWallSlipState(Player owner, StateMachine stateMachine) : base(owner, stateMachine) {}

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
            //상태 변환 처리(벽점프, 착지, 추락)
            
            //점프가 가능한 상태면 벽 점프
            if (controller.JumpInput && controller.CanJump)
            {
                stateMachine.ChangeState(owner.WallJumpState);
                return;
            }
            
            //착지판정
            if (controller.IsGrounded)
            {
                //착지 파티클 재생
                owner.LandingDust.Play();               
                stateMachine.ChangeState(owner.IdleState);
                return;
            }

            //떨어지는데, 벽이 아니라면 추락 판정
            if (owner.Rb.linearVelocity.y <= -0.1f && !controller.IsWall)
            {
                stateMachine.ChangeState(owner.FallState);
                return;
            }
            
            //벽의 반대방향 입력이 들어와도 일단 추락판정
            //왼쪽(-1)이나 오른쪽(1)을 볼 때 반대 입력이 들어왔다면, 두 입력을 곱하면 항상 음수가 된다.
            if (Mathf.Abs(controller.XInput) > 0.1f && controller.XInput * controller.PlayerDirection < 0)
            {
                stateMachine.ChangeState(owner.FallState);
                return;
            }
        }
        public override void OnFixedUpdate()
        {
            //좌우 이동 없이 벽에서 미끄러지도록
            owner.Rb.linearVelocity = new Vector2(0f, controller.WallSlip * -1);
        }
        public override void OnExit()
        {
        }
    }
}