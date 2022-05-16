using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class EventRpsDataManager : Singleton<EventRpsDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, EventRpsData> dataDic;

        public ResourceType DataType => ResourceType.RPSDataDB;

        public EventRpsDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, EventRpsData>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    EventRpsData data = new EventRpsData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public EventRpsData Get(RpsRoundType type)
        {
            ObscuredInt key = (int)type + 1;

            if (!dataDic.ContainsKey(key))
                throw new System.ArgumentException($"가위바위보 데이터가 존재하지 않습니다: type = {type}");

            return dataDic[key];
        }

        public RewardData GetRewardData(RpsRoundType type)
        {
            ObscuredInt key = (int)type + 1;

            if (!dataDic.ContainsKey(key))
                throw new System.ArgumentException($"가위바위보 데이터가 존재하지 않습니다: type = {type}");

            return dataDic[key].reward_data;
        }

        public RewardData[] GetRewardDatas()
        {
            RewardData[] datas = new RewardData[dataDic.Count];

            for (int i = 0; i < datas.Length; i++)
            {
                datas[i] = dataDic[i + 1].reward_data;
            }

            return datas;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
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