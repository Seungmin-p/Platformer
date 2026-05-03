using UnityEngine;

namespace FSM
{
    public class MonsterRunState : MonsterState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Run");
        
        public MonsterRunState(Monster owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            owner.Animator.Play(animHash);
        }
        public override void OnUpdate()
        {
            //상태 변환 처리

            //벽이나 낭떠러지가 앞에 있는 경우
            if (controller.IsWall || controller.IsEdge)
            {
                stateMachine.ChangeState(owner.IdleState);
                return;
            }
        }
        public override void OnFixedUpdate()
        {
            controller.ExecuteMove(controller.MonsterDirection);
        }
        public override void OnExit()
        {
            controller.ExecuteStop();
        }
    }
}