using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class FreeFightRewardDataManager : Singleton<FreeFightRewardDataManager>, IDataManger, IEqualityComparer<FreeFightEventType>
    {
        private readonly Dictionary<FreeFightEventType, BetterList<FreeFightRewardData>> dataDic;

        public ResourceType DataType => ResourceType.FreeFightRewardDataDB;

        public FreeFightRewardDataManager()
        {
            dataDic = new Dictionary<FreeFightEventType, BetterList<FreeFightRewardData>>(this);
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
                    FreeFightRewardData data = new FreeFightRewardData(mpo.AsList());
                    FreeFightEventType eventType = data.event_type.ToEnum<FreeFightEventType>();
                    if (!dataDic.ContainsKey(eventType))
                        dataDic.Add(eventType, new BetterList<FreeFightRewardData>());

                    dataDic[eventType].Add(data);
                }
            }

            foreach (var item in dataDic.Values)
            {
                item.Sort((a, b) => b.kill_count.CompareTo(a.kill_count));
            }
        }

        public FreeFightRewardData[] GetArrayData(FreeFightEventType eventType)
        {
            if (dataDic.ContainsKey(eventType))
                return dataDic[eventType].ToArray();

            return null;
        }

        public RewardData[] GetRewards(FreeFightEventType eventType, int killCount)
        {
            if (dataDic.ContainsKey(eventType))
            {
                BetterList<FreeFightRewardData> dataList = dataDic[eventType];
                foreach (var item in dataList)
                {
                    if (killCount >= item.kill_count)
                        return item.GetRewards();
                }
            }

            return null;
        }

        /// <summary>
        /// 다음 보상까지 남은 Kill Count 수
        /// </summary>
        public int GetNextRemainKillCount(FreeFightEventType eventType, int killCount)
        {
            if (dataDic.ContainsKey(eventType))
            {
                BetterList<FreeFightRewardData> dataList = dataDic[eventType];
                for (int i = dataList.size - 1; i >= 0; i--)
                {
                    if (killCount < dataList[i].kill_count)
                        return dataList[i].kill_count - killCount;
                }
            }

            return 0;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
            #region UNITY_EDITOR
            foreach (var dataList in dataDic.Values)
            {
                foreach (var item in dataList)
                {
                    if (item.reward1_type.ToEnum<RewardType>() == RewardType.Item)
                    {
                        if (ItemDataManager.Instance.Get(item.reward1_value) == null)
                            throw new System.Exception($"59.난전보상 테이블 오류 ID={item.id}, 없는 아이템={item.reward1_value}");
                    }

                    if (item.reward2_type.ToEnum<RewardType>() == RewardType.Item)
                    {
                        if (ItemDataManager.Instance.Get(item.reward2_value) == null)
                            throw new System.Exception($"59.난전보상 테이블 오류 ID={item.id}, 없는 아이템={item.reward2_value}");
                    }
                }
            }
            #endregion
        }

        bool IEqualityComparer<FreeFightEventType>.Equals(FreeFightEventType x, FreeFightEventType y)
        {
            return x == y;
        }

        int IEqualityComparer<FreeFightEventType>.GetHashCode(FreeFightEventType obj)
        {
            return obj.GetHashCode();
        }
    }
}