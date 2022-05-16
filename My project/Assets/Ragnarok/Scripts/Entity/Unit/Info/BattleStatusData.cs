using UnityEngine;

namespace Ragnarok
{
    public class BattleStatusData
    {
        public int AP; // Attack Power. 전투력.

        // 출력용
        public int beforeAP;
        public int afterAP;

        public int HP;
        public int ATK;
        public int DEF;
        public int MATK;
        public int MDEF;
        public int HIT;
        public int FLEE;
        public int CRI;

        public enum Stat
        {
            HP = 0,
            ATK,
            DEF,
            MATK,
            MDEF,
            HIT,
            FLEE,
            CRI,
        }

        public BattleStatusData() { }
        public BattleStatusData(CharacterEntity entity)
        {
            AP = entity.GetTotalAttackPower();

            BattleStatusInfo status = entity.preBattleStatusInfo;
            HP = status.MaxHp;
            ATK = Mathf.Max(status.MeleeAtk, status.RangedAtk);
            DEF = status.Def;
            MATK = status.MAtk;
            MDEF = status.MDef;
            HIT = status.Hit;
            FLEE = status.Flee;
            CRI = status.CriRate;
        }

        /// <summary>
        /// 모든 값이 0인지.
        /// </summary>
        public bool IsZero()
        {
            return AP == 0 && HP == 0 && ATK == 0 && DEF == 0 && MATK == 0 && MDEF == 0 && HIT == 0 && FLEE == 0 && CRI == 0;
        }

        /// <summary>
        /// 두 데이터 간의 차이를 비교.
        /// </summary>
        public static BattleStatusData GetDifference(BattleStatusData before, BattleStatusData after)
        {
            BattleStatusData ret = new BattleStatusData();
            ret.AP = after.AP - before.AP;

            ret.beforeAP = before.AP;
            ret.afterAP = after.AP;

            ret.HP = after.HP - before.HP;
            ret.ATK = after.ATK - before.ATK;
            ret.DEF = after.DEF - before.DEF;
            ret.MATK = after.MATK - before.MATK;
            ret.MDEF = after.MDEF - before.MDEF;
            ret.HIT = after.HIT - before.HIT;
            ret.FLEE = after.FLEE - before.FLEE;
            ret.CRI = after.CRI - before.CRI;

            return ret;
        }

        /// <summary>
        /// 인덱스를 통해 해당 status 반환.
        /// </summary>
        public int GetStatusByIndex(int index)
        {
            switch (index)
            {
                case 0: return HP;
                case 1: return ATK;
                case 2: return DEF;
                case 3: return MATK;
                case 4: return MDEF;
                case 5: return HIT;
                case 6: return FLEE;
                case 7: return CRI;
            }

            return 0;
        }

        /// <summary>
        /// 인덱스를 통해 해당 status의 이름 반환.
        /// </summary>
        public string GetStatusNameByIndex(int index)
        {
            switch (index)
            {
                case 0: return LocalizeKey._48001.ToText(); // HP
                case 1: return LocalizeKey._48002.ToText(); // ATK
                case 2: return LocalizeKey._48003.ToText(); // DEF
                case 3: return LocalizeKey._48004.ToText(); // MATK
                case 4: return LocalizeKey._48005.ToText(); // MDEF
                case 5: return LocalizeKey._48006.ToText(); // HIT
                case 6: return LocalizeKey._48007.ToText(); // FLEE
                case 7: return LocalizeKey._48008.ToText(); // CRI
            }

            return string.Empty;
        }

        /// <summary>
        /// 값을 가진 스탯의 개수를 반환. (AP는 미포함)
        /// </summary>
        public int GetValidStatusCount()
        {
            const int SHOW_CHANGED_STATUS_THRESHOLD = 10; // 백분율 0.1

            int count = 0;
            for (int i = 0; i < Constants.Size.ABILITY_STATUS_COUNT; ++i)
            {
                bool isPercentStat = IsPercentStat(i.ToEnum<Stat>());
                int statDiff = Mathf.Abs(GetStatusByIndex(i));
                if ( (isPercentStat && statDiff >= SHOW_CHANGED_STATUS_THRESHOLD) || (!isPercentStat && statDiff > 0))
                {
                    ++count;
                }
            }

            return count;
        }

        /// <summary>
        /// 백분율로 표기해야하는 스탯인지.
        /// </summary>
        bool IsPercentStat(Stat stat)
        {
            switch (stat)
            {
                case Stat.FLEE:
                case Stat.CRI:
                    return true;
            }
            return false;
        }

        public override string ToString() // For Debug.
        {
            string ret = StringBuilderPool.Get()
                .Append("beforeAP : ").Append(beforeAP).Append("\n")
                .Append("afterAP : ").Append(afterAP).Append("\n")
                .Append("HP : ").Append(HP).Append("\n")
                .Append("ATK : ").Append(ATK).Append("\n")
                .Append("DEF : ").Append(DEF).Append("\n")
                .Append("MATK : ").Append(MATK).Append("\n")
                .Append("MDEF : ").Append(MDEF).Append("\n")
                .Append("HIT : ").Append(HIT).Append("\n")
                .Append("FLEE : ").Append(FLEE).Append("\n")
                .Append("CRI : ").Append(CRI)
                .Release();
            return ret;
        }
    }
}