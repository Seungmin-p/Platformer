using UnityEngine;
using FSM;

namespace FSMGraph
{
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    [CreateAssetMenu(menuName = "FSM/Transition Properties", fileName = "TransitionProperties")]
    public class TransitionProperties : ScriptableObject, ITransitionProperty
    {
        //컨디션 리스트 추가
        public string ButtonDown;
        public bool IsGrounded;
        public bool IsFall;
        public bool UseIsWall;
        public bool IsWall;
        public bool CanJump;
        public bool CanDoubleJump;
        public bool IsEnemyStepped;
        public bool IsOppositionMove;
        public bool CanDash;
        public bool DashFinished;
        public float HorizontalInput;
        public ComparisonOperator HorizontalInputOperator = ComparisonOperator.GreaterThan;

        public bool CanChangeState(Player owner)
        {
            //TODO : 컨디션 루프
            if (!string.IsNullOrEmpty(ButtonDown) && !Input.GetButtonDown(ButtonDown))
                return false;

            if (IsGrounded && !owner.IsGrounded )
                return false;
            
            if(IsFall && !owner.IsFall)
                return false;
            
            if(UseIsWall && IsWall != owner.IsWall)
                return false;
            
            if(CanJump && !owner.CanJump)
                return false;
            
            if(CanDoubleJump && !owner.CanDoubleJump)
                return false;
            
            if(IsEnemyStepped && !owner.IsEnemyStepped)
                return false;
            
            if(CanDash && !owner.CanDash)
                return false;
            
            if(DashFinished && !owner.IsDashFinished)
                return false;

            if (IsOppositionMove)
            {
                if(!owner.IsOppositionMove)
                    return false;
            }

            if (Mathf.Abs(HorizontalInput) > 0.001f && !CompareHorizontalInput(owner.XInput, HorizontalInput, HorizontalInputOperator))
                return false;

            return true;
        }

        private bool CompareHorizontalInput(float ownerValue, float targetValue, ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.Equal:
                    return Mathf.Approximately(ownerValue, targetValue);
                case ComparisonOperator.NotEqual:
                    return !Mathf.Approximately(ownerValue, targetValue);
                case ComparisonOperator.GreaterThan:
                    return Mathf.Abs(ownerValue) > targetValue;
                case ComparisonOperator.LessThan:
                    return Mathf.Abs(ownerValue) < targetValue;
                case ComparisonOperator.GreaterThanOrEqual:
                    return Mathf.Abs(ownerValue) >= targetValue;
                case ComparisonOperator.LessThanOrEqual:
                    return Mathf.Abs(ownerValue) <= targetValue;
                default:
                    return true;
            }
        }
    }
}
