using System;
using FSM;

namespace FSMGraph
{
    [Serializable]
    public class TransitionStatePair
    {
        public ITransitionProperty Properties;
        public Func<State<Player>> NextStateFactory;
    }
}
