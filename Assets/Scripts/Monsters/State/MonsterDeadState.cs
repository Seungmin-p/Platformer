using UnityEngine;

namespace FSM
{
    public class MonsterDeadState : MonsterState
    {
        private Vector2 bounceDirection;
        public MonsterDeadState(Monster owner, StateMachine stateMachine) : base(owner, stateMachine) {}

        //bounceDir 전달용
        public void Setup(Vector2 bounceDir)
        {
            bounceDirection = bounceDir;
        }
        
        public override void OnEnter()
        {
            //몬스터 Hit 판정
            controller.ExecuteDie(bounceDirection);
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