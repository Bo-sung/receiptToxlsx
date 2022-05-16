using Ragnarok.Text;
using UnityEngine;

namespace Ragnarok
{
    public struct CardBattleOption
    {
        public readonly BattleOptionType battleOptionType;
        public readonly long nextMinValue;
        public readonly long nextMaxValue;
        public readonly long totalMinValue;
        public readonly long totalMaxValue;
        public readonly long serverValue;
        public readonly int rate1;
        public readonly int rate2;
        public readonly int value1;
        public readonly int value2;

        public CardBattleOption(BattleOptionType battleOptionType, long nextMinValue, long nextMaxValue, long totalMinValue, long totalMaxValue, long serverValue, int rate1, int rate2)
        {
            this.battleOptionType = battleOptionType;
            this.nextMinValue = nextMinValue;
            this.nextMaxValue = nextMaxValue;
            this.totalMinValue = totalMinValue;
            this.totalMaxValue = totalMaxValue;
            this.serverValue = serverValue;
            this.rate1 = rate1;
            this.rate2 = rate2;
            value1 = MathUtils.ToInt(MathUtils.ToPermyriadValue(rate1), 2);
            value2 = MathUtils.ToInt(MathUtils.ToPermyriadValue(rate2), 2);
        }

        public long GetValue()
        {
            return serverValue;
        }

        public string GetTitleText()
        {
            if (battleOptionType == BattleOptionType.None)
                return string.Empty;

            string titleText = battleOptionType.ToText();

            if (!battleOptionType.IsConditionalOption())
                return titleText;

            switch (battleOptionType)
            {
                // 상태이상 참조
                case BattleOptionType.CrowdControl:
                case BattleOptionType.CrowdControlRateResist:
                case BattleOptionType.BasicActiveSkillCrowdControlRate:
                    var cCName = value2.ToEnum<CrowdControlType>().ToText();
                    return titleText.Replace(ReplaceKey.LINK, cCName.ToLinkText(TextLinkType.CrowdControl, value2.ToEnum<CrowdControlType>().GetIconName()));

                // 속성 참조
                case BattleOptionType.ElementDmgRate:
                case BattleOptionType.ElementDmgRateResist:
                    return titleText.Replace(ReplaceKey.TYPE, value2.ToEnum<ElementType>().ToText());

                // 스킬 참조
                case BattleOptionType.SkillIdDmgRate:
                case BattleOptionType.FirePillar:
                case BattleOptionType.BasicActiveSkillRate:
                case BattleOptionType.ActiveSkill:
                    SkillData skillData = SkillDataManager.Instance.Get(value2, level: 1);
                    string skillName = skillData == null ? string.Empty : skillData.name_id.ToText();
                    return titleText.Replace(ReplaceKey.LINK, skillName.ToLinkText(TextLinkType.Skill, value2));

                // 몬스터 참조
                case BattleOptionType.Colleague:
                    MonsterData monsterData = MonsterDataManager.Instance.Get(value2);
                    string monsterName = monsterData == null ? string.Empty : monsterData.name_id.ToText();
                    return titleText.Replace(ReplaceKey.LINK, monsterName.ToLinkText(TextLinkType.Monster, value2));

                case BattleOptionType.SkillOverride:
                case BattleOptionType.SkillChain:
                    SkillData originalSkillData = SkillDataManager.Instance.Get(value1, level: 1);
                    string originalSkillName = originalSkillData == null ? string.Empty : originalSkillData.name_id.ToText();
                    return titleText.Replace(ReplaceKey.LINK, originalSkillName.ToLinkText(TextLinkType.Skill, value1));

                default:
                    Debug.LogError($"[정의되지 않은 옵션 이름 {nameof(BattleOptionType)}] {nameof(battleOptionType)} = {battleOptionType}");
                    return string.Empty;
            }
        }

        public string GetMaxValueText()
        {
            return GetValeText(totalMaxValue, isSymbol: true);
        }

