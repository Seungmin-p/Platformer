using System;
using Unity.Properties;
using UnityEngine.UIElements;
using UnityEngine;

namespace MyInventory.UIToolkit
{
    public class ItemSlotViewModel
    {
        //아이템 데이터, 이미지, 수량
        private ItemData _itemData;
        private Sprite _iconSprite;
        private int _amount;
        
        //슬롯별로 아이템이 있는지, 필터 상태는 어떤지에 대한 변수
        private bool _hasItem;
        private bool _isFilteredOut;
        
        //==== 런타임 데이터 바인딩 ====
        //인벤토리 슬롯의 아이템 수량 텍스트 영역(Item-Count)의 텍스트에 그대로 바인딩
        [CreateProperty] public string AmountText => _amount.ToString();
        //인벤토리 슬롯의 아이템 수량 텍스트 영역(Item-Count)의 텍스트의 Display 속성에 바인딩
        [CreateProperty] public DisplayStyle AmountVisible => (_hasItem && _amount > 1 ) ? DisplayStyle.Flex : DisplayStyle.None;
        //인벤토리 슬롯의 이미지에 그대로 바인딩
        [CreateProperty] public StyleBackground IconSprite => _hasItem ? new StyleBackground(_iconSprite) : null;
        //인벤토리 슬롯에서 아이템에 필터를 적용할 때, 필터에 걸리지 않은 아이템만 정상적으로 출력
        [CreateProperty] public Color IconTint => _isFilteredOut ? new Color(0.25f, 0.25f, 0.25f, 1f) : Color.white;

        //슬롯 데이터가 변경될 때 마다 호출될 이벤트
        public event Action OnStateChanged;

        //뷰 영역에서 각종 판단을 위해 사용하는 프로퍼티
        public bool HasItem => _hasItem; //아이템 존재 여부
        public ItemData ItemData => _itemData; //아이템 데이터
        public bool IsFilteredOut => _isFilteredOut; //필터 적용 상태

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