using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ActiveBattleOptionList : BattleOptionList, IEqualityComparer<CrowdControlType>
    {
        private readonly Dictionary<CrowdControlType, BattleStatus> crowdControlDic;

        public bool HasDamageValue => HasValue(BattleOptionType.Damage) || HasValue(BattleOptionType.MDamage) || HasValue(BattleOptionType.FixedDamage);
        public bool HasRecoveryValue => HasValue(BattleOptionType.RecoveryHp) || HasValue(BattleOptionType.RecoveryTotalHp);
        public bool HasCrowdControl => crowdControlDic.Count > 0;

        public ActiveBattleOptionList()
        {
            crowdControlDic = new Dictionary<CrowdControlType, BattleStatus>(this);
        }

        public override void Clear()
        {
            base.Clear();

            crowdControlDic.Clear();
        }

        protected override void Add(BattleOptionType battleOptionType, int condition, int value)
        {
            switch (battleOptionType)
            {
                case BattleOptionType.CrowdControl:
                    Plus(crowdControlDic, condition.ToEnum<CrowdControlType>(), new BattleStatus(value, 0));
                    break;

                default:
                    Debug.LogError($"구현 필요: {nameof(battleOptionType)} = {battleOptionType}");
                    break;
            }
        }

        protected override bool IsInvalid(BattleOptionType battleOptionType)
        {
            return base.IsInvalid(battleOptionType) || !battleOptionType.IsActiveOption();
        }

        public override BattleStatus GetCrowdControl(CrowdControlType crowdControlType)
        {
            return crowdControlDic.ContainsKey(crowdControlType) ? crowdControlDic[crowdControlType] : default;
        }

        bool IEqualityComparer<CrowdControlType>.Equals(CrowdControlType x, CrowdControlType y)
        {
            return x == y;
        }

        int IEqualityComparer<CrowdControlType>.GetHashCode(CrowdControlType obj)
        {
            return obj.GetHashCode();
        }
    }
}