using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace MyInventory.UIToolkit
{
    //아이템 버리기 확인 팝업
    public class InventoryConfirmationPopupView : BasePopupView
    {
        private Action _onConfirmCallback; //아이템 버리기 액션
        
        private readonly InventoryConfirmationPopupViewModel _popupViewModel; //뷰모델

        public InventoryConfirmationPopupView(VisualElement rootElement) : base(rootElement)
        {
            //dataSource 지정
            _popupViewModel = new InventoryConfirmationPopupViewModel();
            RootElement.dataSource = _popupViewModel;
            
            //각 버튼별 이벤트 연결
            RootElement.RegisterCallback<ClickEvent>(OnPopupClicked);
        }
        
        private void OnPopupClicked(ClickEvent evt)
        {
            //클릭된 요소가 버튼이 맞는지 확인
            if (evt.target is not Button clickedButton) return;

            //스위치문을 통해 각 버튼별 동작 적용
            switch (clickedButton.name)
            {
                case "btn-confirm": 
                    OnConfirmClicked(); 
                    break;
                    
                case "btn-cancel":  
                    Hide(); 
                    break;
            }

            //클릭 신호가 부모 레이아웃으로 퍼져나가는 것을 방지
            evt.StopPropagation();
        }

        //팝업 내용 구성 후 출력
        public void Show(ItemData itemData, Action onConfirmAction)
        {
            if (itemData == null) return;

            _onConfirmCallback = onConfirmAction;
            _popupViewModel.Setup(itemData);
            
            //데이터 변경 강제 인식
            RootElement.dataSource = null;
            RootElement.dataSource = _popupViewModel;

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