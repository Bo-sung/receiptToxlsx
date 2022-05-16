using MsgPack;

namespace Ragnarok
{
    public class EventDuelRewardDataManager : Singleton<EventDuelRewardDataManager>, IDataManger
    {
        private readonly BetterList<EventDuelRewardData> worldServerRewardDataList;
        private readonly BetterList<EventDuelRewardData> serverRewardDataList;

        public ResourceType DataType => ResourceType.EventDualRewardDataDB;

        public EventDuelRewardDataManager()
        {
            worldServerRewardDataList = new BetterList<EventDuelRewardData>();
            serverRewardDataList = new BetterList<EventDuelRewardData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            worldServerRewardDataList.Clear();
            serverRewardDataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    EventDuelRewardData data = new EventDuelRewardData(mpo.AsList());

                    switch (data.group_id)
                    {
                        case EventDuelRewardData.WORLD_SERVER_REWARD_TYPE:
                            worldServerRewardDataList.Add(data);
                            break;

                        case EventDuelRewardData.SERVER_REWARD_TYPE:
                            serverRewardDataList.Add(data);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            worldServerRewardDataList.Sort(SortByRank);
            serverRewardDataList.Sort(SortByRank);
        }

        public void VerifyData()
        {
            #region UNITY_EDITOR

            #endregion
        }

        /// <summary>
        /// 랭킹에 따른 서버 보상
        /// </summary>
        public RewardData[] GetServerReward(int rank)
        {
            // 순위 밖
            if (rank <= 0)
                return null;

            foreach (var item in serverRewardDataList)
            {
                if (rank >= item.start_rank)
                    return item.GetRewards();
            }

            return null;
        }

        public EventDuelRewardData[] GetWorldServerRewards()
        {
            return worldServerRewardDataList.ToArray();
        }

        public EventDuelRewardData[] GetServerRewards()
        {
            return serverRewardDataList.ToArray();
        }

        private int SortByRank(EventDuelRewardData a, EventDuelRewardData b)
        {
            return a.start_rank.CompareTo(b.start_rank);
        }
    }
}