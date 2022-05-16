using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class UICardSmeltMaterialSelectSlot : UIView, IAutoInspectorFinder
    {
        public interface Impl
        {
            void OnSelect(ItemInfo item);
            bool IsSelect(ItemInfo item);
        }

        public class Info
        {
            public ItemInfo item { get; private set; }
            private readonly Impl impl;

            public Info(ItemInfo item, Impl impl)
            {
                this.item = item;
                this.impl = impl;
            }

            public event Action OnUpdateEvent;

            public bool IsSelect()
            {
                return impl.IsSelect(item);
            }

            public void OnSelect()
            {
                impl.OnSelect(item);
                InvokeUpdateEvent();
            }

            public void InvokeUpdateEvent()
            {
                OnUpdateEvent?.Invoke();
            }
        }

        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] GameObject select;

        Info info;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveEvent();
        }

        protected override void OnLocalize()
        {
        }

        public void Set(Info info)
        {
            RemoveEvent();
            this.info = info;
            AddEvent();
            Refresh();
        }

        void Refresh()
        {
            icon.Set(info.item.IconName);
            labelCount.Text = info.item.ItemCount.ToString();
            SetSelect(info.IsSelect());
        }

        public void SetSelect(bool isSelect)
        {
            select.SetActive(isSelect);
        }

        void OnClick()
        {
            info.OnSelect();
        }

        void AddEvent()
        {
            if (info == null)
                return;

            info.OnUpdateEvent += Refresh;
        }

        void RemoveEvent()
        {
            if (info is null)
                return;

            info.OnUpdateEvent -= Refresh;
        }
    }
}