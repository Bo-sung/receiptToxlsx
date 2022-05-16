using UnityEngine;

namespace Ragnarok.View.Skill
{
    public interface ISkillViewListener
    {
    }

    public interface ISkillViewInfo
    {
        int GetSkillId();
        int GetSkillLevel();
        bool HasSkill(int level);
        SkillData.ISkillData GetSkillData(int level);
        int GetSkillLevelNeedPoint(int plusLevel);
        int GetSkillLevelUpNeedRank(); // 큐펫 스킬 전용
    }

    public abstract class BaseSkillInfoView<T> : UIView<T>, IAutoInspectorFinder
        where T : ISkillViewListener
    {
        [SerializeField] UISkillInfo skillInfo;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UISkillType skillType;
        [SerializeField] UIButtonHelper iconElement;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelPreviewValue previewCooldown;
        [SerializeField] UILabelPreviewValue previewDuration;
        [SerializeField] UILabelPreviewValue previewRange;
        [SerializeField] UILabelPreviewValue previewMpReduce;
        [SerializeField] UIBattleOptionPreviewList battleOption;

        protected ISkillViewInfo info;

        protected int currentLevel;
        protected SkillData.ISkillData currentData;

        protected ElementType elementType;

        protected override void OnLocalize()
        {
            previewCooldown.TitleKey = LocalizeKey._39010; // 대기 시간
            previewDuration.TitleKey = LocalizeKey._39011; // 지속 시간
            previewRange.TitleKey = LocalizeKey._39012; // 사정 거리
            previewMpReduce.TitleKey = LocalizeKey._39022; // MP 소모
        }

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(iconElement.OnClick, ShowElementInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(iconElement.OnClick, ShowElementInfo);
        }

        public virtual void Show(ISkillViewInfo info)
        {
            this.info = info;

            Refresh();
        }

        public virtual void Refresh()
        {
            if (info == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            // 현재 정보 세팅
            currentLevel = info.GetSkillLevel();
            currentData = info.GetSkillData(Mathf.Max(currentLevel, 1)); // 배우지 않은 스킬의 경우 1레벨로 처리

            RefreshLevel();
        }

        protected virtual void RefreshLevel()
        {
            ShowSkill(currentData, currentLevel);
        }

        protected void ShowSkill(SkillData.ISkillData data, int showedLevel)
        {
            if (skillInfo != null)
                skillInfo.Show(data);

            elementType = data.GetSkillElementType();

            labelName.Text = LocalizeKey._39009.ToText() // LV.{LEVEL} {NAME}
                .Replace(ReplaceKey.LEVEL, showedLevel)
                .Replace(ReplaceKey.NAME, data.GetSkillName());

            skillType.Show(data.SkillType);

            iconElement.SpriteName = elementType.GetIconName();
            labelDescription.Text = data.GetSkillDescription();

            previewCooldown.Show(data.GetSkillCooldownTime());
            previewDuration.Show(data.GetSkillDuration());
            previewRange.Show(data.GetSkillRange());
            previewMpReduce.Show(data.GetSkillMPCost(), showPercentagePostfix: true);

            battleOption.Show(data.GetBattleOptions(), data.GetSkillBlowCount());
        }

        protected void ShowSkill(SkillData.ISkillData data1, SkillData.ISkillData data2, int showedLevel)
        {
            if (data2 == null)
            {
                ShowSkill(data1, showedLevel);
                return;
            }

            if (skillInfo != null)
                skillInfo.Show(data2);

            elementType = data2.GetSkillElementType();

            labelName.Text = LocalizeKey._39009.ToText() // LV.{LEVEL} {NAME}
                .Replace(ReplaceKey.LEVEL, showedLevel)
                .Replace(ReplaceKey.NAME, data2.GetSkillName());
            skillType.Show(data2.SkillType);
            iconElement.SpriteName = elementType.GetIconName();
            labelDescription.Text = data2.GetSkillDescription();

            previewCooldown.Show(data1.GetSkillCooldownTime(), data2.GetSkillCooldownTime());
            previewDuration.Show(data1.GetSkillDuration(), data2.GetSkillDuration());
            previewRange.Show(data1.GetSkillRange(), data2.GetSkillRange());
            previewMpReduce.Show(data1.GetSkillMPCost(), data2.GetSkillMPCost(), showPercentagePostfix: true);

            battleOption.Show(data1.GetBattleOptions(), data2.GetBattleOptions(), data1.GetSkillBlowCount(), data2.GetSkillBlowCount());
        }

        protected void ShowElementInfo()
        {
            UI.Show<UISelectPropertyPopup>().ShowElementView(elementType);
        }
    }
}