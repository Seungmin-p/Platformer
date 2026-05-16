using UnityEngine;

namespace FSM
{
    public class MonsterChickenStopState : MonsterState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Idle");
        private Chicken chicken; //Chicken 본체를 담는 변수

        public MonsterChickenStopState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
            //owner를 Chicken으로 형변환 해서 저장
            chicken = (Chicken)owner;
        }

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            chicken.Animator.Play(animHash);
            
            //Stop이기 때문에 멈춤
            controller.ExecuteStop();
        }
        public override void OnUpdate()
        {
        }
        public override void OnFixedUpdate()
        {
        }
        public override void OnExit()
        {
        }
    }
}