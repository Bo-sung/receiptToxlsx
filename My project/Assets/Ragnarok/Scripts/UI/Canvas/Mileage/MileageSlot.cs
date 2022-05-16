using UnityEngine;

namespace Ragnarok.View
{
    public class MileageSlot : UIView, IInspectorFinder
    {
        public enum MileageCompleteType
        {
            /// <summary>
            /// 진행중
            /// </summary>
            InProgress = 1,

            /// <summary>
            /// 보상 대기 중
            /// </summary>
            StandByReward = 2,

            /// <summary>
            /// 보상 완료
            /// </summary>
            ReceivedReward = 3,
        }

        public interface IInput
        {
            RewardData GetReward();
            int GetStep();
            int GetMileage();
        }

        public interface Impl
        {
            int GetCurStep();
            MileageCompleteType GetCompleteType(int step, int needPoint);
            int GetCurMileage();
            int GetPreMileage(int mileage);
            float GetPercent();
            void OnClickedBtnGetReward();
            RewardData[] GetRewards(int groupId);
            bool IsStandByReward(int mileage);
        }

        [SerializeField] UISlider slider;
        [SerializeField] UIGraySprite iconMileage;
        [SerializeField] UILabelHelper labelMileage;
        [SerializeField] UIGridHelper rewardGrid;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabelHelper labelReceivedReward;
        [SerializeField] UIButtonHelper btnGet;

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
            labelReceivedReward.LocalKey = LocalizeKey._4807; // 수령 완료
            btnGet.LocalKey = LocalizeKey._4803; // 받기
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
            bool isShowSlider = index != 0;
            slider.gameObject.SetActive(isShowSlider);

            int mileage = input.GetMileage();
            labelMileage.Text = mileage.ToString();

            // 프로그래스 표시
            int curMileage = impl.GetCurMileage();
            int preMileage = impl.GetPreMileage(mileage);

            // 무조건 즉시 오픈 상자로 들어와야한다.
            RewardData reward = input.GetReward();
            if (reward.RewardType == RewardType.Item)
            {
                int itemId = reward.RewardValue;
                RewardData[] data = impl.GetRewards(itemId);
                if (data != null)
                {
                    rewardGrid.SetValue(data.Length);
                    for (int i = 0; i < rewards.Length; i++)
                    {
                        if (i < data.Length)
                        {
                            rewards[i].SetData(data[i]);
                        }
                    }
                }
                else
                {
                    rewardGrid.SetValue(0);
                }
            }
            else
            {
                rewardGrid.SetValue(0);
            }

            MileageCompleteType completeType = impl.GetCompleteType(input.GetStep(), mileage);

            if (impl.IsStandByReward(mileage))
            {
                slider.value = 1f;
            }
            else if (curMileage <= preMileage) // 미도달
            {
                slider.value = 0f;
            }
            else // 진행중
            {
                slider.value = impl.GetPercent();
            }

            switch (completeType)
            {
                case MileageCompleteType.InProgress:
                    iconMileage.Mode = UIGraySprite.SpriteMode.Grayscale;
                    btnGet.IsEnabled = false;
                    btnGet.SetNotice(false);
                    btnGet.SetActive(true);
                    labelReceivedReward.SetActive(false);
                    break;

                case MileageCompleteType.StandByReward:
                    iconMileage.Mode = UIGraySprite.SpriteMode.None;
                    btnGet.IsEnabled = true;
                    btnGet.SetNotice(true);
                    btnGet.SetActive(true);
                    labelReceivedReward.SetActive(false);
                    break;

                case MileageCompleteType.ReceivedReward:
                    iconMileage.Mode = UIGraySprite.SpriteMode.None;
                    btnGet.SetActive(false);
                    labelReceivedReward.SetActive(true);
                    break;
            }
        }

        private void OnClickedBtnGet()
        {
            impl.OnClickedBtnGetReward();
        }

        bool IInspectorFinder.Find()
        {
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}