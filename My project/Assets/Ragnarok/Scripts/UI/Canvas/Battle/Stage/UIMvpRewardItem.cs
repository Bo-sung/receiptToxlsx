using UnityEngine;

namespace Ragnarok.View
{
    public class UIMvpRewardItem : UIView, IAutoInspectorFinder
    {
        public interface IInput
        {
            event System.Action OnUpdate;

            bool IsShow();
            void SetShow(bool isShow);

            RewardData GetRewardData();
            bool IsWasted();
        }

        [SerializeField] GameObject child;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelWeight;

        IInput input;

        Transform myTransform;

        protected override void Awake()
        {
            base.Awake();

            myTransform = transform;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RemoveEvent();
        }

        protected override void OnLocalize()
        {
            labelWeight.LocalKey = LocalizeKey._48402; // 무게 초과!
        }

        public void SetData(IInput input)
        {
            RemoveEvent();
            this.input = input;
            AddEvent();
            Refresh();
        }

        public void Release(bool isLaunch, UIRewardLauncher.GoodsDestination destination)
        {
            if (input == null)
                return;

            if (isLaunch)
            {
                RewardData rewardData = input.GetRewardData();
                RewardType rewardType = rewardData.RewardType;
                switch (rewardType)
                {
                    case RewardType.Zeny:
                    case RewardType.CatCoin:
                    case RewardType.JobExp:
                    case RewardType.LevelExp:
                    case RewardType.ROPoint:
                        UI.LaunchReward(myTransform.position, rewardType, rewardData.Count, destination);
                        break;

                    // 스킬 포인트는 연출 없음
                    case RewardType.SkillPoint:
                        break;

                    default:
                        UI.LaunchReward(myTransform.position, rewardData.IconName, rewardData.Count, destination);
                        break;
                }
            }

            SetData(null);
        }

        private void AddEvent()
        {
            if (input == null)
                return;

            input.OnUpdate += Refresh;
        }

        private void RemoveEvent()
        {
            if (input == null)
                return;

            input.OnUpdate -= Refresh;
        }

        private void Refresh()
        {
            if (input == null)
                return;

            bool isShow = input.IsShow();
            NGUITools.SetActive(child, isShow);

            if (!isShow)
                return;

            RewardData rewardData = input.GetRewardData();
            rewardHelper.SetData(rewardData);
            labelName.Text = rewardData.ItemName;
            labelWeight.SetActive(input.IsWasted());
        }
    }
}