namespace FSMGraph
{
    public interface IConditionBlockNode
    {
        ICondition CreateRuntimeCondition();
    }
}