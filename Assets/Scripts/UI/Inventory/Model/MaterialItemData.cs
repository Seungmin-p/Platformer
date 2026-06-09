using UnityEngine;

namespace MyInventory
{
    [CreateAssetMenu(fileName = "Item_Material_", menuName = "Inventory System/Item Data/Material", order = 4)]
    public class MaterialItemData : CountableItemData
    {
        public override Item CreateItem()
        {
            return new MaterialItem(this);
        }
    }
}