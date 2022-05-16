using Ragnarok.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class PackageAchievePresenter : ViewPresenter, PackageAchieveSlot.Impl
    {
        public enum PackageType
        {
            LevelUp,
            Senario,
        }

        /******************** Models ********************/
        private readonly ShopModel shopModel;
        private readonly CharacterModel characterModel;
        private readonly DungeonModel dungeonModel;

        /******************** Repositories ********************/
        private readonly RewardGroupDataManager rewardGroupDataRepo;
        private readonly StageDataManager stageDataRepo;

        /******************** Event ********************/

        public event Action OnRewardPackageAchieve
        {
            add { shopModel.OnRewardPackageAchieve += value; }
            remove { shopModel.OnRewardPackageAchieve -= value; }
        }

        public event Action OnPucharseSuccess
        {
            add { shopModel.OnPurchaseSuccess += value; }
            remove { shopModel.OnPurchaseSuccess -= value; }
        }

        private ShopInfo info;

        private List<int> conditionValueList;

        public PackageAchievePresenter()
        {
            shopModel = Entity.player.ShopModel;
            characterModel = Entity.player.Character;
            dungeonModel = Entity.player.Dungeon;
            rewardGroupDataRepo = RewardGroupDataManager.Instance;
            stageDataRepo = StageDataManager.Instance;
            conditionValueList = new List<int>();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SetShopId(int shopId)
        {
            info = shopModel.GetInfo(shopId);
        }

        public string GetPrice()
        {
            return info.CostText;
        }

        /// <summary>
        /// 우편함에 보유중 여부
        /// </summary>
        /// <returns></returns>
        public bool HasMailShopItem()
        {
            return shopModel.HasMailShopItem(info.ID);
        }

        /// <summary>
        /// 진행도
        /// </summary>
        /// <returns></returns>
        public int GetCurStep()
        {
            switch (GetShopConditionType())
            {
                case ShopConditionType.JobLevel:
                    return shopModel.PayJobLevelStep;

                case ShopConditionType.Scenario:
                    return shopModel.PayChapterStep;
            }
            return -1;
        }

        public int GetCurFreeStep()
        {
            switch (GetShopConditionType())
            {
                case ShopConditionType.JobLevel:
                    return shopModel.FreeJobLevelStep;

                case ShopConditionType.Scenario:
                    return shopModel.FreeChapterStep;
            }
            return 0;
        }

        public ShopConditionType GetShopConditionType()
        {
            return info.ShopConditionType;
        }

        /// <summary>
        /// 활성화 여부
        /// </summary>
        /// <returns></returns>
        public bool IsActivation()
        {
            return GetCurStep() != -1;
        }

        public PackageAchieveSlot.IInput[] GetInputs()
        {
            List<RewardGroupData> list = new List<RewardGroupData>();
            list.AddRange(rewardGroupDataRepo.GetList(GetRewardGroupId(isFree: false)));
            list.AddRange(rewardGroupDataRepo.GetList(GetRewardGroupId(isFree: true)));
            list.Sort(SortByGroup);

            conditionValueList.Clear();
            foreach (var item in list)
            {
                conditionValueList.Add(item.condition_value);
            }

            return list.ToArray();
        }

        private int SortByGroup(RewardGroupData x, RewardGroupData y)
        {
            int result = x.condition_value.CompareTo(y.condition_value); // condition_value
            return result;
        }

        public int GetRewardGroupId(bool isFree)
        {
            return rewardGroupDataRepo.GetRewardGroupId(GetShopConditionType(), isFree);
        }

        public bool IsFree(int groupId)
        {
            return GetRewardGroupId(isFree: true) == groupId;
        }

        public string GetTitle(int conditionValue)
        {
            switch (GetShopConditionType())
            {
                case ShopConditionType.JobLevel:
                    return LocalizeKey._3906.ToText().Replace(ReplaceKey.LEVEL, conditionValue); // JOB 레벨\n{LEVEL} 달성

                case ShopConditionType.Scenario:
                    {
                        StageData data = stageDataRepo.Get(conditionValue);
                        return LocalizeKey._3907.ToText().Replace(ReplaceKey.NAME, data.name_id.ToText()); // {NAME}\n클리어
                    }
            }
            return string.Empty;
        }

        /// <summary>
        /// 보상 수령 가능 여부
        /// </summary>
        /// <param name="conditionValue"></param>
        /// <returns></returns>
        public bool IsStandByReward(int conditionValue)
        {
            return GetCurConditionValue() >= conditionValue;
        }

        public int GetCurConditionValue()
        {
            switch (GetShopConditionType())
            {
                case ShopConditionType.JobLevel:
                    return characterModel.JobLevel;

                case ShopConditionType.Scenario:
                    {
                        return Mathf.Max(0, dungeonModel.FinalStageId - 1);
                    }
            }
            return default;
        }

        public float GetPercent()
        {
            int curConditionValue = GetCurConditionValue();
            int preValue = 0;
            foreach (var item in conditionValueList)
            {
                if (item > curConditionValue)
                {
                    return MathUtils.GetProgress(curConditionValue - preValue, item - preValue);
                }
                preValue = item;
            }
            return 0f;
        }


        public int GetPreConditionValue(int condition)
        {
            for (int i = conditionValueList.Count - 1; i >= 0; i--)
            {
                if (condition > conditionValueList[i])
                {
                    return conditionValueList[i];
                }
            }
            return 0;
        }


        /// <summary>
        /// 보상 받기 요청
        /// </summary>
        /// <param name="step"></param>
        public void ReqeustReward(int step, int groupId)
        {
            switch (GetShopConditionType())
            {
                case ShopConditionType.JobLevel:
                    shopModel.RequestPayJobLevelReward(step, IsFree(groupId)).WrapNetworkErrors();
                    break;

                case ShopConditionType.Scenario:
                    shopModel.RequestPayScenarioReward(step, IsFree(groupId)).WrapNetworkErrors();
                    break;
            }
        }

        public string GetTitle()
        {
            switch (GetShopConditionType())
            {
                case ShopConditionType.JobLevel:
                    return LocalizeKey._3900.ToText(); // [285190]레벨[-] [FFBF00]달성[-] [285190]팩[-]

                case ShopConditionType.Scenario:
                    return LocalizeKey._3902.ToText(); // [285190]시나리오 팩[-]
            }
            return string.Empty;
        }

        public string GetDescription()
        {
            switch (GetShopConditionType())
            {
                case ShopConditionType.JobLevel:
                    return LocalizeKey._3901.ToText(); // 총 1,900 냥다래!! 200%의 가치!!

                case ShopConditionType.Scenario:
                    return LocalizeKey._3903.ToText(); // 총 3,850 냥다래!! 233%의 가치!!
            }

            return string.Empty;
        }

        public int GetImmediateRewardCount()
        {
            return info.GetAddGoodsReward().Count;
        }

        public void OnClickedBtnPurchase()
        {
            shopModel.RequestCashShopPurchase(info.ID, isShowNotiPopup: false).WrapNetworkErrors();
        }
    }
}