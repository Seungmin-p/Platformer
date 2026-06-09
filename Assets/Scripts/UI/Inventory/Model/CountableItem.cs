using System;
using UnityEngine;

namespace MyInventory
{
    ///<summary> 수량을 셀 수 있는 아이템 (Model 계층) </summary>
    public abstract class CountableItem : Item
    {
        public CountableItemData CountableData { get; private set; }

        ///<summary> 현재 아이템 개수 </summary>
        public int Amount { get; protected set; }

        ///<summary>이 아이템이 하나의 슬롯에 들어갈 수 있는 최대 개수</summary>
        public int MaxAmount => CountableData.MaxAmount;

        ///<summary>아이템이 최대치까지 차있는지에 대한 여부</summary>
        public bool IsMax => Amount >= CountableData.MaxAmount;

        ///<summary>아이템의 개수가 없는지 체크</summary>
        public bool IsEmpty => Amount <= 0;

        public CountableItem(CountableItemData data, int amount = 1) : base(data)
        {
            //데이터 및 수량 저장
            CountableData = data;
            SetAmount(amount);
        }

        ///<summary>아이템이 슬롯별 최대 개수를 넘은 경우, 최대 개수로 보정</summary>
        public void SetAmount(int amount)
        {
            Amount = Mathf.Clamp(amount, 0, MaxAmount);
        }

        ///<summary>아이템의 현 개수 + 추가 개수를 계산하여, 아이템별 최대 수를 넘은 경우 그 수치를 반환함</summary>
        public int AddAmountAndGetExcess(int amount)
        {
            int nextAmount = Amount + amount;
            SetAmount(nextAmount);

            //최대 수치를 넘지 않는다면 0 반환
            return (nextAmount > MaxAmount) ? (nextAmount - MaxAmount) : 0;
        }

        ///<summary>아이템 복제하면서 나누기</summary>
        public CountableItem SeparateAndClone(int amount)
        {
            //수량이 한개 이하일 경우, 복제 불가
            if(Amount <= 1) return null;

            //나누려는 아이템의 개수가 전체 아이템 개수보다 많다면 전체 -1로 변경
            if(amount > Amount - 1)
                amount = Amount - 1;

            //아이템을 이동한만큼 개수 조정
            SetAmount(Amount - amount);
        
            //옮긴 아이템 개수만큼 복제
            return Clone(amount);
        }

        protected abstract CountableItem Clone(int amount);
    }
}