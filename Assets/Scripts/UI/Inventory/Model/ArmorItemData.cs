using UnityEngine;

namespace MyInventory
{
    [CreateAssetMenu(fileName = "Item_Armor_", menuName = "Inventory System/Item Data/Armor", order = 2)]
    public class ArmorItemData : EquipmentItemData
    {
        [SerializeField] private int _defence = 1;
        public int Defence => _defence;

        public override Item CreateItem() => new ArmorItem(this);
    }
}