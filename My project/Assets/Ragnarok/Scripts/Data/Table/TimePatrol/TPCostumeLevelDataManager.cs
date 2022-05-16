using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TPCostumeLevelDataManager : Singleton<TPCostumeLevelDataManager>, IDataManger
    {
        private readonly Dictionary<(int type, int level), TPCostumeLevelData> dataDic;

        public ResourceType DataType => ResourceType.TPCostumeLevelDataDB;

        public TPCostumeLevelDataManager()
        {
            dataDic = new Dictionary<(int type, int level), TPCostumeLevelData>();
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
                    TPCostumeLevelData data = new TPCostumeLevelData(mpo.AsList());

                    if (dataDic.ContainsKey((data.costume_type, data.smelt_level)))
                        continue;

                    dataDic.Add((data.costume_type, data.smelt_level), data);
                }
            }
        }

        public TPCostumeLevelData Get(int type, int level)
        {
            if (!dataDic.ContainsKey((type, level)))
            {
                Debug.LogError($"82.합체코스튬강화테이블 존재하지 않습니다: {nameof(type)} = {type}, {nameof(level)} = {level}");
                return null;
            }

            return dataDic[(type, level)];
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