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

        //대시 체크 후 상태변환을 진행하는 메소드
        //이후 코드 실행 방지를 위한 bool 타입
        protected bool CheckDashAction()
        {
            //대시 입력이 됐고, 가능한 상태라면
            if (controller.DashInput && controller.CanDash)
            {
                stateMachine.ChangeState(owner.DashState);
                
                return true;
            }
            
            return false;
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