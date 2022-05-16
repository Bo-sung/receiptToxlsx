using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobReward : UICanvas
    {
        public class Input : IUIData
        {
            public Job job;
        }

        public static float attackPowerInfoDelay = 0f;

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UITextureHelper itemIcon;
        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UIGrid grid;
        [SerializeField] UISkillInfo[] skillInfos;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnShortcut;
        [SerializeField] UIButtonHelper btnConfirm;

        JobRewardPresenter presenter;

        protected override void OnInit()
        {
            presenter = new JobRewardPresenter();

            EventDelegate.Add(btnShortcut.OnClick, OnClickedBtnShortcut);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnShortcut.OnClick, OnClickedBtnShortcut);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);

            // 평가 UI 띄우기
            if (presenter.IsShowReviewPopup())
                BuildSettings.Instance.ShowReviewPopup();
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is Input input)
            {
                presenter.SetJob(input.job);
                Refresh();

                SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess);
            }
            else
            {
                CloseUI();
                return;
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._30500; // 전직 보상
            btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
            labelDesc.LocalKey = LocalizeKey._30507; // 새로운 스킬을 배울 수 있게 되어\n기존의 스킬 트리를 초기화 합니다.
            btnShortcut.LocalKey = LocalizeKey._90114; // [스킬 장착 바로가기]
        }

        void OnClickedBtnShortcut()
        {
            UI.Show<UISkill>();
            CloseUI();
        }

        private void Refresh()
        {
            (string iconName, int itemNameId) = presenter.GetJobRewardInfo();
            UISkillInfo.IInfo[] skills = presenter.GetSkills();

            itemIcon.Set(iconName, isAsync: false); // 연출에 의한 꺼짐 방지
            labelItemName.LocalKey = itemNameId;

            int skillCount = skills == null ? 0 : skills.Length;
            for (int i = 0; i < skillInfos.Length; i++)
            {
                skillInfos[i].Show(i < skillCount ? skills[i] : null);
            }

            grid.Reposition();
        }

        private void CloseUI()
        {
            UI.Close<UIJobReward>();
        }

        public override bool Find()
        {
            base.Find();

            skillInfos = GetComponentsInChildren<UISkillInfo>();
            return true;
        }
    }
}