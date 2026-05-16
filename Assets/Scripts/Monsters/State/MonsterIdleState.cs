using UnityEngine;

namespace FSM
{
    public class MonsterIdleState : MonsterState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Idle");
        private float waitTime;
        
        public MonsterIdleState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine) {}

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            owner.Animator.Play(animHash);
            
            //타이머 초기화
            waitTime = 1.0f;
        }
        public override void OnUpdate()
        {
            //상태 변환 처리

            if (waitTime > 0.0f)
            {
                waitTime -= Time.deltaTime;
            }
            else
            {
                //멈췄다가 움직이려 할 때 방향전환
                controller.ExecuteTurn();
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