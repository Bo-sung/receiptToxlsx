using UnityEngine;

namespace Ragnarok.View
{
    public class CardSmeltView : UIView
    {
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnSmelt;
        [SerializeField] UIButtonHelper btnContinuousSmelt;
        [SerializeField] UIButtonHelper btnAutoSaveRestorePoint;
        [SerializeField] GameObject[] btnAutoSaveRestorePointRoots;

        public event System.Action OnCancel;
        public event System.Action OnSmelt;
        public event System.Action OnContinuousSmelt;
        public event System.Action<bool> OnAutoSaveRestorePointToggle;
        private bool curAutoSaveToggleState;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnSmelt.OnClick, OnClickedBtnSmelt);
            EventDelegate.Add(btnContinuousSmelt.OnClick, OnClickedBtnContinuousSmelt);
            EventDelegate.Add(btnAutoSaveRestorePoint.OnClick, OnClickedBtnAutoSaveRestorePoint);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnSmelt.OnClick, OnClickedBtnSmelt);
            EventDelegate.Remove(btnContinuousSmelt.OnClick, OnClickedBtnContinuousSmelt);
            EventDelegate.Remove(btnAutoSaveRestorePoint.OnClick, OnClickedBtnAutoSaveRestorePoint);
        }

        protected override void OnLocalize()
        {
            btnSmelt.LocalKey = LocalizeKey._18504; // 제련
            btnCancel.LocalKey = LocalizeKey._18503; // 나가기
            btnContinuousSmelt.LocalKey = LocalizeKey._18507; // 연속 제련
            btnAutoSaveRestorePoint.LocalKey = LocalizeKey._18522;
        }

        public void SetCanSmelt(bool canSmelt)
        {
            btnSmelt.IsEnabled = canSmelt;
            btnContinuousSmelt.IsEnabled = canSmelt;
        }

        public void SetAutoSaveRestorePoint(bool value)
        {
            curAutoSaveToggleState = value;
            btnAutoSaveRestorePointRoots[0].SetActive(value);
            btnAutoSaveRestorePointRoots[1].SetActive(!value);
        }

        void OnClickedBtnCancel()
        {
            OnCancel?.Invoke();
        }

        void OnClickedBtnSmelt()
        {
            OnSmelt?.Invoke();
        }

        void OnClickedBtnContinuousSmelt()
        {
            OnContinuousSmelt?.Invoke();
        }

        void OnClickedBtnAutoSaveRestorePoint()
        {
            bool newValue = !curAutoSaveToggleState;
            SetAutoSaveRestorePoint(newValue);
            OnAutoSaveRestorePointToggle?.Invoke(newValue);
        }
    }
}