using UnityEngine;
using UnityEngine.UIElements;

namespace MyInventory.UIToolkit
{
    //아이템 정보 팝업
    public class InventoryTooltipView : BasePopupView
    {
        private readonly InventoryTooltipViewModel _popupViewModel; //뷰모델

        //팝업의 위치가 항상 변하는데, 화면 밖으로 나가지 않기 위한 조절이 필요함
        //팝업의 위치는 마우스의 위치 및 팝업의 크기를 이용해서 계산
        //팝업 생성 직후 1프레임 동안은 이 크기를 제대로 인지하지 못하기에 코드로 직접 지정
        private float _cachedWidth = 400f;
        private float _cachedHeight = 300f;

        public InventoryTooltipView(VisualElement tooltipRootElement) : base(tooltipRootElement)
        {
            //dataSource 지정
            _popupViewModel = new InventoryTooltipViewModel();
            RootElement.dataSource = _popupViewModel;

            //유니티 버그로 피킹모드 설정이 풀리는 현상을 대비
            RootElement.pickingMode = PickingMode.Ignore;
            foreach (VisualElement child in RootElement.Children())
            {
                child.pickingMode = PickingMode.Ignore;
            }
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
        public void UpdatePosition(Vector2 panelPosition)
        {
            if (RootElement == null || RootElement.panel == null) return;

            //팝업 생성 직후 1프레임은 RootElement.layout.width 값이 NaN으로 나오기 때문에, 이를 방지함
            if (!float.IsNaN(RootElement.layout.width) && RootElement.layout.width > 0)
            {
                _cachedWidth = RootElement.layout.width;
                _cachedHeight = RootElement.layout.height;
            }

            //RootElement의 panel.visualTree는 UI를 제대로 사용하기 위해,
            //게임 해상도에 맞춰서 투명하게 깔려있는 최상위 UI 같은 개념에 가까움
            //즉, 현재 해상도의 전체 크기를 가져온다는 의미
            float screenWidth = RootElement.panel.visualTree.layout.width;
            float screenHeight = RootElement.panel.visualTree.layout.height;

            //마우스 커서와 툴팁이 완전히 붙어있지 않도록 하는 여백 크기
            float offsetX = 5f;
            float offsetY = 5f;

            //기본 타겟 위치 : 마우스 우측 상단
            //UI 좌표계 특성상 Y축은 - 가 될 수록 위쪽으로 올라감을 의미
            float targetX = panelPosition.x + offsetX;
            float targetY = panelPosition.y - _cachedHeight - offsetY;

            //만약 팝업 출력 위치 + 팝업 크기가 화면의 크기를 벗어나면
            if (targetX + _cachedWidth > screenWidth)
            {
                //마우스 왼쪽에 출력되도록 조절
                targetX = panelPosition.x - _cachedWidth - offsetX;
            }

            //팝업 출력 위치가 0보다 작다면 위로 뚫고 나간다는 것을 의미함
            if (targetY < 0)
            {
                //이 경우 y축 + 간격 조절만 해서 마우스 아래쪽으로 팝업이 출력되게 조절
                targetY = panelPosition.y + offsetY;
            }

            //만약 해상도가 작아서 위 과정을 거치고 나서도 화면 밖으로 나갈 경우, 스크린 안쪽으로 위치 조절
            //해상도가 팝업크기보다 작은 상황이라면 최대한 많은 정보가 화면에 출력되게끔 조절
            targetX = Mathf.Clamp(targetX, 0, Mathf.Max(0, screenWidth - _cachedWidth));
            targetY = Mathf.Clamp(targetY, 0, Mathf.Max(0, screenHeight - _cachedHeight));

            //계산된 최종 위치를 지정해줌
            RootElement.style.left = targetX;
            RootElement.style.top = targetY;
        }

        public override void Hide()
        {
            _popupViewModel?.Clear();
            base.Hide();
        }
    }
}