        public string GetNextMinMaxText()
        {
            if (battleOptionType == BattleOptionType.None)
                return string.Empty;

            if (battleOptionType.IsConditionalSkill())
                return GetValueTextSkill();

            if (nextMinValue == 0 && nextMaxValue == 0)
                return string.Empty;

            if (nextMinValue == nextMaxValue)
            {
                return GetValeText(nextMinValue, isSymbol: false);
            }

            return $"{GetValeText(nextMinValue, isSymbol: false)}~{GetValeText(nextMaxValue, isSymbol: false)}";
        }

        /// <summary>
        /// 1레벨 아이템 획득시 얻을수 있는 수치 표시
        /// </summary>
        /// <returns></returns>
        public string GetTotalMinMaxText()
        {
            if (battleOptionType == BattleOptionType.None)
                return string.Empty;

            if (battleOptionType.IsConditionalSkill())
                return GetValueTextSkill();

            if (totalMinValue == totalMaxValue)
            {
                return GetValeText(totalMinValue, isSymbol: false);
            }

            return $"{GetValeText(totalMinValue, isSymbol: false)}~{GetValeText(totalMaxValue, isSymbol: false)}";
        }

        private string GetValueTextSkill()
        {
            SkillData replaceSkillData = SkillDataManager.Instance.Get(value2, level: 1);
            string replaceSkillName = replaceSkillData == null ? string.Empty : replaceSkillData.name_id.ToText();
            return replaceSkillName.ToLinkText(TextLinkType.Skill, value2);
        }

        public string GetValeText(long value, bool isSymbol)
        {
            if (battleOptionType == BattleOptionType.None)
                return string.Empty;

            var sb = StringBuilderPool.Get();

            int value1 = MathUtils.ToInt(MathUtils.ToPermyriadValue(rate1) * value, 2);
            int value2 = MathUtils.ToInt(MathUtils.ToPermyriadValue(rate2) * value, 2);

            if (value1 != 0) // value1가 존재할 경우
            {
                string valueCustomText = battleOptionType.ToValueCustomText(); // value 커스텀 Text
                if (string.IsNullOrEmpty(valueCustomText))
                {
                    if (value1 > 0 && isSymbol) // 값이 0보다 클 경우
                        sb.Append("+"); // + 표기가 필요

                    if (battleOptionType.IsRateOption()) // 확률 옵션의 경우
                    {
                        sb.Append(MathUtils.ToPercentValue(value1).ToString("0.##")); // 0.## 까지 보이도록 처리
                    }
                    else
                    {
                        sb.Append(value1);
                    }
                }
                else
                {
                    if (battleOptionType.IsRateOption()) // 확률 옵션의 경우
                    {
                        sb.Append(valueCustomText.Replace(ReplaceKey.VALUE, MathUtils.ToPercentValue(value1).ToString("0.##"))); // 0.## 까지 보이도록 처리
                    }
                    else
                    {
                        sb.Append(valueCustomText.Replace(ReplaceKey.VALUE, value1));
                    }
                }
            }

            if (battleOptionType.IsConditionalOption()) // value2를 조건으로 사용하는 경우
            {
                // 아무것도 하지 않음
            }
            else if (value2 != 0) // value2가 존재할 경우
            {
                // value1 의 값이 존재할 경우 구분 쉼표 표시
                if (sb.Length > 0)
                    sb.Append(", ");

                if (value2 > 0 && isSymbol) // 값이 0보다 클 경우
                    sb.Append("+"); // + 표기가 필요

                sb.Append(MathUtils.ToPercentValue(value2).ToString("0.##")); // 0.## 까지 보이도록 처리
                sb.Append("%");
            }

            return sb.Release();
        }

        public float GetRateValue()
        {
            return MathUtils.GetRate((int)(serverValue - totalMinValue), (int)(totalMaxValue - totalMinValue));
        }
    }
}
