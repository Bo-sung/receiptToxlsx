using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEvent : UICanvas<EventPresenter>, EventPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIEventQuestBanner uiEventQuestBanner;

        QuestInfo[] arrayInfo;
        EventQuestGroupInfo questGroupInfo;

        protected override void OnInit()
        {
            presenter = new EventPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnClose.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnClose.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
            questGroupInfo = data as EventQuestGroupInfo;
            UpdateView();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._11000; // 이벤트
        }

        public void UpdateView()
        {
            if (questGroupInfo.RemainTime.ToRemainTime() <= 0)
            {
                CloseUI();
                return;
            }

            uiEventQuestBanner.SetData(questGroupInfo);

            // 이벤트 퀘스트 정보
            arrayInfo = presenter.GetEventQuests(questGroupInfo.GroupId);
            wrapper.Resize(arrayInfo.Length);
        }

        private void CloseUI()
        {
            UI.Close<UIEvent>();
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIProgressEventInfo ui = go.GetComponent<UIProgressEventInfo>();
            ui.SetData(presenter, arrayInfo[index]);
        }
    }
}
