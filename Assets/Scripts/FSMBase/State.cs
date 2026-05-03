using UnityEngine;

namespace FSM
{
    public abstract class State<T> : IState
    {
        protected T owner; //상태 패턴 적용 대상
        protected StateMachine stateMachine; //상태 머신

        public State(T owner, StateMachine stateMachine)
        {
            this.owner = owner;
            this.stateMachine = stateMachine;
        }

        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnFixedUpdate() {}
        public virtual void OnUpdate() {}
    }
    
    //플레이어 전용 상태(컨트롤러 할당용)
    public abstract class PlayerState : State<Player>
    {
        protected PlayerStateController controller;

        public PlayerState(Player owner, StateMachine stateMachine) : base(owner, stateMachine)
        {
            controller = owner as PlayerStateController;
        }
    }

    //몬스터 전용 상태(컨트롤러 할당용)
    public abstract class MonsterState : State<Monster>
    {
        protected MonsterStateController controller;

        public MonsterState(Monster owner, StateMachine stateMachine) : base(owner, stateMachine)
        {
            controller = owner as MonsterStateController;
        }
    }
}