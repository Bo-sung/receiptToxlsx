using UnityEngine;

namespace Ragnarok
{
    public class UIMainQuestInfo : UIQuestInfo, IAutoInspectorFinder
    {
        [SerializeField] UISprite check;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UIButtonHelper btnShortCut;
        [SerializeField] UIGrid rewardGrid;

        GameObject myGameObject;

        protected override void Awake()
        {
            base.Awake();

            myGameObject = gameObject;
            EventDelegate.Add(btnShortCut.OnClick, OnClickdedBtnShortCut);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnShortCut.OnClick, OnClickdedBtnShortCut);
        }

        protected override void UpdateView()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            QuestInfo.QuestCompleteType completeType = questInfo.CompleteType;
            if (completeType == QuestInfo.QuestCompleteType.ReceivedReward)
            {
                SetActive(false);
            }
            else
            {
                SetActive(true);

                int currentValue = questInfo.CurrentValue;
                int maxValue = questInfo.MaxValue;
                labelTitle.Text = $"({questInfo.GetMainQuestGroup()}) {questInfo.Name}";
                labelCondition.Text = $"{questInfo.ConditionText} ({currentValue}/{maxValue})";
                check.enabled = completeType == QuestInfo.QuestCompleteType.StandByReward;
                for (int i = 0; i < 4; i++)
                {
                    rewardHelpers[i].SetData(questInfo.RewardDatas[i]);
                }
                rewardGrid.repositionNow = true;

                bool isReward = completeType == QuestInfo.QuestCompleteType.StandByReward;

                btnComplete.SetActive(isReward);
                btnShortCut.SetActive(!isReward);

                if (!isReward)
                {
                    if (questInfo.ShortCutType == ShortCutType.None)
                    {
                        btnShortCut.IsEnabled = false;
                    }
                    else if (questInfo.ShortCutType == ShortCutType.Stage)
                    {
                        btnShortCut.IsEnabled = presenter.IsShortStage(questInfo.ShortCutValue);
                    }
                    else
                    {
                        btnShortCut.IsEnabled = true;
                    }
                }
            }
        }

        private void SetActive(bool isActive)
        {
            if (myGameObject.activeSelf == isActive)
                return;

            myGameObject.SetActive(isActive);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();
            labelRewardTitle.LocalKey = LocalizeKey._10020; // 보 상
            btnShortCut.LocalKey = LocalizeKey._10022; // 이동하기
        }

        protected override void OnClickedBtnComplete()
        {
            base.OnClickedBtnComplete();
        }

        void OnClickdedBtnShortCut()
        {
            presenter.OnClickedBtnShortCut();
        }
    }
}
