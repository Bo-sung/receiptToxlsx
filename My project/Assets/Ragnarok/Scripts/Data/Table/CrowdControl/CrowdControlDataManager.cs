using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class CrowdControlDataManager : Singleton<CrowdControlDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, CrowdControlData> dataDic;

        public ResourceType DataType => ResourceType.CrowdControlDataDB;

        public CrowdControlDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, CrowdControlData>(ObscuredIntEqualityComparer.Default);
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
                    CrowdControlData data = new CrowdControlData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public CrowdControlData Get(CrowdControlType type)
        {
            ObscuredInt key = (int)type;

            if (!dataDic.ContainsKey(key))
                throw new System.ArgumentException($"상태이상 데이터가 존재하지 않습니다: type = {type}");

            return dataDic[key];
        }

        public int GetOverlapCount(CrowdControlType type)
        {
            ObscuredInt key = (int)type;

            if (!dataDic.ContainsKey(key))
                throw new System.ArgumentException($"상태이상 데이터가 존재하지 않습니다: type = {type}");

            return dataDic[key].overlap_count;
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