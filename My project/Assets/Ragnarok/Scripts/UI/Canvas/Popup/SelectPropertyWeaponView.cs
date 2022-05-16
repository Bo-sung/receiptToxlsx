using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class SelectPropertyWeaponView : UIView
    {
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        //[SerializeField] UISprite iconProperty; // 아이콘 고정으로 표시..

        public event System.Action HideUI;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.OnClick, OnHideUI);
            EventDelegate.Add(btnConfirm.OnClick, OnHideUI);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.OnClick, OnHideUI);
            EventDelegate.Remove(btnConfirm.OnClick, OnHideUI);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._5204;// 무기 속성
            labelDescription.LocalKey = LocalizeKey._5205; // [62AEE4][C]무기 속성[/c][-]은 무기에 적용된 속성이 적용됩니다.
        }
        
        private void OnHideUI()
        {
            HideUI?.Invoke();
        }
    }
}