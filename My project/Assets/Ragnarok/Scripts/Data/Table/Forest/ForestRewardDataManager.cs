using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ForestRewardDataManager : Singleton<ForestRewardDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, ForestRewardData> dataDic;

        public ResourceType DataType => ResourceType.ForestRewardDataDB;

        public ForestRewardDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, ForestRewardData>(ObscuredIntEqualityComparer.Default);
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
                    ForestRewardData data = new ForestRewardData(mpo.AsList());
                    dataDic.Add(data.id, data);
                }
            }
        }

        public ForestRewardData Get(int id)
        {
            if (id == 0)
                return null;

            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"79.미궁숲 선택보상 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
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