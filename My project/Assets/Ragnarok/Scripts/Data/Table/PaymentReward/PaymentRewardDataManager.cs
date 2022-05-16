using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public class PaymentRewardDataManager : Singleton<PaymentRewardDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, PaymentRewardData> dataDic;
        private int maxMileage;

        public ResourceType DataType => ResourceType.PaymentRewardDataDB;

        public PaymentRewardDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, PaymentRewardData>(ObscuredIntEqualityComparer.Default);
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
                    PaymentRewardData data = new PaymentRewardData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);

                    if(data.visable == 1)
                    {
                        if(data.needPoint >= maxMileage)
                        {
                            maxMileage = data.needPoint;
                        }
                    }
                }
            }
        }

        public PaymentRewardData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
                throw new System.ArgumentException($"58.누적결제 보상이 존재하지 않습니다: id = {id}");

            return dataDic[id];
        }

        public PaymentRewardData[] Gets(int mileage)
        {
            return dataDic.Values.Where(x => x.visable <= mileage).ToArray();
        }

        public PaymentRewardData Get(int mileage, int step)
        {
            return dataDic.Values.FirstOrDefault(x => x.visable <= mileage && x.GetStep() == step);
        }

        public int GetMaxMileage()
        {
            return maxMileage;
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
            foreach (var item in dataDic.Values)
            {
                if (item.reward_type.ToEnum<RewardType>() == RewardType.Item)
                {
                    if (ItemDataManager.Instance.Get(item.reward_value) == null)
                        throw new System.Exception($"58.누적결제보상 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value}");
                }
            }
            #endregion
        }
    }
}