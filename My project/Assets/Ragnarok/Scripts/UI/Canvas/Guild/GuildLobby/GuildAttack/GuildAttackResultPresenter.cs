using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildAttackClear"/>
    /// <see cref="UIGuildAttackFail"/>
    /// </summary>
    public sealed class GuildAttackResultPresenter : ViewPresenter
    {
        // <!-- Models --!>

        // <!-- Repositories --!>
        private readonly SkillDataManager skillDataRepo;

        // <!-- Event --!>

        private int buffSkillId;

        public GuildAttackResultPresenter()
        {
            skillDataRepo = SkillDataManager.Instance;
            buffSkillId = BasisType.GUILD_ATTACK_EMPERIUM_BUFF_SKILL_ID.GetInt(); // 버프 스킬 ID
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 엠펠리움 최대 레벨 유무
        /// </summary>
        public bool IsMaxLevel(int level)
        {
            return level == BasisType.GUILD_ATTACK_EMPERIUM_MAX_LEVEL.GetInt();
        }       

        /// <summary>
        /// 엠펠리움 버프
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public IEnumerable<BattleOption> GetBattleOption(int level)
        {
            SkillData.ISkillData skillData = skillDataRepo.Get(buffSkillId, level);

            if (skillData == null)
                return GetBattleOption();

            return skillData.GetBattleOptions();
        }

        IEnumerable<BattleOption> GetBattleOption()
        {
            yield break;
        }

        /// <summary>
        /// 길드 습격 승리 보상
        /// </summary>
        public RewardData GetReward(int level)
        {
            int count = BasisType.GUILD_ATTACK_SUCCESS_REWARD_COUNT.GetInt(level);
            return new RewardData(RewardType.Item, BasisItem.Emperium.GetID(), count);
        }
    }
}