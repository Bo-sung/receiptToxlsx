using UnityEngine;

namespace Ragnarok
{
    public class GuildQuestView : UISubCanvas, IAutoInspectorFinder
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIProgressBar totalProgressBar;
        [SerializeField] UILabelHelper labelTotalProgress;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] UILabelHelper labelNoGuild;

        [SerializeField] GameObject goGuildBase;
        [SerializeField] GameObject goNoGuildBase;

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

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelReward.Text = LocalizeKey._10024.ToText(); // 퀘스트 완료
            labelNoGuild.Text = LocalizeKey._10026.ToText(); // 길드에 가입한 유저만 길드 퀘스트를 받을 수 있습니다.

            Refresh();
        }

        protected override void OnShow()
        {
            Refresh();
        }

        public void Initialize(QuestPresenter presenter)
        {
            this.presenter = presenter;
        }

        void Refresh()
        {
            // 길드 가입 여부에 따라 다르게 출력한다.
            bool haveGuild = presenter.IsHaveGuild();

            if (goGuildBase.activeSelf != haveGuild)
                goGuildBase.SetActive(haveGuild);
            if (goNoGuildBase.activeSelf != !haveGuild)
                goNoGuildBase.SetActive(!haveGuild);

            if (!haveGuild)
                return;

            // 퀘스트 정보 프리팹 업데이트
            arrayInfo = presenter.GetGuildQuests();
            int size = arrayInfo.Length;
            wrapper.Resize(size);

            // 일일 완료 횟수 프로그레스
            labelTotalProgress.Text = $"({presenter.GuildQestRewardCount}/{presenter.GuildQuestRewardLimit})";
            totalProgressBar.value = MathUtils.GetProgress(presenter.GuildQestRewardCount, presenter.GuildQuestRewardLimit);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIProgressGuildQuestInfo ui = go.GetComponent<UIProgressGuildQuestInfo>();
            ui.SetData(presenter, arrayInfo[index]);
        }
    }
}