using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MyInventory.UGUI
{
    //아이템 버리기 확인 팝업
    public class InventoryConfirmationPopupUGUIView : BasePopupUGUIView
    {
        private Action _onConfirmCallback; //아이템 버리기 액션
        
        private InventoryConfirmationPopupUGUIViewModel _popupViewModel; //뷰모델

        [Header("UGUI 컴포넌트 배선")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Button _btnConfirm;
        [SerializeField] private Button _btnCancel;

        protected override void Awake()
        {
            base.Awake();
            
            //dataSource 지정
            _popupViewModel = new InventoryConfirmationPopupUGUIViewModel();
            _popupViewModel.OnStateChanged += Render;
            
            //각 버튼별 이벤트 연결
            if (_btnConfirm != null) _btnConfirm.onClick.AddListener(() => OnButtonClick("btn-confirm"));
            if (_btnCancel != null) _btnCancel.onClick.AddListener(() => OnButtonClick("btn-cancel"));
        }
        
        private void OnButtonClick(string buttonName)
        {
            //클릭된 요소가 버튼이 맞는지 확인

            //스위치문을 통해 각 버튼별 동작 적용
            switch (buttonName)
            {
                case "btn-confirm": 
                    OnConfirmClicked(); 
                    break;
                    
                case "btn-cancel":  
                    Hide(); 
                    break;
            }

            //클릭 신호가 부모 레이아웃으로 퍼져나가는 것을 방지
        }

        private void Render()
        {
            if (_nameText != null) _nameText.text = _popupViewModel.NameText;
        }

        //팝업 내용 구성 후 출력
        public void Show(ItemData itemData, Action onConfirmAction)
        {
            if (itemData == null) return;

            _onConfirmCallback = onConfirmAction;
            _popupViewModel.Setup(itemData);

            //팝업 출력
            base.Show();
        }

        //버리기 확인 버튼 클릭 시 실행
        private void OnConfirmClicked()
        {
            //우선 팝업을 닫아줌
            Hide();

            //콜백 저장 후 _onConfirmCallback은 다시 비워두기
            Action callback = _onConfirmCallback;
            _onConfirmCallback = null; 

            //실제 삭제 처리 시도
            if (callback != null)
            {
                try
                {
                    //인벤토리 모델의 Remove 메소드가 실행됨
                    callback.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PopupSystem] 아이템 삭제 콜백 실행 중 백엔드 에러 발생: {ex.Message}");
                }
            }
            
            _popupViewModel.Clear();
        }
    }
}