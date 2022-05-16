using MsgPack;
using UnityEngine;

namespace Ragnarok
{
    public sealed class EndlessTowerDataManager : Singleton<EndlessTowerDataManager>, IDataManger
    {
        /// <summary>
        /// Key: 층
        /// </summary>
        private readonly BetterList<EndlessTowerData> dataList;

        public ResourceType DataType => ResourceType.EndlessDungeonDataDB;
        public DungeonGroup DungeonGroupInfo { get; private set; }

        public EndlessTowerDataManager()
        {
            dataList = new BetterList<EndlessTowerData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Clear();
            DungeonGroupInfo = null;
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    EndlessTowerData data = new EndlessTowerData(mpo.AsList());
                    dataList.Add(data);
                }
            }
        }

        /// <summary>
        /// 데이터 배열 반환
        /// </summary>
        public EndlessTowerData[] GetArrayData()
        {
            return dataList.ToArray();
        }

        /// <summary>
        /// 해당 층에 따른 데이터 반환
        /// </summary>
        public EndlessTowerData GetByFloor(int floor)
        {
            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].GetFloor() == floor)
                    return dataList[i];
            }

            Debug.LogError($"엔들리스 데이터가 존재하지 않습니다: {nameof(floor)} = {floor}");
            return null;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            if (BasisOpenContetsType.EndlessTower.IsOpend())
            {
                DungeonGroupInfo = new DungeonGroup(DungeonType.EnlessTower, DungeonOpenConditionType.JobLevel, BasisType.ENDLESS_TOWER_LIMIT_JOB_LEVEL.GetInt());
            }
            else
            {
                DungeonGroupInfo = new DungeonGroup(DungeonType.EnlessTower, DungeonOpenConditionType.UpdateLater, 0);
            }
        }

        /// <summary>
        /// 데이터 검증
        /// </summary>
        public void VerifyData()
        {
        }
    }
}