using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using UnityEngine;

namespace Ragnarok
{
    public class CostumeDataManager : Singleton<CostumeDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, CostumeData> dataDic;

        public ResourceType DataType => ResourceType.CostumeDataDB;

        public CostumeDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, CostumeData>(ObscuredIntEqualityComparer.Default);
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
                    CostumeData data = new CostumeData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.costume_id))
                        dataDic.Add(data.costume_id, data);
                }
            }
        }

        public CostumeData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"코스튬 데이터가 존재하지 않습니다: id = {id}");                
                return null;
            }

            return dataDic[id];
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