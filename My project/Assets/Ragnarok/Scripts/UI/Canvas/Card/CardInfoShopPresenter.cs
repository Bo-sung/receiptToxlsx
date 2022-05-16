using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class CardInfoShopPresenter : ViewPresenter
    {
        public override void AddEvent()
        {
            
        }

        public override void RemoveEvent()
        {
            
        }

        public ItemInfo Item { get; private set; }

        public void Set(ItemInfo info)
        {
            Item = info;
        }
    }
}