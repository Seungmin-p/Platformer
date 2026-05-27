namespace FSMGraph
{
    //여러 조건들을 하나로 묶어서 관리하기 위해 사용하는 인터페이스
    public interface ICondition
    {
        bool Evaluate(Player owner);
    }
    
    //트랜지션, 컨텍스트 방식을 둘 다 사용하기 위한 인터페이스
    public interface ITransitionProperty
    {
        bool CanChangeState(Player owner);
    }
}