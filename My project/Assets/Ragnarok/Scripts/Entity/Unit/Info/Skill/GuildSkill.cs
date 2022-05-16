using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public class GuildSkill : SkillInfo, GuildModel.IGuildSkill
    {
        private ObscuredInt exp;

        public int CurExp => Mathf.Min(exp - BasisType.GUILD_SKILL_LEVEL_UP_EXP.GetInt(GetSkillLevel()), NextNeedExp);

        public int NextNeedExp => BasisType.GUILD_SKILL_LEVEL_UP_EXP.GetInt(NextSkillLevel()) - BasisType.GUILD_SKILL_LEVEL_UP_EXP.GetInt(GetSkillLevel());

        public bool IsLevelUp => CurExp >= NextNeedExp;

        public int ExpCoin => BasisType.GUILD_SKILL_EXP_COIN.GetInt();
        public int ExpCash => BasisType.GUILD_SKILL_EXP_CAT_COIN.GetInt();
        public int NeedCoin => BasisType.GUILD_SKILL_BUY_EXP_COIN.GetInt();
        public int NeedCash => BasisType.GUILD_SKILL_BUY_EXP_CAT_COIN.GetInt();

        public bool IsMaxLevel => BasisType.GUILD_SKILL_LEVEL_UP_EXP.GetInt(NextSkillLevel()) == 0;

        /// <summary>
        /// 길드 스킬레벨을 올리기 위해 필요한 길드레벨
        /// </summary>
        public int NeedGuildLevel => BasisType.GUILD_SKILL_LEVEL_LIMIT_GUILD_LEVEL.GetInt(NextSkillLevel());

        int GuildModel.IGuildSkill.GuldId => 0;
        int GuildModel.IGuildSkill.SkillId => (IsInvalidData || !IsInPossession) ? 0 : data.SkillId;
        int GuildModel.IGuildSkill.Exp => exp;
        int GuildModel.IGuildSkill.Level => IsInvalidData ? 0 : GetSkillLevel();

        public int GetSkillLevel()
        {
            return IsInPossession ? SkillLevel : 0;
        }

        public int NextSkillLevel()
        {
            return GetSkillLevel() + 1;
        }

        public void SetExp(int exp)
        {
            this.exp = exp;
        }

        public void PlusExp(byte costType)
        {
            if (costType == 1)
            {
                exp += ExpCoin;
            }
            else if (costType == 2)
            {
                exp += ExpCash;
            }
        }
    }
}