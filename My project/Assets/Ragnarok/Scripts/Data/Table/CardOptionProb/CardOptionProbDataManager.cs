using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class CardOptionProbDataManager : Singleton<CardOptionProbDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, CardOptionProbData> dataDic;

        public ResourceType DataType => ResourceType.CardOptionProbDataDB;

        public CardOptionProbDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, CardOptionProbData>(ObscuredIntEqualityComparer.Default);
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
                    CardOptionProbData data = new CardOptionProbData(mpo.AsList());
                    dataDic.Add(data.id, data);
                }
            }
        }

        public CardOptionProbData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"43.카드옵션확률 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
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