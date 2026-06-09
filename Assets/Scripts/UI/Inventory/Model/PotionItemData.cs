using UnityEngine;

namespace MyInventory
{
    [CreateAssetMenu(fileName = "Item_Potion_", menuName = "Inventory System/Item Data/Potion", order = 3)]
    public class PotionItemData : CountableItemData
    {
        [Header("사용 효과 설정")]
        [SerializeField] float _value; //효과량 (회복량 등)

        public float Value => _value;

        public override Item CreateItem()
        {
            //런타임 상태를 담을 PotionItem 인스턴스를 생성하여 반환
            return new PotionItem(this); 
        }
    }
}