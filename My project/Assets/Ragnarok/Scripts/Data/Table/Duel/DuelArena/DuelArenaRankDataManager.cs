using MsgPack;

namespace Ragnarok
{
    public class DuelArenaRankDataManager : Singleton<DuelArenaRankDataManager>, IDataManger
    {
        private readonly BetterList<DuelArenaRankData> dataList;

        public ResourceType DataType => ResourceType.DuelArenaRankDataDB;

        public DuelArenaRankDataManager()
        {
            dataList = new BetterList<DuelArenaRankData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    DuelArenaRankData data = new DuelArenaRankData(mpo.AsList());
                    dataList.Add(data);
                }
            }

            dataList.Sort(SortByRank);
        }

        public DuelArenaRankData[] GetRewards()
        {
            return dataList.ToArray();
        }

        private int SortByRank(DuelArenaRankData x, DuelArenaRankData y)
        {
            // 참가 보상
            if (y.IsEntryReward())
                return -1;

            return x.ranking_value.CompareTo(y.ranking_value); // 낮은 랭킹 순으로 정렬
        }

        public void Initialize()
        {
        }

        public void VerifyData()
        {
        }
    }
}