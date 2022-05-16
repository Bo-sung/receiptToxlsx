using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class EmperiumDonationView : UIView
    {
        [SerializeField] UIButtonWithIconValueHelper btnDonation;
        [SerializeField] UILabelHelper labelTitleDonation;
        [SerializeField] UIButtonHelper btnHelp;
        [SerializeField] UILabelHelper labelOwned;

        public event Action OnSelect;
        public event Action OnSelectHelp;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnDonation.OnClick, OnClickedBtnDonation);
            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnDonation.OnClick, OnClickedBtnDonation);
            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnLocalize()
        {
            btnDonation.LocalKey = LocalizeKey._38410; // 기부
            labelTitleDonation.LocalKey = LocalizeKey._38409; // 엠펠리움 기부
        }

        void OnClickedBtnDonation()
        {
            OnSelect?.Invoke();
        }

        void OnClickedBtnHelp()
        {
            OnSelectHelp?.Invoke();
        }

        public void SetOwnedCount(int count)
        {
            labelOwned.Text = count.ToString();
        }

        public void SetItem(string iconName, int count)
        {
            btnDonation.SetIconName(iconName);
            btnDonation.SetValue(count.ToString());
        }
    }
}