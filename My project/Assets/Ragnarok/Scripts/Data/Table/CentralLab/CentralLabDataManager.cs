using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class CentralLabDataManager : Singleton<CentralLabDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, CentralLabData> dataDic;
        private readonly BetterList<CentralLabData> dataList;

        public ResourceType DataType => ResourceType.CLabDataDB;

        public CentralLabDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, CentralLabData>(ObscuredIntEqualityComparer.Default);
            dataList = new BetterList<CentralLabData>();
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
            dataList.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    CentralLabData data = new CentralLabData(mpo.AsList());

                    int id = data.id;
                    if (dataDic.ContainsKey(id))
                        continue;

                    dataDic.Add(data.id, data);
                    dataList.Add(data);
                }
            }
        }

        /// <summary>
        /// 특정 id에 해당하는 데이터 반환
        /// </summary>
        public CentralLabData Get(int id)
        {
            if (dataDic.ContainsKey(id))
                return dataDic[id];

            return null;
        }

        /// <summary>
        /// 특정 id에 해당하는 index 반환
        /// </summary>
        public int GetIndex(int id)
        {
            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].id == id)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 인덱스로 데이터 반환 (0부터 maxCount까지)
        /// </summary>
        public CentralLabData GetByIndex(int index)
        {
            if (index < 0 || index > GetMaxIndex())
                return null;

            return dataList[index];
        }

        /// <summary>
        /// 마지막 인덱스 반환
        /// </summary>
        public int GetMaxIndex()
        {
            return dataList.size - 1;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            CentralLabData first = GetByIndex(0);
            if (first == null)
                return;

            int openLevel = BasisType.CLAB_UNLOCK_LEVEL.GetInt();
            first.SetOpenCondition(DungeonOpenConditionType.JobLevel, openLevel);
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

#endif
        }
    }
}