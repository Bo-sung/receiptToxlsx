using UnityEngine;

namespace Ragnarok
{
    public class DailyQuestView : UISubCanvas
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIProgressBar totalProgressBar;
        [SerializeField] UILabelHelper labelTotalProgress;
        [SerializeField] UIButtonHelper btnTotalComplete;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] GameObject goTotalCompleteAdditionalRewardBase; // 일일 최종 퀘 추가 냥다래보상 안내 오브젝트
        [SerializeField] UILabelHelper labelTotalCompleteAdditionalRewardDesc; // 1회 지급
        [SerializeField] UILabelHelper labelAdditionalRewardCount;

        QuestPresenter presenter;
        QuestInfo[] arrayInfo;
        QuestInfo totalClearQuest;

        void Awake()
        {
            EventDelegate.Add(btnTotalComplete.OnClick, OnClickedBtnTotalComplete);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(btnTotalComplete.OnClick, OnClickedBtnTotalComplete);
        }

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow()
        {
            UpdateView();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnTotalComplete.LocalKey = LocalizeKey._10009; // 보상획득
            labelReward.LocalKey = LocalizeKey._10011; // 일일 의뢰 완료 보상
            labelTotalCompleteAdditionalRewardDesc.LocalKey = LocalizeKey._10027; // 1회 지급
            int addRewardCount = BasisType.COMPLETE_FIRST_DAILY_QUEST_REWARD.GetInt();
            labelAdditionalRewardCount.Text = $"x{addRewardCount}";
            UpdateView();
        }

        public void Initialize(QuestPresenter presenter)
        {
            this.presenter = presenter;
        }

        void OnClickedBtnTotalComplete()
        {
            presenter.RequestQuestReward(totalClearQuest);
        }

        private void UpdateView()
        {
            // 퀘스트 정보
            arrayInfo = presenter.GetDailyQuests();
            int size = arrayInfo.Length;
            wrapper.Resize(size);

            // 모든 일일 퀘스트 클리어 정보
            totalClearQuest = presenter.GetDailyTotalClearQuest();
            int totalClearCurrentValue = totalClearQuest.CurrentValue;
            int totalClearMaxValue = totalClearQuest.MaxValue;
            totalProgressBar.value = MathUtils.GetProgress(totalClearCurrentValue, totalClearMaxValue);
            labelTotalProgress.Text = $"({totalClearCurrentValue}/{totalClearMaxValue})";
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(totalClearQuest.RewardDatas[i]);
            }

            // 최종 보상의 추가 냥다래 보상 안내
            goTotalCompleteAdditionalRewardBase.SetActive(!presenter.IsDailyQuestCleared);

            switch (totalClearQuest.CompleteType)
            {
                case QuestInfo.QuestCompleteType.InProgress: // 진행중                   
                    btnTotalComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnTotalComplete.IsEnabled = false;
                    break;
                case QuestInfo.QuestCompleteType.StandByReward: // 보상 대기                    
                    btnTotalComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnTotalComplete.IsEnabled = true;
                    break;
                case QuestInfo.QuestCompleteType.ReceivedReward: // 보상 받음                    
                    btnTotalComplete.LocalKey = LocalizeKey._10014; // 획득완료
                    btnTotalComplete.IsEnabled = false;
                    break;
            }
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIProgressQuestInfo ui = go.GetComponent<UIProgressQuestInfo>();
            ui.SetData(presenter, arrayInfo[index]);
        }
    }
}