using System;
using UnityEngine;

namespace Ragnarok
{
    public class UILobbyChannelView : UISubCanvas<TownPresenter>
        ,TownPresenter.ILobbyChannelView
    {
        [SerializeField] UIButtonHelper btnNowChannelInfo; // 현재 채널 명 -> 채널리스트 On/Off
        [SerializeField] GameObject goChannelList; // 채널 리스트
        [SerializeField] UILabelHelper labelNowChannelInfoCount; // 현재 채널의 인구 수
        [SerializeField] UILabelHelper labelNowStoreCountTitle; // "현재 노점 수"
        [SerializeField] UILabelHelper labelNowStoreCount; // 현재 채널의 노점 수

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        protected override void OnInit()
        {          
            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(presenter.OnElementRefresh);

            EventDelegate.Add(btnNowChannelInfo.OnClick, presenter.OnClickBtnChannelInfo);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnNowChannelInfo.OnClick, presenter.OnClickBtnChannelInfo);
        }

        protected override void OnShow()
        {
            
        }

        protected override void OnHide() { }

        protected override void OnLocalize()
        {
            labelNowStoreCountTitle.Text = LocalizeKey._3034.ToText(); // 현재 노점 수
        }             

        void TownPresenter.ILobbyChannelView.Resize(int count)
        {
            wrapper.Resize(count);
        }

        void TownPresenter.ILobbyChannelView.SetActiveChannelList(bool isActive)
        {
            goChannelList.SetActive(isActive);
        }

        void TownPresenter.ILobbyChannelView.SetLabelNowChannelInfo(string name)
        {
            btnNowChannelInfo.Text = name;
        }

        void TownPresenter.ILobbyChannelView.SetLabelNowChannelInfoCount(string name)
        {
            labelNowChannelInfoCount.Text = name;
        }

        void TownPresenter.ILobbyChannelView.SetLabelNowStoreCount(string name)
        {
            labelNowStoreCount.Text = name;
        }

        bool TownPresenter.ILobbyChannelView.IsActiveChannelList()
        {
            return goChannelList.activeSelf;
        }

        void TownPresenter.ILobbyChannelView.SetRefreshAllItem()
        {
            wrapper.RefreshAllItems();
        }
    }


    /// <summary>
    /// 채널 리스트 프리팹 데이터
    /// </summary>
    public class LobbyChannelInfo : IInfo
    {
        public bool IsInvalidData => default;

        public event Action OnUpdateEvent;

        public int channel;
        public int playerCount;
        //public int storeCount;
        public int maxPlayerCount;
        public int maxStoreCount;
    }
}
