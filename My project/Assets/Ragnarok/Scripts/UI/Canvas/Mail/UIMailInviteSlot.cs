using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class UIMailInviteSlot : UIView
    {
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnInvite;

        System.Action onInvite; 

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnInvite.OnClick, OnClickedBtnInvite);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnInvite.OnClick, OnClickedBtnInvite);
        }

        protected override void OnLocalize()
        {
            labelDesc.LocalKey = LocalizeKey._6600; // 친구를 초대하고, 특별한 보상을 받아가세요!
            btnInvite.LocalKey = LocalizeKey._6601; // 초대하기
        }

        public void SetData(System.Action onInvite)
        {
            this.onInvite = onInvite;
        }

        private void OnClickedBtnInvite()
        {
            onInvite?.Invoke();
        }
    }
}