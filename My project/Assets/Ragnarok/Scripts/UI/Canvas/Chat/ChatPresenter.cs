using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIChat"/>
    /// </summary>
    public sealed class ChatPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void CloseUI();
            void SetNotice(string msg);
            void Drag(UIDragEvent.DragArrow dragArrow);
            void ChatMessageClickEvent(UIChatMessage uiChatMessage);
        }

        private readonly IView view;
        private readonly ChatModel chatModel;
        private readonly CharacterModel characterModel;
        private readonly BattleManager battleManager;
        private readonly GuildModel guildModel;

        /// <summary>
        /// 챗모드 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateChatMode;

        /// <summary>
        /// 새로운 채팅(길드, 귓속말) 빨콩
        /// </summary>
        public event System.Action<bool> OnUpdateAllChatNotice
        {
            add { chatModel.OnUpdateAllChatNotice += value; }
            remove { chatModel.OnUpdateAllChatNotice -= value; }
        }

        /// <summary>
        /// 현재 채팅 모드
        /// </summary>
        public ChatMode ChatMode => chatModel.ChatMode;

        /// <summary>
        /// 현재 채널
        /// </summary>
        public int Channel => chatModel.Channel;

        public ChatPresenter(IView view)
        {
            this.view = view;
            chatModel = Entity.player.ChatModel;
            characterModel = Entity.player.Character;
            guildModel = Entity.player.Guild;
            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            chatModel.OnAddChatEvent += view.Refresh;
        }

        public override void RemoveEvent()
        {
            chatModel.OnAddChatEvent -= view.Refresh;
        }

        /// <summary>
        /// 사용자 CID
        /// </summary>
        public int GetPlayerCID()
        {
            return characterModel.Cid;
        }

        /// <summary>
        /// 채팅 모드 설정
        /// </summary>
        public void SetChatMode(ChatMode chatMode, int whisperCid = default)
        {
            OnUpdateChatMode?.Invoke();
            chatModel.SetChatMode(chatMode, whisperCid);
        }

        public void AddWhisperInfo(int uid, int cid, string nickname)
        {
            chatModel.AddWhisperInfo(uid, cid, nickname);
        }

        /// <summary>
        /// 채팅 대화 개수
        /// </summary>
        public int GetDataSize()
        {
            return chatModel.GetDataSize();
        }

        /// <summary>
        /// 채팅 슬롯 Refresh
        /// </summary>
        public void RefreshChatSlot(UIChatMessage uiChatMsg, int dataIndex)
        {
            uiChatMsg.SetData(this, chatModel.Get(dataIndex, chatModel.CurrentWhisperInfo != null ? chatModel.CurrentWhisperInfo.cid : default));
        }

        public void RequestOthersCharacterInfo(int uid, int cid)
        {
            Entity.player.User.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }

        public void OnClickedChatMessage(UIChatMessage uiChatMessage)
        {
            view.ChatMessageClickEvent(uiChatMessage);
        }

        /// <summary>
        /// 챗모드 개수
        /// </summary>
        public int GetChatModeSize()
        {
            return GetChatModesOrdered().Length + GetWhisperInfos().Length;
        }

        /// <summary>
        /// 챗모드 슬롯 Refresh
        /// </summary>
        public void RefreshChatModeSlot(UIChatModeSlot uiChatModeSlot, int dataIndex)
        {
            ChatMode[] chatModes = GetChatModesOrdered();
            if (dataIndex < chatModes.Length)
            {
                ChatModeInfo chatModeInfo = new ChatModeInfo(chatModes[dataIndex]);
                uiChatModeSlot.SetData(this, chatModeInfo);
            }
            else // 귓속말
            {
                int whisperIndex = dataIndex - chatModes.Length;
                var whisperInfo = GetWhisperInfos()[whisperIndex];
                ChatModeInfo chatModeInfo = new ChatModeInfo(ChatMode.Whisper, whisperInfo.cid, whisperInfo.uid, whisperInfo.nickname);
                uiChatModeSlot.SetData(this, chatModeInfo);
            }
        }

        /// <summary>
        /// 정렬 된 챗모드 목록 반환
        /// </summary>
        private ChatMode[] GetChatModesOrdered()
        {
            List<ChatMode> ret = new List<ChatMode>();

            // 거래소
            if (battleManager.Mode == BattleMode.Lobby)
            {
                ret.Add(ChatMode.Lobby);
            }

            // 길드
            if (guildModel.HaveGuild)
            {
                ret.Add(ChatMode.Guild);
            }

            // 채널채팅
            ret.Add(ChatMode.Channel);

            // 정렬
            ret.Sort((A, B) =>
            {
                return A.GetSortOrder() > B.GetSortOrder() ? -1 : 1;
            });

            return ret.ToArray();
        }

        /// <summary>
        /// 귓속말 대상 정보 목록 반환
        /// </summary>
        private WhisperInfo[] GetWhisperInfos()
        {
            return chatModel.GetWhisperInfos();
        }

        /// <summary>
        /// 채팅 정보
        /// </summary>
        public ChatInfo Get(int index)
        {
            return chatModel.Get(index);
        }

        /// <summary>
        /// 최근 채팅 정보
        /// </summary>
        public ChatInfo GetRecentChat(ChatMode mode, int whisperCid = default)
        {
            return chatModel.GetRecentChat(mode, whisperCid);
        }

        /// <summary>
        /// 채팅 보내기
        /// </summary>
        public void SendChatMessage(string message)
        {
            chatModel.SendChatMessage(message);
        }

        public string GetGuildName()
        {
            return guildModel.GuildName;
        }

        public bool HasNewChatting(ChatMode chatMode = default, int whisperCid = default, bool allCheck = false)
        {
            return chatModel.HasNewChatting(chatMode, whisperCid, allCheck);
        }

        public void DeactiveNotice()
        {
            chatModel.DeactiveNotice(ChatMode);
        }

        /// <summary>
        /// 채널 변경 팝업 띄우기
        /// </summary>
        public async Task ChangeChannel()
        {
            string title = LocalizeKey._29007.ToText(); // 채널 변경
            string message = LocalizeKey._29008.ToText(); // 입장할 채널 번호를 입력해주세요

            int minChannal = chatModel.MinChannel;
            int maxChannel = chatModel.MaxChannel;
            string defaultText = StringBuilderPool.Get()
                .Append(minChannal).Append("~").Append(maxChannel)
                .Release();

            // 채널 변경
            int? result = await UI.InputPopupAsync(title, message, defaultText, maxChannel);
            if (result == null)
                return;

            bool isSuccess = await chatModel.RequestJoinChannelChat(result.Value);
            if (!isSuccess)
                return;

            view.Refresh();
        }

        public async Task UpdateNotice()
        {
            bool isSuccess = await chatModel.RequestToUpdateNoticeAsync();

            view.SetNotice(FilterUtils.GetServerMessage(chatModel.NotiMessage));
        }

        public void DragEvent(UIDragEvent.DragArrow dragArrow)
        {
            view.Drag(dragArrow);
        }
    }
}