using UnityEngine;
using UnityEngine.UIElements;

namespace MyInventory
{
    /// <summary>
    /// 인벤토리 시스템 내의 모든 독립 팝업 UI가 공유하는 네이티브 제어 로직을 관리하는 공용 기반 추상 클래스입니다.
    /// </summary>
    public abstract class BasePopupView
    {
        protected readonly VisualElement RootElement;

        /// <summary> 현재 팝업창이 화면에 활성화되어 노출 중인지 여부입니다. </summary>
        public bool IsVisible => RootElement != null && RootElement.style.display == DisplayStyle.Flex;

        protected BasePopupView(VisualElement rootElement)
        {
            RootElement = rootElement;

            if (RootElement != null)
            {
                // 모든 오버레이 팝업은 화면 전체의 절대 좌표계 위에 가득 얹어집니다.
                RootElement.style.position = Position.Absolute;
                
                // 생성 시점에는 레이아웃 연산 및 클릭 판정에서 완벽히 제외하도록 무조건 숨김 처리
                Hide();
            }
        }

        /// <summary> 팝업을 화면에 즉각 표시합니다. </summary>
        public virtual void Show()
        {
            if (RootElement != null)
            {
                RootElement.style.display = DisplayStyle.Flex;
            }
        }

        /// <summary> 팝업을 화면에서 완벽하게 소멸시키고 판정을 제거합니다. </summary>
        public virtual void Hide()
        {
            if (RootElement != null)
            {
                RootElement.style.display = DisplayStyle.None;
            }
        }
    }
}