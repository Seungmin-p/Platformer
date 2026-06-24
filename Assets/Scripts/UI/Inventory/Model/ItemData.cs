using UnityEngine;

namespace MyInventory
{
    public abstract class ItemData : ScriptableObject
    {
        [Header("기본 아이템 데이터")]
        [SerializeField] int _id;
        [SerializeField] string _name;    // 아이템 이름
        [SerializeField, TextArea(3, 5)] string _tooltip; // 아이템 설명
        [SerializeField] Sprite _iconSprite; // 아이템 아이콘
    
        // [Header("드롭 데이터")]
        // [SerializeField] GameObject _dropItemPrefab; // 바닥에 떨어질 때 생성할 프리팹
    
        public int ID => _id;
        public string Name => _name;
        public string Tooltip => _tooltip;
        public Sprite IconSprite => _iconSprite;
        // public GameObject DropItemPrefab => _dropItemPrefab;

        /// <summary> 타입에 맞는 새로운 아이템 생성 </summary>
        public abstract Item CreateItem();
    }
}