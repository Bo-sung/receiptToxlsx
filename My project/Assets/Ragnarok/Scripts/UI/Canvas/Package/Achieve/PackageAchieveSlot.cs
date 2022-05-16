using UnityEngine;

namespace Ragnarok.View
{
    public class PackageAchieveSlot : UIView
    {
        public interface IInput
        {
            RewardData GetReward();
            int GetStep();
            int GetConditionValue();
            int GetGroupId();
        }

        public interface Impl
        {
            ShopConditionType GetShopConditionType();
            string GetTitle(int conditionValue);
            int GetCurStep();
            int GetCurFreeStep();
            bool IsStandByReward(int conditionValue);
            bool IsActivation();
            void ReqeustReward(int step, int groupId);
            int GetCurConditionValue();
            float GetPercent();
            bool IsFree(int groupId);
            int GetPreConditionValue(int condition);
            bool HasMailShopItem();
        }

        [SerializeField] PackAchieveSlotState payState;
        [SerializeField] PackAchieveSlotState freeState;
        [SerializeField] UITextureHelper rewardIcon;
        [SerializeField] UILabelHelper labelRewardCount;
        [SerializeField] UIButtonHelper btnGet;
        [SerializeField] UISlider slider;
        [SerializeField] UILabelHelper labelNeed;

        private int index;
        private IInput input;
        private Impl impl;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnGet.OnClick, OnClickedBtnGet);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnGet.OnClick, OnClickedBtnGet);
        }

        protected override void OnLocalize()
        {
            labelNeed.LocalKey = LocalizeKey._3911; // 구매 필요
        }

        public void Set(int index, IInput input, Impl impl)
        {
            this.index = index;
            this.input = input;
            this.impl = impl;
            Refresh();
        }

        private void Refresh()
        {
            if (index == 0)
            {
                slider.gameObject.SetActive(false);
            }
            else
            {
                slider.gameObject.SetActive(true);
            }

            int conditionValue = input.GetConditionValue();
            int step = input.GetStep();
            bool isFree = impl.IsFree(input.GetGroupId());
            string stepText = (index + 1).ToString();
            string titleText = impl.GetTitle(conditionValue);

            // 보상 표시
            RewardData reward = input.GetReward();
            rewardIcon.Set(reward.IconName);
            labelRewardCount.Text = $"x{reward.Count}";

            // 프로그래스 표시
            int curConditionValue = impl.GetCurConditionValue();
            int preConditionValue = impl.GetPreConditionValue(conditionValue);

            if (impl.IsStandByReward(conditionValue)) // 도달
            {
                slider.value = 1f;
            }
            else if (curConditionValue <= preConditionValue) // 미도달
            {
                slider.value = 0f;
            }
            else // 진행중
            {
                slider.value = impl.GetPercent();
            }

            payState.SetActive(!isFree);
            freeState.SetActive(isFree);

            if (isFree)
            {
                freeState.Set(stepText, titleText);
                labelNeed.SetActive(false);

                btnGet.SetActive(true);
                int curStep = impl.GetCurFreeStep(); // 보상 받은 스탭
                bool isReceivedReward = step <= curStep; // 보상 받음
                if (isReceivedReward)
                {
                    // 보상 받음
                    btnGet.IsEnabled = false;
                    btnGet.SetNotice(false);
                    btnGet.LocalKey = LocalizeKey._3909; // 수령완료
                }
                else if (step == curStep + 1 && impl.IsStandByReward(conditionValue))
                {
                    // 보상 수령 가능
                    btnGet.IsEnabled = true;
                    btnGet.SetNotice(true);
                    btnGet.LocalKey = LocalizeKey._3905; // 보상받기
                }
                else
                {
                    // 진행중
                    btnGet.IsEnabled = false;
                    btnGet.SetNotice(false);
                    btnGet.LocalKey = LocalizeKey._3905; // 보상받기
                }
            }
            else
            {
                payState.Set(stepText, titleText);

                // 미구매 || 메일함에 있음
                if (!impl.IsActivation() || impl.HasMailShopItem())
                {
                    btnGet.SetActive(false);
                    labelNeed.SetActive(true);
                    return;
                }

                labelNeed.SetActive(false);
                btnGet.SetActive(true);
                int curStep = impl.GetCurStep(); // 보상 받은 스탭
                bool isReceivedReward = step <= curStep; // 보상 받음
                if (isReceivedReward)
                {
                    // 보상 받음
                    btnGet.IsEnabled = false;
                    btnGet.SetNotice(false);
                    btnGet.LocalKey = LocalizeKey._3909; // 수령완료
                }
                else if (step == curStep + 1 && impl.IsStandByReward(conditionValue))
                {
                    // 보상 수령 가능
                    btnGet.IsEnabled = true;
                    btnGet.SetNotice(true);
                    btnGet.LocalKey = LocalizeKey._3905; // 보상받기
                }
                else
                {
                    // 진행중
                    btnGet.IsEnabled = false;
                    btnGet.SetNotice(false);
                    btnGet.LocalKey = LocalizeKey._3905; // 보상받기
                }
            }
        }

        private void OnClickedBtnGet()
        {
            impl.ReqeustReward(input.GetStep(), input.GetGroupId());
        }
    }
}