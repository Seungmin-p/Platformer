namespace MyInventory
{
    ///<summary> 수량 아이템 - 포션 아이템 (Model 계층)</summary>
    public class PotionItem : CountableItem, IUsableItem
    {
        public PotionItem(PotionItemData data, int amount = 1) : base(data, amount) { }

        public bool Use()
        {
            if (IsEmpty) return false;

            //SetAmount를 이용해서 수치 -1
            SetAmount(Amount - 1);

            //실제 게임이라면 여기에 포션 사용 효과 추가
            
            return true;
        }

        protected override CountableItem Clone(int amount)
        {
            //Clone 동작 시 CountableItem 만으로 처리가 가능하도록 CountableItem 형태로 반환
            return new PotionItem(CountableData as PotionItemData, amount);
        }
    }
}