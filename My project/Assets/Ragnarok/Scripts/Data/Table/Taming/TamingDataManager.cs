using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="TamingData"/>
    /// </summary>
    public sealed class TamingDataManager : Singleton<TamingDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, TamingData> dataDic;

        public ResourceType DataType => ResourceType.TamingDataDB;

        public TamingDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, TamingData>(ObscuredIntEqualityComparer.Default);
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
                    TamingData data = new TamingData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public TamingData Get(int id)
        {
            if (dataDic.ContainsKey(id))
                return dataDic[id];

            Debug.LogError($"테이밍 데이터가 존재하지 않습니다: {nameof(id)}] {nameof(id)} = {id}");
            return null;
        }

        public TamingData Get(int rotationValue, int dayType)
        {
            var data = dataDic.Values.ToList()
                .Find(e => e.rotation_value == rotationValue && e.day_type == dayType);

            if (data == null)
            {
                Debug.LogError($"테이밍 데이터가 존재하지 않습니다: {nameof(rotationValue)} = {rotationValue} / {nameof(dayType)} = {dayType}");
                return null;
            }

            return data;
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

#if UNITY_EDITOR
        /// <summary>
        /// 아이템 재료 반환
        /// </summary>
        [System.Obsolete]
        public IEnumerable<int> GetItemMaterialId()
        {
            foreach (var item in dataDic.Values)
            {
                yield return item.use_item_id;
            }
        }
#endif
    }
}