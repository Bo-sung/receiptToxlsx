using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ChatModel : CharacterEntityModel, IEqualityComparer<ChatMode>
    {
        private const int MAX_QUEUE_SIZE = 100;

        public delegate void ChatEvent();

        public interface ISimpleChatInput : IInfo
        {
            int UID { get; }
            int CID { get; }
            string Nickname { get; }
            string Message { get; }
            bool IsGMMsg { get; }
        }

        private class RandomAccessibleQueue<T> : IReadOnlyList<T> where T : class
        {
            private int curBase;
            private int count;
            private readonly T[] buffer;

            public RandomAccessibleQueue(int maxSize)
            {
                buffer = new T[maxSize];
            }

            public T this[int index] => buffer[(curBase + index) % buffer.Length];

            public int Count => count;

            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < count; ++i)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                for (int i = 0; i < count; ++i)
                    yield return this[i];
            }

            public bool Enqueue(T item)
            {
                buffer[(curBase + count) % buffer.Length] = item;

                if (count == buffer.Length)
                {
                    curBase = (curBase + 1) % buffer.Length;
                    return false;
                }

                ++count;
                return true;
            }

            public T Dequeue()
            {
                if (count == 0)
                    return null;
                var ret = buffer[curBase];
                buffer[curBase] = null;
                curBase = (curBase + 1) % buffer.Length;
                --count;
                return ret;
            }

            public void Clear()
            {
                count = 0;
                for (int i = 0; i < buffer.Length; ++i)
                    buffer[i] = null;
            }
        }

        private readonly MultiMazeDataManager multiMazeDataRepo;
        private readonly ProfileDataManager profileDataRepo;
        private readonly RandomAccessibleQueue<ChatInfo> chatList; // 모든 채팅 리스트
        private readonly Dictionary<ChatMode, RandomAccessibleQueue<ChatInfo>> chatDic; // 귓속말을 제외한 모드별 채팅 메세지
        private readonly Dictionary<int, RandomAccessibleQueue<ChatInfo>> whisperDic; // Key: CID
        private readonly Dictionary<int, WhisperInfo> whisperInfoDic; // Key: CID

        private RemainTime chatCoolTime = 0;

        public event ChatEvent OnAddChatEvent;

        // 마을 채팅 말풍선 이벤트
        public event System.Action<ChatInfo> OnLobbyChat;

        /// <summary>
        /// 길드 채팅 말풍선 이벤트
        /// </summary>
        public event System.Action<ChatInfo> OnGuildChat;

        /// <summary>
        /// 확성기 수신 이벤트
        /// </summary>
        public event System.Action<ISimpleChatInput> OnResponseLoudSpeaker;

        /// <summary>
        /// 현재 채팅 모드
        /// </summary>
        public ChatMode ChatMode { get; private set; } = ChatMode.Channel;

        /// <summary>
        /// 귓속말 채팅일 경우의 상대 cid
        /// </summary>
        public int ChatWhisper { get; private set; } = default;

        /// <summary>
        /// 최소 채널
        /// </summary>
        public int MinChannel => 1;

        /// <summary>
        /// 최대 채널
        /// </summary>
        public int MaxChannel { get; private set; }

        /// <summary>
        /// 현재 채널
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// 로비 채널
        /// </summary>
        public int LobbyChannel { get; set; }

        /// <summary>
        /// 공지사항 관련 정보
        /// </summary>
        public string NotiMessage { get; private set; } // 공지사항 메시지
        public int NotiVersion { get; private set; }    // 공지사항 버전

        /// <summary>
        /// 현재 귓속말 상대 정보
        /// </summary>
        public WhisperInfo CurrentWhisperInfo { get; private set; }

        /// <summary>
        /// 확성기 큐
        /// </summary>
        public LoudSpeakerMessageQueue LoudSpeakerMessageManager { get; private set; }

        /// <summary>
        /// 새로운 개인상점 채팅 유무
        /// </summary>
        public bool HasNewSendPrivateStoreChat { get; private set; }

        /// <summary>
        /// 새로운 길드 채팅
        /// </summary>
        public bool HasNewChatGuild { get; private set; }

        /// <summary>
        /// 새로운 귓속말..
        /// </summary>
        public List<int> hasNewChatWhisperList = new List<int>();

        /// <summary>
        /// 새로운 개인상점 채팅이 있을 경우 호출
        /// </summary>
        public event System.Action OnUpdateHasNewSendPrivateStoreChat;

        /// <summary>
        /// 빨콩 상태(통합)가 갱신될때 호출
        /// </summary>
        public event System.Action<bool> OnUpdateAllChatNotice;

        public ChatModel()
        {
            chatList = new RandomAccessibleQueue<ChatInfo>(MAX_QUEUE_SIZE);
            chatDic = new Dictionary<ChatMode, RandomAccessibleQueue<ChatInfo>>(this);
            whisperDic = new Dictionary<int, RandomAccessibleQueue<ChatInfo>>(IntEqualityComparer.Default);
            whisperInfoDic = new Dictionary<int, WhisperInfo>(IntEqualityComparer.Default);
            LoudSpeakerMessageManager = new LoudSpeakerMessageQueue();
            multiMazeDataRepo = MultiMazeDataManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.REQUEST_CHANNEL_CHAT_MSG.AddEvent(OnRequestChannelChatMsg);
                Protocol.RESULT_CHAT_WISPER_MSG.AddEvent(OnResponseWhisper);
                Protocol.REQUEST_CHAT_ITEM_MSG.AddEvent(OnRequestChatItemMsg);
                Protocol.REQUEST_SYSTEM_MSG.AddEvent(OnRequestSystemMsg);
                Protocol.REQUEST_TRADEPRIVATE_ALLUSERCHAT.AddEvent(OnRequestLobbyChatMsg);
                Protocol.REQUEST_GUILD_ROOM_CHAT.AddEvent(OnRequestGuildChatMsg);
                Protocol.REQUEST_TRADEPRIVATE_USERSHOPCHAT.AddEvent(OnResponsePrivateStoreChat);
                Protocol.RECEIVE_LOUD_SPEAKER_MESSAGE.AddEvent(OnResponseLoudSpeakerMessage);
                Protocol.RECEIVE_MATMULMAZE_WAITINGMESSAGE.AddEvent(OnReceiveMatchMultiMazeWaitMessage);
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.REQUEST_CHANNEL_CHAT_MSG.RemoveEvent(OnRequestChannelChatMsg);
                Protocol.RESULT_CHAT_WISPER_MSG.RemoveEvent(OnResponseWhisper);
                Protocol.REQUEST_CHAT_ITEM_MSG.RemoveEvent(OnRequestChatItemMsg);
                Protocol.REQUEST_SYSTEM_MSG.RemoveEvent(OnRequestSystemMsg);
                Protocol.REQUEST_TRADEPRIVATE_ALLUSERCHAT.RemoveEvent(OnRequestLobbyChatMsg);
                Protocol.REQUEST_GUILD_ROOM_CHAT.RemoveEvent(OnRequestGuildChatMsg);
                Protocol.REQUEST_TRADEPRIVATE_USERSHOPCHAT.RemoveEvent(OnResponsePrivateStoreChat);
                Protocol.RECEIVE_LOUD_SPEAKER_MESSAGE.RemoveEvent(OnResponseLoudSpeakerMessage);
                Protocol.RECEIVE_MATMULMAZE_WAITINGMESSAGE.RemoveEvent(OnReceiveMatchMultiMazeWaitMessage);
            }
        }

        public override void ResetData()
        {
            base.ResetData();

            chatList.Clear();
            chatDic.Clear();
            whisperDic.Clear();
            whisperInfoDic.Clear();
            ChatMode = ChatMode.Channel;
        }

        /// <summary>
        /// 채팅 채널 최대치
        /// </summary>
        internal void Initialize(int maxChannel)
        {
            MaxChannel = maxChannel;
        }

        internal void JoinChannelChat()
        {
            RequestJoinChannelChat().WrapNetworkErrors();
        }

        /// <summary>
        /// 공지사항 설정
        /// </summary>
        public void SetNotice(string msg, int version)
        {
            NotiMessage = msg;
            NotiVersion = version;
        }

        /// <summary>
        /// 공지사항 갱신 요청
        /// </summary>
        public async Task<bool> RequestToUpdateNoticeAsync()
        {
            var param = Protocol.NewInstance();
            param.PutInt("1", NotiVersion);
            var response = await Protocol.REQUEST_CHAT_NOTICE_DATA.SendAsync(param);
            if (!response.isSuccess)
                return false;

            // 갱신
            NotiMessage = response.ContainsKey("1") ? response.GetUtfString("1") : NotiMessage;
            NotiVersion = response.ContainsKey("2") ? response.GetInt("2") : NotiVersion;

            return true;
        }

        /// <summary>
        /// 채팅 모드 설정
        /// </summary>
        public void SetChatMode(ChatMode chatMode, int whisperCid = default)
        {
            ChatMode = chatMode;
            ChatWhisper = whisperCid;

            // 빨콩 제거
            if (chatMode == ChatMode.Guild) SetHasNewChatGuild(false);
            if (ChatMode == ChatMode.Whisper) SetHasNewChatWhisper(false, whisperCid);

            if (whisperInfoDic.ContainsKey(whisperCid))
                SetWhisperInfo(whisperInfoDic[whisperCid]);

            // 사이즈 변경 이벤트 호출
            OnAddChatEvent?.Invoke();
        }

        /// <summary>
        /// 귓속말 대상 설정
        /// </summary>
        public void SetWhisperInfo(WhisperInfo info)
        {
            CurrentWhisperInfo = info;
        }

        /// <summary>
        /// 채팅 대화 개수
        /// </summary>
        public int GetDataSize()
        {
            return GetDataSize(ChatMode, CurrentWhisperInfo != null ? CurrentWhisperInfo.cid : default);
        }

        /// <summary>
        /// 채팅 대화 개수
        /// </summary>
        public int GetDataSize(ChatMode mode, int whisperCid = default)
        {
            if (mode == ChatMode.All)
                return chatList.Count;

            if (mode == ChatMode.Whisper)
                return whisperDic.ContainsKey(whisperCid) ? whisperDic[whisperCid].Count : 0;

            return chatDic.ContainsKey(mode) ? chatDic[mode].Count : 0;
        }

        /// <summary>
        /// 귓속말 상대 정보 목록
        /// </summary>
        public WhisperInfo[] GetWhisperInfos()
        {
            return whisperInfoDic.Values.ToArray();
        }

        /// <summary>
        /// 채팅 정보
        /// </summary>
        public ChatInfo Get(int index, int whisperCid = default)
        {
            return Get(ChatMode, index, whisperCid);
        }

        /// <summary>
        /// 채팅 정보
        /// </summary>
        public ChatInfo Get(ChatMode mode, int index, int whisperCid = default)
        {
            if (index < 0 || index >= GetDataSize(mode, whisperCid))
                return null;

            if (mode == ChatMode.Whisper)
                return whisperDic[whisperCid][index];

            if (mode == ChatMode.All)
                return chatList[index];

            return chatDic[mode][index];
        }

        /// <summary>
        /// 최근 채팅 정보
        /// </summary>
        public ChatInfo GetRecentChat(ChatMode mode, int whisperCid = default)
        {
            return Get(mode, GetDataSize(mode, whisperCid) - 1, whisperCid);
        }

        /// <summary>
        /// 채널 입장
        /// </summary>
        public async Task<bool> RequestJoinChannelChat(int channel = 0)
        {
            // 마지막에 접속한 채널 정보로 변경
            if (channel == 0)
            {
                channel = Channel;
            }

            channel = Mathf.Clamp(channel, MinChannel, MaxChannel);

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", channel);
            var response = await Protocol.REQUEST_JOIN_CHANNEL_CHAT.SendAsync(sfs);
            if (response.isSuccess)
            {
                Channel = response.GetInt("1");
                return true;
            }

            Debug.LogError($"채널 입장 실패 Channel={channel}");
            response.ShowResultCode();
            return false;
        }

        /// <summary>
        /// 채팅 메시지 전달
        /// </summary>
        public void SendChatMessage(string message)
        {
            if (chatCoolTime > 0)
            {
                UI.ShowToastPopup(LocalizeKey._45223.ToText()); // 조금만 천천히 입력해주세요.
                return;
            }

            chatCoolTime = 2000;

            switch (ChatMode)
            {
                default:
                case ChatMode.All:
                case ChatMode.Channel:
                    RequestChannelChatMsg(message);
                    break;

                case ChatMode.Guild:
                    RequestGuildChatMsg(message);
                    break;

                case ChatMode.Lobby:
                    RequestLobbyChatMsg(message);
                    break;

                case ChatMode.PrivateStoreChat:
                    Debug.LogError($"RequestPrivateStoreChat 사용");
                    break;

                case ChatMode.Whisper:
                    RequestWhisper(CurrentWhisperInfo.cid, message);
                    break;
            }
        }

        /// <summary>
        /// 채팅 보내기
        /// </summary>
        private void RequestChannelChatMsg(string message)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", message);
            Protocol.REQUEST_CHANNEL_CHAT_MSG.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 채팅 응답
        /// </summary>
        private void OnRequestChannelChatMsg(Response response)
        {
            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            // 메시지를 보낸 사람은 SuccessCode(0번 파라미터) 값이 존재
            // 그 외의 사람들은 SuccessCode(0번 파라미터) 값이 존재하지 않음
            // 1번 파라미터 값이 있는지에 대한 유무로 처리
            if (!response.ContainsKey("1"))
                return;

            int cid = response.GetInt("1"); // 보낸 캐릭터의 cid
            string name = response.GetUtfString("2"); // 보낸 캐릭터 이름
            string message = response.GetUtfString("3"); // 채팅 메시지
            byte job = response.GetByte("4"); // 직업 아이디
            short jobLevel = response.GetShort("5"); // 직업 레벨
            int uid = response.GetInt("6");
            string accountKey = response.GetUtfString("7");
            int channelIndex = response.ContainsKey("8") ? response.GetInt("8") : 0;
            byte gender = response.GetByte("9");
            int profileId = response.GetInt("10");

            AddChatMessage(ChatMode.Channel, cid, name, message, uid, job.ToEnum<Job>(), gender.ToEnum<Gender>(), profileId, accountKey, channel: channelIndex);
        }

        /// <summary>
        /// 로비 채팅 보내기
        /// </summary>
        private void RequestLobbyChatMsg(string message)
        {
            var param = Protocol.NewInstance();
            param.PutUtfString("1", message);
            Protocol.REQUEST_TRADEPRIVATE_ALLUSERCHAT.SendAsync(param).WrapNetworkErrors();
        }

        /// <summary>
        /// 로비 채팅 받음
        /// </summary>
        private void OnRequestLobbyChatMsg(Response response)
        {
            int speakerCID = response.GetInt("1");
            string speakerNickname = response.GetUtfString("2");
            string msg = response.GetUtfString("3");
            byte jobID = response.GetByte("4");
            short jobLevel = response.GetShort("5");
            byte gender = response.GetByte("6");
            int uid = response.ContainsKey("7") ? response.GetInt("7") : default;
            int profileId = response.GetInt("8");

            // 에러 방지 (내 전송 프로토콜의 success가 여기로 들어온다.)
            if (speakerCID == 0)
                return;

            AddChatMessage(ChatMode.Lobby, speakerCID, speakerNickname, msg, uid, job: jobID.ToEnum<Job>(), gender: gender.ToEnum<Gender>(), profileId, channel: LobbyChannel);
        }

        /// <summary>
        /// 길드 채팅 보내기
        /// </summary>
        /// <param name="message"></param>
        private void RequestGuildChatMsg(string message)
        {
            var param = Protocol.NewInstance();
            param.PutUtfString("1", message);
            Protocol.REQUEST_GUILD_ROOM_CHAT.SendAsync(param).WrapNetworkErrors();
        }

        private void OnRequestGuildChatMsg(Response response)
        {
            int speakerCID = response.GetInt("1");
            string speakerNickname = response.GetUtfString("2");
            string msg = response.GetUtfString("3");
            byte jobID = response.GetByte("4");
            short jobLevel = response.GetShort("5");
            byte gender = response.GetByte("6");
            int uid = response.ContainsKey("7") ? response.GetInt("7") : default;
            int profileId = response.GetInt("8");

            // 에러 방지 (내 전송 프로토콜의 success가 여기로 들어온다.)
            if (speakerCID == 0)
                return;

            AddChatMessage(ChatMode.Guild, speakerCID, speakerNickname, msg, uid, job: jobID.ToEnum<Job>(), gender: gender.ToEnum<Gender>(), profileId);
        }

        /// <summary>
        /// 귓속말 보내기
        /// </summary>
        /// <param name="destination_cid">상대의 cid. HexCode는 ChatModel의 CidToHexCode함수를 이용해서 변환.</param>
        /// <param name="msg">귓속말 내용</param>
        private async void RequestWhisper(int destination_cid, string msg)
        {
            var param = Protocol.NewInstance();
            param.PutUtfString("1", msg);
            param.PutInt("2", destination_cid);
            var res = await Protocol.REQUEST_CHAT_WISPER_MSG.SendAsync(param);
            // 내가 보낸 메시지에 대한 응답.
            // 1 : 성공
            // 60 : 실패
            if (res.resultCode == ResultCode.WISPERCHAT_FAIL)
            {
                // TODO: 채팅창에 표시
                //AddChatMessage(ChatMode.All, -1, "시스템", "귓속말 전송 실패", default, default, default);
                res.ShowResultCode();
                return;
            }

            AddChatMessage(ChatMode.Whisper, GetPlayerCID(), GetPlayerNickname(), msg, GetPlayerUid(), GetPlayerJob(), GetPlayerGender(), GetProfileId());
        }

        /// <summary>
        /// 귓속말 수신
        /// </summary>
        private void OnResponseWhisper(Response response)
        {
            int sender_cid = response.GetInt("1");
            string sender_name = response.GetUtfString("2");
            string message = response.GetUtfString("3");
            int sender_uid = response.GetInt("4");
            byte job = response.GetByte("5"); // 직업 아이디
            short jobLevel = response.GetShort("6"); // 직업 레벨
            string accountKey = response.GetUtfString("7");
            int channelIndex = 0;
            byte gender = response.GetByte("8");
            int profeilId = response.GetInt("9");

            // 채팅창에 표시
            AddChatMessage(ChatMode.Whisper, sender_cid, sender_name, message, sender_uid, job.ToEnum<Job>(), gender.ToEnum<Gender>(), profeilId, accountKey, channelIndex);
        }

        /// <summary>
        /// 아이템 메시지 채팅
        /// </summary>
        private void OnRequestChatItemMsg(Response response)
        {
        }

        /// <summary>
        /// 시스템 메시지 수신
        /// </summary>
        private void OnRequestSystemMsg(Response response)
        {
            if (response.isSuccess)
            {
                string message = response.GetUtfString("2");
                AddSystemMessage(message);
            }
        }

        /// <summary>
        /// 개인상점 미니채팅에 채팅 보내기
        /// </summary>
        public void RequestPrivateStoreChat(string message)
        {
#if !UNITY_EDITOR
            if (chatCoolTime > 0)
            {
                UI.ShowToastPopup(LocalizeKey._45223.ToText());
                return;
            }
#endif

            chatCoolTime = 2000;

            var param = Protocol.NewInstance();
            param.PutUtfString("1", message);
            Protocol.REQUEST_TRADEPRIVATE_USERSHOPCHAT.SendAsync(param).WrapNetworkErrors();
        }

        /// <summary>
        /// 개인상점 미니채팅 채팅 수신
        /// </summary>
        private void OnResponsePrivateStoreChat(Response response)
        {
            if (response.ContainsKey("0") && !response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("3"))
            {
                int cid = response.GetInt("1");
                string nickname = response.GetUtfString("2");
                string message = response.GetUtfString("3");
                Job job = response.GetByte("4").ToEnum<Job>();
                short jobLevel = response.GetShort("5");

                AddChatMessage(ChatMode.PrivateStoreChat, cid, nickname, message, uid: default, job, gender: Gender.Female, profileId: 0);
            }
        }

        /// <summary>
        /// 개인상점 나가기 처리
        /// </summary>
        public void RequestPrivateStoreChatExit()
        {
            Protocol.REQUEST_TRADEPRIVATE_EXITUSERSHOP.SendAsync().WrapNetworkErrors();
        }

        /// <summary>
        /// 확성기 수신
        /// </summary>
        private void OnResponseLoudSpeakerMessage(Response response)
        {
            int cid = response.GetInt("1");
            int uid = response.GetInt("2");
            string nick = response.GetUtfString("3");
            string message = response.GetUtfString("4");
            Job job = response.GetByte("5").ToEnum<Job>();
            Gender gender = response.GetByte("6").ToEnum<Gender>();
            int profileId = response.GetInt("7");

            ChatInfo chatInfo = new ChatInfo(profileDataRepo, cid, nick, message, uid, job, gender, profileId, accountKey: default, channel: default);

            LoudSpeakerMessageManager.Enqueue(chatInfo);
            OnResponseLoudSpeaker?.Invoke(chatInfo);
            AddChatMessage(ChatMode.Channel, cid, nick, message, uid, job, gender, profileId);
            AddChatMessage(ChatMode.Lobby, cid, nick, message, uid, job, gender, profileId);
        }

        /// <summary>
        /// 멀티매칭미로 확성기수신
        /// </summary>
        void OnReceiveMatchMultiMazeWaitMessage(Response response)
        {
            if (!PlayerEntity.IsJoinGameMap)
                return;

            int stage = response.GetInt("1");
            string nick = response.GetUtfString("2");
            int cid = response.GetInt("3");
            int uid = response.GetInt("4");
            Job job = response.GetByte("5").ToEnum<Job>();
            Gender gender = response.GetByte("6").ToEnum<Gender>();
            int profileId = response.GetInt("7");

            MultiMazeData data = multiMazeDataRepo.Get(stage);
            string dungeon = data == null ? string.Empty : data.name_id.ToText();
            string message = LocalizeKey._700.ToText() // {DUNGEON}에서 {NAME}님이 기다리고 있어요!
                .Replace(ReplaceKey.DUNGEON, dungeon)
                .Replace(ReplaceKey.NAME, nick);

            MultiMatchChatInfo multiMatchChatInfo = new MultiMatchChatInfo(profileDataRepo, cid, nick, message, uid, job, gender, profileId, accountKey: default, channel: default);
            multiMatchChatInfo.SetStage(stage);

            LoudSpeakerMessageManager.Enqueue(multiMatchChatInfo);
            OnResponseLoudSpeaker?.Invoke(multiMatchChatInfo);
            AddChatMessage(ChatMode.Channel, cid, nick, message, uid, job, gender, profileId);
            AddChatMessage(ChatMode.Lobby, cid, nick, message, uid, job, gender, profileId);
        }

        /// <summary>
        /// 채팅 기록 비우기
        /// </summary>
        public void ClearChatData(ChatMode mode, int whisperCid = default)
        {
            if (mode == ChatMode.Whisper)
            {
                // 귓속말 빨콩 제거
                SetHasNewChatWhisper(false, whisperCid);

                if (!whisperDic.ContainsKey(whisperCid))
                    return;

                whisperDic[whisperCid].Clear();
                return;
            }

            // 길챗 빨콩 제거
            if (mode == ChatMode.Guild)
                SetHasNewChatGuild(false);

            if (!chatDic.ContainsKey(mode))
                return;

            chatDic[mode].Clear();

            if (mode == ChatMode.PrivateStoreChat)
            {
                SetHasNewSendPrivateStoreChat(false); // 새로운 채팅 음슴
            }
        }

        private void AddChatMessage(ChatMode mode, int cid, string name, string message, int uid, Job job, Gender gender, int profileId, string accountKey = "", int channel = 0)
        {
            ChatInfo info = new ChatInfo(profileDataRepo, cid, name, FilterUtils.ReplaceChat(message), uid, job, gender, profileId, accountKey, channel);
            chatList.Enqueue(info);

            if (mode != ChatMode.Whisper)
            {
                if (!chatDic.ContainsKey(mode))
                    chatDic.Add(mode, new RandomAccessibleQueue<ChatInfo>(MAX_QUEUE_SIZE));

                chatDic[mode].Enqueue(info);
            }

            switch (mode)
            {
                case ChatMode.Guild:
                    // 길챗 빨콩
                    SetHasNewChatGuild(true);
                    OnGuildChat?.Invoke(info);
                    break;

                case ChatMode.Whisper:
                    {
                        bool isMyChat = (accountKey.Length == 0); // (GetPlayerCID() == cid);
                        int whisperCid = isMyChat ? CurrentWhisperInfo.cid : cid;

                        if (!isMyChat)
                        {
                            // 귓속말 상대 정보를 기록
                            if (ChatWhisper != whisperCid) AddWhisperInfo(uid, cid, name);
                        }

                        // 귓말 빨콩
                        SetHasNewChatWhisper(true, whisperCid);

                        // 채팅 내용을 기록
                        if (!whisperDic.ContainsKey(whisperCid))
                            whisperDic[whisperCid] = new RandomAccessibleQueue<ChatInfo>(MAX_QUEUE_SIZE);
                        whisperDic[whisperCid].Enqueue(info);
                    }
                    break;

                case ChatMode.Lobby:
                    OnLobbyChat?.Invoke(info);
                    break;

                case ChatMode.PrivateStoreChat:
                    SetHasNewSendPrivateStoreChat(true);
                    break;
            }

            OnAddChatEvent?.Invoke();
        }

        public void SetHasNewSendPrivateStoreChat(bool isNew)
        {
            HasNewSendPrivateStoreChat = isNew;
            OnUpdateHasNewSendPrivateStoreChat?.Invoke();
        }

        public void DeactiveNotice(ChatMode chatMode)
        {
            switch (chatMode)
            {
                case ChatMode.Guild:
                    SetHasNewChatGuild(false);
                    break;

                case ChatMode.Whisper:
                    SetHasNewChatWhisper(false, ChatWhisper);
                    break;
            }
        }

        // 길챗 빨콩 상태 갱신
        public void SetHasNewChatGuild(bool isNew)
        {
            if (HasNewChatGuild != isNew)
            {
                HasNewChatGuild = isNew;
                OnUpdateAllChatNotice?.Invoke(HasNewChatting(allCheck: true));
            }
        }

        // 귓말 빨콩 상태 갱신
        public void SetHasNewChatWhisper(bool isNew, int whisperCid)
        {
            if (isNew)
            {
                if (!hasNewChatWhisperList.Contains(whisperCid))
                {
                    hasNewChatWhisperList.Add(whisperCid);
                    OnUpdateAllChatNotice?.Invoke(HasNewChatting(allCheck: true));
                }
            }
            else
            {
                if (hasNewChatWhisperList.Contains(whisperCid))
                {
                    hasNewChatWhisperList.Remove(whisperCid);
                    OnUpdateAllChatNotice?.Invoke(HasNewChatting(allCheck: true));
                }
            }
        }

        /// <summary>
        /// 빨콩 표시할지..
        /// </summary>
        public bool HasNewChatting(ChatMode chatMode = default, int whisperCid = default, bool allCheck = false)
        {
            if (allCheck)
            {
                return HasNewChatGuild || hasNewChatWhisperList.Count > 0;
            }

            switch (chatMode)
            {
                case ChatMode.Guild:
                    return HasNewChatGuild;

                case ChatMode.Whisper:
                    return hasNewChatWhisperList.Contains(whisperCid);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 공지 사항 메세지
        /// </summary>
        public void AddSystemMessage(string message)
        {
            if (!chatDic.ContainsKey(ChatMode.Channel))
                chatDic.Add(ChatMode.Channel, new RandomAccessibleQueue<ChatInfo>(MAX_QUEUE_SIZE));

            ChatInfo info = new ChatInfo(message);
            chatList.Enqueue(info);
            chatDic[ChatMode.Channel].Enqueue(info);

            OnAddChatEvent?.Invoke();
            LoudSpeakerMessageManager.Enqueue(info);
            OnResponseLoudSpeaker?.Invoke(info);
        }

        /// <summary>
        /// 귓속말 상대 정보 추가
        /// </summary>
        public WhisperInfo AddWhisperInfo(int uid, int cid, string nickname)
        {
            if (!whisperInfoDic.ContainsKey(cid))
                whisperInfoDic[cid] = new WhisperInfo(cid, uid, nickname);

            return whisperInfoDic[cid];
        }

        /// <summary>
        /// 유저 신고하기 요청
        /// </summary>
        /// <param name="targetCid"></param>
        /// <param name="reasonType"></param>
        /// <returns></returns>
        public async Task RequestChatReport(int targetCid, byte reasonType)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", targetCid);
            sfs.PutByte("3", reasonType);
            Response response = await Protocol.REQUEST_REPORT_CHAR.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            UI.Close<UIChatReport>();
            UI.ConfirmPopup(LocalizeKey._90258.ToText()); // 신고가 접수되었습니다.
        }

        bool IEqualityComparer<ChatMode>.Equals(ChatMode x, ChatMode y)
        {
            return x == y;
        }

        int IEqualityComparer<ChatMode>.GetHashCode(ChatMode obj)
        {
            return obj.GetHashCode();
        }

        private int GetPlayerCID() => Entity.Character.Cid;
        private string GetPlayerNickname() => Entity.GetName();
        private int GetPlayerUid() => Entity.User.UID;
        private Job GetPlayerJob() => Entity.Character.Job;
        private Gender GetPlayerGender() => Entity.Character.Gender;
        private int GetProfileId() => Entity.Character.ProfileId;
    }
}