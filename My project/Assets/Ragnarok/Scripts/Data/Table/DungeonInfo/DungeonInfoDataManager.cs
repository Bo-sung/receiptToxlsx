using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class DungeonInfoDataManager : Singleton<DungeonInfoDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, DungeonInfoData> dic; // Key: Id, Value: Data

        public ResourceType DataType => ResourceType.DungeonInfoDataDB;

        public DungeonInfoDataManager()
        {
            dic = new Dictionary<ObscuredInt, DungeonInfoData>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public DungeonInfoData Get(int id)
        {
            if (!dic.ContainsKey(id))
            {
                Debug.LogError($"던전 인포 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dic[id];
        }

        public void ClearData()
        {
            dic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    DungeonInfoData data = new DungeonInfoData(mpo.AsList());
                    dic.Add(data.id, data);
                }
            }
        }

        public void Initialize()
        {

        }

        public void VerifyData()
        {

        }
    }
}