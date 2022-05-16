using UnityEngine;

namespace Ragnarok.View
{
    public class PopupView : SimplePopupView
    {
        [SerializeField] UIButtonHelper btnConfirm;

        int confirmLocalKey;

        public int ConfirmLocalKey
        {
            set
            {
                confirmLocalKey = value;
                UpdateBtnConfirmText();
            }
        }

        public event System.Action OnConfirm;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            UpdateBtnConfirmText();
        }

        protected virtual void OnClickedBtnConfirm()
        {
            OnConfirm?.Invoke();
        }

        public void SetActiveConfirm(bool isActive)
        {
            btnConfirm.SetActive(isActive);
        }

        private void UpdateBtnConfirmText()
        {
            if (confirmLocalKey > 0)
                btnConfirm.LocalKey = confirmLocalKey;
        }

        public void SetIsEnabledBtnConfirm(bool value)
        {
            btnConfirm.IsEnabled = value;
        }

        #region Tutorial
        public UIWidget GetBtnConfirm()
        {
            return btnConfirm.GetComponent<UIWidget>();
        }
        #endregion
    }
}