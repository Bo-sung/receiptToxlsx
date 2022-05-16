using UnityEngine;

namespace Ragnarok
{
    public class GoodsPoolObject : PlayTweenPoolObject
    {
        [SerializeField] UISprite icon;

        public override PlayTweenPoolObject Initialize(string itemIcon)
        {
            icon.spriteName = itemIcon;
            return this;
        }

        public override PlayTweenPoolObject Initialize(string itemIcon, int itemCount)
        {
            throw new System.NotImplementedException();
        }
    }
}