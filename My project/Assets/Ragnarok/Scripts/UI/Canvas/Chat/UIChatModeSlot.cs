using UnityEngine;

namespace Ragnarok
{
    public class UIChatModeSlot : UIInfo<ChatPresenter, ChatModeInfo>, IAutoInspectorFinder
    {
        [SerializeField] UIButtonHelper btnSlot;
        [SerializeField] UISprite icon;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelRecentChat;
        //[SerializeField] UILabelHelper labelRecentTime;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnSlot.OnClick, OnClickedBtnSlot);
        }

        protected override void OnDestroy()
        {
            EventDelegate.Remove(btnSlot.OnClick, OnClickedBtnSlot);

            base.OnDestroy();
        }

        protected override void Refresh()
        {
            ChatInfo recentChat = presenter.GetRecentChat(info.mode, info.cid);

            icon.spriteName = GetIconName(info.mode);
            labelTitle.Text = GetTitle(info.mode);
            labelRecentChat.Text = recentChat != null ? recentChat.message : string.Empty;
            //labelRecentChat.Text = recentChat.

            // 빨콩 체크
            btnSlot.SetNotice(presenter.HasNewChatting(info.mode, info.cid));
        }

        void OnClickedBtnSlot()
        {
            presenter.SetChatMode(info.mode, info.cid);

            // 빨콩 제거
            btnSlot.SetNotice(false);
        }

        private string GetIconName(ChatMode mode)
        {
            switch (mode)
            {
                case ChatMode.Channel: return "Ui_Common_Chat_Free";
                case ChatMode.Guild: return "Ui_Common_Chat_Guild";
                case ChatMode.Whisper: return "Ui_Common_Chat_Personal";
                case ChatMode.Lobby: return "Ui_Common_Chat_Exchange";
            }

            return "Ui_Common_Chat_Free";
        }

        private string GetTitle(ChatMode mode)
        {
            if(mode == ChatMode.Whisper)
                return $"{info.nickname} ({MathUtils.CidToHexCode(info.cid)})";

            string title = mode.GetName();

            switch (mode)
            {
                case ChatMode.Channel: return title.Replace(ReplaceKey.VALUE, presenter.Channel);
                case ChatMode.Guild: return title.Replace(ReplaceKey.NAME, presenter.GetGuildName());
                case ChatMode.Lobby: return title.Replace(ReplaceKey.VALUE, Entity.player.ChatModel.LobbyChannel);
            }

            return title;
        }


    }
}