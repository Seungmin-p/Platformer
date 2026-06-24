using UnityEngine;
using UnityEngine.UIElements;

namespace MyInventory.UIToolkit
{
    //헤더 영역에 붙여진 드래그 기능
    public class WindowDragManipulator : PointerManipulator
    {
        //인벤토리 창, 인벤토리 창의 위치 좌표를 저장할 키
        private readonly VisualElement _windowRoot;
        private readonly string _saveKey;
        
        //인벤토리 헤더를 드래그하기 위해 클릭한 위치와 그 때의 인벤토리 창 위치
        private Vector2 _startPointerPosition;
        private Vector2 _startWindowPosition;
        private bool _isDragging; //드래그중인 상태

        public WindowDragManipulator(VisualElement windowRoot, string saveKey = "InventoryWindowPos")
        {
            _windowRoot = windowRoot;
            _saveKey = saveKey;
            
            //좌클릭 시에만 작동하도록 필터링
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        //마우스 이벤트 콜백 등록
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut); //마우스 제어권이 사라졌을 때
        }

        //마우스 이벤트 콜백 해제
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        //마우스 클릭 시
        private void OnPointerDown(PointerDownEvent evt)
        {
            //!CanStartManipulation(evt) -> 좌클릭이 맞는지 확인
            if (_isDragging || !CanStartManipulation(evt)) return;

            //드래그 상태 활성화 및 시작한 마우스 위치 및 창 위치 기억
            _isDragging = true;
            _startPointerPosition = evt.position;
            _startWindowPosition = new Vector2(_windowRoot.layout.x, _windowRoot.layout.y);
            
            //마우스가 빠르게 움직여 헤더를 벗어나더라도 쫓아올 수 있게 포인터 캡쳐
            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            //!target.HasPointerCapture(evt.pointerId) -> 마우스 포인터를 독점하고 있는 상태여야 함
            if (!_isDragging || !target.HasPointerCapture(evt.pointerId)) return;

            //마우스 이동량 계산
            Vector2 delta = (Vector2)evt.position - _startPointerPosition;
            
            //마우스 이동량과 동일하게, 인벤토리 창의 좌표 업데이트
            _windowRoot.style.left = _startWindowPosition.x + delta.x;
            _windowRoot.style.top = _startWindowPosition.y + delta.y;

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging || !target.HasPointerCapture(evt.pointerId)) return;

            //드래그 및 포인터 독점 해제
            _isDragging = false;
            target.ReleasePointer(evt.pointerId);
            evt.StopPropagation();

            //최종 좌표 저장
            SavePosition();
        }

        //마우스 독점이 의도치않게 사라졌을 때
        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            //드래그 중이었다면 해제하고, 위치 저장
            if (_isDragging)
            {
                _isDragging = false;
                SavePosition();
            }
        }
        
        //현재 위치 저장
        private void SavePosition()
        {
            PlayerPrefs.SetFloat(_saveKey + "_X", _windowRoot.resolvedStyle.left);
            PlayerPrefs.SetFloat(_saveKey + "_Y", _windowRoot.resolvedStyle.top);
            PlayerPrefs.Save();
        }

        //저장된 위치를 이용하여 창 위치 조정
        public void LoadPosition()
        {
            if (PlayerPrefs.HasKey(_saveKey + "_X") && PlayerPrefs.HasKey(_saveKey + "_Y"))
            {
                _windowRoot.style.left = PlayerPrefs.GetFloat(_saveKey + "_X");
                _windowRoot.style.top = PlayerPrefs.GetFloat(_saveKey + "_Y");
            }
        }
    }
}