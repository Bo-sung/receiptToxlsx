using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class CardOptionDataManager : Singleton<CardOptionDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, CardOptionData> dataDic;

        public ResourceType DataType => ResourceType.CardOptionDataDB;

        public CardOptionDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, CardOptionData>(ObscuredIntEqualityComparer.Default);
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
                    CardOptionData data = new CardOptionData(mpo.AsList());
                    dataDic.Add(data.id, data);
                }
            }
        }

        public CardOptionData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"42.카드옵션 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
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