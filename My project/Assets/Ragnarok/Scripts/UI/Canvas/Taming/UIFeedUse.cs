using Ragnarok.View;
using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIFeedUse : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UISwipe swipe;
        [SerializeField] UITextureHelper iconFeedItem;

        public event Action OnUseFeed;

        protected override void OnInit()
        {
            swipe.OnSwipe += OnSwipe;
        }

        protected override void OnClose()
        {
            swipe.OnSwipe -= OnSwipe;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        private void OnSwipe(UISwipe.SwipeType type)
        {
            if (type == UISwipe.SwipeType.Up)
            {
                OnUseFeed?.Invoke();
            }
        }

        public void SetIcon(string iconName)
        {
            iconFeedItem.Set(iconName);
        }
    }
}