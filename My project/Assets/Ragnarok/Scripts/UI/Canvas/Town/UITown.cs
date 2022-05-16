using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UITown : UICanvas<TownPresenter>
        , TownPresenter.IView
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UILobbyChannelView lobbychannel;

        protected override void OnInit()
        {
            presenter = new TownPresenter(this, lobbychannel);
            lobbychannel.Initialize(presenter);
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            presenter.Refresh();
        }

        public void SetChannel(int channelId)
        {
            presenter.SetChannelId(channelId);
        }

        public void SetChannelList(List<LobbyChannelInfo> lobbyChannelInfoList)
        {
            presenter.SetChannelList(lobbyChannelInfoList);
        }

        /// <summary>
        /// 채널뷰 Refresh (현재 채널 인원 수, 노점 수)
        /// </summary>
        public void RefreshChannelView()
        {
            presenter.RefreshCurrentChannelView();
        }
    }
}