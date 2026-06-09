using UnityEngine;

namespace MyInventory
{
    /// <summary> 장비 아이템 데이터 기반 클래스 </summary>
    public abstract class EquipmentItemData : ItemData
    {
        [SerializeField] private int _maxDurability = 100;
        public int MaxDurability => _maxDurability;
    }
}