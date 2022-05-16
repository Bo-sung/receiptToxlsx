using UnityEngine;

namespace Ragnarok
{
    public sealed class SimpleRewardItemPacket : IPacket<Response>, UIRewardItem.IInput
    {
        public int item_id;
        public int count;

        // 클라이언트 전용
        private string itemIcon;
        private int weight;

        void IInitializable<Response>.Initialize(Response response)
        {
            item_id = response.GetInt("1");
            count = response.GetInt("2");
        }

        public void Initialize(ItemDataManager.IItemDataRepoImpl itemDataRepoImpl)
        {
            ItemData itemData = itemDataRepoImpl.Get(item_id);
            if (itemData == null)
            {
                itemIcon = string.Empty;
                weight = 0;
#if UNITY_EDITOR
                Debug.LogError($"itemData is Null: {nameof(item_id)} = {item_id}");
#endif
            }
            else
            {
                itemIcon = itemData.icon_name;
                weight = itemData.weight;
            }
        }

        public string GetItemIcon()
        {
            return itemIcon;
        }

        public int GetItemCount()
        {
            return count;
        }

        public int GetWeight()
        {
            return weight;
        }
    }
}