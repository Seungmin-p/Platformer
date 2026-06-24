using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MyInventory.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class InventoryView : MonoBehaviour
    {
        [Header("UI Templates")] 
        //아이템 슬롯, 툴팁, 버리기 확인, 아이템 나누기 UI
        [SerializeField] VisualTreeAsset _slotTemplate;
        [SerializeField] VisualTreeAsset _tooltipTemplate; 
        [SerializeField] VisualTreeAsset _confirmationTemplate; 
        [SerializeField] VisualTreeAsset _splitTemplate;

        [Header("인벤토리 구성을 위한 슬롯 정보")]
        //인벤토리 한줄에 들어갈 슬롯의 수, 슬롯의 크기, 슬롯별로 할당된 좌+우 마진 크기
        [SerializeField] int _columnCount = 7;
        [SerializeField] float _slotSize = 100f;
        [SerializeField] float _slotHorizontalMargin = 6f;

        //뷰모델 데이터
        private InventoryViewModel _viewModel;
        
        //인벤토리 레이아웃
        //UI 제어용 변수, 인벤토리 창, 슬롯영역
        private VisualElement _root;
        private VisualElement _inventoryWindow;
        private VisualElement _slotArea;

        //실제 팝업 제어용 인스턴스
        private InventoryTooltipView _tooltipView;
        private InventoryConfirmationPopupView _confirmationPopupView; 
        private InventoryAmountSplitPopupView _amountSplitPopupView;

        //슬롯 뷰 들을 순서대로 담아둘 리스트
        private readonly List<ItemSlotView> _slotViews = new List<ItemSlotView>();

        //바인딩처리
        public void Bind(InventoryViewModel viewModel)
        {
            //혹시 모를 중복 바인딩을 방지하기 위한 언바인드 실행
            Unbind();
            
            //뷰 모델 연결
            _viewModel = viewModel;
            _root = GetComponent<UIDocument>().rootVisualElement;

            //인벤토리 창, 슬롯 영역
            _inventoryWindow = _root.Q<VisualElement>("Inventory");
            _slotArea = _root.Q<VisualElement>("Slot-Area");

            //각 버튼별 이벤트 연결
            if (_inventoryWindow != null)
            {
                _inventoryWindow.RegisterCallback<ClickEvent>(OnInventoryWindowClicked);
            }
            
            //인벤토리 창을 마우스로 잡고 위치를 옮기기 위한 바인딩,
            //헤더 영역을 지정하고, 문제가 없다면
            VisualElement headerArea = _root.Q<VisualElement>("Header-Area");
            if (_inventoryWindow != null && headerArea != null)
            {
                //위치 방식을 Absolute으로 지정, 마우스 입력 제어 객체 생성
                _inventoryWindow.style.position = Position.Absolute;
                WindowDragManipulator dragManipulator = new WindowDragManipulator(_inventoryWindow, "MyInventoryPos");
                
                //헤더 영역에만 동작 지정
                headerArea.AddManipulator(dragManipulator);
                
                //인벤토리의 초기 위치를 기존 정보에서 로딩
                dragManipulator.LoadPosition();
            }

            //아이템 설명(툴팁) 팝업 초기화
            if (InstantiateAndAttach(_tooltipTemplate) is VisualElement tooltipRoot)
                _tooltipView = new InventoryTooltipView(tooltipRoot);

            //아이템 버리기 팝업 초기화
            if (InstantiateAndAttach(_confirmationTemplate) is VisualElement confirmRoot)
                _confirmationPopupView = new InventoryConfirmationPopupView(confirmRoot);

            //아이템 수량 나누기 팝업 초기화
            if (InstantiateAndAttach(_splitTemplate) is VisualElement splitRoot)
                _amountSplitPopupView = new InventoryAmountSplitPopupView(splitRoot);

            //초기 레이아웃 설정
            ApplyDynamicLayout();
            
            //슬롯 및 UI 이벤트 설정
            GenerateSlotsUI();
            
            //시작 필터는 전체(All)
            _viewModel?.SetFilter(ItemFilterType.All);

            //인벤토리창은 시작할땐 기본적으로 숨김처리
            if (_inventoryWindow != null)
            {
                _inventoryWindow.style.display = DisplayStyle.None;
            }
        }
        
        //클릭 이벤트 버튼들을 묶은 메소드
        private void OnInventoryWindowClicked(ClickEvent evt)
        {
            //클릭된 요소가 버튼이 맞는지 확인
            if (evt.target is not Button clickedButton) return;

            //스위치문을 통해 각 버튼별 동작 적용
            switch (clickedButton.name)
            {
                case "btn-close":           CloseWindow(); break;
                case "btn-sort":            _viewModel?.CommandSort(); break;
                case "btn-trim":            _viewModel?.CommandTrim(); break;
                case "btn-filter-all":      _viewModel?.SetFilter(ItemFilterType.All); break;
                case "btn-filter-equip":    _viewModel?.SetFilter(ItemFilterType.Equipment); break;
                case "btn-filter-consume":  _viewModel?.SetFilter(ItemFilterType.Consumable); break;
                case "btn-filter-material": _viewModel?.SetFilter(ItemFilterType.Material); break;
            }

            evt.StopPropagation();
        }
        
        //세 팝업들을 초기화 하기 위한 메소드
        private VisualElement InstantiateAndAttach(VisualTreeAsset template)
        {
            if (template == null || _root == null) return null;

            //툴팁 인스턴스 생성 이후, UI Toolkit이 임의로 씌운 껍데기(TemplateContainer)를 벗겨내고 실제 루트 데이터 가져오기
            TemplateContainer container = template.Instantiate();
            VisualElement realRoot = container.childCount > 0 ? container[0] : null;

            if (realRoot != null)
            {
                //root 추가 및 뷰 등록
                _root.Add(realRoot);
            }

            return realRoot;
        }
        
        //인벤토리 레이아웃 조정
        private void ApplyDynamicLayout()
        {
            if (_slotArea == null) return;

            //마진 포함 슬롯 사이즈를 슬롯을 배치할 수만큼 곱하고 미세한 오류 방지를 위해 + 1
            float itemActualWidth = _slotSize + _slotHorizontalMargin;
            float requiredGridWidth = (itemActualWidth * _columnCount) + 1f;

            //좌우 공간을 맞춰주기 위한 처리
            _slotArea.style.paddingLeft = _slotHorizontalMargin;
            float finalGridWidth = requiredGridWidth + _slotHorizontalMargin;
            _slotArea.style.width = finalGridWidth;

            //사이즈 축소 방지 및 중앙 정렬
            _slotArea.style.flexShrink = 0;
            _slotArea.style.marginLeft = StyleKeyword.Auto;
            _slotArea.style.marginRight = StyleKeyword.Auto;

            //슬롯 배치의 시작점, 배치 방향, 줄바꿈 처리
            _slotArea.style.justifyContent = Justify.FlexStart;
            _slotArea.style.flexDirection = FlexDirection.Row;
            _slotArea.style.flexWrap = Wrap.Wrap;
        }

        //슬롯 배치
        private void GenerateSlotsUI()
        {
            if (_slotArea == null || _slotTemplate == null) return;

            //기존 이벤트가 있다면 모두 제거
            foreach (var slotView in _slotViews)
            {
                slotView.ClearSubscribedEvents();
            }
            
            //시작할 때 깔끔하게 슬롯 정보 클리어
            _slotArea.Clear();
            _slotViews.Clear();

            //가방 크기만큼 반복
            for (int i = 0; i < _viewModel.Capacity; i++)
            {
                //슬롯 인스턴스를 가져와서 영역에 추가
                VisualElement slotInstance = _slotTemplate.Instantiate();
                _slotArea.Add(slotInstance);

                //슬롯 인스턴스의 진짜 데이터를 가져와서 유저 데이터에 저장하고, 피킹모드를 명확하게 지정
                VisualElement realSlotRoot = slotInstance[0];
                realSlotRoot.userData = i;
                realSlotRoot.pickingMode = PickingMode.Position;

                //ItemSlotView 생성 및 연결
                ItemSlotView slotView = new ItemSlotView(
                    realSlotRoot,
                    _root,
                    i,
                    (fromIdx, toIdx) => _viewModel.RequestSwapSlots(fromIdx, toIdx),
                    (index) => _viewModel.RequestUseItem(index),
                    (idx) => InterceptDeleteAction(idx)
                );

                //툴팁 호출 이벤트 연결
                slotView.OnPointerEnterSlot += (itemData, pos) => _tooltipView?.Show(itemData, pos);
                
                //툴팁 위치 이동 이벤트 연결
                slotView.OnPointerMoveSlot += (pos) => _tooltipView?.UpdatePosition(pos);
                
                //툴팁 소멸 이벤트 연결
                slotView.OnPointerExitSlot += () => _tooltipView?.Hide();

                //아이템 분할처리 이벤트 연결
                slotView.OnSplitDropRequested += (fromIdx, toIdx) => InterceptSplitAction(fromIdx, toIdx);

                //슬롯 뷰에 슬롯 뷰 모델 바인딩처리를 진행하고 슬롯 뷰 목록에 추가
                slotView.Bind(_viewModel.Slots[i]);
                _slotViews.Add(slotView);
            }
        }

        //I키가 입력되면 실행
        public void ToggleWindow()
        {
            //인벤토리 창 활성화, 만약 이미 활성화 되어있다면 비활성화
            if (_inventoryWindow == null) return;
            bool isVisible = _inventoryWindow.style.display == DisplayStyle.Flex;
            _inventoryWindow.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            
            //인벤토리가 닫히면 모든 팝업을 닫음
            if (isVisible) CloseAllPopups();
        }

        //닫기 버튼이 클릭되면 실행
        public void CloseWindow()
        {
            //인벤토리 및 팝업 닫기
            if (_inventoryWindow != null)
            {
                _inventoryWindow.style.display = DisplayStyle.None;
                CloseAllPopups();
            }
        }

        //모든 팝업을 닫는 메소드
        private void CloseAllPopups()
        {
            _tooltipView?.Hide();
            _confirmationPopupView?.Hide();
            _amountSplitPopupView?.Hide();
        }

        //아이템을 인벤토리 밖으로 끌어다 놓을 시 실행되는 삭제요청 메소드
        private void InterceptDeleteAction(int index)
        {
            if (_viewModel == null || index < 0 || index >= _viewModel.Slots.Count) return;

            //타겟 슬롯에 문제가 없는지 체크
            var targetSlotVm = _viewModel.Slots[index];
            if (!targetSlotVm.HasItem || _confirmationPopupView == null) return;

            //아이템 삭제 요청
            _confirmationPopupView.Show(targetSlotVm.ItemData, () =>
            {
                _viewModel.RequestRemoveItem(index);
            });
        }

        //아이템 분할 요청 액션
        private void InterceptSplitAction(int fromIdx, int toIdx)
        {
            if (_viewModel == null) return;

            //분할 시작, 도착 슬롯 지정
            var srcSlotVm = _viewModel.Slots[fromIdx];
            var destSlotVm = _viewModel.Slots[toIdx];

            //시작 슬롯에 아이템이 없거나, 도착슬롯에 이미 아이템이 있지는 않은지 체크
            if (!srcSlotVm.HasItem || destSlotVm.HasItem || _amountSplitPopupView == null) return;

            //아이템이 몇개있는지 확인해서, 유효할때만 진행하고, 유효하지 않다면 1 지정 후 리턴
            if (int.TryParse(srcSlotVm.AmountText, out int totalAmount) == false) totalAmount = 1;
            if (totalAmount <= 1) return;

            //아이템 분할 요청
            _amountSplitPopupView.Show(srcSlotVm.ItemData, totalAmount, (splitAmount) =>
            {
                _viewModel.RequestSeparateItem(fromIdx, toIdx, splitAmount);
            });
        }
        
        //삭제 시 언바인드
        private void OnDestroy()
        {
            Unbind();
        }
        
        public void Unbind()
        {
            if (_inventoryWindow != null)
            {
                _inventoryWindow.UnregisterCallback<ClickEvent>(OnInventoryWindowClicked);
            }

            //뷰 모델의 언바인드 진행
            if (_viewModel != null) _viewModel.UnbindFromModel();

            _viewModel = null;
        }
    }
}