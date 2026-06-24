using UnityEngine;
using System;

namespace MyInventory.UGUI
{
    public class InventoryAmountSplitPopupUGUIViewModel
    {
        private ItemData _itemDataModel; //주입받은 아이템 데이터 모델
        
        //현재 개수, 현재 입력 가능한 최대 개수
        private int _currentAmount;
        private int _maxLimitAmount;
        
        //팝업 상단 아이템 이름 바인딩
        public string NameText => _itemDataModel != null ? _itemDataModel.Name : string.Empty;
        public string AmountText => _currentAmount.ToString();
        
        public int CurrentAmount => _currentAmount;

        public event Action OnStateChanged;

        //팝업이 열릴 때 데이터를 세팅하는 메소드
        public void Setup(ItemData itemData, int maxAmount)
        {
            _itemDataModel = itemData;
            _maxLimitAmount = maxAmount - 1; //아이템 이동이 아니라 나누기기 때문에 총 아이템 개수 -1
            
            //아이템 나누기를 시도할 때 기본값은 아이템 개수의 절반으로 지정
            ApplyValueRange(maxAmount / 2);
        }
        
        //수량 필드의 값 변경을 적용하는 메소드
        private void ApplyValueRange(int newAmount)
        {
            //현재 입력된 개수를 1에서 최대 입력 가능 수치까지로 제한
            _currentAmount = Mathf.Clamp(newAmount, 1, _maxLimitAmount);
            OnStateChanged?.Invoke();
        }
        
        //-, + 버튼을 눌렀을 때 실행되는 메소드
        public void AdjustAmount(int delta)
        {
            ApplyValueRange(_currentAmount + delta);
        }

        //_currentAmount의 값이 변경되었을 때 실행
        public void UpdateAmountFromText(string text)
        {
            //사용자가 입력한 텍스트(evt.newValue)를 int로 변환 시도 후, 성공 시 true
            if (int.TryParse(text, out int parsedResult))
            {
                //전환 성공 시 전환된 숫자인 parsedResult를 가져와서 ApplyValueRange에 적용
                ApplyValueRange(parsedResult);
            }
            else
            {
                //int 전환 실패 시, 그냥 기존 값을 그대로 유지
                ApplyValueRange(_currentAmount);
            }
        }

        public void Clear()
        {
            _itemDataModel = null;
            _currentAmount = 0;
            _maxLimitAmount = 0;
            OnStateChanged?.Invoke();
        }
    }
}