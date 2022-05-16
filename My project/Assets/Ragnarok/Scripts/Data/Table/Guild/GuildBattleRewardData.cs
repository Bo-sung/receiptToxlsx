using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class GuildBattleRewardData : System.IComparable<GuildBattleRewardData>, UIRankRewardElement.IInput
    {
        public readonly int end;
        private RewardData[] rewards;

        public GuildBattleRewardData(int end, params RewardData[] rewards)
        {
            this.end = end;
            this.rewards = rewards;
        }

        /// <summary>
        /// 참여 보상 여부
        /// </summary>
        public bool IsEntryReward()
        {
            return end == 0;
        }

        /// <summary>
        /// 최대 엠펠리움 Hp 세팅 (수비 보상일 경우에만 해당)
        /// </summary>
        public virtual void SetMaxEmperiumHp(int hp)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 보상 다시 세팅 (상자 아이템 펼쳐서 보여주기 위함)
        /// </summary>
        public void SetRewards(RewardData[] rewards)
        {
            this.rewards = rewards;
        }

        public abstract string GetName();

        public RewardData GetRewardData(int index)
        {
            if (index >= rewards.Length)
                return null;

            return rewards[index];
        }

        public RewardData[] GetRewards()
        {
            return rewards;
        }

        public abstract int CompareTo(GuildBattleRewardData other);

        public static GuildBattleRewardData Create(IList<MessagePackObject> data)
        {
            const int ATTACK_REWARD_GROUP = 1;
            const int DEFENSE_REWARD_GROUP = 2;
            const int RANK_REWARD_GROUP = 3;
            const int EVENT_REWARD_GROUP = 4;

            int index = 0;
            int id = data[index++].AsInt32();
            int reward_group = data[index++].AsInt32();
            int start_rank = data[index++].AsInt32();
            int end_rank = data[index++].AsInt32();
            int mail_text = data[index++].AsInt32();
            int reward_type_1 = data[index++].AsInt32();
            int reward_value_1 = data[index++].AsInt32();
            int reward_count_1 = data[index++].AsInt32();
            int reward_type_2 = data[index++].AsInt32();
            int reward_value_2 = data[index++].AsInt32();
            int reward_count_2 = data[index++].AsInt32();
            int reward_type_3 = data[index++].AsInt32();
            int reward_value_3 = data[index++].AsInt32();
            int reward_count_3 = data[index++].AsInt32();
            RewardData reward1 = new RewardData(reward_type_1, reward_value_1, reward_count_1);
            RewardData reward2 = new RewardData(reward_type_2, reward_value_2, reward_count_2);
            RewardData reward3 = new RewardData(reward_type_3, reward_value_3, reward_count_3);

            switch (reward_group)
            {
                case ATTACK_REWARD_GROUP:
                    return new AttackGuildBattleRewardData(end_rank, reward1, reward2, reward3);

                case DEFENSE_REWARD_GROUP:
                    return new DefenseGuildBattleRewardData(end_rank, reward1, reward2, reward3);

                case RANK_REWARD_GROUP:
                    return new RankGuildBattleRewardData(start_rank, end_rank, reward1, reward2, reward3);

                case EVENT_REWARD_GROUP:
                    return new EventGuildBattleRewardData(start_rank, end_rank, reward1, reward2, reward3);

                default:
#if UNITY_EDITOR
                    Debug.LogError($"잘못된 데이터: {nameof(reward_group)} = {reward_group}");
#endif
                    return null;
            }
        }
    }
}