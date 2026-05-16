using System.Collections.Generic;
using UnityEngine;
using FSM;
using FSMGraph;

public class PlayerFallState : PlayerState
{
    private readonly List<TransitionStatePair> transitions;
    private static readonly int animHash = Animator.StringToHash("Fall");

    public PlayerFallState(Player owner, StateMachine<Player> sm) : base(owner, sm)
    {
        transitions = new List<TransitionStatePair>();
    }

    public PlayerFallState(Player owner, StateMachine<Player> sm, List<TransitionStatePair> transitions) : base(owner, sm)
    {
        this.transitions = transitions ?? new List<TransitionStatePair>();
    }

    public override void OnEnter()
    {
        owner.Animator.Play(animHash);
        
        owner.CanJump = false;
    }

    public override void OnUpdate()
    {
        foreach (var transition in transitions)
        {
            if (transition.Properties != null && transition.Properties.CanChangeState(owner))
            {
                stateMachine.ChangeState(transition.NextStateFactory());
                return;
            }
        }
    }

    public override void OnFixedUpdate()
    {
        controller.ExecuteMove(controller.XInput);
    }
}
