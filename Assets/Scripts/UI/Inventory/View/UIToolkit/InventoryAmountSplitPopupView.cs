using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace MyInventory.UIToolkit
{
    public class InventoryAmountSplitPopupView : BasePopupView
    {
        private readonly InventoryAmountSplitPopupViewModel _popupViewModel; //뷰모델

        private Action<int> _onConfirmCallback; //아이템 나누기 액션

        public InventoryAmountSplitPopupView(VisualElement rootElement) : base(rootElement)
        {
            //dataSource 지정
            _popupViewModel = new InventoryAmountSplitPopupViewModel();
            RootElement.dataSource = _popupViewModel;
            
            //각 버튼별 이벤트 연결
            RootElement.RegisterCallback<ClickEvent>(OnPopupClicked);
            
            if (RootElement != null)
            {
                //수량 필드의 실제 텍스트 입력 영역을 가져옴
                VisualElement textInputZone = RootElement.Q("unity-text-input");
                if (textInputZone != null)
                {
                    //실제 텍스트 입력 영역의 텍스트 정렬을 중앙으로 지정
                    textInputZone.style.unityTextAlign = TextAnchor.MiddleCenter;
                }

                //입력 필드의 값(string)이 변경되는걸 감지해서 감지될 때 마다 OnTextFieldInputChanged 실행
                RootElement.RegisterCallback<ChangeEvent<string>>(OnTextFieldInputChanged);
            }
        }
        
        private void OnPopupClicked(ClickEvent evt)
        {
            //클릭된 요소가 버튼이 맞는지 확인
            if (evt.target is not Button clickedButton) return;

            //스위치문을 통해 각 버튼별 동작 적용
            switch (clickedButton.name)
            {
                case "btn-minus":   _popupViewModel.AdjustAmount(-1); break;
                case "btn-plus":    _popupViewModel.AdjustAmount(1); break;
                case "btn-confirm": OnConfirmClicked(); break;
                case "btn-cancel":  Hide(); break;
            }

            //클릭 신호가 부모 레이아웃으로 퍼져나가는 것을 방지
            evt.StopPropagation();
        }

        //팝업 내용 구성 후 출력
        public void Show(ItemData itemData, int maxAmount, Action<int> onConfirmAction)
        {
            if (itemData == null || maxAmount <= 1) return;

            _onConfirmCallback = onConfirmAction;
            _popupViewModel.Setup(itemData, maxAmount);

            //팝업 출력
            base.Show();
        }

        //_amountField의 값이 변경되었을 때 실행
        private void OnTextFieldInputChanged(ChangeEvent<string> evt)
        {
            //이벤트를 발생시킨 주체가 수량 입력 텍스트 필드인지 이름표(name)로 식별 진행
            if (evt.target is TextField textField && textField.name == "field-amount")
            {
                //만약 플러스, 마이너스 버튼으로 값을 변경한 경우라면 뷰모델 및 UI에서 보이는 값이 이미 같음
                //따라서 이 경우 아래 내용은 패스
                if (evt.newValue == _popupViewModel.AmountText) return;
                
                _popupViewModel.UpdateAmountFromText(evt.newValue);
                
                //SetValueWithoutNotify을 이용해서 이벤트 호출 없이 값을 변경하여 추가 실행 방지
                textField.SetValueWithoutNotify(_popupViewModel.AmountText);
            }
        }

        //확인 버튼 누를 시 
        private void OnConfirmClicked()
        {
            //우선 팝업을 닫아줌
            Hide();
            
            //콜백 저장 후 _onConfirmCallback은 다시 비워두기
            Action<int> callback = _onConfirmCallback;
            _onConfirmCallback = null;

            if (callback != null && _popupViewModel.CurrentAmount > 0)
            {
                try
                {
                    //인벤토리 모델의 SeparateItem 메소드가 실행됨
                    callback.Invoke(_popupViewModel.CurrentAmount);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SplitPopup] 에러: {ex.Message}");
                }
            }
        }
    }
}