using UnityEngine;

namespace FSM
{
    public class MonsterTrunkAttackState : MonsterState
    {
        //애니메이션 전환의 성능 향상을 위해 미리 해쉬값으로 변환해둠
        private static readonly int attackAnimHash = Animator.StringToHash("Attack");
        private static readonly int idleAnimHash = Animator.StringToHash("Idle");
        private float attackTimer;
        private bool hasAttack;
        private Trunk trunk; //Trunk 본체를 담는 변수

        public MonsterTrunkAttackState(Monster owner, StateMachine stateMachine) : base(owner, stateMachine)
        {
            //owner를 Trunk로 형변환 해서 저장
            trunk = (Trunk)owner;
        }

        public override void OnEnter()
        {
            //상태 설정에서 Play 방식으로 변경
            trunk.Animator.Play(attackAnimHash);

            //공격 타이머 및 공격을 이미 했는지 판단하는 변수
            attackTimer = 0.3f;
            hasAttack = false;
        }
        public override void OnUpdate()
        {
            if (attackTimer > 0.0f)
            {
                attackTimer -= Time.deltaTime;
            }
            else if( !hasAttack )
            {
                //애니메이션 모션 딜레이(0.3초)가 지나고 아직 공격하지 않은 상태라면 공격
                //trunk 형변환이 되어있기 때문의 클래스 내 메소드 사용 가능
                hasAttack = true;
                trunk.TrunkAttack();
                trunk.Animator.Play(idleAnimHash);
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