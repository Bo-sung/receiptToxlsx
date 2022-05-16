using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class MvpDataManager : Singleton<MvpDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, MvpData> dataDic; // key: Id, Value: Data
        private readonly Dictionary<ObscuredInt, MvpData[]> arrDataDic; // key: GroupId, Value: DataArray

        public ResourceType DataType => ResourceType.MvpDataDB;

        public MvpDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, MvpData>(ObscuredIntEqualityComparer.Default);
            arrDataDic = new Dictionary<ObscuredInt, MvpData[]>(ObscuredIntEqualityComparer.Default);
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
            arrDataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    MvpData data = new MvpData(mpo.AsList());
                    dataDic.Add(data.id, data);
                }
            }
        }

        public MvpData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"MVP 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dataDic[id];
        }

        public MvpData[] GetArrayGroup(int groupId)
        {
            if (!arrDataDic.ContainsKey(groupId))
            {
                Debug.LogError($"MVP 데이터가 존재하지 않습니다: {nameof(groupId)} = {groupId}");
                return null;
            }

            return arrDataDic[groupId];
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            Dictionary<int, Buffer<MvpData>> tempBufferDic = new Dictionary<int, Buffer<MvpData>>(IntEqualityComparer.Default);
            foreach (var item in dataDic.Values)
            {
                int groupId = item.group_id;
                if (!tempBufferDic.ContainsKey(groupId))
                    tempBufferDic.Add(groupId, new Buffer<MvpData>());

                tempBufferDic[groupId].Add(item);
            }

            foreach (var item in tempBufferDic)
            {
                arrDataDic.Add(item.Key, item.Value.GetBuffer(isAutoRelease: true));
            }

            tempBufferDic.Clear();
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                if (MonsterDataManager.Instance.Get(item.monster_id) == null)
                {
                    throw new System.Exception($"40.MVP 테이블 오류 ID={item.id}, 없는 몬스터={item.monster_id}");
                }
            }
#endif
        }
    }
}