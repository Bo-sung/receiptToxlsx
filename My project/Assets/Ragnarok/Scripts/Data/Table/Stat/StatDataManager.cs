using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class StatDataManager : Singleton<StatDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, StatData> dataDic;

        public ResourceType DataType => ResourceType.StatDataDB;

        private ObscuredInt possibleRebirthLv = -1;  // 전승 가능한 레벨
        /// <summary>전승 가능한 레벨</summary>
        public int PossibleRebirthLv { get { return possibleRebirthLv; } }

        public StatDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, StatData>(ObscuredIntEqualityComparer.Default);
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
                    StatData data = new StatData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.base_lv))
                        dataDic.Add(data.base_lv, data);

                    if (possibleRebirthLv == -1 && data.transmission_stat > 0)
                    {
                        possibleRebirthLv = data.base_lv;
                    }
                }
            }
        }

        public StatData Get(int baseLv)
        {
            if (!dataDic.ContainsKey(baseLv))
                throw new System.ArgumentException($"스탯 데이터가 존재하지 않습니다: baseLv = {baseLv}");

            return dataDic[baseLv];
        }

        public int GetTotalPoint(int level)
        {
            int totalPoint = 0;
            foreach (var item in dataDic.Values)
            {
                totalPoint += item.stat;

                if (item.base_lv == level)
                    break;
            }

            return totalPoint;
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