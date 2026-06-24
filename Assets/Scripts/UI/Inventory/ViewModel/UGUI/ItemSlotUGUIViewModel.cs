using System;
using UnityEngine;

namespace MyInventory.UGUI
{
    public class ItemSlotUGUIViewModel
    {
        //아이템 데이터, 이미지, 수량
        private ItemData _itemData;
        private Sprite _iconSprite;
        private int _amount;
        
        //슬롯별로 아이템이 있는지, 필터 상태는 어떤지에 대한 변수
        private bool _hasItem;
        private bool _isFilteredOut;
        
        public string AmountText => _amount.ToString();
        public bool AmountVisible => (_hasItem && _amount > 1);
        public Sprite IconSprite => _hasItem ? _iconSprite : null;
        public Color IconTint => _isFilteredOut ? new Color(0.25f, 0.25f, 0.25f, 1f) : Color.white;

        //슬롯 데이터가 변경될 때 마다 호출될 이벤트
        public event Action OnStateChanged;

        //아이템 존재 여부, 이미지, 데이터
        public bool HasItem => _hasItem;
        public ItemData ItemData => _itemData; 
        
        //수량 표시, 수량의 문자열, 필터 적용 상태
        public bool IsFilteredOut => _isFilteredOut;

        //인벤토리 뷰 모델에서 호출하는 슬롯 업데이트 메소드
        public void UpdateSlot(ItemData itemData, Sprite icon, int amount, bool isFilteredOut)
        {
            //모든 데이터들이 기존 데이터와 동일하다면
            if (_itemData == itemData && _iconSprite == icon && _amount == amount && 
                _hasItem == (icon != null) && _isFilteredOut == isFilteredOut) 
                return;

            //전달받은 데이터들 저장하고, 상태 변경 이벤트 호출
            _itemData = itemData;
            _iconSprite = icon;
            _amount = amount;
            _hasItem = icon != null && amount > 0;
            _isFilteredOut = isFilteredOut;
            
            OnStateChanged?.Invoke();
        }

        //인벤토리 뷰 모델에서 호출하는 슬롯 클리어 메소드
        public void ClearSlot()
        {
            if (!_hasItem && !_isFilteredOut) return;

            //모든 데이터 초기화 이후, 상태 변경 이벤트 호출
            _itemData = null;
            _iconSprite = null;
            _amount = 0;
            _hasItem = false;
            _isFilteredOut = false;

            OnStateChanged?.Invoke();
        }
    }
}