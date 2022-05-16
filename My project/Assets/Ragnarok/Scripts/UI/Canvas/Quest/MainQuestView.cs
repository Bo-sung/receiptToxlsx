using UnityEngine;

namespace Ragnarok
{
    public class MainQuestView : UISubCanvas
    {
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UIMainQuestInfo mainQuestInfo;

        QuestPresenter presenter;

        protected override void OnInit()
        {
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
            labelNoData.LocalKey = LocalizeKey._10005; // 모든 메인 퀘스트를 완료하였습니다
        }

        public void Initialize(QuestPresenter presenter)
        {
            this.presenter = presenter;
        }

        private void UpdateView()
        {
            QuestInfo mainQuest = presenter.GetMainQuest();

            labelNoData.gameObject.SetActive(mainQuest.IsInvalidData);
            mainQuestInfo.SetData(presenter, mainQuest);
        }
    }
}