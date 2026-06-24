using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MyInventory.UGUI
{
    public class InventoryAmountSplitPopupUGUIView : BasePopupUGUIView
    {
        private InventoryAmountSplitPopupUGUIViewModel _popupViewModel; //뷰모델

        private Action<int> _onConfirmCallback; //아이템 나누기 액션

        [Header("UGUI 컴포넌트 배선")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TMP_InputField _amountInputField;
        [SerializeField] private Button _btnMinus;
        [SerializeField] private Button _btnPlus;
        [SerializeField] private Button _btnConfirm;
        [SerializeField] private Button _btnCancel;

        protected override void Awake()
        {
            base.Awake();
            
            //dataSource 지정
            _popupViewModel = new InventoryAmountSplitPopupUGUIViewModel();
            _popupViewModel.OnStateChanged += Render;
            
            //각 버튼별 이벤트 연결
            if (_btnMinus != null) _btnMinus.onClick.AddListener(() => OnButtonClick("btn-minus"));
            if (_btnPlus != null) _btnPlus.onClick.AddListener(() => OnButtonClick("btn-plus"));
            if (_btnConfirm != null) _btnConfirm.onClick.AddListener(() => OnButtonClick("btn-confirm"));
            if (_btnCancel != null) _btnCancel.onClick.AddListener(() => OnButtonClick("btn-cancel"));
            
            if (_amountInputField != null)
            {
                //수량 필드의 실제 텍스트 입력 영역을 가져옴
                //실제 텍스트 입력 영역의 텍스트 정렬을 중앙으로 지정
                _amountInputField.textComponent.alignment = TextAlignmentOptions.Center;

                //입력 필드의 값(string)이 변경되는걸 감지해서 감지될 때 마다 OnTextFieldInputChanged 실행
                _amountInputField.onValueChanged.AddListener(OnTextFieldInputChanged);
            }
        }
        
        private void OnButtonClick(string buttonName)
        {
            //클릭된 요소가 버튼이 맞는지 확인

            //스위치문을 통해 각 버튼별 동작 적용
            switch (buttonName)
            {
                case "btn-minus":   _popupViewModel.AdjustAmount(-1); break;
                case "btn-plus":    _popupViewModel.AdjustAmount(1); break;
                case "btn-confirm": OnConfirmClicked(); break;
                case "btn-cancel":  Hide(); break;
            }

            //클릭 신호가 부모 레이아웃으로 퍼져나가는 것을 방지
        }

        private void Render()
        {
            if (_nameText != null) _nameText.text = _popupViewModel.NameText;
            
            if (_amountInputField != null && _amountInputField.text != _popupViewModel.AmountText)
            {
                //SetValueWithoutNotify을 이용해서 이벤트 호출 없이 값을 변경하여 추가 실행 방지
                // TMP_InputField 컴포넌트 규격에 맞춰 SetTextWithoutNotify API로 정밀 변환 완료했습니다.
                _amountInputField.SetTextWithoutNotify(_popupViewModel.AmountText);
            }
        }

        //팝업 내용 구성 후 출력
        public void Show(ItemData itemData, int maxAmount, Action<int> onConfirmAction)
        {
            if (itemData == null || maxAmount <= 1) return;

            _onConfirmCallback = onConfirmAction;
            _popupViewModel.Setup(itemData, maxAmount);
            
            //데이터 변경 강제 인식
            Render();

            //팝업 출력
            base.Show();
        }

        //_amountField의 값이 변경되었을 때 실행
        private void OnTextFieldInputChanged(string newValue)
        {
            //이벤트를 발생시킨 주체가 수량 입력 텍스트 필드인지 이름표(name)로 식별 진행
            //만약 플러스, 마이너스 버튼으로 값을 변경한 경우라면 뷰모델 및 UI에서 보이는 값이 이미 같음
            //따라서 이 경우 아래 내용은 패스
            if (newValue == _popupViewModel.AmountText) return;
            
            _popupViewModel.UpdateAmountFromText(newValue);
            
            //SetValueWithoutNotify을 이용해서 이벤트 호출 없이 값을 변경하여 추가 실행 방지
            _amountInputField.SetTextWithoutNotify(_popupViewModel.AmountText);
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