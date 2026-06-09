namespace MyInventory
{
    ///<summary> 사용 가능한 아이템 인터페이스 (Model 계층) </summary>
    public interface IUsableItem
    {
        ///<summary> 아이템 사용 (성공 여부 리턴) </summary>
        bool Use();
    }
}