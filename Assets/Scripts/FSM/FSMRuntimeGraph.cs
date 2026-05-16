using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FSM;

namespace FSMGraph
{
    public class FSMRuntimeGraph : ScriptableObject
    {
        [SerializeReference]
        public List<FSMRuntimeNode> Nodes = new();
        public List<FSMRuntimeConnection> Connections = new();

        public StateMachine<Player> CreateStateMachine(Player owner)
        {
            if (owner == null)
            {
                Debug.LogError("FSMRuntimeGraph: Owner is null.");
                return null;
            }

            var fsm = new StateMachine<Player>();

            var runtimeNodeById = Nodes
                .Where(node => !string.IsNullOrEmpty(node.Id))
                .ToDictionary(node => node.Id, node => node);

            var stateNodeById = Nodes
                .OfType<FSMRuntimeStateNode>()
                .Where(node => !string.IsNullOrEmpty(node.Id))
                .ToDictionary(node => node.Id, node => node);

            var transitionToTargetState = new Dictionary<string, string>();
            foreach (var connection in Connections)
            {
                if (!runtimeNodeById.TryGetValue(connection.FromNodeId, out var fromNode) ||
                    !runtimeNodeById.TryGetValue(connection.ToNodeId, out var toNode))
                {
                    continue;
                }

                if (fromNode is FSMRuntimeTransitionNode transition && toNode is FSMRuntimeStateNode targetState)
                {
                    transitionToTargetState[transition.Id] = targetState.Id;
                }
            }

            var transitionPairsByStateId = new Dictionary<string, List<TransitionStatePair>>();
            foreach (var connection in Connections)
            {
                if (!runtimeNodeById.TryGetValue(connection.FromNodeId, out var fromNode) ||
                    !runtimeNodeById.TryGetValue(connection.ToNodeId, out var toNode))
                {
                    continue;
                }

                if (fromNode is FSMRuntimeStateNode state && toNode is FSMRuntimeTransitionNode transition)
                {
                    if (!transitionToTargetState.TryGetValue(transition.Id, out var targetStateId))
                        continue;

                    if (!transitionPairsByStateId.TryGetValue(state.Id, out var list))
                    {
                        list = new List<TransitionStatePair>();
                        transitionPairsByStateId[state.Id] = list;
                    }

                    list.Add(new TransitionStatePair
                    {
                        Properties = transition.Properties,
                        NextStateFactory = () => CreateState(owner, fsm, targetStateId, transitionPairsByStateId, stateNodeById),
                    });
                }
            }

            var startState = FindStartState(runtimeNodeById, stateNodeById);
            if (startState == null)
            {
                Debug.LogError("FSMRuntimeGraph: No state nodes were found in the runtime graph.");
                return null;
            }

            var initialState = CreateState(owner, fsm, startState.Id, transitionPairsByStateId, stateNodeById);
            if (initialState == null)
            {
                Debug.LogError("FSMRuntimeGraph: Failed to create initial state.");
                return null;
            }

            fsm.ChangeState(initialState);
            return fsm;
        }

        private State<Player> CreateState(
            Player owner,
            StateMachine<Player> fsm,
            string stateNodeId,
            Dictionary<string, List<TransitionStatePair>> transitionPairsByStateId,
            Dictionary<string, FSMRuntimeStateNode> stateNodeById)
        {
            if (!stateNodeById.TryGetValue(stateNodeId, out var stateNode))
            {
                Debug.LogError($"FSMRuntimeGraph: Cannot create state for unknown state node id '{stateNodeId}'.");
                return null;
            }

            var transitions = transitionPairsByStateId.TryGetValue(stateNodeId, out var pairList)
                ? pairList
                : new List<TransitionStatePair>();

            //상태 추가
            switch (stateNode.StateType)
            {
                case "PlayerIdleState":
                    return new PlayerIdleState(owner, fsm, transitions);
                case "PlayerRunState":
                    return new PlayerRunState(owner, fsm, transitions);
                case "PlayerFallState":
                    return new PlayerFallState(owner, fsm, transitions);
                case "PlayerWallState":
                    return new PlayerWallState(owner, fsm, transitions);
                case "PlayerWallJumpState":
                    return new PlayerWallJumpState(owner, fsm, transitions);
                case "PlayerJumpState":
                    return new PlayerJumpState(owner, fsm, transitions);
                case "PlayerDoubleJumpState":
                    return new PlayerDoubleJumpState(owner, fsm, transitions);
                case "PlayerDashState":
                    return new PlayerDashState(owner, fsm, transitions);
                default:
                    Debug.LogError($"FSMRuntimeGraph: Unknown state type '{stateNode.StateType}'.");
                    return null;
            }
        }

        private FSMRuntimeStateNode FindStartState(
            Dictionary<string, FSMRuntimeNode> runtimeNodeById,
            Dictionary<string, FSMRuntimeStateNode> stateNodeById)
        {
            var startConnection = Connections
                .FirstOrDefault(c => runtimeNodeById.TryGetValue(c.FromNodeId, out var fromNode) && fromNode is FSMRuntimeStartNode &&
                                     runtimeNodeById.TryGetValue(c.ToNodeId, out var toNode) && toNode is FSMRuntimeStateNode);

            if (startConnection != null)
                return stateNodeById[startConnection.ToNodeId];

            return stateNodeById.Values.FirstOrDefault(node => node.IsStartState) ?? stateNodeById.Values.FirstOrDefault();
        }
    }
}