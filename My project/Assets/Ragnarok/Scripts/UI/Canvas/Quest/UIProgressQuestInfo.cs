using UnityEngine;

namespace Ragnarok
{
    public class UIProgressQuestInfo : UIQuestInfo, IAutoInspectorFinder
    {
        [SerializeField] protected UIProgressBar progressBar;
        [SerializeField] protected UILabelHelper labelProgress;
        [SerializeField] protected UILabelHelper labReward;
        [SerializeField] protected GameObject completeBase;

        protected override void UpdateView()
        {
            if (IsInvalid())
                return;

            for (int i = 0; i < rewardHelpers.Length; i++)
            {
                rewardHelpers[i].SetData(questInfo.RewardDatas[i]);
            }

            int currentValue = questInfo.CurrentValue;
            int maxValue = questInfo.MaxValue;

            if (labelTitle)
                labelTitle.Text = questInfo.Name;

            labReward.LocalKey = LocalizeKey._10015; // 보상 목록
            labelCondition.Text = questInfo.ConditionText;
            progressBar.gameObject.SetActive(true);
            progressBar.value = MathUtils.GetProgress(currentValue, maxValue);
            labelProgress.Text = $"({currentValue}/{maxValue})";

            switch (questInfo.CompleteType)
            {
                case QuestInfo.QuestCompleteType.InProgress: // 진행중
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnComplete.IsEnabled = false;
                    if (completeBase)
                        completeBase.SetActive(false);
                    break;

                case QuestInfo.QuestCompleteType.StandByReward: // 보상 대기
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnComplete.IsEnabled = true;

                    if (completeBase)
                        completeBase.SetActive(false);
                    break;

                case QuestInfo.QuestCompleteType.ReceivedReward: // 보상 받음
                    btnComplete.LocalKey = LocalizeKey._10014; // 획득완료
                    btnComplete.IsEnabled = false;
                    if (completeBase)
                        completeBase.SetActive(true);
                    break;
            }
        }
    }
}