#define CHALLENGE

using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

#if CHALLENGE
using System.Linq;
#endif

namespace Ragnarok
{
    public sealed class PanelBuffDataManager : Singleton<PanelBuffDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, PanelBuffData> dataDic;
        public ResourceType DataType => ResourceType.PanelBuffDataDB;

        public PanelBuffDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, PanelBuffData>(ObscuredIntEqualityComparer.Default);
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
                    PanelBuffData data = new PanelBuffData(mpo.AsList());

                    // Add DataDic
                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public PanelBuffData Get(int id)
        {
            if (dataDic.ContainsKey(id))
                return dataDic[id];

            Debug.Log($"패널버프 데이터가 존재하지 않습니다: {nameof(id)}] {nameof(id)} = {id}");
            return null;
        }

#if CHALLENGE
        public PanelBuffData[] GetArray()
        {
            Buffer<PanelBuffData> buffer = new Buffer<PanelBuffData>();

            foreach (var item in dataDic.Values)
            {
                BattleOptionType battleOptionType = item.battle_option_type.ToEnum<BattleOptionType>();

                // 즉발 옵션 임시 제거
                if (battleOptionType.IsActiveOption())
                    continue;

                // 특정 조건 옵션 임시 제거
                if (battleOptionType.IsConditionalOption())
                    continue;

                // 이미지 없는 옵션 임시 제거
                string spriteName = battleOptionType.ToSpriteName();
                if (string.IsNullOrEmpty(spriteName))
                    continue;

                buffer.Add(item);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }
#endif

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