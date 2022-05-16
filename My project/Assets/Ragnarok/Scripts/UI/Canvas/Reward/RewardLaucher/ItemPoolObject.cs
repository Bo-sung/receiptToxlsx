using UnityEngine;

namespace Ragnarok
{
    public class ItemPoolObject : PlayTweenPoolObject
    {
        [SerializeField] UIRewardItem item;

        public override PlayTweenPoolObject Initialize(string itemIcon)
        {
            throw new System.NotImplementedException();
        }

        public override PlayTweenPoolObject Initialize(string itemIcon, int itemCount)
        {
            item.SetData(itemIcon, itemCount);
            return this;
        }
    }
}