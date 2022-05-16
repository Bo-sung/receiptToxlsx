using UnityEngine;

namespace Ragnarok.View
{
    public class SelectPopupView : PopupView
    {
        [SerializeField] UIButtonHelper btnCancel;

        int cancelLocalKey;

        public int CancelLocalKey
        {
            set
            {
                cancelLocalKey = value;
                UpdateBtnCancelText();
            }
        }

        public event System.Action OnCancel;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            UpdateBtnCancelText();
        }

        protected virtual void OnClickedBtnCancel()
        {
            OnCancel?.Invoke();
        }

        private void UpdateBtnCancelText()
        {
            if (cancelLocalKey > 0)
                btnCancel.LocalKey = cancelLocalKey;
        }
    }
}