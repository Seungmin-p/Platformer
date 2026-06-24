using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace MyInventory.UGUI
{
    public class ItemSlotUGUIView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
    {
        //각종 UI 처리용
        [Header("UGUI 컴포넌트 행렬 배선")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountTextComponent;
        [SerializeField] private Image _highlightElement;
        
        //하이라이트 처리 색상
        private static readonly Color HighlightBgColor = new Color(1f, 1f, 1f, 0.12f); 
        private static readonly Color HighlightBorderColor = new Color(1f, 0.84f, 0f, 1f); 
        
        //뷰 모델, 인덱스, 더블클릭 판정용
        private ItemSlotUGUIViewModel _viewModel;
        private int _index;
        private float _lastClickTime;
        private const float DOUBLE_CLICK_THRESHOLD = 0.2f;
        
        //아이템 이동, 사용, 버리기
        private Action<int, int> _onDropRequest; 
        private Action<int> _onUseRequest;
        private Action<int> _onDeleteRequest;

        //마우스가 슬롯 영역에 들어오고, 안에서 움직이고, 나가는 부분에 대한 판정용 이벤트 
        public event Action<ItemData, Vector2> OnPointerEnterSlot;
        public event Action<Vector2> OnPointerMoveSlot; 
        public event Action OnPointerExitSlot;
        
        //아이템 분할처리용
        public event Action<int, int> OnSplitDropRequested;

        private Transform _screenRoot;

        public int Index => _index;

        public void SetSlotIndex(int index) => _index = index;

        public void Setup(Transform screenRoot, Action<int, int> onDropRequest, Action<int> onUseRequest, Action<int> onDeleteRequest)
        {
            //전달받은 각종 데이터 저장
            _screenRoot = screenRoot;
            _onDropRequest = onDropRequest;
            _onDeleteRequest = onDeleteRequest;
            _onUseRequest = onUseRequest;
    
            //ItemDragManipulator 인스턴스 확보
            ItemDragManipulatorUGUI dragManipulator = GetComponent<ItemDragManipulatorUGUI>();
            if (dragManipulator == null) dragManipulator = gameObject.AddComponent<ItemDragManipulatorUGUI>();

            // [구조 개선] 찾기 실패 확률이 높은 transform.Find 대신 인스펙터에 직결된 _iconImage 레퍼런스를 직접 주입합니다.
            dragManipulator.Setup(
                _screenRoot as RectTransform, 
                _index, 
                () => _viewModel?.HasItem ?? false, 
                _iconImage,
                _onDropRequest, 
                _onDeleteRequest
            );
            
            //아이템 이동 시, 쉬프트를 누르고 이동했다면 실행될 내용 구독
            dragManipulator.OnSplitDropRequest += (fromIdx, toIdx) => OnSplitDropRequested?.Invoke(fromIdx, toIdx);
            
            //드래그 조작기 등록
            // (UGUI 조작기 컴포넌트 자동 가동 체계로 대체 완료되었습니다)

            //클릭, 진입, 이동, 이탈 감지용
            // (UGUI 표준 이벤트 핸들러 포인터 인터페이스 상속 구조를 통해 엔진 이벤트를 수신합니다)
        }
        
        //인벤토리 뷰를 통해 호출되는 뷰모델 바인딩
        public void Bind(ItemSlotUGUIViewModel viewModel)
        {
            _viewModel = viewModel;
            
            //런타임 바인딩을 위해, 루트 엘리먼트의 데이터 소스를 명확하게 지정
            // (UGUI는 동적 이벤트 리스너 파이프라인으로 우회하여 정석 매핑합니다)
            
            //슬롯 데이터가 변경될 때 마다 렌더 실행
            _viewModel.OnStateChanged += Render;
            
            Render();
        }
        
        //슬롯의 뷰 모델 데이터가 변하면
        private void Render()
        {
            if (_viewModel == null) return;

            //필터 밖이면 하이라이트, 팝업 제외
            if (_viewModel.IsFilteredOut)
            {
                ClearHighlightAndPopup();
            }

            //아이템을 들고있다면
            if (_viewModel.HasItem)
            {
                //필터 상태에 걸러진다면 어둡게, 그렇지 않다면 정상적으로 아이콘을 보여줌 -> 바인딩
                if (_iconImage != null)
                {
                    _iconImage.enabled = true;
                    _iconImage.sprite = _viewModel.IconSprite;
                    _iconImage.color = _viewModel.IconTint;
                }

                //이때 아이템이 2개 이상이라면 수량 영역을 보여지게 하고,
                //그렇지 않다면 수량 레이블을 숨기도록 런타임 바인딩 된 상태
                if (_amountTextComponent != null)
                {
                    _amountTextComponent.gameObject.SetActive(_viewModel.AmountVisible);
                    _amountTextComponent.text = _viewModel.AmountText;
                }
            }
            //아이템이 없다면
            else
            {
                //만약 아이콘 더미데이터가 남아있다면 초기화 -> 바인딩
                if (_iconImage != null)
                {
                    _iconImage.enabled = false;
                    _iconImage.sprite = null;
                }
                
                //수량 레이블 숨김처리 -> 바인딩
                if (_amountTextComponent != null)
                {
                    _amountTextComponent.gameObject.SetActive(false);
                }
            }
        }
        
        //마우스 클릭 시 실행
        public void OnPointerDown(PointerEventData eventData)
        {
            //0 = 좌클릭, 1 = 우클릭, 2 = 휠클릭
            
            //우클릭이 진행됐다면
            if (eventData.button == PointerEventData.InputButton.Right) 
            {
                //사용 요청 진행
                _onUseRequest?.Invoke(_index);
                return;
            }

            //좌클릭이 진행됐다면
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                //현재 게임 시간에서 마지막으로 클릭한 시간을 빼서 마지막 클릭 이후 시간 체크
                float timeSinceLastClick = Time.time - _lastClickTime;
                
                //마지막으로 클릭한 시간이 0.2초 이내라면 더블클릭 판정
                if (timeSinceLastClick < DOUBLE_CLICK_THRESHOLD)
                {
                    //사용 요청 진행
                    _onUseRequest?.Invoke(_index);
                    
                    //세번 클릭 시 더블클릭이 두번되는걸 방지하기 위한 초기화
                    _lastClickTime = 0;
                }
                else
                {
                    //더블클릭 판정을 위한 시간 기록
                    _lastClickTime = Time.time;
                }
            }
        }

        //마우스가 슬롯 영역에 들어오면 실행
        public void OnPointerEnter(PointerEventData eventData)
        {
            //필터로 인해 어두워진 슬롯은 패스
            if (_viewModel != null && _viewModel.IsFilteredOut) return;

            //하이라이트 적용
            if (_highlightElement != null)
            {
                _highlightElement.gameObject.SetActive(true);
                _highlightElement.color = HighlightBgColor;
            }

            //아이템이 있다면
            if (_viewModel != null && _viewModel.HasItem)
            {
                //툴팁 출력을 위한 이벤트 호출
                OnPointerEnterSlot?.Invoke(_viewModel.ItemData, eventData.position);
            }
        }

        //슬롯 내에서 마우스가 움직일 때 실행
        public void OnPointerMove(PointerEventData eventData)
        {
            //필터에 의해 어두워진 슬롯 패스
            if (_viewModel != null && _viewModel.IsFilteredOut) return;
            
            //아이템이 있다면
            if (_viewModel != null && _viewModel.HasItem)
            {
                //툴팁 위치 업데이트
                OnPointerMoveSlot?.Invoke(eventData.position);
            }
        }

        //마우스가 슬롯을 빠져 나갈 때 실행
        public void OnPointerExit(PointerEventData eventData)
        {
            //하이라이트 및 팝업 제거
            ClearHighlightAndPopup();
        }

        private void ClearHighlightAndPopup()
        {
            //하이라이트 및 팝업 제거
            if (_highlightElement != null)
            {
                _highlightElement.gameObject.SetActive(false);
            }
            OnPointerExitSlot?.Invoke();
        }

        public void ClearSubscribedEvents()
        {
            //이벤트 자체를 null로 만들어서 람다식 포함 모든 이벤트 연결 해제
            OnPointerEnterSlot = null;
            OnPointerMoveSlot = null;
            OnPointerExitSlot = null;
            OnSplitDropRequested = null;
            
            //추후 재사용 등을 위한 바인딩 연결 해제
            if (_viewModel != null) _viewModel.OnStateChanged -= Render;
            _viewModel = null;
        }
    }
}