using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace MyInventory
{
    public class InventoryAmountSplitPopupView : BasePopupView
    {
        private readonly Label _nameLabel;
        private readonly TextField _amountField;
        private readonly Button _btnPlus;
        private readonly Button _btnMinus;
        private readonly Button _btnConfirm;
        private readonly Button _btnCancel;

        private int _currentAmount;
        private int _maxLimitAmount;
        private Action<int> _onConfirmCallback;

        public InventoryAmountSplitPopupView(VisualElement rootElement) : base(rootElement)
        {
            _nameLabel = RootElement.Q<Label>("split-item-name");
            _amountField = RootElement.Q<TextField>("field-amount");
            _btnMinus = RootElement.Q<Button>("btn-minus");
            _btnPlus = RootElement.Q<Button>("btn-plus");
            _btnConfirm = RootElement.Q<Button>("btn-confirm");
            _btnCancel = RootElement.Q<Button>("btn-cancel");

            if (_btnMinus != null) _btnMinus.clicked += () => ApplyValueRange(_currentAmount - 1);
            if (_btnPlus != null) _btnPlus.clicked += () => ApplyValueRange(_currentAmount + 1);
            if (_btnConfirm != null) _btnConfirm.clicked += OnConfirmClicked;
            if (_btnCancel != null) _btnCancel.clicked += Hide;

            if (_amountField != null)
            {
                // UI Toolkit 내부의 텍스트 입력 전용 노드인 '#unity-text-input'을 찾아냅니다.
                VisualElement textInputZone = _amountField.Q("unity-text-input");
                if (textInputZone != null)
                {
                    // 런타임 렌더링 시 무조건 텍스트가 정중앙에 위치하도록 강제 앵커링합니다.
                    textInputZone.style.unityTextAlign = TextAnchor.MiddleCenter;
                }

                _amountField.RegisterCallback<ChangeEvent<string>>(OnTextFieldInputChanged);
            }
        }

        public void Show(ItemData itemData, int currentSlotTotalAmount, Action<int> onConfirmAction)
        {
            if (itemData == null || currentSlotTotalAmount <= 1) return;

            _onConfirmCallback = onConfirmAction;
            if (_nameLabel != null) _nameLabel.text = itemData.Name;

            _maxLimitAmount = currentSlotTotalAmount - 1;
            ApplyValueRange(1);

            base.Show();
        }

        private void ApplyValueRange(int value)
        {
            _currentAmount = Mathf.Clamp(value, 1, _maxLimitAmount);
            if (_amountField != null)
            {
                _amountField.SetValueWithoutNotify(_currentAmount.ToString());
            }
        }

        private void OnTextFieldInputChanged(ChangeEvent<string> evt)
        {
            if (int.TryParse(evt.newValue, out int parsedResult))
            {
                ApplyValueRange(parsedResult);
            }
            else
            {
                ApplyValueRange(_currentAmount);
            }
        }

        private void OnConfirmClicked()
        {
            Hide();
            Action<int> callback = _onConfirmCallback;
            _onConfirmCallback = null;

            if (callback != null && _currentAmount > 0)
            {
                try { callback.Invoke(_currentAmount); }
                catch (Exception ex) { Debug.LogError($"[SplitPopup] 에러: {ex.Message}"); }
            }
        }
    }
}