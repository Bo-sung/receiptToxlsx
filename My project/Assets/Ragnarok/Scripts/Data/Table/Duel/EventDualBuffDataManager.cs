using MsgPack;

namespace Ragnarok
{
    public sealed class EventDualBuffDataManager : Singleton<EventDualBuffDataManager>, IDataManger
    {
        private readonly BetterList<EventDualBuffData> rankRewardList;
        private EventDualBuffData perfectRewardData; // 완전승리보상

        public ResourceType DataType => ResourceType.EventDualBuffDataDB;

        public EventDualBuffDataManager()
        {
            rankRewardList = new BetterList<EventDualBuffData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            rankRewardList.Release();
            perfectRewardData = null;
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    EventDualBuffData data = new EventDualBuffData(mpo.AsList());

                    switch (data.rank_type)
                    {
                        case EventDualBuffData.PERFECT_RANK_TYPE:
                            perfectRewardData = data;
                            break;

                        case EventDualBuffData.NORMAL_RANK_TYPE:
                            rankRewardList.Add(data);
                            break;
                    }
                }
            }
        }

        public EventDualBuffData GetPerfectReward()
        {
            return perfectRewardData;
        }

        public EventDualBuffData[] GetNormalRewards()
        {
            return rankRewardList.ToArray();
        }

        public EventDualBuffData GetNormalReward(int rank)
        {
            foreach (var item in rankRewardList)
            {
                if (item.Rank == rank)
                    return item;
            }

            return rankRewardList.Find(a => a.Rank == 0); // 값이 없으면 참여보상
        }

        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

#endif
        }
    }
}