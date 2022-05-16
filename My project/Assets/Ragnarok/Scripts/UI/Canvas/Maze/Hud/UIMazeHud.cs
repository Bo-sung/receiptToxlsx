using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMazeHud : UICanvas
    {
        protected override UIType uiType => UIType.Hide;

        [SerializeField] UIWidget anchors;
        [SerializeField] UIButtonHelper btnClick;

        public Action OnClicked;

        protected override void OnInit()
        {
            EventDelegate.Add(btnClick.OnClick, OnClickedBtnClick);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClick.OnClick, OnClickedBtnClick);
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

        public void ShowHud(GameObject target)
        {
            Show();
            anchors.SetAnchor(target, 0, 0, 0, 0);
        }

        void OnClickedBtnClick()
        {
            OnClicked?.Invoke();
        }
    }
}