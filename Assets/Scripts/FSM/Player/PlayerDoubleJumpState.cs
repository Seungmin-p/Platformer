using System.Collections.Generic;
using UnityEngine;
using FSM;
using FSMGraph;

public class PlayerDoubleJumpState : PlayerState
{
    private readonly List<TransitionStatePair> transitions;
    private static readonly int animHash = Animator.StringToHash("Double_Jump");

    public PlayerDoubleJumpState(Player owner, StateMachine<Player> sm) : base(owner, sm)
    {
        transitions = new List<TransitionStatePair>();
    }

    public PlayerDoubleJumpState(Player owner, StateMachine<Player> sm, List<TransitionStatePair> transitions) : base(owner, sm)
    {
        this.transitions = transitions ?? new List<TransitionStatePair>();
    }

    public override void OnEnter()
    {        
        controller.ExecuteJump(controller.JumpForce);
        owner.Animator.Play(animHash);
        
        owner.CanDoubleJump = false;

        for (int i = 0; i < 6; i++)
        {
            var emitParams = new ParticleSystem.EmitParams();
            emitParams.startSize = Random.Range(0.5f, 1.5f);
            emitParams.startColor = new Color(1, 1, 1, Random.Range(0.4f, 1.0f));
            owner.JumpDust.Play();
        }
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
        if (!controller.IsWall)
        {
            controller.ExecuteMove(controller.XInput);
        }
        else if(owner.IsOppositionMove)
        {
            //만약 앞에있는게 벽이라면, 반대방향키 눌렀을 때 바로 빠져나올 수 있어야함
            controller.ExecuteMove(controller.XInput);
        }
    }
}
