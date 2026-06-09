using UnityEngine;

namespace MyInventory
{
    //수량 아이템이 가질 데이터
    public abstract class CountableItemData : ItemData
    {
        [Header("슬롯 하나에 들어갈 수량 설정")]
        [SerializeField, Min(1)] private int _maxAmount = 99;
    
        public int MaxAmount => _maxAmount;
    }
}