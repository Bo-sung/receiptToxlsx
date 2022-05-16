using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class BattleOptionList : IEqualityComparer<BattleOptionType>
    {
        private readonly Dictionary<BattleOptionType, BattleStatus> plusOptionDic;

        public bool HasValue(BattleOptionType type) => ContainsKey(type);

        public BattleOptionList()
        {
            plusOptionDic = new Dictionary<BattleOptionType, BattleStatus>(this);
        }

        public virtual void Clear()
        {
            plusOptionDic.Clear();
        }

        public void AddRange(IEnumerable<BattleOption> collection)
        {
            foreach (var item in collection)
            {
                if (IsInvalid(item.battleOptionType))
                    continue;

                Add(item);
            }
        }

        public void Add(BattleOption option)
        {
            if (option.battleOptionType.IsConditionalOption())
            {
                Add(option.battleOptionType, option.value2, option.value1);
            }
            else
            {
                Plus(plusOptionDic, option.battleOptionType, new BattleStatus(option.value1, option.value2));
            }
        }

        public BattleStatus Get(BattleOptionType battleOptionType)
        {
            if (IsInvalid(battleOptionType) || battleOptionType.IsConditionalOption())
            {
                Debug.LogError($"정의되지 않은 타입: {nameof(battleOptionType)} = {battleOptionType}");
                return default;
            }

            return HasValue(battleOptionType) ? plusOptionDic[battleOptionType] : default;
        }

        public virtual BattleStatus GetCrowdControl(CrowdControlType crowdControlType)
        {
            return default;
        }

        public virtual BattleStatus GetCrowdControlRateResist(CrowdControlType crowdControlType)
        {
            return default;
        }

        public virtual BattleStatus GetBasicActiveSkillCrowdControlRate(CrowdControlType crowdControlType)
        {
            return default;
        }

        public virtual BattleStatus GetElementDmgRate(ElementType elementType)
        {
            return default;
        }

        public virtual BattleStatus GetElementDmgRateResist(ElementType elementType)
        {
            return default;
        }

        public virtual Dictionary<int, int> GetSkillIdDmgRateDic()
        {
            return default;
        }

        public virtual Dictionary<int, int> GetFireActiveSkillRateDic()
        {
            return default;
        }

        public virtual Dictionary<int, int> GetBasicActiveSkillRateDic()
        {
            return default;
        }

        public virtual Dictionary<int, int> GetActiveSkillRateDic()
        {
            return default;
        }

        public virtual Dictionary<int, int> GetColleagueRateDic()
        {
            return default;
        }

        public virtual Dictionary<int, int> GetSkillOverrideDic()
        {
            return default;
        }

        public virtual Dictionary<int, List<int>> GetSkillChainDic()
        {
            return default;
        }

        /// <summary>
        /// 유효하지 않은 타입
        /// </summary>
        protected virtual bool IsInvalid(BattleOptionType battleOptionType)
        {
            return battleOptionType == BattleOptionType.None;
        }

        /// <summary>
        /// 조건 타입 추가
        /// </summary>
        protected abstract void Add(BattleOptionType battleOptionType, int condition, int value);

        protected void Plus<TKey>(Dictionary<TKey, BattleStatus> dic, TKey key, BattleStatus value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] += value;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        protected void Plus<TKey>(Dictionary<TKey, int> dic, TKey key, int value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] += value;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        private bool ContainsKey(BattleOptionType battleOptionType)
        {
            return plusOptionDic.ContainsKey(battleOptionType);
        }

        bool IEqualityComparer<BattleOptionType>.Equals(BattleOptionType x, BattleOptionType y)
        {
            return x == y;
        }

        int IEqualityComparer<BattleOptionType>.GetHashCode(BattleOptionType obj)
        {
            return obj.GetHashCode();
        }
    }
}