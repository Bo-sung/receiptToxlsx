using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class PvERankRewardDataManager : Singleton<PvERankRewardDataManager>, IDataManger
    {
        private readonly List<PvERankRewardData> dataList;
        private readonly List<PvERankRewardData> singleDataList;

        public ResourceType DataType => ResourceType.PvERankRewardDataDB;

        public PvERankRewardDataManager()
        {
            dataList = new List<PvERankRewardData>();
            singleDataList = new List<PvERankRewardData>();
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
            singleDataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    PvERankRewardData data = new PvERankRewardData(mpo.AsList());
                    if (data.group_id == 0) // 협동 대전 보상
                    {
                        dataList.Add(data);
                    }
                    else if (data.group_id == 1) // 싱글 대전 보상
                    {
                        singleDataList.Add(data);
                    }
                }
            }

            dataList.Sort(SortByRank);
            singleDataList.Sort(SortByRank);
        }

        public PvERankRewardData[] GetAgentRewards()
        {
            return dataList.ToArray();
        }

        public PvERankRewardData[] GetSingleRewards()
        {
            return singleDataList.ToArray();
        }

        public PvERankRewardData Get(int rank)
        {
            // ranking_value 가 낮은 순으로 정렬되어 있는 상태 (단, 참가보상은 맨 마지막에 위치)
            for (int i = 0; i < dataList.Count; i++)
            {
                // 마지막의 경우 참가보상이므로 그냥 반환
                if (dataList[i].IsEntryReward())
                    return dataList[i];

                // 참여보상이 아닌 조건 추가
                if (rank > 0 && rank <= dataList[i].ranking_value)
                    return dataList[i];
            }

            return null;
        }

        public PvERankRewardData GetSingleReward(int rank)
        {
            // ranking_value 가 낮은 순으로 정렬되어 있는 상태 (단, 참가보상은 맨 마지막에 위치)
            for (int i = 0; i < singleDataList.Count; i++)
            {
                // 마지막의 경우 참가보상이므로 그냥 반환
                if (singleDataList[i].IsEntryReward())
                    return singleDataList[i];

                // 참여보상이 아닌 조건 추가
                if (rank > 0 && rank <= singleDataList[i].ranking_value)
                    return singleDataList[i];
            }

            return null;
        }

        private int SortByRank(PvERankRewardData x, PvERankRewardData y)
        {
            // 참가 보상
            if (y.IsEntryReward())
                return -1;

            return x.ranking_value.CompareTo(y.ranking_value); // 낮은 랭킹 순으로 정렬
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