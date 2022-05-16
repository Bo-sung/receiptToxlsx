using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPartsInfoSlot : UIInfo<InvenPresenter, ItemInfo>
    {
        [SerializeField] UIPartsProfile partsProfile;
        [SerializeField] UIButtonHelper btnShowInfo;
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
        }

        public override void SetData(ItemInfo info)
        {
            base.SetData( info);
            partsProfile.SetData(info);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;


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
                UI.Show<UIPartsInfo>(info);
                if(info.IsNew && presenter != null)
                {
                    presenter.HideNew(info);                   
                    partsProfile.SetData(info);
                }
            }
            else
            {
                onClickevent?.Invoke(info);
            }
        }
    }
}