using MsgPack;

namespace Ragnarok
{
    public sealed class DiceRewardDataManager : Singleton<DiceRewardDataManager>, IDataManger
    {
        private readonly BetterList<DiceRewardData> dataList;

        public ResourceType DataType => ResourceType.DiceRewardDataDB;

        public DiceRewardDataManager()
        {
            dataList = new BetterList<DiceRewardData>();
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
                    DiceRewardData data = new DiceRewardData(mpo.AsList());
                    dataList.Add(data);
                }
            }
        }

        /// <summary>
        /// 완주 보상 정보 반환
        /// </summary>
        public DiceRewardData[] GetRewards()
        {
            return dataList.ToArray();
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