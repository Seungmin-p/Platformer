using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyInventory
{
    //인벤토리 Model
    public class Inventory
    {
        //인벤토리 슬롯 배열 및 내부 인덱스 연산용 변수
        private Item[] _items;
        private readonly HashSet<int> _indexSetForUpdate = new HashSet<int>();
        
        //정렬처리용 변수
        private IComparer<Item> _itemComparer;

        //외부 호출용 인벤토리 크기 프로퍼티
        public int Capacity { get; private set; }

        //MVVM 구조를 위한 이벤트
        public event Action<int> OnSlotUpdated; //슬롯 업데이트
        public event Action OnAllSlotsUpdated; //전체 슬롯 업데이트
        public event Action<int> OnCapacityChanged; //인벤토리 크기 업데이트

        //정렬을 위한 아이템 유형 별 가중치
        private readonly static Dictionary<Type, int> _sortWeightDict = new Dictionary<Type, int>
        {
            { typeof(WeaponItemData), 10000 },
            { typeof(ArmorItemData),  20000 }, 
            { typeof(PotionItemData),   30000 }, 
            { typeof(MaterialItemData),   40000 }, 
        };

        //생성자를 이용해서 초기 세팅 진행
        public Inventory(int initialCapacity, int maxCapacity)
        {
            //인벤토리 칸 수가 최대치를 넘을 수 없게 함
            if (initialCapacity > maxCapacity) initialCapacity = maxCapacity;
            
            //Array.Sort를 이용하기 위한 IComparer 규격의 객체를 미리 저장해둠
            _itemComparer = Comparer<Item>.Create(CompareItems);
            
            //최대치에 맞춰서 인벤토리 공간 확보 및 현재 칸 조정
            _items = new Item[maxCapacity];
            Capacity = initialCapacity;
        }

        //바인딩 처리 후 호출되는 인벤토리 크기 할당용 초기화 메소드
        public void Initialize()
        {
            //인벤토리 크기 할당을 위한 이벤트 호출
            OnCapacityChanged?.Invoke(Capacity);
        }

        //인벤토리에 아이템을 추가하는 로직
        public void Add(ItemData data, int amount)
        {
            //추가해야하는 갯수
            int remaining = amount;

            //만약 아이템이 수량이 있는 아이템(소모품, 재료)이라면
            if (data is CountableItemData)
            {
                //인벤토리 전체 검사
                for (int i = 0; i < Capacity; i++)
                {
                    //각 슬롯별로 수량 아이템인지 확인하고, 유형이 같은지, 또한 이미 최대수치인지 확인
                    if (_items[i] is CountableItem ci && ci.Data == data && !ci.IsMax)
                    {
                        //아이템 수치 업데이트 및 최대 개수를 초과한 경우 반환받음
                        remaining = ci.AddAmountAndGetExcess(remaining);
                        UpdateSlot(i);
                        
                        //최대 수치를 초과하지 않아 남은 아이템이 없다면 리턴
                        //만약 최대수치를 초과한다면 남은 인벤토리를 체크하면서 같은 아이템에 추가되거나, 다음 단계로 진행
                        if (remaining <= 0) return;
                    }
                }
            }

            //수량 아이템이 아니었거나, 수량 아이템인데 최대 수치를 넘겨서 새 칸을 할당 받아야 하는경우
            if (remaining > 0)
            {
                //인벤토리 전체 검사
                for (int i = 0; i < Capacity; i++)
                {
                    //슬롯이 비어있다면
                    if (_items[i] == null)
                    {
                        //현재 추가하려는 아이템 데이터에 맞춰서 아이템 생성 후 슬롯에 넣어줌
                        Item newItem = data.CreateItem();
                        _items[i] = newItem;

                        //만약 아이템이 수량 아이템이라면
                        if (newItem is CountableItem ci)
                        {
                            //남은 수량만큼 추가, 현재 기능상으로는 남은 아이템이 다시 한번 99개 이상이 될 수가 없음
                            ci.SetAmount(remaining);
                        }
                        
                        //슬롯 업데이트
                        UpdateSlot(i);
                        return;
                    }
                }
            }

            //만약 인벤토리 내 빈칸을 못찾은 경우
            if (remaining > 0)
            {
                Debug.LogWarning($"[Inventory] 인벤토리가 가득 찼습니다! 남은 아이템 수량: {remaining}");
            }
        }

        //인벤토리 슬롯 내 아이템을 삭제하는 메소드
        public void Remove(int index)
        {
            //인덱스에 문제가 없다면
            if (!IsValidIndex(index)) return;
            
            //인덱스를 비우고, 슬롯 업데이트 이벤트 호출
            _items[index] = null;
            UpdateSlot(index);
        }

        //두 아이템의 위치를 바꿔주는 메소드
        public void Swap(int indexA, int indexB)
        {
            //두 인덱스가 문제 없는지 확인
            if (!IsValidIndex(indexA) || !IsValidIndex(indexB) || indexA == indexB ) return;

            Item itemA = _items[indexA];
            Item itemB = _items[indexB];

            //두 아이템이 다 비어있지 않고, 아이템이 서로 똑같고, 수량 아이템인지 먼저 체크
            if (itemA != null && itemB != null &&
                itemA.Data == itemB.Data &&
                itemA is CountableItem ciA && itemB is CountableItem ciB)
            {
                int maxAmount = ciB.MaxAmount;
                int sum = ciA.Amount + ciB.Amount;
                
                //두 아이템의 수량을 합쳐서 최대수치를 넘는지 체크
                if (sum <= maxAmount)
                {
                    //최대 수량보다 적다면 옮기기 시작한 슬롯 내 아이템 개수를 0, 옮긴 슬롯의 아이템 개수를 합한 수량으로 변경
                    ciA.SetAmount(0);
                    ciB.SetAmount(sum);
                }
                else
                {
                    //최대치를 넘는다면 최대치와, 최대치를 빼고 남은 개수를 각각 지정
                    ciA.SetAmount(sum - maxAmount);
                    ciB.SetAmount(maxAmount);
                }
            }
            else
            {
                //새 칸이 비어있거나, 두 아이템이 같지 않거나, 수량아이템이 아니라면 단순하게 두 아이템 교체
                _items[indexA] = itemB;
                _items[indexB] = itemA;
            }
            //두 슬롯 업데이트
            UpdateSlot(indexA, indexB);
        }

        //아이템을 나누는 메소드
        public void SeparateItem(int fromIndex, int toIndex, int amount)
        {
            //두 인덱스 범위에 문제 없는지 체크
            if (fromIndex < 0 || fromIndex >= Capacity || toIndex < 0 || toIndex >= Capacity) return;
            
            //나누려는 슬롯에 아이템이 있으면 패스
            if (_items[toIndex] != null) return; 

            //시작 아이템이 수량 아이템인 경우
            if (_items[fromIndex] is CountableItem countableItem)
            {
                //아이템을 나누고 나눠진 개수만큼 복제해서 받음
                CountableItem separatedItem = countableItem.SeparateAndClone(amount);
                
                if (separatedItem != null)
                {
                    //새 슬롯에 복제된 아이템 넣어주기
                    _items[toIndex] = separatedItem;
                    
                    //슬롯 업데이트
                    UpdateSlot(fromIndex, toIndex);
                }
            }
        }

        //아이템 사용 메소드
        public void Use(int index)
        {
            if (!IsValidIndex(index)) return;

            //사용 가능 아이템인 경우
            if (_items[index] is IUsableItem uItem)
            {
                //아이템 사용, 슬롯 업데이트
                if (uItem.Use()) UpdateSlot(index);
            }
        }

        //Trim 정렬 메소드
        public void TrimAll()
        {
            //HashSet을 새로 만드는 대신 기존에 정해둔걸 클리어해서 이용
            _indexSetForUpdate.Clear();

            int i = -1;
            
            //인벤토리 범위 내에서 빈칸을 찾을 때 까지 진행
            while (i + 1 < Capacity && _items[++i] != null) ;
            
            //확정된 i의 위치를 j에도 저장해서 탐색 시작 위치 조정
            int j = i;

            //인벤토리 범위를 벗어날 때 까지 반복
            while (true)
            {
                //아이템을 찾을 때 까지 진행
                while (++j < Capacity && _items[j] == null) ;
                
                //만약 인벤토리 범위를 벗어나면 멈춤
                if (j >= Capacity) break;
                
                //빈 공간인 i와 아이템이 있는 j를 HashSet에 저장
                _indexSetForUpdate.Add(i);
                _indexSetForUpdate.Add(j);
                
                //두 슬롯의 데이터를 서로 바꿔주고 다음 인덱스 체크
                _items[i] = _items[j];
                _items[j] = null;
                i++;
            }

            //_indexSetForUpdate 내에 존재하는 인덱스 전부 슬롯 업데이트
            foreach (var index in _indexSetForUpdate) UpdateSlot(index);
        }

        //Sort 정렬 메소드
        public void SortAll()
        {
            //아이템 수량을 합치고 나서 Trim 정렬 진행
            MergeCountableItems();
            TrimAll(); 

            //빈칸을 찾을때까지 인벤토리 내부 체크
            int i = 0;
            while (i < Capacity && _items[i] != null) i++;

            //while을 빠져 나왔다면 빈칸을 찾았거나, 인벤토리 범위를 넘어갔으면 i는 곧 아이템의 개수를 의미함
            //이때 i가 1보다 크다면 Sort 정렬 진행
            if (i > 1)
            {
                //아이템들을 대상으로 0 인덱스부터 i개 만큼만 진행
                //이때 _itemComparer 이 CompareItems 메소드를 이용한 정렬 규칙을 의미함
                Array.Sort(_items, 0, i, _itemComparer);
                
                //전체 정렬 이벤트 호출
                OnAllSlotsUpdated?.Invoke(); 
            }
        }

        //인덱스 범위에 문제가 없는지 확인
        private bool IsValidIndex(int index) => index >= 0 && index < Capacity;

        //두 아이템을 정렬
        private int CompareItems(Item a, Item b)
        {
            //두 아이템의 ID값 + 정렬 가중치
            int weightA = a.Data.ID + _sortWeightDict[a.Data.GetType()];
            int weightB = b.Data.ID + _sortWeightDict[b.Data.GetType()];

            //종류 정렬, 두 아이템의 타입이 다르다면 오름차순(CompareTo) 정렬
            if (weightA != weightB) return weightA.CompareTo(weightB);

            //종류가 같다면 둘 다 수량아이템인지 체크
            if (a is CountableItem ciA && b is CountableItem ciB)
            {
                //수량에 맞춰서 B->A로 내림차순 정렬 즉, 수량이 더 높을수록 앞에 배치
                return ciB.Amount.CompareTo(ciA.Amount); 
            }
            
            //종류가 같고, 둘 다 수량아이템이 아니라면 아직은 굳이 정렬할 필요 없음
            return 0;
        }

        //슬롯 업데이트 이벤트를 호출하는 메소드
        private void UpdateSlot(int index)
        {
            //인덱스에 문제가 없는지 체크
            if (!IsValidIndex(index)) return;
         
            //수량 아이템의 수량이 0이 된 경우 null 처리
            if (_items[index] is CountableItem ci && ci.IsEmpty)
            {
                _items[index] = null;
            }

            //슬롯 정보 업데이트 이벤트 호출
            OnSlotUpdated?.Invoke(index);
        }

        //배열을 전달받아서 개별 UpdateSlot 동작 진행
        private void UpdateSlot(params int[] indices)
        {
            foreach (var i in indices) UpdateSlot(i);
        }

        //흩어진 수량 아이템들을 최대한 합쳐주는 메소드
        private void MergeCountableItems()
        {
            //인벤토리 범위 -1까지 체크
            for (int i = 0; i < Capacity - 1; i++)
            {
                //i 인덱스 슬롯의 아이템이 수량 아이템인경우
                if (_items[i] is CountableItem targetItem)
                {
                    //i를 제외하고 다음꺼부터 체크
                    for (int j = i + 1; j < Capacity; j++)
                    {
                        //아이템 수량이 최대치면 패스
                        if (targetItem.IsMax) break;

                        //대상 아이템이 수량 아이템이면서, I 아이템과 J 아이템의 유형이 같다면
                        if (_items[j] is CountableItem sourceItem && targetItem.Data == sourceItem.Data)
                        {
                            //아이템 최대치 - 현재수량, 즉 더 넣을 수 있는 크기를 의미
                            int spaceLeft = targetItem.MaxAmount - targetItem.Amount;
                            
                            //원래 있던 수량과 비교해서 더 적은쪽 선택
                            int amountToMove = Mathf.Min(spaceLeft, sourceItem.Amount);
                            
                            //이동하는 수량만큼 더하고 빼줌
                            targetItem.SetAmount(targetItem.Amount + amountToMove);
                            sourceItem.SetAmount(sourceItem.Amount - amountToMove);

                            //이동된 아이템이 비었다면 null
                            if (sourceItem.IsEmpty)
                            {
                                _items[j] = null;
                            }
                        }
                    }
                }
            }
        }
        
        //뷰 모델에서 호출할 데이터 조회용 메소드들
        //각각 해당 인덱스에 아이템이 존재하는지, 아이템이 무엇인지, 수량이 얼마나 되는지 확인
        public bool HasItem(int index) => IsValidIndex(index) && _items[index] != null;
        public Item GetItem(int index) => IsValidIndex(index) ? _items[index] : null;
        public int GetCurrentAmount(int index)
        {
            if (!IsValidIndex(index)) return -1;
            if (_items[index] == null) return 0;
            if (!(_items[index] is CountableItem ci)) return 1;
            return ci.Amount;
        }
    }
}