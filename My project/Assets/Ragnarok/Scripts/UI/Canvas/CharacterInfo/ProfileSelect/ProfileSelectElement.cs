using UnityEngine;

namespace Ragnarok.View
{
    public class ProfileSelectElement : UIElement<ProfileSelectElement.IInput>
    {
        public interface IInput
        {
            int Id { get; }
            string ProfileName { get; }
            int NeedMileage { get; }
        }

        [SerializeField] UITextureHelper profile;
        [SerializeField] UIButtonHelper btnSelect;
        [SerializeField] GameObject goSelect;
        [SerializeField] GameObject goLock;
        [SerializeField] UILabelHelper labelNeedMileage;

        public event System.Action<int> OnSelect;

        private int myMileage;
        private int selectedProfileId;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            if (IsLock())
            {
                string message = LocalizeKey._4062.ToText(); // 마일리지가 부족합니다.
                UI.ShowToastPopup(message);
                return;
            }

            OnSelect?.Invoke(info.Id);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            profile.Set(info.ProfileName, isAsync: false);
            labelNeedMileage.Text = info.NeedMileage.ToString("N0");
            RefreshSelect();
            RefreshLock();
        }

        public void Initialize(int myMileage)
        {
            this.myMileage = myMileage;
            RefreshLock();
        }

        public void SetSelectedProfileId(int selectedProfileId)
        {
            this.selectedProfileId = selectedProfileId;
            RefreshSelect();
        }

        private void RefreshSelect()
        {
            int id = info == null ? 0 : info.Id;
            NGUITools.SetActive(goSelect, id == selectedProfileId);
        }

        private void RefreshLock()
        {
            NGUITools.SetActive(goLock, IsLock());
        }

        private bool IsLock()
        {
            int needMileage = info == null ? 0 : info.NeedMileage;
            return myMileage < needMileage;
        }
    }
}