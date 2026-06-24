using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace MyInventory.UIToolkit
{
    public class ItemDragManipulator : PointerManipulator
    {
        
        //인벤토리 최상위 엘리먼트, 드래그 시 보일 고스트 아이콘
        private readonly VisualElement _screenRoot;
        private VisualElement _ghostIcon;
        
        //시작 인덱스, 드래그 상태 플래그
        private readonly int _srcIndex;
        private bool _isDragging;

        //아이템이 존재하는지 확인, 아이템 이동, 아이템 삭제
        private readonly Func<bool> _hasItemCheck;
        private readonly Action<int, int> _onDropRequest;
        private readonly Action<int> _onDeleteRequest;
        
        //분할 팝업 호출용 이벤트
        public event Action<int, int> OnSplitDropRequest;

        public ItemDragManipulator(VisualElement screenRoot, int srcIndex, Func<bool> hasItemCheck, 
                                   Action<int, int> onDropRequest, Action<int> onDeleteRequest)
        {
            _screenRoot = screenRoot;
            _srcIndex = srcIndex;
            _hasItemCheck = hasItemCheck;
            _onDropRequest = onDropRequest;
            _onDeleteRequest = onDeleteRequest;
            _isDragging = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            //마우스 클릭, 이동, 뗌 이벤트 연동
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            //마우스 클릭, 이동, 뗌 이벤트 연동 해제
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        //마우스 클릭 시 실행
        private void OnPointerDown(PointerDownEvent evt)
        {
            //좌클릭이 아니라면 리턴
            if (_hasItemCheck == null || !_hasItemCheck() || evt.button != 0) return;

            //드래그 상태, 포인터 활성화
            _isDragging = true;
            target.CapturePointer(evt.pointerId);

            //아이콘 가져오기
            VisualElement originalIcon = target.Q<VisualElement>("Item-Icon");
            _ghostIcon = new VisualElement();
            
            //아이콘 크기 맞추기
            float width = target.layout.width > 0 ? target.layout.width : 100f;
            float height = target.layout.height > 0 ? target.layout.height : 100f;
            _ghostIcon.style.width = width;
            _ghostIcon.style.height = height;

            //아이콘 색상 지정
            if (originalIcon != null && originalIcon.style.backgroundImage.value.sprite != null)
            {
                _ghostIcon.style.backgroundImage = originalIcon.style.backgroundImage;
                _ghostIcon.style.unityBackgroundImageTintColor = Color.white;
            }

            //새 아이콘 객체를 Absolute 로 지정해서, 화면 내 어디든 움직일 수 있게해줌
            //반투명하게 표현하고, 피킹모드 Ignore
            _ghostIcon.style.position = Position.Absolute;
            _ghostIcon.style.opacity = 0.6f;
            _ghostIcon.pickingMode = PickingMode.Ignore;

            //작업을 거친 고스트 아이콘을 추가하고, 마우스 좌표로 위치 업데이트
            _screenRoot.Add(_ghostIcon);
            UpdateGhostPosition(evt.position);

            //쓸데없는 동작을 방지하기 위한 클릭 비활성화
            evt.StopPropagation();
        }

        //마우스를 움직일 때 실행
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging || _ghostIcon == null) return;

            //고스트 아이콘의 위치를 마우스 위치와 동기화
            UpdateGhostPosition(evt.position);
            evt.StopPropagation();
        }

        //마우스 클릭을 땔 때 실행
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging) return;

            //드래그 상태 비활성화, 마우스 클릭을 위한 포인터 재활성화
            _isDragging = false;
            target.ReleasePointer(evt.pointerId);

            //고스트 아이콘 삭제
            if (_ghostIcon != null)
            {
                _ghostIcon.RemoveFromHierarchy();
                _ghostIcon = null;
            }

            //마우스를 뗀 위치의 UI 엘리먼트를 가져옴
            VisualElement pickedElement = _screenRoot.panel.Pick(evt.position);
            
            //만약 아무런 엘리먼트도 찾지 못했다면
            if (pickedElement == null)
            {
                //인벤토리 밖을 의미하기 때문에 추가 검사 없이 빠르게 삭제 요청
                _onDeleteRequest?.Invoke(_srcIndex);
                return;
            }

            //찾은 엘리먼트를 기준으로 슬롯 정보를 가져와서 체크 진행
            VisualElement targetSlot = FindParentSlot(pickedElement);
            if (targetSlot != null && targetSlot.userData is int destIndex)
            {
                //슬롯이 확인됐으면서, 쉬프트를 누른 채 옮긴경우
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    //아이템 분할 요청
                    OnSplitDropRequest?.Invoke(_srcIndex, destIndex);
                }
                else
                {
                    //쉬프트를 누른게 아니라면 일반적인 아이템 이동 요청
                    _onDropRequest?.Invoke(_srcIndex, destIndex);
                }
                return;
            }

            //슬롯은 아닌데, 인벤토리 내부인 경우
            if (IsInsideInventoryWindow(pickedElement))
            {
                //별 다른 처리 없이 그냥 패스
                return;
            }

            //인벤토리 내부 영역이 아니라면 외부를 의미하기에 삭제 요청
            //인벤토리 외 별도의 UI가 추가될 경우를 대비한 처리
            _onDeleteRequest?.Invoke(_srcIndex);
            evt.StopPropagation();
        }

        //고스트 아이콘의 위치를 마우스와 동기화 해주는 메소드
        private void UpdateGhostPosition(Vector2 panelPosition)
        {
            if (_ghostIcon == null) return;
            
            //고스트 아이콘의 크기를 가져와서
            float width = _ghostIcon.style.width.value.value;
            float height = _ghostIcon.style.height.value.value;

            //고스트 아이콘의 중앙이 마우스 포인트 위치에 오도록 조절
            _ghostIcon.style.left = panelPosition.x - (width / 2f);
            _ghostIcon.style.top = panelPosition.y - (height / 2f);
        }

        //슬롯을 찾는 메소드
        private VisualElement FindParentSlot(VisualElement element)
        {
            VisualElement current = element;
            while (current != null)
            {
                //거슬러 올라가면서 슬롯을 찾으면 리턴
                if (current.ClassListContains("slot-default") || current.userData is int)
                {
                    return current;
                }
                current = current.parent;
            }
            return null;
        }

        //인벤토리 범위 내를 판단하는 메소드
        private bool IsInsideInventoryWindow(VisualElement element)
        {
            VisualElement current = element;
            while (current != null)
            {
                //거슬러 올라가면서 인벤토리를 찾으면 true 리턴
                if (current.name == "Inventory")
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }
    }
}