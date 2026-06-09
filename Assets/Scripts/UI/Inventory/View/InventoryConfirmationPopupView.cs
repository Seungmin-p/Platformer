using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace MyInventory
{
    /// <summary>
    /// 아이템 파기 요청 시 화면 중앙에 나타나 승인 여부를 검증하는 모달형 경고창 뷰 클래스입니다.
    /// </summary>
    public class InventoryConfirmationPopupView : BasePopupView
    {
        private readonly Label _nameLabel;
        private readonly Button _btnConfirm;
        private readonly Button _btnCancel;
        private Action _onConfirmCallback;

        public InventoryConfirmationPopupView(VisualElement rootElement) : base(rootElement)
        {
            _nameLabel = RootElement.Q<Label>("popup-item-name");
            _btnConfirm = RootElement.Q<Button>("btn-confirm");
            _btnCancel = RootElement.Q<Button>("btn-cancel");

            if (_btnConfirm != null) _btnConfirm.clicked += OnConfirmClicked;
            if (_btnCancel != null) _btnCancel.clicked += Hide;
        }

        public void Show(ItemData itemData, Action onConfirmAction)
        {
            if (itemData == null) return;

            _onConfirmCallback = onConfirmAction;
            
            if (_nameLabel != null)
            {
                _nameLabel.text = itemData.Name;
            }

            base.Show();
        }

        private void OnConfirmClicked()
        {
            // [버그 수정 핵심] 팝업 화면을 최우선적으로 닫아 유저의 마우스 연타 및 오조작을 차단합니다.
            Hide();

            // 콜백을 로컬 변수에 안전하게 백업한 뒤 멤버 변수를 비워 중복 실행 예방 안전망을 구축합니다.
            Action callback = _onConfirmCallback;
            _onConfirmCallback = null; 

            // 이후 실제 가방 백엔드 데이터베이스 삭제 명령을 안전하게 수행합니다.
            if (callback != null)
            {
                try
                {
                    callback.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PopupSystem] 아이템 삭제 콜백 실행 중 백엔드 에러 발생: {ex.Message}");
                }
            }
        }
    }
}