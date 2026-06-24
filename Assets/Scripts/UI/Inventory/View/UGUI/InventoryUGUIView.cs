using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace MyInventory.UGUI
{
    public class InventoryUGUIView : MonoBehaviour
    {
        [Header("UI Templates")] 
        //아이템 슬롯, 툴팁, 버리기 확인, 아이템 나누기 UI
        [SerializeField] GameObject _slotTemplate;
        [SerializeField] GameObject _tooltipTemplate; 
        [SerializeField] GameObject _confirmationTemplate; 
        [SerializeField] GameObject _splitTemplate;

        [Header("인벤토리 구성을 위한 슬롯 정보")]
        //인벤토리 한줄에 들어갈 슬롯의 수, 슬롯의 크기, 슬롯별로 할당된 좌+우 마진 크기
        [SerializeField] int _columnCount = 7;
        [SerializeField] float _slotSize = 100f;
        [SerializeField] float _slotHorizontalMargin = 6f;

        //뷰모델 데이터
        private InventoryUGUIViewModel _viewModel;
        
        //인벤토리 레이아웃
        //UI 제어용 변수, 인벤토리 창, 슬롯영역
        [SerializeField] GameObject _inventoryWindow;
        [SerializeField] RectTransform _slotArea;

        [Header("UGUI 전용 버튼 컴포넌트 배선")]
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnSort;
        [SerializeField] private Button _btnTrim;
        [SerializeField] private Button _btnFilterAll;
        [SerializeField] private Button _btnFilterEquip;
        [SerializeField] private Button _btnFilterConsume;
        [SerializeField] private Button _btnFilterMaterial;

        [Header("UGUI 전용 헤더 드래그 영역")]
        [SerializeField] private GameObject _headerArea;
        [SerializeField] private Transform _canvasOverlayRoot;

        //실제 팝업 제어용 인스턴스
        private InventoryTooltipUGUIView _tooltipView;
        private InventoryConfirmationPopupUGUIView _confirmationPopupView; 
        private InventoryAmountSplitPopupUGUIView _amountSplitPopupView;

        //슬롯 뷰 들을 순서대로 담아둘 리스트
        private readonly List<ItemSlotUGUIView> _slotViews = new List<ItemSlotUGUIView>();

        //바인딩처리
        public void Bind(InventoryUGUIViewModel viewModel)
        {
            //혹시 모를 중복 바인딩을 방지하기 위한 언바인드 실행
            Unbind();
            
            //뷰 모델 연결
            _viewModel = viewModel;

            //인벤토리 창, 슬롯 영역
            if (_inventoryWindow == null || _slotArea == null)
            {
                Debug.LogError("[InventoryUGUIView] 필수 UI 컴포넌트가 인스펙터에 누락되었습니다.");
            }

            //버튼별 이벤트 등록(UGUI용)
            if (_btnClose != null) _btnClose.onClick.AddListener(CloseWindow);
            if (_btnSort != null) _btnSort.onClick.AddListener(() => _viewModel?.CommandSort());
            if (_btnTrim != null) _btnTrim.onClick.AddListener(() => _viewModel?.CommandTrim());
            if (_btnFilterAll != null) _btnFilterAll.onClick.AddListener(() => _viewModel?.SetFilter(ItemFilterType.All));
            if (_btnFilterEquip != null) _btnFilterEquip.onClick.AddListener(() => _viewModel?.SetFilter(ItemFilterType.Equipment));
            if (_btnFilterConsume != null) _btnFilterConsume.onClick.AddListener(() => _viewModel?.SetFilter(ItemFilterType.Consumable));
            if (_btnFilterMaterial != null) _btnFilterMaterial.onClick.AddListener(() => _viewModel?.SetFilter(ItemFilterType.Material));
            
            //인벤토리 창을 마우스로 잡고 위치를 옮기기 위한 바인딩,
            //헤더 영역을 지정하고, 문제가 없다면
            GameObject headerArea = _headerArea;
            if (_inventoryWindow != null && headerArea != null)
            {
                //위치 방식을 Absolute으로 지정, 마우스 입력 제어 객체 생성
                RectTransform windowRect = null;
                if (_inventoryWindow.transform.childCount > 0)
                {
                    windowRect = _inventoryWindow.transform.GetChild(0) as RectTransform;
                }
                
                if (windowRect == null)
                {
                    windowRect = _inventoryWindow.GetComponent<RectTransform>();
                }
                
                WindowDragManipulatorUGUI dragHandler = headerArea.GetComponent<WindowDragManipulatorUGUI>();
                if (dragHandler == null) dragHandler = headerArea.AddComponent<WindowDragManipulatorUGUI>();
                
                //헤더 영역에만 동작 지정
                dragHandler.Setup(windowRect, "MyInventoryPos");
                
                //인벤토리의 초기 위치를 기존 정보에서 로딩
                dragHandler.LoadPosition();
            }

            //아이템 설명(툴팁) 팝업
            if (_tooltipTemplate != null && _canvasOverlayRoot != null)
            {
                GameObject tooltipInstance = Instantiate(_tooltipTemplate, _canvasOverlayRoot);
                _tooltipView = tooltipInstance.GetComponent<InventoryTooltipUGUIView>();

                LayoutElement layoutElement = tooltipInstance.GetComponent<LayoutElement>();
                if (layoutElement == null) layoutElement = tooltipInstance.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;
            }

            //아이템 버리기 팝업
            if (_confirmationTemplate != null && _canvasOverlayRoot != null)
            {
                GameObject confirmationInstance = Instantiate(_confirmationTemplate, _canvasOverlayRoot);
                _confirmationPopupView = confirmationInstance.GetComponent<InventoryConfirmationPopupUGUIView>();
            }

            //아이템 수량 나누기 팝업
            if (_splitTemplate != null && _canvasOverlayRoot != null)
            {
                GameObject splitInstance = Instantiate(_splitTemplate, _canvasOverlayRoot);
                _amountSplitPopupView = splitInstance.GetComponent<InventoryAmountSplitPopupUGUIView>();
            }

            //초기 레이아웃 설정
            ApplyDynamicLayout();
            
            //슬롯 및 UI 이벤트 설정
            GenerateSlotsUI();
            
            //시작 필터는 전체(All)
            _viewModel?.SetFilter(ItemFilterType.All);

            //인벤토리창은 시작할땐 기본적으로 숨김처리
            if (_inventoryWindow != null)
            {
                _inventoryWindow.SetActive(false);
            }
        }
        
        //클릭 이벤트 버튼들을 묶은 메소드
        private void OnInventoryWindowClicked(string clickedButtonName)
        {
            // 명시적 람다 바인딩 체계로 전형 대체되어 사용되지 않습니다.
        }
        
        //인벤토리 레이아웃 조정
        private void ApplyDynamicLayout()
        {
            if (_slotArea == null) return;

            GridLayoutGroup gridLayout = _slotArea.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                //슬롯 배치를 위한 기초작업
                gridLayout.childAlignment = TextAnchor.UpperCenter;
            }
            
            Canvas.ForceUpdateCanvases();
        }

        //슬롯 배치
        private void GenerateSlotsUI()
        {
            if (_slotArea == null || _slotTemplate == null) return;

            //기존 이벤트가 있다면 모두 제거
            foreach (var slotView in _slotViews)
            {
                slotView.ClearSubscribedEvents();
                Destroy(slotView.gameObject);
            }
            
            //시작할 때 깔끔하게 슬롯 정보 클리어
            _slotViews.Clear();

            //가방 크기만큼 반복
            for (int i = 0; i < _viewModel.Capacity; i++)
            {
                //슬롯 인스턴스를 가져와서 영역에 추가
                GameObject slotInstance = Instantiate(_slotTemplate, _slotArea);

                //슬롯 인스턴스의 진짜 데이터를 가져와서 유저 데이터에 저장하고, 피킹모드를 명확하게 지정
                ItemSlotUGUIView slotView = slotInstance.GetComponent<ItemSlotUGUIView>();
                slotView.SetSlotIndex(i);

                //ItemSlotView 생성 및 연결
                slotView.Setup(
                    _canvasOverlayRoot,
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
            bool isVisible = _inventoryWindow.activeSelf;
            _inventoryWindow.SetActive(!isVisible);
            
            //인벤토리가 닫히면 모든 팝업을 닫음
            if (isVisible) CloseAllPopups();
        }

        //닫기 버튼이 클릭되면 실행
        public void CloseWindow()
        {
            //인벤토리 및 팝업 닫기
            if (_inventoryWindow != null)
            {
                _inventoryWindow.SetActive(false);
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
            if (_btnClose != null) _btnClose.onClick.RemoveAllListeners();
            if (_btnSort != null) _btnSort.onClick.RemoveAllListeners();
            if (_btnTrim != null) _btnTrim.onClick.RemoveAllListeners();
            if (_btnFilterAll != null) _btnFilterAll.onClick.RemoveAllListeners();
            if (_btnFilterEquip != null) _btnFilterEquip.onClick.RemoveAllListeners();
            if (_btnFilterConsume != null) _btnFilterConsume.onClick.RemoveAllListeners();
            if (_btnFilterMaterial != null) _btnFilterMaterial.onClick.RemoveAllListeners();

            //뷰 모델의 언바인드 진행
            if (_viewModel != null) _viewModel.UnbindEvents();
            _viewModel = null;
        }
    }
}