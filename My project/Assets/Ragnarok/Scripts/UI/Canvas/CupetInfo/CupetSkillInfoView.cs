using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public class CupetSkillInfoView : SkillInfoView
    {
        [SerializeField] UILabelHelper labelSkillLevelUpCondition; // 스킬 레벨업 조건 라벨 (큐펫스킬에 쓰임)
        [SerializeField] UIGridHelper rate; // 스킬 레벨업 조건 그리드 (큐펫스킬에 쓰임)

        public override void Refresh()
        {
            base.Refresh();

            if (info == null)
                return;

            int needRank = info.GetSkillLevelUpNeedRank();
            if (needRank == 0)
            {
                labelSkillLevelUpCondition.Text = string.Empty;
                rate.SetValue(needRank);
            }
            else
            {
                if (info.HasSkill(default))
                {
                    labelSkillLevelUpCondition.Text = LocalizeKey._19037.ToText(); // 레벨 업 조건
                }
                else
                {
                    labelSkillLevelUpCondition.Text = LocalizeKey._19038.ToText(); // 해금 조건
                }

                rate.SetValue(needRank);
            }
        }
    }
}