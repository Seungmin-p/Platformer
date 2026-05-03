using UnityEngine;

namespace FSM
{
    public class MonsterRadishHitState : MonsterState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int animHash = Animator.StringToHash("Hit");
        private Radish radish; //Radish 본체를 담는 변수

        public MonsterRadishHitState(Monster owner, StateMachine stateMachine) : base(owner, stateMachine)
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
            //땅 착지 후 Idle 전환처리
            if (radish.IsGrounded())
            {
                stateMachine.ChangeState(owner.IdleState);
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