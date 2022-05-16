using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class GuildBattleRewardDataManager : Singleton<GuildBattleRewardDataManager>, IDataManger
    {
        // IComparable Interface 를 사용하여 Sort하기 때문에 BetterList 를 사용하지 않습니다.
        private readonly List<GuildBattleRewardData> attackRewardDataList;
        private readonly List<GuildBattleRewardData> defenseRewardDataList;
        private readonly List<GuildBattleRewardData> rankRewardDataList;
        private readonly List<GuildBattleRewardData> eventRewardDataList;

        public ResourceType DataType => ResourceType.GuildBattleRewardDB;

        public GuildBattleRewardDataManager()
        {
            attackRewardDataList = new List<GuildBattleRewardData>();
            defenseRewardDataList = new List<GuildBattleRewardData>();
            rankRewardDataList = new List<GuildBattleRewardData>();
            eventRewardDataList = new List<GuildBattleRewardData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            attackRewardDataList.Clear();
            defenseRewardDataList.Clear();
            rankRewardDataList.Clear();
            eventRewardDataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    GuildBattleRewardData data = GuildBattleRewardData.Create(mpo.AsList());

                    if (data is AttackGuildBattleRewardData)
                    {
                        attackRewardDataList.Add(data);
                    }
                    else if (data is DefenseGuildBattleRewardData)
                    {
                        defenseRewardDataList.Add(data);
                    }
                    else if (data is RankGuildBattleRewardData)
                    {
                        rankRewardDataList.Add(data);
                    }
                    else if (data is EventGuildBattleRewardData)
                    {
                        eventRewardDataList.Add(data);
                    }
                }
            }

            attackRewardDataList.Sort();
            defenseRewardDataList.Sort();
            rankRewardDataList.Sort();
            eventRewardDataList.Sort();
        }

        public GuildBattleRewardData[] GetAttackRewards()
        {
            return attackRewardDataList.ToArray();
        }

        public GuildBattleRewardData[] GetDefenseRewards()
        {
            return defenseRewardDataList.ToArray();
        }

        public GuildBattleRewardData[] GetRankRewards()
        {
            return rankRewardDataList.ToArray();
        }

        public GuildBattleRewardData[] GetEventRewards()
        {
            return eventRewardDataList.ToArray();
        }

        public RewardData[] GetAttackRewards(int damage)
        {
            for (int i = 0; i < attackRewardDataList.Count; i++)
            {
                if (damage >= attackRewardDataList[i].end)
                    return attackRewardDataList[i].GetRewards();
            }

            return null;
        }

        /// <summary>
        /// 다음 보상까지 남은 Damage
        /// </summary>
        public int GetNextRemainDamage(int damage)
        {
            for (int i = attackRewardDataList.Count - 1; i >= 0; i--)
            {
                if (attackRewardDataList[i].IsEntryReward())
                    continue;

                if (attackRewardDataList[i].end > damage)
                    return attackRewardDataList[i].end;
            }

            return 0;
        }

        public void Initialize()
        {
            ReloadRewards(BoxDataManager.Instance, attackRewardDataList, defenseRewardDataList, rankRewardDataList, eventRewardDataList);

            int maxEmperiumHp = BasisGuildWarInfo.EmperiumMaxHp.GetInt();
            for (int i = 0; i < defenseRewardDataList.Count; i++)
            {
                defenseRewardDataList[i].SetMaxEmperiumHp(maxEmperiumHp);
            }
        }

        private void ReloadRewards(BoxDataManager.IBoxDataRepoImpl boxDataRepo, params List<GuildBattleRewardData>[] arrDataList)
        {
            Buffer<RewardData> rewardBuffer = new Buffer<RewardData>();
            foreach (var dataList in arrDataList)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    RewardData[] rewards = dataList[i].GetRewards();
                    foreach (var reward in rewards)
                    {
                        if (reward.RewardType == RewardType.Item && reward.ItemType == ItemType.Box)
                        {
                            rewardBuffer.AddRange(boxDataRepo.ToBoxRewards(reward)); // 박스 아이템 펼쳐서 보여주기
                        }
                        else
                        {
                            rewardBuffer.Add(reward);
                        }
                    }

                    dataList[i].SetRewards(rewardBuffer.GetBuffer(isAutoRelease: true));
                }
            }
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

#endif
        }
    }
}