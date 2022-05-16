using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIConsumableInfoSlot : UIInfo<InvenPresenter, ItemInfo>
    {
        [SerializeField] UIConsumableProfile consumalbelProfile;
        [SerializeField] UIButtonHelper btnShowInfo;
        [SerializeField] UISprite coolDown;
        Action<ItemInfo> onClickevent;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnShowInfo.OnClick, OnClickedBtnShowInfo);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnShowInfo.OnClick, OnClickedBtnShowInfo);
            Timing.KillCoroutines(gameObject);
        }

        public override void SetData(ItemInfo info)
        {
            base.SetData(info);
            consumalbelProfile.SetData(info);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(DoCoolDown(), gameObject);
        }

        private void OnEnable()
        {
            if (IsInvalid())
                return;

            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(DoCoolDown(), gameObject);
        }

        public void OnClickedEvent(Action<ItemInfo> onClickevent)
        {
            this.onClickevent = onClickevent;
        }

        /// <summary>
        /// 아이템 정보 보기 버튼 클릭
        /// </summary>
        void OnClickedBtnShowInfo()
        {
            if (onClickevent == null)
            {
                UI.Show<UIConsumableInfo>(info);
                if (info.IsNew && presenter != null)
                {
                    presenter.HideNew(info);
                    consumalbelProfile.SetData(info);
                }
            }
            else
            {
                onClickevent?.Invoke(info);
            }
        }

        IEnumerator<float> DoCoolDown()
        {
            while (info.IsCooldown())
            {
                //Debug.Log($"{info.RemainCoolDown} = {info.CoolDown}");    
                if (coolDown)
                    coolDown.fillAmount = Mathf.Lerp(0, 1, info.RemainCooldown / info.Cooldown);
                yield return Timing.WaitForOneFrame;
            }
            if (coolDown)
                coolDown.fillAmount = 0;
        }
    }
}