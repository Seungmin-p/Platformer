using UnityEngine;
using System;
using System.Collections.Generic;

namespace FSMGraph
{
    [Serializable]
    public class ContextProperties : ITransitionProperty
    {
        [SerializeReference] public List<ICondition> Conditions = new List<ICondition>();

        public bool CanChangeState(Player owner)
        {
            //조건들 체크
            foreach (var condition in Conditions)
            {      
                //안맞는 조건이 하나라도 있다면 false
                if (!condition.Evaluate(owner))
                {
                    return false;
                }
            }

            //조건에 전부 문제 없다면 통과
            return true;
        }
    }
    
    //단순 Bool 판정인 속성들
    public enum BoolStateType
    {
        IsGrounded,
        IsFall,
        IsWall,
        CanJump,
        CanDoubleJump,
        IsEnemyStepped,
        CanDash,
        DashFinished,
        IsOppositionMove
    }
    
    /* 각 상태별 조건 검사 진행 */
    
    //Enum을 이용해 Bool 판정 로직 하나로 이용
    public class PlayerBoolCondition : ICondition
    {
        public BoolStateType StateType;
        public bool ExpectedValue;

        public bool Evaluate(Player owner)
        {
            bool actualValue = false;
            
            switch (StateType)
            {
                case BoolStateType.IsGrounded: actualValue = owner.IsGrounded; break;
                case BoolStateType.IsFall: actualValue = owner.IsFall; break;
                case BoolStateType.IsWall: actualValue = owner.IsWall; break;
                case BoolStateType.CanJump: actualValue = owner.CanJump; break;
                case BoolStateType.CanDoubleJump: actualValue = owner.CanDoubleJump; break;
                case BoolStateType.IsEnemyStepped: actualValue = owner.IsEnemyStepped; break;
                case BoolStateType.CanDash: actualValue = owner.CanDash; break;
                case BoolStateType.DashFinished: actualValue = owner.IsDashFinished; break;
                case BoolStateType.IsOppositionMove: actualValue = owner.IsOppositionMove; break;
            }

            return actualValue == ExpectedValue;
        }
    }
    
    //버튼 입력 판정(점프, 대시)
    public class InputCondition : ICondition
    {
        public string ButtonName;
        public bool Evaluate(Player owner)
        {
            if (string.IsNullOrEmpty(ButtonName)) return true;
            return Input.GetButtonDown(ButtonName);
        }
    }

    //움직임 입력 판정(좌,우)
    public class HorizontalInputCondition : ICondition
    {
        public ComparisonOperator Op;
        public float TargetValue;

        public bool Evaluate(Player owner)
        {
            float ownerValue = owner.XInput;
            switch (Op)
            {
                case ComparisonOperator.Equal: return Mathf.Approximately(ownerValue, TargetValue);
                case ComparisonOperator.NotEqual: return !Mathf.Approximately(ownerValue, TargetValue);
                case ComparisonOperator.GreaterThan: return Mathf.Abs(ownerValue) > TargetValue;
                case ComparisonOperator.LessThan: return Mathf.Abs(ownerValue) < TargetValue;
                case ComparisonOperator.GreaterThanOrEqual: return Mathf.Abs(ownerValue) >= TargetValue;
                case ComparisonOperator.LessThanOrEqual: return Mathf.Abs(ownerValue) <= TargetValue;
                default: return true;
            }
        }
    }
}