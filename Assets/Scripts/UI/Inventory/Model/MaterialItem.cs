namespace MyInventory
{
    /// <summary> 수량 아이템 - 소비가 불가능한 재료 (Model 계층) </summary>
    public class MaterialItem : CountableItem
    {
        //포션과 다르게 IUsableItem를 상속받지 않아 Use가 없음
        public MaterialItem(MaterialItemData data, int amount = 1) : base(data, amount) { }

        protected override CountableItem Clone(int amount)
        {
            return new MaterialItem(CountableData as MaterialItemData, amount);
        }
    }
}