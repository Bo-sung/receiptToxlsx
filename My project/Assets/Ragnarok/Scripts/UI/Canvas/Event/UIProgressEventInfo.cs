using UnityEngine;

namespace Ragnarok
{
    public sealed class UIProgressEventInfo : UIInfo<EventPresenter, QuestInfo>, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelTitle, labelCondition;
        [SerializeField] UIButtonHelper btnComplete;
        [SerializeField] UIRewardHelper[] rewardHelpers;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabelHelper labelProgress;
        [SerializeField] UILabelHelper labReward;
        [SerializeField] GameObject completeBase;
        [SerializeField] GameObject fxComplete;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnComplete.OnClick, OnClickedBtnComplete);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnComplete.OnClick, OnClickedBtnComplete);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            NGUITools.SetActive(fxComplete, false);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            for (int i = 0; i < rewardHelpers.Length; i++)
            {
                rewardHelpers[i].SetData(info.RewardDatas[i]);
            }

            int currentValue = info.CurrentValue;
            int maxValue = info.MaxValue;

            if (labelTitle)
                labelTitle.Text = info.Name;

            labReward.LocalKey = LocalizeKey._11004; // 보상 목록
            labelCondition.Text = info.ConditionText;
            progressBar.gameObject.SetActive(true);
            progressBar.value = MathUtils.GetProgress(currentValue, maxValue);
            labelProgress.Text = $"({currentValue}/{maxValue})";

            switch (info.CompleteType)
            {
                case QuestInfo.QuestCompleteType.InProgress: // 진행중
                    btnComplete.LocalKey = LocalizeKey._11002; // 보상획득
                    btnComplete.IsEnabled = false;
                    if (completeBase)
                        completeBase.SetActive(false);
                    break;

                case QuestInfo.QuestCompleteType.StandByReward: // 보상 대기
                    btnComplete.LocalKey = LocalizeKey._11002; // 보상획득
                    btnComplete.IsEnabled = true;
                    if (completeBase)
                        completeBase.SetActive(false);
                    break;

                case QuestInfo.QuestCompleteType.ReceivedReward: // 보상 받음
                    btnComplete.LocalKey = LocalizeKey._11003; // 획득완료
                    btnComplete.IsEnabled = false;
                    if (completeBase)
                        completeBase.SetActive(true);
                    break;
            }
        }

        void OnClickedBtnComplete()
        {
            if (IsInvalid())
                return;

            if (info.RemainTime.ToRemainTime() <= 0)
                return;

            presenter.RequestQuestRewardAsync(info).WrapNetworkErrors();

            NGUITools.SetActive(fxComplete, false);
            NGUITools.SetActive(fxComplete, true);
        }
    }
}
