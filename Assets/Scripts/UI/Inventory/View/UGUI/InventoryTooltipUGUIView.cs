using UnityEngine;
using TMPro;

namespace MyInventory.UGUI
{
    //아이템 정보 팝업
    public class InventoryTooltipUGUIView : BasePopupUGUIView
    {
        private InventoryTooltipUGUIViewModel _popupViewModel; //뷰모델

        [Header("UGUI 컴포넌트 배선")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descText;

        private float _cachedWidth = 400f;
        private float _cachedHeight = 300f;

        private Canvas _cachedCanvas;    //상위 캔버스 캐싱
        
        //Canvas 잠금 족쇄를 우회하여 실제 마우스 위치로 부드럽게 움직일 실제 시각 배경 패널
        private RectTransform _tooltipBackground;

        protected override void Awake()
        {
            base.Awake();
            
            //dataSource 지정
            _popupViewModel = new InventoryTooltipUGUIViewModel();
            _popupViewModel.OnStateChanged += Render;

            //매 프레임 탐색하는 병목을 완전히 제거하기 위해 Awake 시점에 조기 확보 고정
            _cachedCanvas = GetComponentInParent<Canvas>();
            
            //[안전 예외 방어] 프리팹 내부의 실질적 이동 타겟인 Tooltip-Background를 자동으로 안전하게 찾아옵니다.
            if (transform.childCount > 0)
            {
                _tooltipBackground = transform.GetChild(0) as RectTransform;
            }
        }

        private void Render()
        {
            if (_nameText != null) _nameText.text = _popupViewModel.NameText;
            if (_descText != null) _descText.text = _popupViewModel.DescText;
        }

        //팝업 내용 구성 후 출력
        public void Show(ItemData data, Vector2 panelPosition)
        {
            if (data == null) return;

            //데이터를 뷰모델에 전달
            _popupViewModel.Setup(data);

            //위치 조정
            UpdatePosition(panelPosition);
            
            //출력
            base.Show();
        }

        //화면 경계를 계산해서 팝업이 밖으로 나가지 않도록 조절
        public void UpdatePosition(Vector2 screenPosition)
        {
            if (_tooltipBackground == null) return;
            
            // 런타임 실시간 상태를 안전하게 반영하기 위해 기존의 무결했던 검증 수식으로 전면 롤백했습니다.
            Canvas canvas = _cachedCanvas != null ? _cachedCanvas : GetComponentInParent<Canvas>();
            if (canvas == null) return;
            
            Camera uiCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
            
            // [정석 공식 복구] 사용자님께서 성공하셨던 스크린-월드 치환 공식을 그대로 유지합니다.
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, screenPosition, uiCamera, out Vector3 worldPoint))
            {
                // 잠겨있는 프리팹 본체(transform) 대신, 자유로운 자식 배경 패널의 position을 직접 조준 타격합니다.
                _tooltipBackground.position = worldPoint;
                
                // 마우스 커서 아랫부분에 툴팁 글자가 겹쳐서 가려지지 않도록 미세한 우측 하단 여백 오프셋 수치만 가미합니다.
                _tooltipBackground.position += new Vector3(20f, -20f, 0f);
            }
        }

        public override void Hide()
        {
            _popupViewModel?.Clear();
            base.Hide();
        }
    }
}