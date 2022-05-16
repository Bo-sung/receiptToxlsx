using UnityEngine;

namespace Ragnarok.View
{
    public class ProfileSelectView : UIView
    {
        [SerializeField] UILabelValue mileage;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] ProfileSelectElement element;

        private SuperWrapContent<ProfileSelectElement, ProfileSelectElement.IInput> wrapContent;
        private int curMileage;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<ProfileSelectElement, ProfileSelectElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectElement;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectElement;
            }
        }

        protected override void OnLocalize()
        {
            mileage.TitleKey = LocalizeKey._4060; // 내 마일리지
            UpdateMyMileage();
        }

        void OnSelectElement(int id)
        {
            OnSelect?.Invoke(id);
        }

        public void Initialize(int curMileage)
        {
            this.curMileage = curMileage;

            foreach (var item in wrapContent)
            {
                item.Initialize(this.curMileage);
            }

            UpdateMyMileage();
        }

        public void SetData(ProfileSelectElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        public void Refresh(ProfileSelectElement.IInput selected)
        {
            // 현재 선택한 보상으로 세팅
            int selectedProfileId = selected.Id;
            foreach (var item in wrapContent)
            {
                item.SetSelectedProfileId(selectedProfileId);
            }
        }

        private void UpdateMyMileage()
        {
            mileage.Value = curMileage.ToString("N0");
        }
    }
}