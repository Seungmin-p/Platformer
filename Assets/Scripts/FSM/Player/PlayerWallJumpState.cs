using System.Collections.Generic;
using UnityEngine;
using FSM;
using FSMGraph;

public class PlayerWallJumpState : PlayerState
{
    private readonly List<TransitionStatePair> transitions;
    private static readonly int animHash = Animator.StringToHash("Jump");
    private float wallJumpTimer;

    public PlayerWallJumpState(Player owner, StateMachine<Player> sm) : base(owner, sm)
    {
        transitions = new List<TransitionStatePair>();
    }

    public PlayerWallJumpState(Player owner, StateMachine<Player> sm, List<TransitionStatePair> transitions) : base(owner, sm)
    {
        this.transitions = transitions ?? new List<TransitionStatePair>();
    }

    public override void OnEnter()
    {
        owner.Animator.Play(animHash);
        
        //벽 점프 실행
        WallJump();
        
        controller.CanJump = false;
    }

    public override void OnUpdate()
    {
        wallJumpTimer -= Time.deltaTime;
        
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
        //벽 점프 직후 0.1초동안 이동 조작 불가, 타이머가 지난 후 이동 가능
        if (wallJumpTimer <= 0 && !controller.IsWall)
        {
            //공중에서도 좌/우 이동은 가능해야함
            controller.ExecuteMove(controller.XInput);
        }
    }
    
    private void WallJump()
    {
        //벽의 반대 방향으로 밀리는 점프 실행해야함
        //상태 및 타이머 설정
        wallJumpTimer = 0.1f;
                    
        //방향 전환처리
        controller.PlayerDirection *= -1; 
        owner.transform.localScale = new Vector3( owner.transform.localScale.x * -1, 1, 1);

        //붙은 벽의 반대방향으로 밀려야함
        owner.Rb.linearVelocity = new Vector2((controller.JumpForce / 3 ) * controller.PlayerDirection, controller.JumpForce);
    }
}
