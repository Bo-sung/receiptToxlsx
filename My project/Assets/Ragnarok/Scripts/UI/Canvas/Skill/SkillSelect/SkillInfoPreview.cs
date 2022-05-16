using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class SkillInfoPreview : SkillInfoPreview<ISkillViewListener>
    {
    }

    public class SkillInfoPreview<T> : BaseSkillInfoView<T>, IAutoInspectorFinder
        where T : ISkillViewListener
    {
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UIPressButton btnMinus;
        [SerializeField] UIPressButton btnPlus;

        protected int plusLevel;
        protected bool isMaxLevelSkill;

        protected override void Awake()
        {
            base.Awake();

            if (btnMinus)
                EventDelegate.Add(btnMinus.onClick, OnClickedMinus);

            if (btnPlus)
                EventDelegate.Add(btnPlus.onClick, OnClickedPlus);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (btnMinus)
                EventDelegate.Remove(btnMinus.onClick, OnClickedMinus);

            if (btnPlus)
                EventDelegate.Remove(btnPlus.onClick, OnClickedPlus);
        }

        void OnClickedMinus()
        {
            --plusLevel;

            if (plusLevel <= 0) // 음수가 된 경우 최대치로.
            {
                while (info.HasSkill(currentLevel + plusLevel + 1))
                {
                    ++plusLevel;
                }
            }

            RefreshLevel();
        }

        void OnClickedPlus()
        {
            ++plusLevel;

            if (!info.HasSkill(currentLevel + plusLevel)) // 최대치를 넘어간 경우 1로.
            {
                plusLevel = 1;
            }

            RefreshLevel();
        }

        public override void Show(ISkillViewInfo info)
        {
            plusLevel = 1; // 미리보기 레벨 초기화

            base.Show(info);
        }

        protected override void RefreshLevel()
        {
            base.RefreshLevel();

            int previewLevel = currentLevel + plusLevel; // 미리 볼 스킬 레벨
            SkillData.ISkillData previewData = info.GetSkillData(previewLevel);
            isMaxLevelSkill = previewData == null; // 다음 스킬이 존재하지 않음

            if (btnMinus != null)
                btnMinus.isEnabled = !isMaxLevelSkill;

            if (btnPlus != null)
                btnPlus.isEnabled = !isMaxLevelSkill;

            if (isMaxLevelSkill)
            {
                if (labelCount != null)
                    labelCount.Text = LocalizeKey._39016.ToText(); // MAX
            }
            else
            {
                if (labelCount != null)
                    labelCount.Text = plusLevel.ToString();

                ShowSkill(currentData, previewData, previewLevel);
            }
        }
    }
}