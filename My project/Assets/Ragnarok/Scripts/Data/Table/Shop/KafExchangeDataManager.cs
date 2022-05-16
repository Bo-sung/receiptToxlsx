using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class KafExchangeDataManager : Singleton<KafExchangeDataManager>, IDataManger
    {
        private readonly Dictionary<int, KafExchangeData> dataDic;

        public ResourceType DataType => ResourceType.KafExchangeDataDB;

        public KafExchangeDataManager()
        {
            dataDic = new Dictionary<int, KafExchangeData>(IntEqualityComparer.Default);
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
                    KafExchangeData data = new KafExchangeData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public KafExchangeData Get(int id)
        {
            if (id == 0)
                return null;

            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"083.카프라교환 데이터가 존재하지 않습니다: id = {id}");
                return null;
            }

            return dataDic[id];
        }

        public KafExchangeData[] GetArrayData(KafraType kafraType)
        {
            BetterList <KafExchangeData> list = new BetterList<KafExchangeData>();
            int type = kafraType.ToIntValue();
            foreach (var item in dataDic.Values)
            {
                if (item.type == type)
                {
                    list.Add(item);
                }
            }
            list.Sort((x, y) => x.sort.CompareTo(y.sort));
            return list.ToArray();
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// 데이터 검증
        /// </summary>
        public void VerifyData()
        {
        }
    }
}