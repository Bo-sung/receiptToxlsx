using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    class ChatPreviewPresenter : ViewPresenter
    {
        private UIChatPreview view;
        private ChatModel chatModel;
        private bool queueChat = false;

        public ChatPreviewPresenter(UIChatPreview view)
        {
            this.view = view;
            chatModel = Entity.player.ChatModel;
        }

        public override void AddEvent()
        {
            chatModel.OnAddChatEvent += OnAddChatEvent;
        }

        public override void RemoveEvent()
        {
            chatModel.OnAddChatEvent -= OnAddChatEvent;
        }

        public void SetActive(bool value)
        {
            queueChat = value;
        }

        private void OnAddChatEvent()
        {
            if (!queueChat)
                return;

            var lastChat = chatModel.GetRecentChat(ChatMode.All);
            if (lastChat == null)
                return;

            var curChat = chatModel.GetRecentChat(ChatMode.Channel);
            var curWhisperChat = chatModel.GetRecentChat(ChatMode.Whisper, lastChat.cid);
            var curGuildChat = chatModel.GetRecentChat(ChatMode.Guild);

            if (lastChat == curChat && !curChat.isGMMsg)
            {
                view.EnqueueChat(curChat, ChatMode.Channel);
            }
            else if (lastChat == curWhisperChat)
            {
                view.EnqueueChat(curWhisperChat, ChatMode.Whisper);
            }
            else if (lastChat == curGuildChat)
            {
                view.EnqueueChat(curGuildChat, ChatMode.Guild);
            }
        }
    }
}
