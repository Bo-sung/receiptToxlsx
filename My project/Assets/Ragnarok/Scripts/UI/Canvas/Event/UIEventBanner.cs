using UnityEngine;

namespace Ragnarok
{
    public class UIEventBanner : UISubCanvas, EventBannerPresenter.IView, IAutoInspectorFinder
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelNoData;

        EventBannerPresenter presenter;
        EventQuestGroupInfo[] arrayInfo;

        protected override void OnInit()
        {
            presenter = new EventBannerPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
        }

        protected override void OnShow()
        {
            presenter.RequestEventQuest();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelNoData.LocalKey = LocalizeKey._9016; // 현재 이벤트가 없습니다.
        }

        public void UpdateView()
        {
            arrayInfo = presenter.GetEventQuestGroupInfos();
            wrapper.Resize(arrayInfo.Length);
            labelNoData.SetActive(arrayInfo.Length == 0);
        }
        private void OnItemRefresh(GameObject go, int index)
        {
            UIEventQuestBanner ui = go.GetComponent<UIEventQuestBanner>();
            arrayInfo[index].Set(presenter);
            ui.SetData(presenter, arrayInfo[index]);
        }

        void EventBannerPresenter.IView.CloseUI()
        {
            UI.Close<UIDailyCheck>();
        }
    }
}