namespace FSM
{
    public class StateMachine
    {
        private IState currentState;
        public IState CurrentState => currentState;

        public void ChangeState(IState newState)
        {
            currentState?.OnExit();
            currentState = newState;
            currentState?.OnEnter();
        }

        public void Update()
        {
            currentState?.OnUpdate();
        }

        public void FixedUpdate()
        {
            currentState?.OnFixedUpdate();
        }
    }
}
