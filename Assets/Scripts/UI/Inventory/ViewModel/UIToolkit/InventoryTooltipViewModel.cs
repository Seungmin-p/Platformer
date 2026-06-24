using UnityEngine;
using Unity.Properties;

namespace MyInventory.UIToolkit
{
    public class InventoryTooltipViewModel
    {
        private ItemData _itemDataModel; //주입받은 아이템 데이터 모델

        //팝업 상단 아이템 이름 바인딩
        [CreateProperty] public string NameText => _itemDataModel != null ? _itemDataModel.Name : string.Empty;
        [CreateProperty] public string DescText => _itemDataModel != null ? _itemDataModel.Tooltip : string.Empty;

        //팝업이 열릴 때 데이터를 세팅하는 메소드
        public void Setup(ItemData itemData)
        {
            _itemDataModel = itemData;
        }

        public void Clear()
        {
            _itemDataModel = null;
        }
    }
}