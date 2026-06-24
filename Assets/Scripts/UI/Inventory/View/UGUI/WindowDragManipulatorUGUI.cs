using UnityEngine;
using UnityEngine.EventSystems;

namespace MyInventory.UGUI
{
    //헤더 영역에 붙여진 드래그 기능
    public class WindowDragManipulatorUGUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
    {
        //인벤토리 창, 인벤토리 창의 위치 좌표를 저장할 키
        [SerializeField] private RectTransform _windowRoot;
        [SerializeField] private string _saveKey = "InventoryWindowPos";
        
        //인벤토리 헤더를 드래그하기 위해 클릭한 위치와 그 때의 인벤토리 창 위치
        private Vector2 _startPointerPosition;
        private Vector2 _startWindowPosition;
        private bool _isDragging; //드래그중인 상태

        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        //외부 뷰 클래스(InventoryUGUIView)에서 런타임에 의존성을 주입하기 위한 설정 메서드
        public void Setup(RectTransform windowRoot, string saveKey = "InventoryWindowPos")
        {
            _windowRoot = windowRoot;
            _saveKey = saveKey;
        }

        //마우스 이벤트 콜백 등록
        //마우스 제어권이 사라졌을 때
        //마우스 이벤트 콜백 해제

        //마우스 클릭 시
        public void OnPointerDown(PointerEventData eventData)
        {
            // UGUI 드래그 파이프라인의 원자적 흐름을 보장하기 위해 클릭 시점은 엔진 기본 이벤트 수신용 통로로 유지합니다.
        }

        //UGUI 이벤트 시스템의 드래그 파이프라인 진입을 보장하기 위한 필수 구현부
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (_isDragging) return;

            // [정밀 보정] 드래그가 엔진에 의해 공식 인정되는 시점(OnBeginDrag)에 시작 포인터와 창의 좌표를 캡처하여 오차를 원천 차단합니다.
            _isDragging = true;
            _startPointerPosition = eventData.position;
            if (_windowRoot != null)
            {
                _startWindowPosition = _windowRoot.anchoredPosition;
            }
        }

        //마우스를 움직일 때 실행
        public void OnDrag(PointerEventData eventData)
        {
            //!target.HasPointerCapture(evt.pointerId) -> 마우스 포인터를 독점하고 있는 상태여야 함
            if (!_isDragging || _windowRoot == null) return;

            //마우스 이동량 계산
            Vector2 delta = eventData.position - _startPointerPosition;
            if (_canvas != null)
            {
                delta /= _canvas.scaleFactor;
            }
            
            //마우스 이동량과 동일하게, 인벤토리 창의 좌표 업데이트
            _windowRoot.anchoredPosition = _startWindowPosition + delta;
        }

        //UGUI 이벤트 시스템의 드래그 파이프라인 종료를 보장하기 위한 필수 구현부
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;

            //최종 좌표 저장
            SavePosition();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 드래그 종료 및 데이터 저장은 OnEndDrag 파이프라인에서 무결하게 처리되도록 정렬되었습니다.
        }

        //현재 위치 저장
        private void SavePosition()
        {
            if (_windowRoot == null) return;
            PlayerPrefs.SetFloat(_saveKey + "_X", _windowRoot.anchoredPosition.x);
            PlayerPrefs.SetFloat(_saveKey + "_Y", _windowRoot.anchoredPosition.y);
            PlayerPrefs.Save();
        }

        //저장된 위치를 이용하여 창 위치 조정
        public void LoadPosition()
        {
            if (_windowRoot == null) return;
            if (PlayerPrefs.HasKey(_saveKey + "_X") && PlayerPrefs.HasKey(_saveKey + "_Y"))
            {
                _windowRoot.anchoredPosition = new Vector2(
                    PlayerPrefs.GetFloat(_saveKey + "_X"),
                    PlayerPrefs.GetFloat(_saveKey + "_Y")
                );
            }
        }
    }
}