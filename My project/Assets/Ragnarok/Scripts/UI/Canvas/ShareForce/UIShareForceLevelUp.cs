using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIShareForceLevelUp : UICanvas, IInspectorFinder, TutorialShareVice2ndOpen.IShareForceLevelUpImpl
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UITextureHelper iconShareForce;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] ShareForceOptionSlot[] optionSlots;
        [SerializeField] UILabelHelper labelMaterial;
        [SerializeField] UIGrid gridMaterial;
        [SerializeField] ShareForceMaterialSlot[] slots;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIItemCostButtonHelper btnLevelUp;
        [SerializeField] GameObject fxLevelUp;
        [SerializeField] GameObject goLimitInfo;
        [SerializeField] UILabelHelper labelLimit;

        private ShareForceType type;

        private ShareForceLevelUpPresenter presenter;

        protected override void OnInit()
        {
            presenter = new ShareForceLevelUpPresenter();
            presenter.OnShareForceLevelUp += OnShareForceLevelUp;
            presenter.AddEvent();

            EventDelegate.Add(btnCancel.OnClick, OnBack);
            EventDelegate.Add(btnLevelUp.OnClick, OnClickedBtnLevelUp);
        }

        protected override void OnClose()
        {
            presenter.OnShareForceLevelUp -= OnShareForceLevelUp;
            presenter.RemoveEvent();

            EventDelegate.Remove(btnCancel.OnClick, OnBack);
            EventDelegate.Remove(btnLevelUp.OnClick, OnClickedBtnLevelUp);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
            fxLevelUp.SetActive(false);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._49600; // 쉐어포스 강화
            labelMaterial.LocalKey = LocalizeKey._49603; // 강화 재료
            btnCancel.LocalKey = LocalizeKey._49604; // 나가기
            btnLevelUp.LocalKey = LocalizeKey._49605; // 강화
        }

        public void Set(ShareForceType type)
        {
            this.type = type;
            UpdateView();
        }

        private void UpdateView()
        {
            UpdateProfile();
            UpdateOption();
            UpdateMaterial();
            UpdateBtnLevelUp();
        }

        private void UpdateProfile()
        {
            iconShareForce.SetItem(type.GetTextureName());
            labelLevel.Text = LocalizeKey._49601.ToText().Replace(ReplaceKey.LEVEL, presenter.GetLevel(type)); // Lv.{LEVEL}
        }

        private void UpdateOption()
        {
            int level = presenter.GetLevel(type);
            int nextLevel = level + 1;
            int shareForce = presenter.GetShareForce(level);
            int nextShareForce = presenter.GetShareForce(nextLevel);
            bool isMaxLevel = presenter.IsShareForceMaxLevel(type);
            string levelText = isMaxLevel ? string.Empty : LocalizeKey._49601.ToText().Replace(ReplaceKey.LEVEL, level);
            string nextLevelText = isMaxLevel ? string.Empty : LocalizeKey._49601.ToText().Replace(ReplaceKey.LEVEL, nextLevel);
            optionSlots[0].Set(type.GetNameId().ToText(), levelText, nextLevelText);
            optionSlots[1].Set(LocalizeKey._49602.ToText(), shareForce.ToString(), nextShareForce.ToString()); // 쉐어포스
        }

        private void UpdateMaterial()
        {
            bool isMaxLevel = presenter.IsShareForceMaxLevel(type);
            RewardData[] materials = presenter.GetMaterials(type);
            for (int i = 0; i < slots.Length; i++)
            {
                if (materials == null || isMaxLevel)
                {
                    slots[i].Set(null, 0);
                }
                else
                {
                    slots[i].Set(materials[i], presenter.GetItemCount(materials[i].ItemId));
                }
            }

            gridMaterial.repositionNow = true;
        }

        private void UpdateBtnLevelUp()
        {
            RewardData item = presenter.GetZeny(type);
            bool isMaxLevel = presenter.IsShareForceMaxLevel(type);
            if (item == null || isMaxLevel)
            {
                btnLevelUp.SetItemIcon(RewardType.Zeny.IconName());
                btnLevelUp.SetItemCount(0);
                btnLevelUp.IsEnabled = false;
                if (isMaxLevel)
                {
                    goLimitInfo.SetActive(true);
                    labelLimit.Text = LocalizeKey._90293.ToText(); // 최대 레벨에 도달하였습니다.
                }
            }
            else
            {
                btnLevelUp.SetItemIcon(item.IconName);
                btnLevelUp.SetItemCount(item.Count);

                int jobLevel = presenter.GetJobLevel();
                int needJobLevel = presenter.GetNeedJobLevel(type);
                bool isNeedJobLevel = jobLevel >= needJobLevel; // 필요 직업레벨 도달 여부
                btnLevelUp.IsEnabled = isNeedJobLevel;
                goLimitInfo.SetActive(!isNeedJobLevel);
                if (!isNeedJobLevel)
                {
                    labelLimit.Text = LocalizeKey._90282.ToText().Replace(ReplaceKey.LEVEL, needJobLevel); // JOB Lv이 부족하여 불가능합니다.(JOB Lv {LEVEL} 필요)
                }
            }
        }

        private void OnClickedBtnLevelUp()
        {
            presenter.RequestShareForceLevelUp(type);
        }

        /// <summary>
        /// 쉐어포스 레벨업
        /// </summary>
        private void OnShareForceLevelUp()
        {
            UpdateView();
            ShowEffect();
        }

        private void ShowEffect()
        {
            fxLevelUp.SetActive(false);
            fxLevelUp.SetActive(true);
            for (int i = 0; i < optionSlots.Length; i++)
            {
                optionSlots[i].ShowFx();
            }
        }

        UIWidget TutorialShareVice2ndOpen.IShareForceLevelUpImpl.GetBtnLevelUpWidget()
        {
            return btnLevelUp.GetComponent<UIWidget>();
        }

        public override bool Find()
        {
            optionSlots = GetComponentsInChildren<ShareForceOptionSlot>();
            slots = GetComponentsInChildren<ShareForceMaterialSlot>();
            return true;
        }
    }
}