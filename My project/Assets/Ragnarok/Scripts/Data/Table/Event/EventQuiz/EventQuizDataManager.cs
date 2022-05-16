using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class EventQuizDataManager : Singleton<EventQuizDataManager>, IDataManger
    {
        private readonly BetterList<EventQuizData> dataList;
        private readonly Dictionary<int, int> maxSeqDic;

        public ResourceType DataType => ResourceType.EventQuizDataDB;

        public EventQuizDataManager()
        {
            dataList = new BetterList<EventQuizData>();
            maxSeqDic = new Dictionary<int, int>(IntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Release();
            maxSeqDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    EventQuizData data = new EventQuizData(mpo.AsList());
                    dataList.Add(data);

                    int startData = data.start_date;
                    if (maxSeqDic.ContainsKey(startData))
                    {
                        //maxSeqDic[dailyGroup] = Mathf.Max(data.seq, maxSeqDic[dailyGroup]);
                        maxSeqDic[startData] = data.seq; // maxSeq로 변경
                    }
                    else
                    {
                        maxSeqDic.Add(startData, data.seq);
                    }
                }
            }
        }

        public EventQuizData Get(int startData, int index)
        {
            if (startData == 0)
                return null;

            int seq = index + 1; // 시퀀스 (데이터 시퀀스는 1부터 시작)
            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].start_date == startData && dataList[i].seq == seq)
                    return dataList[i];
            }

            return null;
        }

        public void Initialize()
        {
            int startData;
            for (int i = 0; i < dataList.size; i++)
            {
                startData = dataList[i].start_date;
                if (!maxSeqDic.ContainsKey(startData))
                    continue;

                dataList[i].SetMaxSeq(maxSeqDic[startData]);
            }

            maxSeqDic.Clear();
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var quizData in dataList)
            {
                if (quizData is View.QuizQuizView.IInput item)
                {
                    if (item.Reward.RewardType == RewardType.Item)
                    {
                        if (ItemDataManager.Instance.Get(item.Reward.ItemId) == null)
                        {
                            throw new System.Exception($"69.퀴즈 테이블 ID={quizData.id}, 없는 아이템={item.Reward.ItemId}");
                        }
                    }
                }
            }
#endif
        }
    }
}