using UnityEngine;

namespace MyInventory
{
    [CreateAssetMenu(fileName = "Item_Weapon_", menuName = "Inventory System/Item Data/Weapon", order = 1)]
    public class WeaponItemData : EquipmentItemData
    {
        [SerializeField] private int _damage = 1;
        public int Damage => _damage;

        public override Item CreateItem() => new WeaponItem(this);
    }
}