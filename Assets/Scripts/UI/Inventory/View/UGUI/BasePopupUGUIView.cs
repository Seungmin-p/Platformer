using UnityEngine;

namespace MyInventory.UGUI
{
    //팝업들의 베이스가 되는 클래스
    public abstract class BasePopupUGUIView : MonoBehaviour
    {
        //각 팝업별 메인 컨테이너
        protected RectTransform RootElement;

        //현재 팝업이 화면에 보여지고 있는 상태인지
        public bool IsVisible => RootElement != null && RootElement.gameObject.activeSelf;

        protected virtual void Awake()
        {
            RootElement = GetComponent<RectTransform>();

            //팝업이 있다면
            if (RootElement != null)
            {
                //포지션 방식을 Absolute로 지정 (유동적으로 위치를 조절하기 위함)
                RootElement.anchorMin = Vector2.zero;
                RootElement.anchorMax = Vector2.zero;
                RootElement.pivot = Vector2.zero;
                
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
                RootElement.gameObject.SetActive(true);
            }
        }

        //팝업을 화면에서 숨김
        public virtual void Hide()
        {
            if (RootElement != null)
            {
                RootElement.gameObject.SetActive(false);
            }
        }
    }
}