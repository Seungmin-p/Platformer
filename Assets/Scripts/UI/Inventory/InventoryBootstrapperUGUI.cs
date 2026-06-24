using UnityEngine;
using Random = UnityEngine.Random;

namespace MyInventory.UGUI
{
    public class InventoryBootstrapperUGUI : MonoBehaviour
    {
        //인벤토리의 모델과 뷰를 입력받아서 뷰모델 생성 및 연결
        [SerializeField, Range(8, 64)] int _initialCapacity = 32;
        [SerializeField, Range(8, 64)] int _maxCapacity = 64;
        [SerializeField] InventoryUGUIView _inventoryView;
        
        [Header("아이템 목록")]
        [SerializeField] ItemData[] _itemDatabase;

        private InventoryUGUIViewModel _viewModel;
        
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
            _viewModel = new InventoryUGUIViewModel(_inventoryModel);
            _inventoryView.Bind(_viewModel);

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
                // 동적으로 할당된 C# 모델 객체가 실재할 때만 안전하게 데이터가 가동되도록 보호합니다.
                if (_inventoryModel != null && _itemDatabase != null && _itemDatabase.Length > 0)
                {
                    int randomIndex = Random.Range(0, _itemDatabase.Length);
                    ItemData randomItem = _itemDatabase[randomIndex];
                    
                    int amount = randomItem is CountableItemData ? Random.Range(5, 20) : 1;
                    _inventoryModel.Add(randomItem, amount);
                }
            }
        }

        private void OnDestroy()
        {
            if (_viewModel != null) _viewModel.UnbindEvents();
        }
    }
}