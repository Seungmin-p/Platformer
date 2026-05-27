using System;
using FSMGraph;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace FSMGraph
{
    [Serializable]
    public class FSMContextNode : ContextNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            //TODO : 공통 옵션 추가하기
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").Build();
            context.AddOutputPort("Out").Build();
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class InputCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<string>("ButtonName")
                .WithDisplayName("ButtonName")
                .WithDefaultValue("")
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("ButtonName").TryGetValue<string>(out string btnName);
    
            return new InputCondition 
            { 
                ButtonName = btnName 
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class GroundedCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("IsGrounded")
                .WithDisplayName("IsGrounded")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("IsGrounded").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.IsGrounded,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class FallCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("IsFall")
                .WithDisplayName("IsFall")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("IsFall").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.IsFall,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class WallCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("IsWall")
                .WithDisplayName("IsWall")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("IsWall").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.IsWall,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class CanJumpCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("CanJump")
                .WithDisplayName("CanJump")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("CanJump").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.CanJump,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class CanDoubleJumpCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("CanDoubleJump")
                .WithDisplayName("CanDoubleJump")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("CanDoubleJump").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.CanDoubleJump,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class EnemySteppedCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("IsEnemyStepped")
                .WithDisplayName("IsEnemyStepped")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("IsEnemyStepped").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.IsEnemyStepped,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class CanDashCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("CanDash")
                .WithDisplayName("CanDash")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("CanDash").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.CanDash,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class DashFinishedCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("DashFinished")
                .WithDisplayName("DashFinished")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("DashFinished").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.DashFinished,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class OppositionMoveCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>("IsOppositionMove")
                .WithDisplayName("IsOppositionMove")
                .WithDefaultValue(true)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("IsOppositionMove").TryGetValue<bool>(out bool val);
            
            return new PlayerBoolCondition 
            { 
                StateType = BoolStateType.IsOppositionMove,
                ExpectedValue = val
            };
        }
    }

    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class HorizontalInputCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            //비교 연산자
            context.AddOption<ComparisonOperator>("Operator")
                .WithDisplayName("Operator")
                .WithDefaultValue(ComparisonOperator.GreaterThan)
                .Build();

            //비교할 수치
            context.AddOption<float>("CompareValue")
                .WithDisplayName("Value")
                .WithDefaultValue(0.1f)
                .Build();
        }
        
        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("Operator").TryGetValue<ComparisonOperator>(out ComparisonOperator opVal);
            GetNodeOptionByName("CompareValue").TryGetValue<float>(out float targetVal);

            return new HorizontalInputCondition 
            { 
                Op = opVal,
                TargetValue = targetVal 
            };
        }
    }
}