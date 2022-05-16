using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class SkillInfoView : SkillInfoPreview<SkillInfoView.IListener>, IAutoInspectorFinder, TutorialSkillLearn.ILevelUpSkillImpl
    {
        public interface IListener : ISkillViewListener
        {
            void OnLevelUp(int skillId, int plusLevel);
            void OnSelectChangeSkill(int skillId);
        }

        [SerializeField] UICostButtonHelper btnLevelUpSkill;
        [SerializeField] UIButtonHelper btnSelectSkill;
        [SerializeField] UILabelHelper labelNeedSkillPoint;

        private int skillPoint;
        private int needSkillPoint;

        protected override void Awake()
        {
            base.Awake();

            if (btnLevelUpSkill)
                EventDelegate.Add(btnLevelUpSkill.OnClick, OnClickedBtnLevelUpSkill);

            if (btnSelectSkill)
                EventDelegate.Add(btnSelectSkill.OnClick, OnClickedBtnSelectSkill);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (btnLevelUpSkill)
                EventDelegate.Remove(btnLevelUpSkill.OnClick, OnClickedBtnLevelUpSkill);

            if (btnSelectSkill)
                EventDelegate.Remove(btnSelectSkill.OnClick, OnClickedBtnSelectSkill);
        }

        void OnClickedBtnLevelUpSkill()
        {
            if (info == null)
                return;

            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnLevelUp(info.GetSkillId(), plusLevel);
            }

            isClickedBtnLevelUpSkill = true;
        }

        void OnClickedBtnSelectSkill()
        {
            if (info == null)
                return;

            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnSelectChangeSkill(info.GetSkillId());
            }
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            if (btnLevelUpSkill != null)
            {
                btnLevelUpSkill.LocalKey = LocalizeKey._39015; // 레벨업
            }

            if (btnSelectSkill != null)
            {
                btnSelectSkill.LocalKey = LocalizeKey._39100; // 스킬 선택
            }

            if (labelNeedSkillPoint != null)
            {
                labelNeedSkillPoint.LocalKey = LocalizeKey._39014; // 필요 포인트
            }
        }

        public void SetSkillPoint(int skillPoint)
        {
            this.skillPoint = skillPoint;

            RefreshBtnLevelUpSkill(); // 레벨업 버튼 새로고침
        }

        protected override void RefreshLevel()
        {
            base.RefreshLevel();

            if (info == null)
                return;

            needSkillPoint = info.GetSkillLevelNeedPoint(plusLevel);

            bool isLevelUpSkill = currentData.SkillType == SkillType.Active || currentData.SkillType == SkillType.Passive || currentData.SkillType == SkillType.BasicActiveSkill;
            if (btnLevelUpSkill != null)
            {
                btnLevelUpSkill.CostText = needSkillPoint.ToString();
                btnLevelUpSkill.SetActive(isLevelUpSkill);
            }

            if (btnSelectSkill != null)
                btnSelectSkill.SetActive(!isLevelUpSkill);

            RefreshBtnLevelUpSkill(); // 레벨업 버튼 새로고침
        }

        private void RefreshBtnLevelUpSkill()
        {
            if (btnLevelUpSkill != null)
                btnLevelUpSkill.IsEnabled = !isMaxLevelSkill && (needSkillPoint <= skillPoint); // 최대 레벨이 아니며, 스킬포인트가 넉넉한 경우
        }

        #region Tutorial
        bool isClickedBtnLevelUpSkill;

        UIWidget TutorialSkillLearn.ILevelUpSkillImpl.GetBtnLevelUpSkill()
        {
            return btnLevelUpSkill.GetComponent<UIWidget>();
        }

        bool TutorialSkillLearn.ILevelUpSkillImpl.IsClickedBtnLevelUpSkill()
        {
            if (isClickedBtnLevelUpSkill)
            {
                isClickedBtnLevelUpSkill = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}