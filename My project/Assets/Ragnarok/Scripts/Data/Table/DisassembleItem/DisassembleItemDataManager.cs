using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DisassembleItemDataManager : Singleton<DisassembleItemDataManager>, IDataManger
    {
        private readonly Dictionary<(ObscuredInt type, ObscuredInt rating), DisassembleItemData> dataDic;

        public ResourceType DataType => ResourceType.DisassembleItemDataDB;

        public DisassembleItemDataManager()
        {
            dataDic = new Dictionary<(ObscuredInt type, ObscuredInt rating), DisassembleItemData>(ObscuredIntTupleEqualityComparer.Default);
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
                    DisassembleItemData data = new DisassembleItemData(mpo.AsList());

                    if (!dataDic.ContainsKey((data.type, data.rating)))
                        dataDic.Add((data.type, data.rating), data);
                }
            }
        }

        public DisassembleItemData Get(int type, int rating)
        {
            if (!dataDic.ContainsKey((type, rating)))
            {
                Debug.LogError($"분해 데이터가 존재하지 않습니다: type = {type}, rating = {rating}");
                return null;
            }

            return dataDic[(type, rating)];
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
