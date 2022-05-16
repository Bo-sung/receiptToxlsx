using Ragnarok.Text;
using UnityEngine;

namespace Ragnarok
{
    public interface IBattleOption
    {
        BattleOptionType BattleOptionType { get; }
        int Value1 { get; }
        int Value2 { get; }
    }

    public struct BattleOption : IBattleOption
    {
        public readonly BattleOptionType battleOptionType;
        public readonly int value1;
        public readonly int value2;

        BattleOptionType IBattleOption.BattleOptionType => battleOptionType;
        int IBattleOption.Value1 => value1;
        int IBattleOption.Value2 => value2;

        public BattleOption(int battleOptionType, int value1, int value2)
            : this(battleOptionType.ToEnum<BattleOptionType>(), value1, value2)
        {
        }

        public BattleOption(BattleOptionType battleOptionType, int value1, int value2)
        {
            this.battleOptionType = battleOptionType;
            this.value1 = value1;
            this.value2 = value2;
        }

        /// <summary>
        /// 전투 옵션 텍스트
        /// </summary>
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

        /// <summary>
        /// 전투 수치 텍스트
        /// </summary>
        public string GetValueText(int skillBlowCount = 0)
        {
            if (battleOptionType == BattleOptionType.None)
                return string.Empty;

            if (battleOptionType == BattleOptionType.SkillOverride || battleOptionType == BattleOptionType.SkillChain)
            {
                SkillData replaceSkillData = SkillDataManager.Instance.Get(value2, level: 1);
                string replaceSkillName = replaceSkillData == null ? string.Empty : replaceSkillData.name_id.ToText();
                return replaceSkillName.ToLinkText(TextLinkType.Skill, value2);
            }

            var sb = StringBuilderPool.Get();

            if (value1 != 0) // value1가 존재할 경우
            {
                string valueCustomText = battleOptionType.ToValueCustomText(skillBlowCount); // value 커스텀 Text
                if (string.IsNullOrEmpty(valueCustomText))
                {
                    if (value1 > 0) // 값이 0보다 클 경우
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
                        sb.Append(valueCustomText
                            .Replace(ReplaceKey.VALUE, value1)
                            .Replace(ReplaceKey.COUNT, skillBlowCount));
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

                string perValueCustomText = battleOptionType.ToPerValueCustomText(skillBlowCount); // perValue 커스텀 Text
                if (string.IsNullOrEmpty(perValueCustomText))
                {
                    if (value2 > 0) // 값이 0보다 클 경우
                        sb.Append("+"); // + 표기가 필요

                    sb.Append(MathUtils.ToPercentValue(value2).ToString("0.##")); // 0.## 까지 보이도록 처리
                    sb.Append("%");
                }
                else
                {
                    sb.Append(perValueCustomText
                        .Replace(ReplaceKey.VALUE, MathUtils.ToPercentValue(value2).ToString("0.##"))
                        .Replace(ReplaceKey.COUNT, skillBlowCount)); // 0.## 까지 보이도록 처리 + 공격 횟수 처리
                }
            }

            return sb.Release();
        }

        /// <summary>
        /// 전투 옵션 + 전투 수치
        /// </summary>
        public string GetDescription()
        {
            if (battleOptionType == BattleOptionType.None)
                return string.Empty;

            return StringBuilderPool.Get()
                .Append(GetTitleText())
                .Append(" ")
                .Append(GetValueText())
                .Release();
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            return $"{nameof(battleOptionType)} = {battleOptionType}, {nameof(value1)} = {value1}, {nameof(value2)} = {value2}";
        }
#endif
    }
}