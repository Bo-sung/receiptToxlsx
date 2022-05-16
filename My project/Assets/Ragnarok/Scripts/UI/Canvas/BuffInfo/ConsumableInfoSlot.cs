using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class ConsumableInfoSlot : UIView
    {
        public interface Impl
        {
            void OnSelect(ItemInfo item);
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

            public void InvokeUpdateEvent()
            {
                OnUpdateEvent?.Invoke();
            }
        }

        [SerializeField] UIConsumableProfile consumalbelProfile;
        [SerializeField] UIButtonHelper btnShowInfo;
        [SerializeField] UISprite coolDown;

        Info info;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnShowInfo.OnClick, OnClickedBtnShowInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveEvent();
            EventDelegate.Remove(btnShowInfo.OnClick, OnClickedBtnShowInfo);
            Timing.KillCoroutines(gameObject);
        }

        private void OnEnable()
        {
            if (info is null)
                return;

            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(DoCoolDown(), gameObject);
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

        void Refresh()
        {
            consumalbelProfile.SetData(info.item);
            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(DoCoolDown(), gameObject);
        }

        IEnumerator<float> DoCoolDown()
        {
            while (info.item.IsCooldown())
            {
                if (coolDown)
                    coolDown.fillAmount = MathUtils.GetProgress(info.item.RemainCooldown, info.item.Cooldown);
                yield return Timing.WaitForOneFrame;
            }
            if (coolDown)
                coolDown.fillAmount = 0;
        }

        void OnClickedBtnShowInfo()
        {
            info.OnSelect();
        }
    }
}