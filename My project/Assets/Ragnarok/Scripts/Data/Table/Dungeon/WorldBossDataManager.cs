using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class WorldBossDataManager : Singleton<WorldBossDataManager>, IDataManger
    {
        private readonly Dictionary<int, WorldBossData> dataDic;
        private readonly List<WorldBossData> dataList;

        public ResourceType DataType => ResourceType.WorldBossDataDB;

        public WorldBossDataManager()
        {
            dataDic = new Dictionary<int, WorldBossData>(IntEqualityComparer.Default);
            dataList = new List<WorldBossData>();
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
            dataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    WorldBossData data = new WorldBossData(mpo.AsList());

                    if (dataDic.ContainsKey(data.id))
                        continue;

                    dataDic.Add(data.id, data);
                    dataList.Add(data);
                }
            }
        }

        public WorldBossData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"월드보스 데이터가 존재하지 않습니다: id = {id}");
                return null;
            }

            return dataDic[id];
        }

        public List<WorldBossData> GetList()
        {
            return dataList;
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