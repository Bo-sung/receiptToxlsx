using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class NabihoIntimacyDataManager : Singleton<NabihoIntimacyDataManager>, IDataManger
    {
        private readonly Dictionary<int, NabihoIntimacyData> levelDataDic; // 레벨에 따른 데이터
        private readonly BetterList<NabihoIntimacyData> dataList;
        private int maxLevel;

        public ResourceType DataType => ResourceType.NabihoIntimacyDataDB;

        public NabihoIntimacyDataManager()
        {
            levelDataDic = new Dictionary<int, NabihoIntimacyData>(IntEqualityComparer.Default);
            dataList = new BetterList<NabihoIntimacyData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            levelDataDic.Clear();
            dataList.Clear();
            maxLevel = 0;
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    NabihoIntimacyData data = new NabihoIntimacyData(mpo.AsList());
                    levelDataDic.Add(data.IntimacyLevel, data);
                    dataList.Add(data);
                }
            }
        }

        public NabihoIntimacyData Get(int level)
        {
            if (!levelDataDic.ContainsKey(level))
            {
                Debug.LogError($"92.나비호 친밀도 데이터가 존재하지 않습니다: {nameof(level)} = {level}");
                return null;
            }

            return levelDataDic[level];
        }

        public int GetLevel(int exp)
        {
            for (int i = 0; i < dataList.size; i++)
            {
                if (exp < dataList[i].TotalNeedExp)
                    return dataList[i].IntimacyLevel;
            }

            return 0;
        }

        public int GetMaxLevel()
        {
            return maxLevel;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            int preTotalNeedExp = 0;
            for (int i = 0; i < dataList.size; i++)
            {
                maxLevel = Mathf.Max(maxLevel, dataList[i].IntimacyLevel); // 최대 레벨 세팅
                dataList[i].SetPreTotalNeedExp(preTotalNeedExp);

                preTotalNeedExp = dataList[i].TotalNeedExp;
            }

            NabihoIntimacyData data = Get(maxLevel);
            if (data != null)
                data.SetMaxLevel();
        }

        public void VerifyData()
        {
        }
    }
}