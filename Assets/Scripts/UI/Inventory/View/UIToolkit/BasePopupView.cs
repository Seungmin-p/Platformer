using UnityEngine.UIElements;

namespace MyInventory.UIToolkit
{
    //팝업들의 베이스가 되는 클래스
    public abstract class BasePopupView
    {
        //각 팝업별 메인 컨테이너
        protected readonly VisualElement RootElement;

        //현재 팝업이 화면에 보여지고 있는 상태인지
        public bool IsVisible => RootElement != null && RootElement.style.display == DisplayStyle.Flex;

        protected BasePopupView(VisualElement rootElement)
        {
            RootElement = rootElement;

            //팝업이 있다면
            if (RootElement != null)
            {
                //포지션 방식을 Absolute로 지정 (유동적으로 위치를 조절하기 위함)
                RootElement.style.position = Position.Absolute;
                
                //생성 직후는 숨김처리
                Hide();
            }
        }

        //팝업을 화면에 노출
        //각 팝업에서 별도 맞춤 작업 후 base.Show();를 통해 실행
        public virtual void Show()
        {
            if (RootElement != null)
            {
                RootElement.style.display = DisplayStyle.Flex;
            }
        }

        //팝업을 화면에서 숨김
        public virtual void Hide()
        {
            if (RootElement != null)
            {
                RootElement.style.display = DisplayStyle.None;
            }
        }
    }
}