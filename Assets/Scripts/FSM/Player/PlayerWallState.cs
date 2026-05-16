using System.Collections.Generic;
using UnityEngine;
using FSM;
using FSMGraph;

public class PlayerWallState : PlayerState
{
    private readonly List<TransitionStatePair> transitions;
    private static readonly int animHash = Animator.StringToHash("Wall");

    public PlayerWallState(Player owner, StateMachine<Player> sm) : base(owner, sm)
    {
        transitions = new List<TransitionStatePair>();
    }

    public PlayerWallState(Player owner, StateMachine<Player> sm, List<TransitionStatePair> transitions) : base(owner, sm)
    {
        this.transitions = transitions ?? new List<TransitionStatePair>();
    }

    public override void OnEnter()
    {
        owner.Animator.Play(animHash);
        
        owner.CanJump = true;
        owner.CanDoubleJump = true;
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
        //좌우 이동 없이 벽에서 미끄러지도록
        owner.Rb.linearVelocity = new Vector2(0f, controller.WallSlip * -1);
    }
}
