using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class PassiveBattleOptionList : BattleOptionList, IEqualityComparer<CrowdControlType>, IEqualityComparer<ElementType>
    {
        private readonly Dictionary<CrowdControlType, BattleStatus> crowdControlRateResistDic;
        private readonly Dictionary<CrowdControlType, BattleStatus> basicActiveSkillCrowdControlRateDic;
        private readonly Dictionary<ElementType, BattleStatus> elementDmgRateDic;
        private readonly Dictionary<ElementType, BattleStatus> elementDmgRateResistDic;
        private readonly Dictionary<int, int> skillIdDmgRateDic;
        private readonly Dictionary<int, int> fireActiveSkillRateDic;
        private readonly Dictionary<int, int> basicActiveSkillRateDic;
        private readonly Dictionary<int, int> activeSkillRateDic;
        private readonly Dictionary<int, int> colleagueRateDic;
        private readonly Dictionary<int, int> skillOverrideDic;
        private readonly Dictionary<int, List<int>> skillChainDic;

        public PassiveBattleOptionList()
        {
            crowdControlRateResistDic = new Dictionary<CrowdControlType, BattleStatus>(this);
            basicActiveSkillCrowdControlRateDic = new Dictionary<CrowdControlType, BattleStatus>(this);
            elementDmgRateDic = new Dictionary<ElementType, BattleStatus>(this);
            elementDmgRateResistDic = new Dictionary<ElementType, BattleStatus>(this);
            skillIdDmgRateDic = new Dictionary<int, int>(IntEqualityComparer.Default);
            fireActiveSkillRateDic = new Dictionary<int, int>(IntEqualityComparer.Default);
            basicActiveSkillRateDic = new Dictionary<int, int>(IntEqualityComparer.Default);
            activeSkillRateDic = new Dictionary<int, int>(IntEqualityComparer.Default);
            colleagueRateDic = new Dictionary<int, int>(IntEqualityComparer.Default);
            skillOverrideDic = new Dictionary<int, int>(IntEqualityComparer.Default);
            skillChainDic = new Dictionary<int, List<int>>(IntEqualityComparer.Default);
        }

        public override void Clear()
        {
            base.Clear();

            crowdControlRateResistDic.Clear();
            basicActiveSkillCrowdControlRateDic.Clear();
            elementDmgRateDic.Clear();
            elementDmgRateResistDic.Clear();
            skillIdDmgRateDic.Clear();
            fireActiveSkillRateDic.Clear();
            basicActiveSkillRateDic.Clear();
            activeSkillRateDic.Clear();
            colleagueRateDic.Clear();
            skillOverrideDic.Clear();
            skillChainDic.Clear();
        }

        public override BattleStatus GetCrowdControlRateResist(CrowdControlType crowdControlType)
        {
            return crowdControlRateResistDic.ContainsKey(crowdControlType) ? crowdControlRateResistDic[crowdControlType] : default;
        }

        public override BattleStatus GetBasicActiveSkillCrowdControlRate(CrowdControlType crowdControlType)
        {
            return basicActiveSkillCrowdControlRateDic.ContainsKey(crowdControlType) ? basicActiveSkillCrowdControlRateDic[crowdControlType] : default;
        }

        public override BattleStatus GetElementDmgRate(ElementType elementType)
        {
            return elementDmgRateDic.ContainsKey(elementType) ? elementDmgRateDic[elementType] : default;
        }

        public override BattleStatus GetElementDmgRateResist(ElementType elementType)
        {
            return elementDmgRateResistDic.ContainsKey(elementType) ? elementDmgRateResistDic[elementType] : default;
        }

        public override Dictionary<int, int> GetSkillIdDmgRateDic()
        {
            return skillIdDmgRateDic;
        }

        public override Dictionary<int, int> GetFireActiveSkillRateDic()
        {
            return fireActiveSkillRateDic;
        }

        public override Dictionary<int, int> GetBasicActiveSkillRateDic()
        {
            return basicActiveSkillRateDic;
        }

        public override Dictionary<int, int> GetActiveSkillRateDic()
        {
            return activeSkillRateDic;
        }

        public override Dictionary<int, int> GetColleagueRateDic()
        {
            return colleagueRateDic;
        }

        public override Dictionary<int, int> GetSkillOverrideDic()
        {
            return skillOverrideDic;
        }

        public override Dictionary<int, List<int>> GetSkillChainDic()
        {
            return skillChainDic;
        }

        protected override void Add(BattleOptionType battleOptionType, int condition, int value)
        {
            switch (battleOptionType)
            {
                case BattleOptionType.CrowdControlRateResist:
                    Plus(crowdControlRateResistDic, condition.ToEnum<CrowdControlType>(), new BattleStatus(value, 0));
                    break;

                case BattleOptionType.BasicActiveSkillCrowdControlRate:
                    Plus(basicActiveSkillCrowdControlRateDic, condition.ToEnum<CrowdControlType>(), new BattleStatus(value, 0));
                    break;

                case BattleOptionType.ElementDmgRate:
                    Plus(elementDmgRateDic, condition.ToEnum<ElementType>(), new BattleStatus(value, 0));
                    break;

                case BattleOptionType.ElementDmgRateResist:
                    Plus(elementDmgRateResistDic, condition.ToEnum<ElementType>(), new BattleStatus(value, 0));
                    break;

                case BattleOptionType.SkillIdDmgRate:
                    Plus(skillIdDmgRateDic, condition, value);
                    break;

                case BattleOptionType.FirePillar:
                    Plus(fireActiveSkillRateDic, condition, value);
                    break;

                case BattleOptionType.Colleague:
                    Plus(colleagueRateDic, condition, value);
                    break;

                case BattleOptionType.BasicActiveSkillRate:
                    Plus(basicActiveSkillRateDic, condition, value);
                    break;

                case BattleOptionType.ActiveSkill:
                    Plus(activeSkillRateDic, condition, value);
                    break;

                case BattleOptionType.SkillOverride:
                    if (skillOverrideDic.ContainsKey(value))
                    {
                        skillOverrideDic[value] = condition;
                    }
                    else
                    {
                        skillOverrideDic.Add(value, condition);
                    }
                    break;

                case BattleOptionType.SkillChain:
                    if (!skillChainDic.ContainsKey(value))
                        skillChainDic.Add(value, new List<int>());

                    skillChainDic[value].Add(condition);
                    break;

                default:
                    Debug.LogError($"구현 필요: {nameof(battleOptionType)} = {battleOptionType}");
                    break;
            }
        }

        protected override bool IsInvalid(BattleOptionType battleOptionType)
        {
            return base.IsInvalid(battleOptionType) || battleOptionType.IsActiveOption();
        }

        bool IEqualityComparer<CrowdControlType>.Equals(CrowdControlType x, CrowdControlType y)
        {
            return x == y;
        }

        int IEqualityComparer<CrowdControlType>.GetHashCode(CrowdControlType obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<ElementType>.Equals(ElementType x, ElementType y)
        {
            return x == y;
        }

        int IEqualityComparer<ElementType>.GetHashCode(ElementType obj)
        {
            return obj.GetHashCode();
        }

#if UNITY_EDITOR
        public BattleOption[] ToArrayForServerCheck()
        {
            Buffer<BattleOption> buffer = new Buffer<BattleOption>();

            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (item == BattleOptionType.None)
                    continue;

                if (item.IsConditionalOption())
                    continue;

                if (!HasValue(item))
                    continue;

                BattleStatus status = Get(item);
                buffer.Add(new BattleOption(item, status.value, status.perValue));
            }

            foreach (CrowdControlType item in System.Enum.GetValues(typeof(CrowdControlType)))
            {
                if (!crowdControlRateResistDic.ContainsKey(item))
                    continue;

                BattleStatus status = crowdControlRateResistDic[item];
                buffer.Add(new BattleOption(BattleOptionType.CrowdControlRateResist, status.value, status.perValue));
            }

            foreach (CrowdControlType item in System.Enum.GetValues(typeof(CrowdControlType)))
            {
                if (!basicActiveSkillCrowdControlRateDic.ContainsKey(item))
                    continue;

                BattleStatus status = basicActiveSkillCrowdControlRateDic[item];
                buffer.Add(new BattleOption(BattleOptionType.BasicActiveSkillCrowdControlRate, status.value, status.perValue));
            }

            foreach (ElementType item in System.Enum.GetValues(typeof(ElementType)))
            {
                if (!elementDmgRateDic.ContainsKey(item))
                    continue;
                
                BattleStatus status = elementDmgRateDic[item];
                buffer.Add(new BattleOption(BattleOptionType.ElementDmgRate, status.value, (int)item)); // 서버와 value2 값을 맞추기 위함
            }

            foreach (ElementType item in System.Enum.GetValues(typeof(ElementType)))
            {
                if (!elementDmgRateResistDic.ContainsKey(item))
                    continue;

                BattleStatus status = elementDmgRateResistDic[item];
                buffer.Add(new BattleOption(BattleOptionType.ElementDmgRateResist, status.value, (int)item));
            }

            foreach (var item in skillIdDmgRateDic)
            {
                buffer.Add(new BattleOption(BattleOptionType.SkillIdDmgRate, item.Value, item.Key)); // value: stat, key: condition
            }

            foreach (var item in fireActiveSkillRateDic)
            {
                buffer.Add(new BattleOption(BattleOptionType.FirePillar, item.Value, item.Key)); // value: stat, key: condition
            }

            foreach (var item in basicActiveSkillRateDic)
            {
                buffer.Add(new BattleOption(BattleOptionType.BasicActiveSkillRate, item.Value, item.Key)); // value: stat, key: condition
            }

            foreach (var item in activeSkillRateDic)
            {
                buffer.Add(new BattleOption(BattleOptionType.ActiveSkill, item.Value, item.Key)); // value: stat, key: condition
            }

            foreach (var item in colleagueRateDic)
            {
                buffer.Add(new BattleOption(BattleOptionType.Colleague, item.Value, item.Key)); // value: stat, key: condition
            }

            foreach (var item in skillOverrideDic)
            {
                buffer.Add(new BattleOption(BattleOptionType.SkillOverride, item.Key, item.Value)); // value: stat, key: condition
            }

            foreach (var list in skillChainDic)
            {
                foreach (var item in list.Value)
                {
                    buffer.Add(new BattleOption(BattleOptionType.SkillChain, list.Key, item)); // value: stat, key: condition
                }
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }
#endif
    }
}