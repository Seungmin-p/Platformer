using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace FSMGraph
{
    [Serializable]
    internal class TransitionNode : FSMNode
    {
        public const string ButtonDownPortName = "ButtonDown";
        public const string IsGroundedPortName = "IsGrounded";
        public const string IsFallPortName = "IsFall";
        public const string IsWallPortName = "IsWall";
        public const string UseIsWallPortName = "UseIsWall";
        public const string CanJumpPortName = "CanJump";
        public const string CanDoubleJumpPortName = "CanDoubleJump";
        public const string IsEnemySteppedPortName = "IsEnemyStepped";
        public const string IsOppositionMovePortName = "IsOppositionMove";
        public const string CanDashPortName = "CanDash";
        public const string DashFinishedPortName = "DashFinished";
        public const string HorizontalInputPortName = "HorizontalInput";
        public const string HorizontalInputOperatorPortName = "HorizontalInputOperator";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);

            context.AddInputPort<string>(ButtonDownPortName)
                .WithDisplayName("Button Down")
                .Build();

            context.AddInputPort<bool>(IsGroundedPortName)
                .WithDisplayName("Is Grounded")
                .Build();
            
            context.AddInputPort<bool>(IsFallPortName)
                .WithDisplayName("Is Fall")
                .Build();
            
            context.AddInputPort<bool>(UseIsWallPortName)
                .WithDisplayName("Use Is Wall")
                .Build();
            
            context.AddInputPort<bool>(IsWallPortName)
                .WithDisplayName("Is Wall")
                .Build();
            
            context.AddInputPort<bool>(CanJumpPortName)
                .WithDisplayName("Can Jump")
                .Build();
            
            context.AddInputPort<bool>(CanDoubleJumpPortName)
                .WithDisplayName("Can DoubleJump")
                .Build();
            
            context.AddInputPort<bool>(IsEnemySteppedPortName)
                .WithDisplayName("Is Enemy Stepped")
                .Build();
            
            context.AddInputPort<bool>(IsOppositionMovePortName)
                .WithDisplayName("Is Opposition Move")
                .Build();
            
            context.AddInputPort<bool>(CanDashPortName)
                .WithDisplayName("Can Dash")
                .Build();
            
            context.AddInputPort<bool>(DashFinishedPortName)
                .WithDisplayName("Dash Finished")
                .Build();

            context.AddInputPort<float>(HorizontalInputPortName)
                .WithDisplayName("Horizontal Input")
                .Build();

            context.AddInputPort<ComparisonOperator>(HorizontalInputOperatorPortName)
                .WithDisplayName("Horizontal Input Operator")
                .WithDefaultValue(ComparisonOperator.GreaterThan)
                .Build();
        }
    }
}
