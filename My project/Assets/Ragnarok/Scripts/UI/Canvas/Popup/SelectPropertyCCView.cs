using UnityEngine;

namespace Ragnarok.View
{
    public class SelectPropertyCCView : UIView
    {
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UISprite iconProperty;

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
            labelTitle.LocalKey = LocalizeKey._5206;// 상태 이상 정보
        }

        public void ShowPropertyDesc(CrowdControlType type)
        {
            iconProperty.spriteName = type.GetIconName();
            labelDescription.Text = type.ToDescText(); // CC 상태에 빠지면....
        }

        private void OnHideUI()
        {
            HideUI?.Invoke();
        }
    }
}