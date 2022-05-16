using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class PvETierDataManager : Singleton<PvETierDataManager>, IDataManger
    {
        private readonly Dictionary<int, PvETierData> dataDic;
        private readonly List<PvETierData> dataList;
        public const string SINGLE_GRADE_ICON_NAME = "RankSingleTier";

        public ResourceType DataType => ResourceType.PvETierDataDB;

        public PvETierDataManager()
        {
            dataDic = new Dictionary<int, PvETierData>(IntEqualityComparer.Default);
            dataList = new List<PvETierData>();
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
                    PvETierData data = new PvETierData(mpo.AsList());

                    if (dataDic.ContainsKey(data.id))
                        continue;

                    dataDic.Add(data.id, data);
                    dataList.Add(data);
                }
            }

            dataList.Sort(SortByTier);
        }

        public PvETierData Get(int tier)
        {
            if (!dataDic.ContainsKey(tier))
            {
                Debug.LogError($"티어 데이터가 존재하지 않습니다: {nameof(tier)} = {tier}");
                return null;
            }

            return dataDic[tier];
        }

        public int GetTier(int score)
        {
            // tier_value 가 높은 순으로 정렬되어 있는 상태
            for (int i = 0; i < dataList.Count; i++)
            {
                if (score >= dataList[i].tier_value)
                    return dataList[i].id;
            }

            return 1;
        }

        public PvETierData[] GetArray()
        {
            return dataList.ToArray();
        }

        private int SortByTier(PvETierData x, PvETierData y)
        {
            return y.tier_value.CompareTo(x.tier_value); // 높은 티어 순으로 정렬
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