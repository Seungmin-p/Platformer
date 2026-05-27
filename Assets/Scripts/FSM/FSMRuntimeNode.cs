using System;
using UnityEngine;

namespace FSMGraph
{
    [Serializable]
    public abstract class FSMRuntimeNode
    {
        public string Id;
    }

    [Serializable]
    public class FSMRuntimeStateNode : FSMRuntimeNode
    {
        public string StateType;
        public bool IsStartState;
    }

    [Serializable]
    public class FSMRuntimeTransitionNode : FSMRuntimeNode
    {
        //트랜지션 노드와 컨텍스트 노드 중 데이터가 있는 것을 반환하여, 둘 다 사용 가능하도록 구성
        public TransitionProperties TransitionProperties;
        
        [SerializeReference] public ContextProperties ContextProperties;
        
        public ITransitionProperty Properties 
        {
            get 
            {
                if (ContextProperties != null)
                    return ContextProperties;
                
                return TransitionProperties;
            }
        }
    }

    [Serializable]
    public class FSMRuntimeStartNode : FSMRuntimeNode
    {
    }

    [Serializable]
    public class FSMRuntimeConnection
    {
        public string FromNodeId;
        public string ToNodeId;
    }
}
