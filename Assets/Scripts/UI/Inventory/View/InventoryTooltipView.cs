using UnityEngine;
using UnityEngine.UIElements;

namespace MyInventory
{
    /// <summary>
    /// 공용 팝업 기반을 상속받아 아이템 정보 출력 및 마우스 커서 실시간 추적(화면 이탈 방지 포함)을 처리하는 툴팁 뷰입니다.
    /// </summary>
    public class InventoryTooltipView : BasePopupView
    {
        private readonly Label _nameLabel;
        private readonly Label _descLabel;

        // UI Toolkit은 툴팁이 켜지는 첫 프레임에는 텍스트 길이에 따른 레이아웃 연산이 안 끝나 크기가 NaN일 수 있습니다.
        // 첫 1프레임 튐 현상을 막기 위한 근사치 기본 캐싱 변수입니다.
        private float _cachedWidth = 400f; 
        private float _cachedHeight = 300f;

        public InventoryTooltipView(VisualElement tooltipRootElement) : base(tooltipRootElement)
        {
            _nameLabel = RootElement.Q<Label>("Tooltip-Name");
            _descLabel = RootElement.Q<Label>("Tooltip-Desc");

            RootElement.pickingMode = PickingMode.Ignore;
            if (_nameLabel != null) _nameLabel.pickingMode = PickingMode.Ignore;
            if (_descLabel != null) _descLabel.pickingMode = PickingMode.Ignore;
        }

        public void Show(ItemData data, Vector2 panelPosition)
        {
            if (data == null) return;

            if (_nameLabel != null) _nameLabel.text = data.Name;
            if (_descLabel != null) _descLabel.text = data.Tooltip;

            UpdatePosition(panelPosition);
            base.Show();
        }

        /// <summary> 화면 경계를 계산하여 툴팁이 밖으로 잘려 나가지 않도록 4방향 위치를 지능적으로 보정합니다. </summary>
        public void UpdatePosition(Vector2 panelPosition)
        {
            if (RootElement == null || RootElement.panel == null) return;

            // 레이아웃 엔진이 툴팁의 실제 렌더링 크기 계산을 완료했다면, 그 크기를 캐싱하여 정확도를 높입니다.
            if (!float.IsNaN(RootElement.layout.width) && RootElement.layout.width > 0)
            {
                _cachedWidth = RootElement.layout.width;
                _cachedHeight = RootElement.layout.height;
            }

            // UI Toolkit 패널의 실제 스크린 해상도를 가져옵니다.
            float screenWidth = RootElement.panel.visualTree.layout.width;
            float screenHeight = RootElement.panel.visualTree.layout.height;

            // 커서와 툴팁이 너무 바짝 붙지 않도록 띄우는 여백 설정
            float offsetX = 15f;
            float offsetY = 15f;

            // 1. 기본 타겟 위치: 마우스 우측 상단 (사용자님 요청 사항)
            float targetX = panelPosition.x + offsetX;
            float targetY = panelPosition.y - _cachedHeight - offsetY;

            // 2. 우측 화면 경계를 벗어나는 경우 -> 마우스 좌측으로 툴팁을 뒤집음
            if (targetX + _cachedWidth > screenWidth)
            {
                targetX = panelPosition.x - _cachedWidth - offsetX;
            }

            // 3. 상단 화면 경계를 벗어나는 경우 -> 마우스 하단으로 툴팁을 뒤집음
            if (targetY < 0)
            {
                targetY = panelPosition.y + offsetY;
            }

            // 4. (안전망) 위 계산을 거치고도 기기 해상도가 작아 화면 밖으로 나갈 경우, 스크린 내부에 강제로 가둡니다.
            targetX = Mathf.Clamp(targetX, 0, Mathf.Max(0, screenWidth - _cachedWidth));
            targetY = Mathf.Clamp(targetY, 0, Mathf.Max(0, screenHeight - _cachedHeight));

            RootElement.style.left = targetX;
            RootElement.style.top = targetY;
        }
    }
}