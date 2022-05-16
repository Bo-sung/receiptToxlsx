using UnityEngine;

namespace Ragnarok
{
    public class AchievementView : UISubCanvas
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIProgressBar totalProgressBar;
        [SerializeField] UILabelHelper labelTotalProgress;
        [SerializeField] UILabelHelper labelAchieve;

        QuestPresenter presenter;
        QuestInfo[] arrayInfo;

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
            labelAchieve.LocalKey = LocalizeKey._10021; // 업적 달성률
        }

        public void Initialize(QuestPresenter presenter)
        {
            this.presenter = presenter;
        }

        private void UpdateView()
        {
            // 퀘스트 정보
            arrayInfo = presenter.GetAchievements();
            int size = arrayInfo.Length;
            wrapper.Resize(size);

            // 모든 업적 퀘스트 진행도
            float progress = presenter.GetAchievementProgress();
            totalProgressBar.value = progress;
            labelTotalProgress.Text = $"{progress:P0}";
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIProgressQuestInfo ui = go.GetComponent<UIProgressQuestInfo>();
            ui.SetData(presenter, arrayInfo[index]);
        }
    }
}