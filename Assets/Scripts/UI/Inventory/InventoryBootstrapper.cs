using UnityEngine;
using Random = UnityEngine.Random;

namespace MyInventory.UIToolkit
{
    public class InventoryBootstrapper : MonoBehaviour
    {
        //인벤토리의 모델과 뷰를 이용해서 뷰모델 생성 및 연결하기 위해 연결받음
        [SerializeField, Range(8, 64)] int _initialCapacity = 32;
        [SerializeField, Range(8, 64)] int _maxCapacity = 64;
        [SerializeField] InventoryView _inventoryView;
        
        [Header("아이템 목록")]
        [SerializeField]  ItemData[] _itemDatabase;
        
        //인벤토리 모델
        private Inventory _inventoryModel;

        private void Start()
        {
            if (_inventoryView == null)
            {
                Debug.LogError("[InventoryBootstrapper] 인스펙터 컴포넌트 할당을 확인해주세요.");
                return;
            }
            
            //인벤토리 모델 생성
            _inventoryModel = new Inventory(_initialCapacity, _maxCapacity);
            
            //인벤토리 모델 데이터를 기준으로 뷰모델 생성 이후 뷰에서 바인딩 진행
            InventoryViewModel viewModel = new InventoryViewModel(_inventoryModel);
            _inventoryView.Bind(viewModel);

            //바인딩 이후 초기화 진행
            _inventoryModel.Initialize();
            
            //게임 시작 시 모든 아이템 종류를 인벤토리에 넣어줌
            if (_itemDatabase != null && _itemDatabase.Length > 0)
            {
                foreach (var itemData in _itemDatabase)
                {
                    if (itemData == null) continue;

                    //장비 아이템은 1개씩, 수량성 아이템(포션, 재료)은 무작위 개수로 지급
                    int amount = itemData is CountableItemData ? Random.Range(1, 99) : 1;
                    _inventoryModel.Add(itemData, amount);
                }
            }
        }
        
        //키 입력은 실제 게임 적용시엔 게임 매니저 등에서 관리
        private void Update()
        {
            //I를 눌러 인벤토리를 열고 닫음
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (_inventoryView != null)
                {
                    _inventoryView.ToggleWindow();
                }
            }
            
            //K를 누르면 무작위 아이템 획득
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (_itemDatabase != null && _itemDatabase.Length > 0)
                {
                    int randomIndex = Random.Range(0, _itemDatabase.Length);
                    ItemData randomItem = _itemDatabase[randomIndex];
                    
                    int amount = randomItem is CountableItemData ? Random.Range(5, 20) : 1;
                    _inventoryModel.Add(randomItem, amount);
                    
                    Debug.Log($"[Test Drop] {randomItem.Name} {amount}개 획득!");
                }
            }
        }
    }
}