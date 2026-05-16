using System.Collections.Generic;
using UnityEngine;
using FSM;
using FSMGraph;

public class PlayerIdleState : PlayerState
{
    private readonly List<TransitionStatePair> transitions;
    private static readonly int animHash = Animator.StringToHash("Idle");

    public PlayerIdleState(Player owner, StateMachine<Player> sm) : base(owner, sm)
    {
        transitions = new List<TransitionStatePair>();
    }

    public PlayerIdleState(Player owner, StateMachine<Player> sm, List<TransitionStatePair> transitions) : base(owner, sm)
    {
        this.transitions = transitions ?? new List<TransitionStatePair>();
    }

    public override void OnEnter()
    {
        owner.Animator.Play(animHash);

        //게임 시작 직후가 아닌 경우에만
        if (!owner.IsFirstLanded)
        {
            //착지 파티클 출력
            owner.LandingDust.Play();
        }
        else
        {
            owner.IsFirstLanded = false;
        }
        
        
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
        owner.Rb.linearVelocity = new Vector2(0f, owner.Rb.linearVelocity.y);
    }
}
