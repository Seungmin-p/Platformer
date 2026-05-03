using UnityEngine;

namespace FSM
{
    public class MonsterRadishFlyState : MonsterState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Fly");
        private Radish radish; //Trunk 본체를 담는 변수

        public MonsterRadishFlyState(Monster owner, StateMachine stateMachine) : base(owner, stateMachine)
        {
            //owner를 Radish로 형변환 해서 저장
            radish = (Radish)owner;
        }

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            radish.Animator.Play(animHash);
        }
        public override void OnUpdate()
        {
            //비행처리
            radish.RadishFlying();
        }
        public override void OnFixedUpdate()
        {
            
        }
        public override void OnExit()
        {
            //빠져나가는 경우가 피격 상태가 유일하므로, 여기서 추락처리
            radish.RadishFallDown();
        }
    }
}