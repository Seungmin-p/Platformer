using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace MyInventory.UIToolkit
{
    //필터 타입 enum
    public enum ItemFilterType
    {
        All,
        Equipment,
        Consumable,
        Material
    }

    public class InventoryViewModel
    {
        //인벤토리 모델, 슬롯 뷰 모델 리스트
        private readonly Inventory _model;
        private readonly List<ItemSlotViewModel> _slots;

        //인벤토리 칸 수를 모델에서 가져오는 프로퍼티
        public int Capacity => _model != null ? _model.Capacity : 0;
        
        //뷰 영역에서 슬롯을 안전하게 다룰 수 있도록 List 대신 ReadOnlyCollection 사용
        public ReadOnlyCollection<ItemSlotViewModel> Slots { get; }
        
        //현재 활성화 된 필터를 확인
        public ItemFilterType CurrentFilter { get; private set; } = ItemFilterType.All;

        public InventoryViewModel(Inventory model)
        {
            //모델 및 슬롯 리스트 등록
            _model = model;
            _slots = new List<ItemSlotViewModel>();

            //에디터 환경에서의 오류를 방지하기 위한 예외처리
            if (_model == null)
            {
                Slots = _slots.AsReadOnly();
                return;
            }

            //슬롯을 채워줌
            for (int i = 0; i < Capacity; i++)
            {
                _slots.Add(new ItemSlotViewModel());
            }
            
            //ReadOnlyCollection 타입으로 저장될 수 있도록 AsReadOnly 이용
            Slots = _slots.AsReadOnly();
            
            //변경된 슬롯 업데이트
            _model.OnSlotUpdated += RefreshSingleSlot;
            
            //전체 슬롯 업데이트
            _model.OnAllSlotsUpdated += RefreshAllSlots;
            
            //인벤토리 최대 크기 변경 이벤트
            // _model.OnCapacityChanged += HandleCapacityChanged;

            //전체 슬롯 새로고침
            RefreshAllSlots();
        }

        //뷰 영역에서 필터를 지정하면서 호출
        public void SetFilter(ItemFilterType filterType)
        {
            //만약 현재 필터와 같다면 무시
            if (CurrentFilter == filterType) return;
            
            //그렇지 않다면 현재 필터를 변경하고, 전체 슬롯 새로고침
            CurrentFilter = filterType;
            RefreshAllSlots();
        }

        //필터에 맞지 않는 아이템들을 true로 반환해주는 메소드
        private bool IsFilteredOut(Item item)
        {
            //false 리턴 시 화면에 그대로 출력을 의미함
            if (item == null) return false;
            if (CurrentFilter == ItemFilterType.All) return false;
            
            if (CurrentFilter == ItemFilterType.Equipment && item is EquipmentItem) return false;
            if (CurrentFilter == ItemFilterType.Consumable && item is PotionItem) return false;
            if (CurrentFilter == ItemFilterType.Material && item is MaterialItem) return false;
            
            //각 필터별로 체크를 했을 때, 현재 필터와 다른 아이템들은 true가 되는데, 이때 어둡게 처리가 진행됨
            return true;
        }

        //전달된 인덱스에 맞는 슬롯을 업데이트 해주는 메소드
        private void RefreshSingleSlot(int index)
        {
            if (index < 0 || index >= _slots.Count || _model == null) return;

            //해당 인덱스의 아이템을 가져옴
            Item item = _model.GetItem(index); 

            if (item != null)
            {
                //아이템이 존재하면 현재 수량 및 필터 상태 체크
                int amount = _model.GetCurrentAmount(index);
                bool isFilteredOut = IsFilteredOut(item); 
                
                //슬롯 내 아이템 정보 업데이트
                _slots[index].UpdateSlot(item.Data, item.Data.IconSprite, amount, isFilteredOut);
            }
            else
            {
                //아이템이 없다면 슬롯 초기화
                _slots[index].ClearSlot();
            }
        }

        //모든 슬롯 새로고침
        public void RefreshAllSlots()
        {
            if (_model == null) return;
            
            //슬롯 전체를 돌면서
            for (int i = 0; i < _slots.Count; i++)
            {
                //인벤토리 범위 안이라면 새로고침, 밖이라면 클리어
                if (i < _model.Capacity) RefreshSingleSlot(i);
                //추후 가방 크기 변경 등을 위해 범위 밖은 클리어 처리
                else _slots[i].ClearSlot();
            }
        }

        //인벤토리 크기 변경 이벤트 호출 시 사용,
        // private void HandleCapacityChanged(int newCapacity) => RefreshAllSlots();

        //뷰에서 Sort, Trim 버튼을 눌렀을 때 호출되어서, 모델의 Sort, Trim 동작 실행
        public void CommandSort() { if (_model != null) _model.SortAll(); }
        public void CommandTrim() { if (_model != null) _model.TrimAll(); }
        
        //뷰에서 아이템 스왑, 사용, 삭제할 때 각각 모델의 관련 동작 실행
        public void RequestSwapSlots(int fromIdx, int toIdx) { if (_model != null) _model.Swap(fromIdx, toIdx); }
        public void RequestUseItem(int idx) { if (_model != null) _model.Use(idx); }
        public void RequestRemoveItem(int idx) { if (_model != null) _model.Remove(idx); }

        //뷰에서 호출받아서 모델 영역에게 아이템 분할을 요청
        public void RequestSeparateItem(int fromIdx, int toIdx, int amount)
        {
            if (_model != null)
            {
                _model.SeparateItem(fromIdx, toIdx, amount);
            }
        }
        
        //이벤트 해제
        public void UnbindFromModel()
        {
            if (_model == null) return;
            _model.OnSlotUpdated -= RefreshSingleSlot;
            _model.OnAllSlotsUpdated -= RefreshAllSlots;
            // _model.OnCapacityChanged -= HandleCapacityChanged;
        }
    }
}