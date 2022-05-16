using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class SmeltRateDataManager : Singleton<SmeltRateDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, SmeltRateData> dataDic;

        public ResourceType DataType => ResourceType.SmeltRateDataDB;

        public SmeltRateDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, SmeltRateData>(ObscuredIntEqualityComparer.Default);
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
                    SmeltRateData data = new SmeltRateData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.lv))
                        dataDic.Add(data.lv, data);
                }
            }
        }

        public SmeltRateData Get(int lv)
        {
            if (!dataDic.ContainsKey(lv))
                throw new System.ArgumentException($"제련 확률 데이터가 존재하지 않습니다: lv = {lv}");

            return dataDic[lv];
        }

        /// <summary>
        /// 제련 확률
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="rating"></param>
        /// <returns></returns>
        public string Rate(int lv, int rating)
        {
            return (Get(lv).rating_success_rate[rating] * 0.01f).ToString("0.##");
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