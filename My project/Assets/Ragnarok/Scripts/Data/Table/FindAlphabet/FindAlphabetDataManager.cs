using MsgPack;

namespace Ragnarok
{
    public sealed class FindAlphabetDataManager : Singleton<FindAlphabetDataManager>, IDataManger
    {
        private readonly BetterList<FindAlphabetData> dataList;

        public ResourceType DataType => ResourceType.FindAlphabetDataDB;

        public FindAlphabetDataManager()
        {
            dataList = new BetterList<FindAlphabetData>();
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
                    FindAlphabetData data = new FindAlphabetData(mpo.AsList());
                    dataList.Add(data);
                }
            }
        }

        /// <summary>
        /// 보상 정보 반환
        /// </summary>
        public FindAlphabetData[] GetRewards()
        {
            return dataList.ToArray();
        }

        public int GetMaxRewardCount()
        {
            return dataList.size;
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