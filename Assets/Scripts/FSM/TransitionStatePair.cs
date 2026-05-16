using System;
using FSM;

namespace FSMGraph
{
    [Serializable]
    public class TransitionStatePair
    {
        public TransitionProperties Properties;
        public Func<State<Player>> NextStateFactory;
    }
}
