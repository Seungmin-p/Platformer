using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace MyInventory.UGUI
{
    public enum ItemFilterType
    {
        All,
        Equipment,
        Consumable,
        Material
    }
    
    public class InventoryUGUIViewModel
    {
        //인벤토리 모델, 슬롯 뷰 모델 리스트
        private readonly Inventory _model;
        private readonly List<ItemSlotUGUIViewModel> _slots;

        //인벤토리 칸 수를 모델에서 가져오는 프로퍼티
        public int Capacity => _model != null ? _model.Capacity : 0;
        
        //뷰 영역에서 슬롯을 안전하게 다룰 수 있도록 List 대신 ReadOnlyCollection 사용
        public ReadOnlyCollection<ItemSlotUGUIViewModel> Slots { get; }
        
        //현재 활성화 된 필터를 확인
        public ItemFilterType CurrentFilter { get; private set; } = ItemFilterType.All;

        public InventoryUGUIViewModel(Inventory model)
        {
            //모델 및 슬롯 리스트 등록
            _model = model;
            _slots = new List<ItemSlotUGUIViewModel>();

            //에디터 환경에서의 오류를 방지하기 위한 예외처리
            if (_model == null)
            {
                Slots = _slots.AsReadOnly();
                return;
            }

            //슬롯을 채워줌
            for (int i = 0; i < _model.Capacity; i++)
            {
                _slots.Add(new ItemSlotUGUIViewModel());
            }
            Slots = _slots.AsReadOnly();

            _model.OnSlotUpdated += RefreshSingleSlot;
            _model.OnAllSlotsUpdated += RefreshAllSlots;
            
            RefreshAllSlots();
        }

        public void UnbindEvents()
        {
            if (_model == null) return;
            _model.OnSlotUpdated -= RefreshSingleSlot;
            _model.OnAllSlotsUpdated -= RefreshAllSlots;
        }

        public void SetFilter(ItemFilterType filterType)
        {
            if (CurrentFilter == filterType) return;
            CurrentFilter = filterType;
            RefreshAllSlots();
        }

        private void RefreshAllSlots()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                //인벤토리 범위 안이라면 새로고침, 밖이라면 클리어
                if (i < _model.Capacity) RefreshSingleSlot(i);
                //추후 가방 크기 변경 등을 위해 범위 밖은 클리어 처리
                else _slots[i].ClearSlot();
            }
        }

        private void RefreshSingleSlot(int i)
        {
            if (i < 0 || i >= _slots.Count) return;

            if (_model.HasItem(i))
            {
                Item item = _model.GetItem(i);
                int amount = _model.GetCurrentAmount(i);
                bool isFiltered = IsFilteredOut(item.Data);

                _slots[i].UpdateSlot(item.Data, item.Data.IconSprite, amount, isFiltered);
            }
            else
            {
                _slots[i].ClearSlot();
            }
        }

        private bool IsFilteredOut(ItemData data)
        {
            if (CurrentFilter == ItemFilterType.All) return false;
            if (CurrentFilter == ItemFilterType.Equipment && (data is WeaponItemData || data is ArmorItemData)) return false;
            if (CurrentFilter == ItemFilterType.Consumable && data is PotionItemData) return false;
            if (CurrentFilter == ItemFilterType.Material && data is MaterialItemData) return false;
            
            return true;
        }

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
    }
}