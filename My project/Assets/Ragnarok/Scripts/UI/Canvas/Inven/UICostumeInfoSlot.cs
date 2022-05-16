using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class UICostumeInfoSlot : UIView
    {
        public interface Impl
        {
            void OnSelect(ItemInfo item);
            bool IsDisassemble(long itemNo);
            TweenAlpha TweenAlpha { get; }
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

            public void OnSelect()
            {
                impl.OnSelect(item);
                InvokeUpdateEvent();
            }

            public bool IsDisassemble()
            {
                return impl.IsDisassemble(item.ItemNo);
            }

            public TweenAlpha GetTweenAlpha()
            {
                return impl.TweenAlpha;
            }

            public void InvokeUpdateEvent()
            {
                OnUpdateEvent?.Invoke();
            }           
        }

        [SerializeField] UICostumeProfile costumeProfile;
        [SerializeField] UIButtonHelper btnSelect;

        protected Info info;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveEvent();
            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnLocalize()
        {            
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

        public void Set(Info info)
        {
            RemoveEvent();
            this.info = info;
            AddEvent();
            Refresh();
        }

        private void OnClickedBtnSelect()
        {
            if (IsInvalid())
                return;

            info.OnSelect();
        }

        protected virtual void Refresh()
        {
            if(IsInvalid())
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            costumeProfile.Set(info.item);
        }

        bool IsInvalid()
        {
            return info == null || info.item == null;
        }    
    }
}