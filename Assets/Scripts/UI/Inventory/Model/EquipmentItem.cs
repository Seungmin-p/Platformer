using System;
using UnityEngine;

namespace MyInventory
{
    ///<summary> 단일 아이템 - 내구도와 장착 상태를 가지는 장비 (Model 계층) </summary>
    public abstract class EquipmentItem : Item, IUsableItem
    {
        public EquipmentItemData EquipmentData { get; private set; }
        
        public int Durability { get; protected set; }
        public bool IsEquipped { get; protected set; }

        public event Action<int> OnDurabilityChanged;
        public event Action<bool> OnEquipStateChanged;

        public EquipmentItem(EquipmentItemData data) : base(data)
        {
            EquipmentData = data;
            Durability = data.MaxDurability;
            IsEquipped = false;
        }

        public void SetDurability(int value)
        {
            Durability = Mathf.Clamp(value, 0, EquipmentData.MaxDurability);
            OnDurabilityChanged?.Invoke(Durability);
        }

        //가방 안에서 장비를 우클릭하면 장착/해제가 토글됩니다.
        public bool Use()
        {
            IsEquipped = !IsEquipped;
            OnEquipStateChanged?.Invoke(IsEquipped);
            
            //추후 캐릭터 스탯이나 장비창 UI 시스템과 이벤트를 연동할 수 있습니다.
            return true; 
        }
    }
}