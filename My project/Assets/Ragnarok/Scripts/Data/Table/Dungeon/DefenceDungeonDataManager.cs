using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DefenceDungeonDataManager : Singleton<DefenceDungeonDataManager>, IDataManger
    {
        private readonly Dictionary<int, DefenceDungeonData> dataDic;
        private readonly List<DefenceDungeonData> dataList;

        public ResourceType DataType => ResourceType.DefenceDungeonDataDB;

        public DefenceDungeonDataManager()
        {
            dataDic = new Dictionary<int, DefenceDungeonData>(IntEqualityComparer.Default);
            dataList = new List<DefenceDungeonData>();
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
                    DefenceDungeonData data = new DefenceDungeonData(mpo.AsList());

                    if (dataDic.ContainsKey(data.id))
                        continue;

                    dataDic.Add(data.id, data);
                    dataList.Add(data);
                }
            }
        }

        public DefenceDungeonData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"디펜스 던전 데이터가 존재하지 않습니다: id = {id}");
                return null;
            }

            return dataDic[id];
        }

        public List<DefenceDungeonData> GetList()
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