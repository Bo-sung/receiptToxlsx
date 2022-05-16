using Ragnarok.View.ServerChange;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIServerChange : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelSubTitle;
        [SerializeField] UILabelValue recentlyServer;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UIButtonHelper btnSelect;

        [SerializeField] GameObject prefab;
        [SerializeField] SuperScrollListWrapper wrapper;

        ServerChangePresenter presenter;
        UIServerChangeSlot selectedSlot;

        public event System.Action<ServerInfoPacket[], bool> RefreshServerList;

        protected override void OnInit()
        {
            presenter = new ServerChangePresenter();
            presenter.AddEvent();

            EventDelegate.Add(btnExit.OnClick, OnClickBtnExit);
            EventDelegate.Add(btnClose.OnClick, OnClickBtnClose);
            EventDelegate.Add(btnSelect.OnClick, OnClickBtnSelect);

            RefreshServerList += OnRefreshServerList;

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SetClickCallback(OnItemClick);
            //wrapper.ScrollView.onDragFinished = OnDragFinished;
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            RefreshServerList -= OnRefreshServerList;
        }

        protected override void OnShow(IUIData data = null)
        {
            selectedSlot = null;
            btnSelect.IsEnabled = false;
            recentlyServer.ValueKey = presenter.GetServerNameKey(); // --- 접속중인 서버 이름
            presenter.GetServerList(RefreshServerList);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._1100; // 서버 선택
            labelSubTitle.LocalKey = LocalizeKey._1101; // 서버 리스트
            recentlyServer.TitleKey = LocalizeKey._1102; // 가장 최근에 접속한 서버
            btnClose.LocalKey = LocalizeKey._1103; // 돌아가기
            btnSelect.LocalKey = LocalizeKey._1104; // 접속하기
        }

        private void OnClickBtnSelect()
        {
            if (selectedSlot != null)
            {
                presenter.ChangeServer(selectedSlot.GetServerGroupId());
            }
        }

        private void OnRefreshServerList(ServerInfoPacket[] infos, bool isServer)
        {
            if (isServer) presenter.SetServerInfos(infos); // 서버 정보 저장..

            wrapper.Resize(infos.Length);
        }

        private void OnClickBtnClose()
        {
            UI.Close<UIServerChange>();
        }

        private void OnClickBtnExit()
        {
            UI.Close<UIServerChange>();
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIServerChangeSlot ui = go.GetComponent<UIServerChangeSlot>();
            ui.SetData(presenter.GetServerInfo(index), ui == selectedSlot);
        }

        private void OnItemClick(GameObject go, int index)
        {
            if (selectedSlot != null) selectedSlot.SetActiveSelect(false); // 이전에 선택된 슬롯 선택해제

            selectedSlot = go.GetComponent<UIServerChangeSlot>();
            selectedSlot.SetActiveSelect(true);
            btnSelect.IsEnabled = presenter.ChangeableServer(index);
        }
    }
}