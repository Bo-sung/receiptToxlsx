using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ClickerDungeonDataManager : Singleton<ClickerDungeonDataManager>, IDataManger, IEqualityComparer<DungeonType>
    {
        private readonly Dictionary<int, ClickerDungeonData> dataDic;
        private readonly Dictionary<DungeonType, BetterList<ClickerDungeonData>> dataListDic;

        public ResourceType DataType => ResourceType.ClickerDungeonDataDB;

        public ClickerDungeonDataManager()
        {
            dataDic = new Dictionary<int, ClickerDungeonData>(IntEqualityComparer.Default);
            dataListDic = new Dictionary<DungeonType, BetterList<ClickerDungeonData>>();
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
            dataListDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ClickerDungeonData data = new ClickerDungeonData(mpo.AsList());
                    dataDic.Add(data.id, data);

                    DungeonType dungeonType = data.type.ToEnum<DungeonType>();
                    if (!dataListDic.ContainsKey(dungeonType))
                        dataListDic.Add(dungeonType, new BetterList<ClickerDungeonData>());

                    dataListDic[dungeonType].Add(data);
                }
            }
        }

        public ClickerDungeonData Get(int id)
        {
            if (dataDic.ContainsKey(id))
                return dataDic[id];

            Debug.Log($"클리커 데이터가 존재하지 않습니다: {nameof(id)}] {nameof(id)} = {id}");
            return null;
        }

        public ClickerDungeonData[] GetArray(DungeonType dungeonType)
        {
            if (dataListDic.ContainsKey(dungeonType))
                return dataListDic[dungeonType].ToArray();

            Debug.LogError($"정의되지 않은 던전 타입: {nameof(dungeonType)} = {dungeonType}");
            return null;
        }

        /// <summary>
        /// 특정 id에 해당하는 index 반환
        /// </summary>
        public int GetIndex(DungeonType dungeonType, int id)
        {
            if (dataListDic.ContainsKey(dungeonType))
            {
                BetterList<ClickerDungeonData> list = dataListDic[dungeonType];
                for (int i = 0; i < list.size; i++)
                {
                    if (list[i].id == id)
                        return i;
                }
            }
            else
            {
                Debug.LogError($"정의되지 않은 던전 타입: {nameof(dungeonType)} = {dungeonType}");
            }

            return -1;
        }

        /// <summary>
        /// 인덱스로 데이터 반환 (0부터 maxCount까지)
        /// </summary>
        public ClickerDungeonData GetByIndex(DungeonType dungeonType, int index)
        {
            if (index < 0 || index > GetMaxIndex(dungeonType))
                return null;

            if (dataListDic.ContainsKey(dungeonType))
                return dataListDic[dungeonType][index];

            Debug.LogError($"정의되지 않은 던전 타입: {nameof(dungeonType)} = {dungeonType}");
            return null;
        }

        /// <summary>
        /// 인덱스로 데이터 반환 (0부터 maxCount까지)
        /// </summary>
        public int GetMaxIndex(DungeonType dungeonType)
        {
            if (dataListDic.ContainsKey(dungeonType))
                return dataListDic[dungeonType].size - 1;

            Debug.LogError($"정의되지 않은 던전 타입: {nameof(dungeonType)} = {dungeonType}");
            return -1;
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
#if UNITY_EDITOR

#endif
        }

        bool IEqualityComparer<DungeonType>.Equals(DungeonType x, DungeonType y)
        {
            return x == y;
        }

        int IEqualityComparer<DungeonType>.GetHashCode(DungeonType obj)
        {
            return obj.GetHashCode();
        }
    }
}