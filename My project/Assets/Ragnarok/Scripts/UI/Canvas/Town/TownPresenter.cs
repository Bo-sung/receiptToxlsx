using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class TownPresenter : ViewPresenter
    {
        public interface IView
        {
        }

        public interface ILobbyChannelView
        {
            void Resize(int count);
            void SetActiveChannelList(bool isActive);
            void SetLabelNowChannelInfo(string name);
            void SetLabelNowChannelInfoCount(string name);
            void SetLabelNowStoreCount(string name);
            bool IsActiveChannelList();
            void SetRefreshAllItem();
        }

        private readonly IView view;
        private readonly ILobbyChannelView lobbyChannelView;

        private int channelId;
        List<LobbyChannelInfo> lobbyChannelInfoList;

        public TownPresenter(IView view, ILobbyChannelView lobbyChannelView)
        {
            this.view = view;
            this.lobbyChannelView = lobbyChannelView;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SetChannelList(List<LobbyChannelInfo> lobbyChannelInfoList)
        {
            this.lobbyChannelInfoList = lobbyChannelInfoList;
            lobbyChannelView.SetActiveChannelList(false);
            RefreshChannelListView();
        }

        private void RefreshChannelListView()
        {
            if (lobbyChannelInfoList is null)
                return;

            //lobbyChannelView.SetActiveChannelList(false);
            lobbyChannelView.Resize(lobbyChannelInfoList.Count);
        }

        private void RefreshCurrentChannelView(int playerCount, int storeCount)
        {
            if (lobbyChannelInfoList is null)
                return;

            RefreshChannelListView();
            LobbyChannelInfo curChannelInfo = lobbyChannelInfoList[channelId - 1];
            curChannelInfo.playerCount = playerCount;

            // 플레이어 수 / 맥스
            lobbyChannelView.SetLabelNowChannelInfoCount(
                LocalizeKey._3033.ToText() // {VALUE}/{MAX}
                .Replace(ReplaceKey.VALUE, playerCount)
                .Replace(ReplaceKey.MAX, curChannelInfo.maxPlayerCount));

            // 상점 수 / 맥스
            lobbyChannelView.SetLabelNowStoreCount(
                LocalizeKey._3033.ToText() // {VALUE}/{MAX}
                .Replace(ReplaceKey.VALUE, storeCount)
                .Replace(ReplaceKey.MAX, curChannelInfo.maxStoreCount));

            lobbyChannelView.SetRefreshAllItem();

            Refresh();
        }

        public void Refresh()
        {
            if (lobbyChannelInfoList is null)
                return;

            // 현재 채널명
            lobbyChannelView.SetLabelNowChannelInfo(
                LocalizeKey._3032.ToText() // 채널{VALUE}
                .Replace(ReplaceKey.VALUE, channelId));
        }

        public void RefreshCurrentChannelView()
        {
            UnitEntity[] unitList = BattleManager.Instance.unitList.FindAll(e => (e.type == UnitEntityType.MultiPlayer || e.type == UnitEntityType.Player)).ToArray();
            CharacterEntity[] characterList = (from unit in unitList let chara = unit as CharacterEntity select chara).ToArray();

            int playerCount = characterList.Length;
            int storeCount = (from chara in characterList where chara.Trade.SellingState == PrivateStoreSellingState.SELLING select chara).Count();

            RefreshCurrentChannelView(playerCount, storeCount);
        }

        public void OnElementRefresh(GameObject go, int index)
        {
            LobbyChannelSlot slot = go.GetComponent<LobbyChannelSlot>();
            slot.SetData(this, lobbyChannelInfoList[index]);
        }

        public void SetChannelId(int channelId)
        {
            this.channelId = channelId;
        }

        #region 버튼 이벤트

        /// <summary>
        /// 거래소 채널 버튼
        /// </summary>
        public void OnClickBtnChannelInfo()
        {
            bool isOpen = lobbyChannelView.IsActiveChannelList();
            lobbyChannelView.SetActiveChannelList(!isOpen);
            if (isOpen)
                lobbyChannelView.SetRefreshAllItem();
        }

        public void GoToPersonalShop(int connectChannel)
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            lobbyChannelView.SetActiveChannelList(false);
            BattleManager.Instance.StartBattle(BattleMode.Lobby, connectChannel);
        }       

        #endregion
    }
}
