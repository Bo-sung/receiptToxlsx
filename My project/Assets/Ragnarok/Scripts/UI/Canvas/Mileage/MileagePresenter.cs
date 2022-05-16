using Ragnarok.View;
using System;
using System.Collections.Generic;

namespace Ragnarok
{
    public class MileagePresenter : ViewPresenter, MileageSlot.Impl
    {
        /******************** Models ********************/
        private readonly ShopModel shopModel;

        /******************** Repositories ********************/
        private readonly PaymentRewardDataManager paymentRewardDataRepo;
        private readonly ItemDataManager itemDataRepo;
        private readonly BoxDataManager boxDataRepo;

        private readonly List<int> mileageList;

        /******************** Event ********************/
        public event Action OnUpdateMileageReward
        {
            add { shopModel.OnUpdateMileageReward += value; }
            remove { shopModel.OnUpdateMileageReward -= value; }
        }

        public MileagePresenter()
        {
            shopModel = Entity.player.ShopModel;
            paymentRewardDataRepo = PaymentRewardDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            boxDataRepo = BoxDataManager.Instance;
            mileageList = new List<int>();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void OnClickedBtnGetReward()
        {
            shopModel.RequestMileageReward().WrapNetworkErrors();
        }

        public int GetCurStep()
        {
            return shopModel.MileageRewardStep;
        }

        public int GetCurMileage()
        {
            return shopModel.Mileage;
        }

        public MileageSlot.IInput[] GetInputs()
        {
            PaymentRewardData[] inputs = paymentRewardDataRepo.Gets(GetCurMileage());

            mileageList.Clear();
            for (int i = 0; i < inputs.Length; i++)
            {
                mileageList.Add(inputs[i].needPoint);
            }

            return inputs;
        }

        public string GetMileageText()
        {
            return GetCurMileage().ToString();
        }

        public MileageSlot.MileageCompleteType GetCompleteType(int step, int needPoint)
        {
            if (step <= GetCurStep())
            {
                return MileageSlot.MileageCompleteType.ReceivedReward;
            }

            if (step == GetCurStep() + 1 && needPoint <= GetCurMileage())
            {
                return MileageSlot.MileageCompleteType.StandByReward;
            }

            return MileageSlot.MileageCompleteType.InProgress;
        }

        public int GetPreMileage(int mileage)
        {
            for (int i = mileageList.Count - 1; i >= 0; i--)
            {
                if (mileage > mileageList[i])
                {
                    return mileageList[i];
                }
            }
            return 0;
        }

        public float GetPercent()
        {
            int curMileage = GetCurMileage();
            int preValue = 0;
            foreach (var item in mileageList)
            {
                if (item > curMileage)
                {
                    return MathUtils.GetProgress(curMileage - preValue, item - preValue);
                }
                preValue = item;
            }
            return 0f;
        }

        public RewardData[] GetRewards(int itemid)
        {
            ItemData item = itemDataRepo.Get(itemid);

            // 없는 아이템이거나 상자가 아님
            if (item == null || item.event_id == 0)
                return null;

            BoxData box = boxDataRepo.Get(item.event_id);

            if (box == null)
                return null;

            return box.rewards;
        }

        /// <summary>
        /// 보상 수령 가능 여부
        /// </summary>
        public bool IsStandByReward(int mileage)
        {
            return GetCurMileage() >= mileage;
        }
    }
}