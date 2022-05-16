using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BotCoefficientDataManager : Singleton<BotCoefficientDataManager>, IDataManger, IEqualityComparer<BattleOptionType>
    {
        private readonly Dictionary<BattleOptionType, BotCoefficientData> dataDic;

        public ResourceType DataType => ResourceType.BotCoefficientDataDB;

        public BotCoefficientDataManager()
        {
            dataDic = new Dictionary<BattleOptionType, BotCoefficientData>(this);
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
                    BotCoefficientData data = new BotCoefficientData(mpo.AsList());

                    BattleOptionType type = data.id.ToEnum<BattleOptionType>();
                    if (!dataDic.ContainsKey(type))
                        dataDic.Add(type, data);
                }
            }

#if UNITY_EDITOR
            var sb = StringBuilderPool.Get();
            foreach (BattleOptionType type in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (type == BattleOptionType.None)
                    continue;

                if (dataDic.ContainsKey(type))
                    continue;

                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append($"[{nameof(BattleOptionType)}에 해당하는 제련 테이블이 존재하지 않음: {nameof(type)} = {type}");
            }

            string log = StringBuilderPool.Release(sb);
            Debug.LogWarning(log);
#endif
        }

        public PlusStatus Get(BattleOptionType type, int smelt)
        {
            if (type == 0)
                return default;

            if (!dataDic.ContainsKey(type))
            {
                Debug.LogWarning($"[올바르지 않은 {nameof(type)}] {nameof(type)} = {type}");
                return default;
            }

            int q1 = smelt / 1; // 1로 나눈 값
            int q2 = smelt / 2; // 2로 나눈 값
            int q5 = smelt / 5; // 5로 나눈 값
            int q10 = smelt / 10; // 10로 나눈 값

            BotCoefficientData data = dataDic[type];
            return new PlusStatus
            {
                value1 = data.q1_value1 * q1 + data.q2_value1 * q2 + data.q5_value1 * q5 + data.q10_value1 * q10,
                value2 = data.q1_value2 * q1 + data.q2_value2 * q2 + data.q5_value2 * q5 + data.q10_value2 * q10,
            };
        }

        bool IEqualityComparer<BattleOptionType>.Equals(BattleOptionType x, BattleOptionType y)
        {
            return x == y;
        }

        int IEqualityComparer<BattleOptionType>.GetHashCode(BattleOptionType obj)
        {
            return obj.GetHashCode();
        }

        public struct PlusStatus
        {
            /// <summary>
            /// 추갸되는 Value1, Value2
            /// </summary>
            public int value1, value2;
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