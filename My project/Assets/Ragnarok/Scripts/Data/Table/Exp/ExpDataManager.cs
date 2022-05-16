using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class ExpDataManager : Singleton<ExpDataManager>, IDataManger
    {
        public enum ExpType
        {
            CharacterBase,
            CharacterJob,
            Cupet,
        }

        public struct Output
        {
            public int level;
            public long curExp;
            public long maxExp;
        }

        private readonly Dictionary<ObscuredInt, ExpData> dataDic;
        private readonly Buffer<int> expBuffer;

        public ResourceType DataType => ResourceType.ExpDataDB;

        public ExpDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, ExpData>(ObscuredIntEqualityComparer.Default);
            expBuffer = new Buffer<int>();
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
            expBuffer.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ExpData data = new ExpData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.level))
                        dataDic.Add(data.level, data);
                }
            }
        }        

        /// <summary>
        /// 큐펫 경험치 표기
        /// </summary>
        public string GetCupetExpString(long totalExp)
        {
            long lastExp = 0;
            long maxExp = 0;
            foreach (var item in dataDic.Values)
            {
                if (totalExp < item.cupet_lv_need_exp)
                {
                    maxExp = item.cupet_lv_need_exp;
                    break;
                }

                lastExp = item.cupet_lv_need_exp;
            }

            if (maxExp == 0) // 만렙인 경우 (테이블에 더 높은 exp가 없음)
                return LocalizeKey._19011.ToText(); // MAX

            return LocalizeKey._19010.ToText() // {VALUE}/{MAX}
                .Replace(ReplaceKey.VALUE, totalExp - lastExp)
                .Replace(ReplaceKey.MAX, maxExp - lastExp);
        }

        /// <summary>
        /// 큐펫 경험치 퍼센트
        /// </summary>
        public float GetCupetExpPercent(long totalExp)
        {
            long lastExp = 0;
            long maxExp = 0;
            foreach (var item in dataDic.Values)
            {
                if (totalExp < item.cupet_lv_need_exp)
                {
                    maxExp = item.cupet_lv_need_exp;
                    break;
                }

                lastExp = item.cupet_lv_need_exp;
            }

            if (maxExp == 0)
                return 1f;

            return MathUtils.GetProgress(totalExp - lastExp, maxExp - lastExp);
        }

        /// <summary>
        /// 소환에 필요한 조각 개수
        /// </summary>
        public int GetNeedSummonPieceCount()
        {
            return BasisType.CUPET_NEED_MON_PIECE.GetInt(1); // 소환에 필요한 조각 개수
        }        

        public Output Get(long totalExp, ExpType expType)
        {
            int level = 0;
            long curExp = 0;
            long maxExp = 0;

            long preNeedExp = 0;
            foreach (ExpData data in dataDic.Values)
            {
                long needExp = data.GetNeedExp(expType);

                level = data.level;
                curExp = totalExp - preNeedExp;
                maxExp = needExp - preNeedExp;

                if (totalExp < needExp)
                    return new Output { level = level, curExp = curExp, maxExp = maxExp, };

                preNeedExp = data.GetNeedExp(expType);
            }

            return new Output { level = level, curExp = curExp, maxExp = maxExp, };
        }

        public ExpData Get(int level)
        {
            if (dataDic.ContainsKey(level))
                return dataDic[level];

            return null;
        }

        public int[] GetMaxPoints(ExpType expType, int maxLevel)
        {
            long preNeedExp = 0;
            foreach (ExpData data in dataDic.Values)
            {
                // 제한레벨
                if (data.level >= maxLevel)
                    break;

                long needExp = data.GetNeedExp(expType);

                // 데이터 없음
                if (needExp == 0)
                    break;

                expBuffer.Add((int)(needExp - preNeedExp));

                preNeedExp = needExp;
            }

            return expBuffer.GetBuffer(isAutoRelease: true);
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