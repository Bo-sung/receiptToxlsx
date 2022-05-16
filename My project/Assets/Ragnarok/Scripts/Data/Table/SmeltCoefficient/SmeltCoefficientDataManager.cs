using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class SmeltCoefficientDataManager : Singleton<SmeltCoefficientDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, List<SmeltCoefficientData>> dataDic;

        public ResourceType DataType => ResourceType.SmeltCoefficientDataDB;

        public SmeltCoefficientDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, List<SmeltCoefficientData>>(ObscuredIntEqualityComparer.Default);
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
            const int SMELT_COUNT = 4; // 각 id에 해당하는 제련 계수는 총 4개 (atk, matk, def, medf)

            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    SmeltCoefficientData data = new SmeltCoefficientData(mpo.AsList());

                    int id = data.id;

                    if (!dataDic.ContainsKey(id))
                        dataDic.Add(id, new List<SmeltCoefficientData>(SMELT_COUNT));

                    dataDic[id].Add(data);
                }
            }

#if UNITY_EDITOR
            var sb = StringBuilderPool.Get();
            foreach (var item in dataDic)
            {
                if (item.Value.Count == SMELT_COUNT)
                    continue;

                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append($"[seq 개수가 맞지 않음: id = {item.Key}, {nameof(item.Value.Count)} = {item.Value.Count}");
            }

            string log = StringBuilderPool.Release(sb);
            Debug.LogWarning(log);
#endif
        }

        /// <summary>
        /// 제련 값 반환
        /// </summary>
        public PlusStatus Get(BattleItemIndex itemIndex, int smelt)
        {
            if (itemIndex == default)
                return default;

            // 제련 수치가 0일 경우
            if (smelt == 0)
                return default;

            int id = (int)itemIndex;
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogWarning($"[올바르지 않은 {nameof(itemIndex)}] {nameof(itemIndex)} = {itemIndex}");
                return default;
            }

            int q1 = smelt / 1; // 1로 나눈 값
            int q2 = smelt / 2; // 2로 나눈 값
            int q5 = smelt / 5; // 5로 나눈 값
            int q10 = smelt / 10; // 10로 나눈 값

            List<SmeltCoefficientData> list = dataDic[id];
            return new PlusStatus
            {
                atk = GetSmeltValue(list[0], q1, q2, q5, q10),
                matk = GetSmeltValue(list[1], q1, q2, q5, q10),
                def = GetSmeltValue(list[2], q1, q2, q5, q10),
                mdef = GetSmeltValue(list[3], q1, q2, q5, q10),
            };
        }

        private int GetSmeltValue(SmeltCoefficientData data, int q1, int q2, int q5, int q10)
        {
            return data.q1 * q1 + data.q2 * q2 + data.q5 * q5 + data.q10 * q10;
        }

        public struct PlusStatus
        {
            /// <summary>
            /// 추가되는 물리공격력/마법공격력/물리방어력/마법방어력
            /// </summary>
            public int atk, matk, def, mdef;
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