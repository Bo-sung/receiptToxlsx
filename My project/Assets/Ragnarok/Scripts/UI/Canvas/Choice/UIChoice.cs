using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIChoice : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnSelect1;
        [SerializeField] UIButtonHelper btnSelect2;
        [SerializeField] UIButtonHelper btnSelect3;

        private bool isUpdate;

        public event Action<int> OnChoice;

        protected override void OnInit()
        {
            EventDelegate.Add(btnSelect1.OnClick, OnClickedBtnSelect1);
            EventDelegate.Add(btnSelect2.OnClick, OnClickedBtnSelect2);
            EventDelegate.Add(btnSelect3.OnClick, OnClickedBtnSelect3);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnSelect1.OnClick, OnClickedBtnSelect1);
            EventDelegate.Remove(btnSelect2.OnClick, OnClickedBtnSelect2);
            EventDelegate.Remove(btnSelect3.OnClick, OnClickedBtnSelect3);
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

        public void SetTitle(string text)
        {
            labelTitle.Text = text;
        }

        public void Set(string text1, string text2)
        {
            btnSelect1.Text = text1;
            btnSelect2.Text = text2;
            btnSelect3.SetActive(false);
        }

        public void Set(string text1, string text2, string text3, bool isUpdate = false)
        {
            btnSelect1.Text = text1;
            btnSelect2.Text = text2;
            btnSelect3.Text = text3;
            btnSelect3.SetActive(true);
            this.isUpdate = isUpdate;
        }

        void OnClickedBtnSelect1()
        {
            OnChoice?.Invoke(1);
            Hide();
        }

        void OnClickedBtnSelect2()
        {
            OnChoice?.Invoke(2);
            Hide();
        }

        void OnClickedBtnSelect3()
        {
            if(isUpdate)
            {
                UI.ShowToastPopup(LocalizeKey._90045.ToText()); // 업데이트 예정입니다.
                return;
            }
            OnChoice?.Invoke(3);
            Hide();
        }
    }
}