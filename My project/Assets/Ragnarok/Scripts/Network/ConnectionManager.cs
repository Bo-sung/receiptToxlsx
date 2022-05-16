using MEC;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class ConnectionManager : GameObjectSingleton<ConnectionManager>
    {
        private const string TAG = nameof(ConnectionManager);

        public delegate void OnServerChangeEvent(BattleMode pre, BattleMode cur);

        private const float PING_DELAY = 30f;

        private const float TIME_PAUSE_PING_DELAY = 60f;

        private static readonly char[] SEPARATOR = { ',' };

        private readonly GameServerConfig gameServerConfig = new GameServerConfig(); // 게임 서버 정보
        private readonly ResponseAwaiterQueue responseQueue = new ResponseAwaiterQueue(); // 응답 대기 큐
        private readonly GroupSendingQueue sendingQueue = new GroupSendingQueue(); // 호출 대기 큐

        private BuildSettings buildSettings;
        private LoginManager loginManager;
        private BattleManager battleManager;
        private PlayerEntity player;

        // <!-- Events --!>
        public static event OnServerChangeEvent OnServerChange; // 서버 변경 시도 이벤트
        public static event OnServerChangeEvent OnSuccessServerChange; // 서버 변경 성공 이벤트

        public event System.Action OnLostConnect; // 연결 해제 이벤트
        public event System.Action OnReconnect; // 재접속 이벤트
        public event System.Action OnDuplicateLoginDetected; // 접속 중 중복 로그인 감지 (재접속 필요)

        // <!-- Network --!>
        private SmartFox sfs2x;
        private bool isFinishedAuthServerConnect; // 인증 서버 연결 완료 여부
        private bool isTryServerChange; // 서버 이동 시도
        private bool isReconnecting; // 재접속 여부
        private bool isServerAllReady; // 서버 모든 준비 완료 여부
        private int lastJoinCid; // 마지막으로 게임맵 입장한 cid
        private BattleMode lastStartedBattleMode; // 마지막 진입 전투

        // <!-- Temp --!>
        private int savedLobbyChangeChannelId;
        private int savedSellerCid; // 판매자 cid
        private bool isTryGoToTitle;
        private bool isTryApplicationQuit;

        // <!-- SmartBeat --!>
        private CodeStage.AntiCheat.ObscuredTypes.ObscuredString uuid;

        // 온버프 활성화 여부(빌드시에만 적용)
        [SerializeField]
        private bool isOnBuff;

        // 약관동의 GDPR 여부(빌드시에만 적용)
        [SerializeField]
        private bool isAgreeGDPR;

        void Start()
        {
            buildSettings = BuildSettings.Instance;
            loginManager = LoginManager.Instance;
            battleManager = BattleManager.Instance;

            player = Entity.player;

            Initialize();

            AddEvents();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            isTryApplicationQuit = false;

            Disconnect();
            RemoveEvents();
        }

        void OnApplicationQuit()
        {
            Disconnect();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            isTryGoToTitle = false;

            responseQueue.Clear();
            sendingQueue.Clear();

            isFinishedAuthServerConnect = false;
            isTryServerChange = false;
            isReconnecting = false;
            isServerAllReady = false;
            lastJoinCid = 0;
            lastStartedBattleMode = default;

            savedLobbyChangeChannelId = 0;
            savedSellerCid = 0;

            Disconnect();
        }

        void Initialize()
        {
            gameServerConfig.Initialize();

#if UNITY_EDITOR
            string savedLastConnectIp = GetLastConnectIp(); // 마지막에 접속한 ip
            string connectIp = buildSettings.ServerConfig.connectIP; // 현재 접속하려는 ip

            if (!string.Equals(savedLastConnectIp, connectIp))
            {

                LoginManager.DeleteAccountInfo(); // 마지막 접속 정보 제거  
                DeleteServerGroupId();
            }
#endif
        }

        public void DeleteServerGroupId()
        {
            gameServerConfig.DeleteServerGroupId(); // 마지막으로 선택한 게임서버 그룹 id 제거
        }

        void Update()
        {
            sfs2x?.ProcessEvents();
        }

        /// <summary>
        /// 빌드 버전 반환
        /// </summary>
        public string GetBuildVersion()
        {
            return buildSettings.BuildVersion;
        }

        /// <summary>
        /// 로고 이미지 이름 반환
        /// </summary>
        public string GetLogoName()
        {
            return buildSettings.GetLogoName();
        }

        /// <summary>
        /// 페이스북 사용 여부
        /// </summary>
        public bool IsFaceBookFriend()
        {
            return buildSettings.IsFaceBookFriend;
        }

        /// <summary>
        /// 서버 선택
        /// </summary>
        public void SelectServer(int groupId)
        {
            gameServerConfig.SelectServer(groupId);
        }

        /// <summary>
        /// 선택한 서버 GroupId
        /// </summary>
        public int GetSelectServerGroupId()
        {
            return gameServerConfig.selectedGroupId;
        }

        /// <summary>
        /// 서버이름 LocalizeKey; 기본값은 현재서버
        /// </summary>
        public int GetServerNameKey(int serverID = -1)
        {
            if (serverID < 0) return BasisType.SERVER_NAME_ID.GetInt(gameServerConfig.selectedGroupId);
            else return BasisType.SERVER_NAME_ID.GetInt(serverID);
        }

        /// <summary>
        /// 선택된 서버 있는지 여부
        /// </summary>
        public bool HasSelectedServer()
        {
            return gameServerConfig.HasSelectedServer();
        }

        /// <summary>
        /// 지역 코드 조회
        /// </summary>
        public void RequestDetectCountry()
        {
            string tempCountryCode = null;
#if UNITY_EDITOR
            tempCountryCode = LanguageConfig.KOREAN.type;
#else
            tempCountryCode = GamePotUnity.GamePot.getCountry();
#endif

            string countryCode = tempCountryCode ?? string.Empty;
            Debug.Log($"{nameof(countryCode)} = { countryCode}");
            gameServerConfig.SetCountry(countryCode.ToUpper());
        }

        /// <summary>
        /// Url 주소 반환
        /// </summary>
        public string GetResourceUrl(string typeName, int version)
        {
            return $"{gameServerConfig.resourceUrl}/resources/{typeName}_{version}";
        }

        /// <summary>
        /// 텍스쳐 다운로드 Url 주소 반환
        /// </summary>
        public string GetResourceUrl(string fileName)
        {
            return $"{gameServerConfig.resourceUrl}/resources/eventimg/{fileName}";
        }

        /// <summary>
        /// 접속중인 서버 위치
        /// </summary>
        public string GetServerPosition()
        {
            return gameServerConfig.serverPosition;
        }

        public async Task AsyncAllServerInfo(System.Action<ServerInfoPacket[], bool> refreshServer)
        {
            Response response = await Protocol.REQUEST_ALL_SERVER_INFO.SendAsync();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            var infos = response.GetPacketArray<ServerInfoPacket>("1");
            infos = infos.Where(i => i.ServerExist).ToArray(); // 정보가 있는 서버리스트만..
            refreshServer?.Invoke(infos, true);
        }

        /// <summary>
        /// 인증서버 접속
        /// </summary>
        public async Task AsyncAuthServerConnect()
        {
            // 이미 접속되어 있는 경우
            if (IsConnected())
                return;

            // 토스 서버 입장해야 하는 경우
            if (gameServerConfig.isToss)
            {
                await AsyncConnect(gameServerConfig.connectIp, gameServerConfig.connectPort);
            }
            else
            {
                AuthServerConfig serverConfig = buildSettings.ServerConfig;
                await AsyncConnect(serverConfig.connectIP, serverConfig.connectPort);
            }
        }

        /// <summary>
        /// 인증서버 로그인
        /// </summary>
        public async Task AsyncAuthServerLogin()
        {
            // SmartBeat id 셋팅
            SetSmartBeatId();

            await AsyncAuthServerConnect(); // 인증서버 접속 (방어코드)

            AuthLoginType authLoginType = loginManager.GetAuthLoginType();
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", buildSettings.AppVersion); // 앱 버전
            sfs.PutByte("2", (byte)buildSettings.StoreType); // 스토어 타입
            sfs.PutByte("5", authLoginType.ToByteValue()); // 로그인 타입

#if UNITY_EDITOR

            if (LoginManager.IsUseInputLogin)
            {
                sfs.PutInt("g", LoginManager.ServerGroup - 1); // 선택 서버
            }
            else
            {
                if (HasSelectedServer())
                {
                    sfs.PutInt("g", GetSelectServerGroupId()); // 선택 서버
                }
                else
                {
                    sfs.PutUtfString("nc", GameServerConfig.CountryCode); // 지역코드
                }
            }

#else
            if (HasSelectedServer())
            {
                sfs.PutInt("g", GetSelectServerGroupId()); // 선택 서버
            }
            else
            {
                sfs.PutUtfString("nc", GameServerConfig.CountryCode); // 지역코드
            }
#endif

#if !UNITY_EDITOR
            sfs.PutUtfString("3", GamePotSettings.MemberInfo.token); // 토큰
            sfs.PutUtfString("m", GamePotUnity.GamePot.getMemberId()); // 멤버 아이디
            sfs.PutUtfString("s", GamePotUnity.GamePot.getMemberSocialId()); // 소셜 아이디
#endif

            string id = loginManager.GetAccountKey();
            string password = loginManager.GetAccountPassword();
            await AsyncLogin(id, password, Config.AuthZone, sfs);
        }

        /// <summary>
        /// 게임서버 접속
        /// </summary>
        public async Task AsyncGameServerConnect()
        {
            // 토스 서버 입장해야 하는 경우 => 인증부터 다시 처리
            if (gameServerConfig.isToss)
            {
                await AsyncAuthServerConnect();
                await AsyncAuthServerLogin();

                await AsyncGameServerConnect();
            }
            else
            {
                await AsyncConnect(gameServerConfig.connectIp, gameServerConfig.connectPort);
            }
        }

        /// <summary>
        /// 게임서버 로그인
        /// </summary>
        public async Task AsyncGameServerLogin()
        {
            // 연결 끊어짐 - 재접속
            if (!IsConnected())
            {
                await AsyncGameServerConnect();
            }

            AuthLoginType authLoginType = loginManager.GetAuthLoginType();
            LanguageConfig languageConfig = LanguageConfig.GetBytKey(Language.Current);
            var sfs = Protocol.NewInstance();
            sfs.PutByteArray("1", GetEncrypt(gameServerConfig.password)); // 패스워드 (암호화)
            sfs.PutInt("2", gameServerConfig.updateKey); // 업데이트 키
            sfs.PutInt("4", buildSettings.AppVersion); // 앱 버전
            sfs.PutByte("5", (byte)buildSettings.StoreType); // 스토어 타입
            sfs.PutByte("9", (byte)(authLoginType == AuthLoginType.INPUT ? 0 : 1));
            sfs.PutByteArray("10", GetEncrypt(loginManager.GetUuid())); // 유저 고유번호 (암호화)
            sfs.PutInt("12", gameServerConfig.userSessionKey); // 유저 세션 키
            sfs.PutUtfString("14", languageConfig.type); // 선택 언어 타입
            sfs.PutByte("15", (byte)languageConfig.Key); // 선택 언어 키
            sfs.PutInt("g", gameServerConfig.zoneIndex); // 서버 zone 인덱스

#if !UNITY_EDITOR
            sfs.PutUtfString("dt", GamePotUnity.GamePot.getMemberId()); // 멤버 아이디
            NPushInfo info = GamePotUnity.GamePot.getPushStatus();
            sfs.PutByte("16", info.enable.ToByteValue()); // 푸쉬 여부
            sfs.PutByte("17", info.night.ToByteValue()); // 야간 푸쉬 여부
#endif

            string id = gameServerConfig.accountKey;
            string password = gameServerConfig.password;
            await AsyncLogin(id, password, Config.GameZone, sfs);
        }

        /// <summary>
        /// 패킷 보내기
        /// </summary>
        public async Task<Response> AsyncSend(Protocol protocol, ISFSObject param)
        {
            // 타이틀 진행 중 또는 Quit 진행 중 일 경우에는 Send 보내지 않도록 처리
            if (isTryGoToTitle || isTryApplicationQuit)
                throw new EmptyNetworkException();

            // 재접속 중
            if (IsReconnecting())
                await Awaiters.While(IsReconnecting); // 재접속 완료중 대기

            // 연결 끊어짐 - 재접속
            if (!IsConnected())
            {
                // 재연결 가능
                if (CanReconnect())
                {
                    UI.ShowIndicator();
                    await AsyncReconnect(); // 재연결
                    UI.HideIndicator();
                }
                else
                {
                    throw new ConnectException(ResultCode.DISCONNECT, string.Empty);
                }
            }

            // 진입을 위한 프로토콜이 아닐 경우
            if (!protocol.IsEnterProtocol())
            {
                if (!IsServerAllReady())
                    await Awaiters.Until(IsServerAllReady); // 모든 서버 완료까지 대기
            }

            return await AsyncSendRequest(protocol, param);
        }

        private TaskAwaiter AsyncConnect(string host, int port)
        {
            TaskAwaiter awaiter = new TaskAwaiter();
            responseQueue.Enqueue(awaiter);

            Disconnect(); // 기존 연결 해제
            sfs2x = new SmartFox(Debug.isDebugBuild);
            Debug.Log($"sfs2x.Version={sfs2x.Version}");

            sfs2x.AddEventListener(SFSEvent.CONNECTION, OnConnection); // 연결 성공/실패
            sfs2x.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost); // 연결 끊어짐
            sfs2x.AddEventListener(SFSEvent.LOGIN, OnLogin); // 로그인 성공
            sfs2x.AddEventListener(SFSEvent.LOGIN_ERROR, OnLogin); // 로그인 실패
            sfs2x.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponses); // 커스터마이징

            Debug.Log($"[{TAG}] Connect: {nameof(host)} = {host}, {nameof(port)} = {port}");
            sfs2x.Connect(host, port);
            return awaiter;
        }

        private TaskAwaiter AsyncLogin(string accountKey, string password, string zone, ISFSObject sfs)
        {
            TaskAwaiter awaiter = new TaskAwaiter();
            responseQueue.Enqueue(awaiter);

            Debug.Log($"[{TAG}] Login: {nameof(accountKey)} = {accountKey}, {nameof(password)} = {password}\nParameters = {SmartFoxDumpUtils.GetDump(sfs)}");
            sfs2x.Send(new LoginRequest(accountKey, password, zone, sfs));
            return awaiter;
        }

        private ResponseAwaiter AsyncSendRequest(Protocol protocol, ISFSObject param)
        {
            // 타이틀 진행 중 또는 Quit 진행 중 일 경우에는 Send 보내지 않도록 처리
            if (isTryGoToTitle || isTryApplicationQuit)
                throw new EmptyNetworkException();

            if (protocol.protocolType == Protocol.ProtocolType.ResponseOnly)
                throw new System.Exception($"[응답만 할 수 있는 프로토콜] {protocol.ToString()}");

            ResponseAwaiter awaiter = new ResponseAwaiter(param);
            responseQueue.Enqueue(protocol.Key, awaiter);

            if (protocol.isIndicator)
            {
                UI.ShowIndicator();
            }

            if (protocol.sendingQueueGroupId.HasValue)
            {
                // 이미 보낸 그룹 아이디가 존재할 경우 대기열에 넣고 나중에 처리
                int queueGroupId = protocol.sendingQueueGroupId.Value;
                if (sendingQueue.HasSendingGroup(queueGroupId))
                {
                    sendingQueue.EnqueuePacket(queueGroupId, protocol, param);
                    return awaiter;
                }

                sendingQueue.AddSendingGroupId(queueGroupId); // 보낸 그룹 아이디 추가
            }

            SendRequest(protocol, param);
            return awaiter;
        }

        private void SendRequest(Protocol protocol, ISFSObject param)
        {
            if (protocol.IsShowLog())
            {
                Debug.Log($"<color=yellow>[클라 => 서버] {protocol.ToString()}</color>{SmartFoxDumpUtils.GetDump(param)}");
            }

            sfs2x.Send(new ExtensionRequest(protocol.Key, param));
        }

        public void Disconnect()
        {
            if (sfs2x != null)
            {
                sfs2x.RemoveAllEventListeners(); // 이벤트 제거

                if (sfs2x.IsConnected)
                    sfs2x.Disconnect();
            }

            sfs2x = null;
        }

        /// <summary>
        /// 강제 서버 변경
        /// </summary>
        private async Task ChangeServer(string connect, int port, int updateKey, int serverKey)
        {
            isTryServerChange = true;

            OnServerChange?.Invoke(lastStartedBattleMode, battleManager.GetSaveBattleMode());

            Disconnect(); // 기존 연결 해제 (다음 서버로 연결하기 위함)

            responseQueue.Clear(); // 응답 대기 큐 비우기
            sendingQueue.Clear(); // 호출 대기 큐 비우기

            gameServerConfig.SetConnectInfo(connect, port, updateKey, serverKey); // 서버 세팅

            await AsyncReconnect(); // 재연결
        }

        /// <summary>
        /// 재연결
        /// </summary>
        private async Task AsyncReconnect(float delay = 0f)
        {
            Debug.Log($"[{TAG}] 재접속 시도");

            OnReconnect?.Invoke();

            isReconnecting = true;
            isServerAllReady = false;

            if (delay > 0f)
                await Task.Delay(System.TimeSpan.FromSeconds(delay));

            if (!isFinishedAuthServerConnect)
            {
                await AsyncAuthServerConnect();
                await AsyncAuthServerLogin();
            }

            await AsyncGameServerConnect();
            await AsyncGameServerLogin();

            isReconnecting = false;

            // 캐릭터 접속 흔적이 음슴
            if (lastJoinCid == 0)
            {
                isServerAllReady = true;
            }
            else
            {
                await player.CharacterList.RequestJoinGame(isReconnection: true); // 캐릭터 게임 입장
            }

            BattleMode savedBattleMode = battleManager.GetSaveBattleMode();

            if (isTryServerChange)
            {
                isTryServerChange = false;
                OnSuccessServerChange?.Invoke(lastStartedBattleMode, savedBattleMode); // 재접속 성공
            }

            // 게임 입장 흔적이 음슴
            if (lastStartedBattleMode == default)
            {
                if (savedBattleMode == default)
                {
                    isServerAllReady = true;
                }
                else // 난전 등으로 인한 Stage 입장 처리
                {
                    if (savedBattleMode.IsReconnectableMode())
                    {
                        battleManager.StartSavedMode();
                    }
                    else
                    {
                        isServerAllReady = true;
                        Debug.LogError($"예측하지 못한 battleMode: {nameof(savedBattleMode)} = {savedBattleMode}");
                    }
                }
            }
            else
            {
                if (lastStartedBattleMode.IsReconnectableMode())
                {
                    battleManager.StartSavedMode();
                }
                else
                {
                    throw new ConnectException(ResultCode.DISCONNECT, string.Empty);
                }
            }
        }

        void OnConnection(BaseEvent evt)
        {
            const string SUCCESS = "success";
            bool isSuccess = (bool)evt.Params[SUCCESS];

            TaskAwaiter awaiter = responseQueue.Dequeue();
            ConnectException connectException;
            if (isSuccess)
            {
                connectException = null; // 성공
                StartCheckInternetState(); // 인터넷 상태 체크 시작
            }
            else
            {
                Disconnect(); // 기존 연결 해제

                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    connectException = new ConnectException(ResultCode.DISCONNECT, string.Empty); // 인터넷 꺼져있음
                }
                else
                {
                    connectException = new ConnectException(ResultCode.SERVER_IS_BUSY, string.Empty); // 서버 점검 중
                }
            }

            awaiter.Complete(connectException); // Finish
        }

        void OnConnectionLost(BaseEvent evt)
        {
            Disconnect(); // 기존 연결 해제

            const string REASON = "reason";
            string reason = (string)evt.Params[REASON];
#if UNITY_EDITOR
            Debug.LogError($"[{TAG}] 연결 끊어짐: 이유 = {reason}");
#endif

            OnLostConnect?.Invoke(); // 연결 끊어짐 이벤트 호출

            if (reason.Equals(ClientDisconnectionReason.BAN))
            {
                ResultCode.DUPLICATION_LOGIN.Execute(); // 다른 디바이스에서 로그인 했을경우
                return;
            }

            if (reason.Equals(ClientDisconnectionReason.KICK))
            {
                ResultCode.USER_KICK.Execute(); // 유저 킥
                return;
            }

            // 재연결 가능
            if (CanReconnect())
                return;

            ResultCode.DISCONNECT.Execute(); // 연결 끊어짐
        }

        /// <summary>
        /// 재연결 가능 상태 여부
        /// </summary>
        private bool CanReconnect()
        {
            BattleMode currentBattleMode = battleManager.Mode;
            if (currentBattleMode == default)
                return true;

            if (currentBattleMode.IsReconnectableMode())
                return true;

            return false;
        }

        void OnLogin(BaseEvent evt)
        {
            string eventType = evt.Type;
            if (eventType == SFSEvent.LOGIN)
            {
                Debug.Log($"[{TAG}] OnLoginSuccess: {nameof(isFinishedAuthServerConnect)} = {isFinishedAuthServerConnect}, {nameof(gameServerConfig.isToss)} = {gameServerConfig.isToss}");

                // 게임 서버 연결 성공!
                if (isFinishedAuthServerConnect)
                {
                    TaskAwaiter awaiter = responseQueue.Dequeue();
                    awaiter.Complete(null);

                    StartPingPong(); // 핑퐁 시작

                    if (lastJoinCid == 0)
                    {
                        isServerAllReady = true;
                    }
                }
            }
            else if (eventType == SFSEvent.LOGIN_ERROR)
            {
                Disconnect();

                const short GENERIC_ERRIR_CODE = 28; // 참고: http://docs2x.smartfoxserver.com/AdvancedTopics/client-error-messages
                const string CODE = "errorCode";
                const string MESSAGE = "errorMessage";
                short errorCode = (short)evt.Params[CODE];
                string errorMessage = evt.Params.Contains(MESSAGE) ? (string)evt.Params[MESSAGE] : string.Empty;
                Debug.Log($"[{TAG}] OnLoginError: {nameof(errorCode)} = {errorCode}, {nameof(errorMessage)} = {errorMessage}");

                TaskAwaiter awaiter = responseQueue.Dequeue();
                if (errorCode == GENERIC_ERRIR_CODE)
                {
                    string[] results = errorMessage.Split(SEPARATOR, System.StringSplitOptions.RemoveEmptyEntries);
                    if (results.Length > 0 && int.TryParse(results[0], out int errorKey))
                    {
                        ResultCode resultCode = ResultCode.GetByKey(errorKey);
                        if (resultCode == ResultCode.DUPLICATION_LOGIN)
                        {
                            awaiter.Complete(new DuplicateLoginDetectedException());
                            OnDuplicateLoginDetected?.Invoke();
                        }
                        else
                        {
                            string message = results.Length > 1 ? results[1] : string.Empty;
                            awaiter.Complete(new ConnectException(resultCode, message));
                        }
                    }
                    else
                    {
                        awaiter.Complete(new ConnectException(ResultCode.UNKNOWN, errorMessage));
                    }
                }
                else
                {
                    string message = SFSErrorCodes.GetErrorMessage(errorCode, sfs2x.Logger, errorMessage);
                    awaiter.Complete(new ConnectException(ResultCode.FAIL, message));
                }
            }
        }

        void OnExtensionResponses(BaseEvent evt)
        {
            const string CMD = "cmd";
            const string DATA = "params";
            string cmd = evt.Params[CMD] as string;
            ISFSObject param = evt.Params[DATA] as SFSObject;

            Protocol protocol = Protocol.GetByKey(cmd);
            if (protocol == null)
            {
                Debug.LogError($"유효하지 않은 프로토콜: {nameof(cmd)} = {cmd}\nParameters = {SmartFoxDumpUtils.GetDump(param)}");
                return;
            }

            if (protocol.IsShowLog())
            {
                Debug.Log($"<color=cyan>[서버 => 클라] {protocol.ToString()}</color>{SmartFoxDumpUtils.GetDump(param)}");
            }

            if (protocol.protocolType == Protocol.ProtocolType.Request)
            {
                ResponseAwaiter awaiter = responseQueue.Dequeue(cmd);
                Response response = new Response(param, awaiter.sendParam);

                // 실패의 경우 강제 로그 표기
                if (!response.isSuccess && !protocol.IsShowLog())
                {
                    Debug.Log($"<color=cyan>[서버 => 클라] {protocol.ToString()}</color>{SmartFoxDumpUtils.GetDump(param)}");
                }

                // 고정 이벤트 호출
                for (int i = 0; i < protocol.fixedEventList.Count; i++)
                {
                    protocol.fixedEventList[i]?.Invoke(response);
                }

                awaiter.Complete(response, null);
            }
            else
            {
                if (protocol.fixedEventList.Count > 0)
                {
                    Response response = new Response(param);

                    // 고정 이벤트 호출
                    for (int i = 0; i < protocol.fixedEventList.Count; i++)
                    {
                        protocol.fixedEventList[i]?.Invoke(response);
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError($"처리하지 않은 프로토콜: {protocol.ToString()}, {nameof(protocol.protocolType)} = {protocol.protocolType}");
#endif
                }
            }

            // Queue에 남아있는 패킷이 존재할 경우
            if (protocol.sendingQueueGroupId.HasValue)
            {
                int queueGroupId = protocol.sendingQueueGroupId.Value;
                if (sendingQueue.HasPacketQueue(queueGroupId))
                {
                    ProtocolTuple tuple = sendingQueue.DequeuePacket(queueGroupId);
                    SendRequest(tuple.protocol, tuple.param);
                }
                else
                {
                    sendingQueue.RemoveSendingGroupId(queueGroupId);
                }
            }

            if (protocol.isIndicator)
            {
                UI.HideIndicator();
            }
        }

        void OnGameServerInfo(Response response)
        {
            // GameServerInfo 응답까지 와야 Connection 완료
            TaskAwaiter awaiter = responseQueue.Dequeue();

            if (response.isSuccess)
            {
                Disconnect(); // 기존 연결 해제 (다음 서버로 연결하기 위함)

                GameServerPacket gameServerPacket = response.GetPacket<GameServerPacket>("1");
                bool isToss = response.GetByte("2") == 1;
                int userSessionKey = response.GetInt("3");
                string accountKey = response.GetUtfString("4");
                string password = response.ContainsKey("5") ? response.GetUtfString("5") : string.Empty;
                int linkeLogin = response.ContainsKey("6") ? response.GetInt("6") : -1;
                string serverPosition = response.GetUtfString("7");
                string resourceUrl;

                if (isToss)
                {
#if UNITY_EDITOR
                    LoginManager.DeleteAccountInfo(); // 기존 계정 정보 제거
#endif
                    resourceUrl = gameServerPacket.tossResourceUrl;
                }
                else
                {
                    // 토스 서버로 접속한 경우
                    if (gameServerConfig.isToss)
                    {
                        resourceUrl = gameServerConfig.resourceUrl;
                    }
                    else
                    {
                        resourceUrl = buildSettings.ServerConfig.resourceUrl;
                    }

                    isFinishedAuthServerConnect = true; // 인증서버 연결 완료

                    // 인증 서버 연결 성공!
                    gameServerConfig.SelectServer(gameServerPacket.zoneIndex);
                    gameServerConfig.SaveServerGroupId(); // 현재 게임서버 그룹 아이디 저장

#if UNITY_EDITOR
                    string connectIp = buildSettings.ServerConfig.connectIP; // 접속 ip
                    SaveLastConnectIp(connectIp); // 접속 ip 저장

                    AuthLoginType authLoginType = loginManager.GetAuthLoginType();
                    if (authLoginType == AuthLoginType.INPUT)
                        password = loginManager.GetAccountPassword(); // Input 로그인 시에는 password 값이 빈 값으로 온다.

                    LoginManager.SaveAccountInfo(accountKey, password); // 마지막 로그인 정보 저장
#endif
                }

                gameServerConfig.SetConnectInfo(gameServerPacket, isToss, userSessionKey, accountKey, password, linkeLogin, resourceUrl, serverPosition);
                awaiter.Complete(null); // 인증서버 로그인 성공
            }
            else
            {
                awaiter.Complete(new ResponseException(response)); // 인증서버 로그인 실패
            }
        }

        void OnReceiveMultiMazeLobbyJoinServerChange(Response response)
        {
            string ip = response.GetUtfString("1");
            int port = response.GetInt("2");
            int update_key = response.GetInt("3");
            int server_key = response.GetInt("4");
            ChangeServer(ip, port, update_key, server_key).WrapNetworkErrors();
        }

        void OnReceiveMultiMazeLobbyExitServerChange(Response response)
        {
            string ip = response.GetUtfString("1");
            int port = response.GetInt("2");
            int update_key = response.GetInt("3");
            int server_key = response.GetInt("4");
            ChangeServer(ip, port, update_key, server_key).WrapNetworkErrors();
        }

        void OnReceiveLobbyServerChange(Response response)
        {
            string ip = response.GetUtfString("1");
            int port = response.GetInt("2");
            int update_key = response.GetInt("3");
            int server_key = response.GetInt("4");
            savedLobbyChangeChannelId = response.GetInt("5");
            savedSellerCid = response.GetInt("6");
            ChangeServer(ip, port, update_key, server_key).WrapNetworkErrors();
        }

        void OnPublicMessage(Response response)
        {
            response.ShowResultCode();
        }

        void OnRequestPingPong(Response response)
        {
            StartPingPong();
        }

        void OnJoinedCharacter()
        {
            lastJoinCid = player.Character.Cid;
        }

        void OnStartBattle(BattleMode mode)
        {
            lastStartedBattleMode = mode;
            isServerAllReady = true;
        }

        public int GetSellerCid()
        {
            int sellerCid = savedSellerCid;
            savedSellerCid = 0; // 초기화
            return sellerCid;
        }

        private ByteArray GetEncrypt(string s)
        {
            AesEncrypter encrypter = new AesEncrypter(sfs2x.SessionToken);
            return new ByteArray(encrypter.encrypt(Encoding.UTF8.GetBytes(s)));
        }

        public string GetDecrypt(ByteArray s)
        {
            AesEncrypter encrypter = new AesEncrypter(sfs2x.SessionToken);
            return Encoding.UTF8.GetString(encrypter.decrypt(s.Bytes));
        }

        private bool IsConnected()
        {
            return sfs2x != null && sfs2x.IsConnected;
        }

        private bool IsReconnecting()
        {
            return isReconnecting;
        }

        private bool IsServerAllReady()
        {
            return isServerAllReady;
        }

        private void StartPingPong()
        {
            Timing.RunCoroutineSingleton(YieldPingPong().CancelWith(gameObject), nameof(YieldPingPong), SingletonBehavior.Overwrite);
        }

        private void StartCheckInternetState()
        {
            Timing.RunCoroutineSingleton(YieldCheckNetworkState().CancelWith(gameObject), nameof(YieldCheckNetworkState), SingletonBehavior.Overwrite);
        }

        private void AddEvents()
        {
            Protocol.GAME_SERVER_INFO.AddEvent(OnGameServerInfo);
            Protocol.RECEIVE_MULMAZE_WAITINGROOM_SERVERCHANGE.AddEvent(OnReceiveMultiMazeLobbyJoinServerChange);
            Protocol.RECEIVE_BATTLEFIELD_SERVERCHANGE.AddEvent(OnReceiveMultiMazeLobbyExitServerChange);
            Protocol.REQUEST_TRADE_PIRVATE_SERVER_CHANGE.AddEvent(OnReceiveLobbyServerChange);
            Protocol.PUBLIC_MESSAGE.AddEvent(OnPublicMessage);
            Protocol.REQUEST_PING.AddEvent(OnRequestPingPong);

            player.CharacterList.OnCharacterInit += OnJoinedCharacter;
            BattleManager.OnStart += OnStartBattle;

            BattleTime.OnPause += OnBattlePause;
            BattleTime.OnResume += OnBattleResume;
            Application.lowMemory += OnLowMemory;
        }

        private void RemoveEvents()
        {
            Application.lowMemory -= OnLowMemory;
            BattleTime.OnPause -= OnBattlePause;
            BattleTime.OnResume -= OnBattleResume;

            BattleManager.OnStart -= OnStartBattle;
            player.CharacterList.OnCharacterInit -= OnJoinedCharacter;

            Protocol.GAME_SERVER_INFO.RemoveEvent(OnGameServerInfo);
            Protocol.RECEIVE_MULMAZE_WAITINGROOM_SERVERCHANGE.RemoveEvent(OnReceiveMultiMazeLobbyJoinServerChange);
            Protocol.RECEIVE_BATTLEFIELD_SERVERCHANGE.RemoveEvent(OnReceiveMultiMazeLobbyExitServerChange);
            Protocol.REQUEST_TRADE_PIRVATE_SERVER_CHANGE.RemoveEvent(OnReceiveLobbyServerChange);
            Protocol.PUBLIC_MESSAGE.RemoveEvent(OnPublicMessage);
            Protocol.REQUEST_PING.RemoveEvent(OnRequestPingPong);
        }

        void OnLowMemory()
        {
            //System.GC.Collect();
            //Resources.UnloadUnusedAssets();
            //UnityEngine.Debug.Log("메모리 부족!!!!");
        }

        void OnBattlePause()
        {
            Timing.RunCoroutine(YieldRequestPause(), Segment.RealtimeUpdate, nameof(YieldRequestPause));
        }

        void OnBattleResume()
        {
            Timing.KillCoroutines(nameof(YieldRequestPause));
        }

        IEnumerator<float> YieldPingPong()
        {
            if (!IsConnected())
                yield break;

            yield return Timing.WaitForSeconds(PING_DELAY);

            if (!IsConnected())
                yield break;

            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", (long)Time.realtimeSinceStartup);
            sfs.PutInt("2", player.preBattleStatusInfo.Flee);
            sfs.PutInt("3", player.preBattleStatusInfo.AtkSpd);
            sfs.PutInt("4", player.preBattleStatusInfo.AtkRange);
            sfs.PutInt("5", player.preBattleStatusInfo.CriDmgRate);
            Protocol.REQUEST_PING.SendAsync(sfs).WrapNetworkErrors();
        }

        IEnumerator<float> YieldCheckNetworkState()
        {
            NetworkReachability savedNetworkState = Application.internetReachability;

            while (IsConnected())
            {
                if (savedNetworkState != Application.internetReachability) // 인터넷 상태 변경
                {
                    Disconnect(); // 연결 강제 해제
                    break;
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        IEnumerator<float> YieldRequestPause()
        {
            while (true)
            {
                Protocol.REQUEST_PAUSE.SendAsync().WrapNetworkErrors();
                yield return Timing.WaitForSeconds(TIME_PAUSE_PING_DELAY);
            }
        }

        public static void Reconnect(float delay = 0f)
        {
            Instance.AsyncReconnect(delay).WrapNetworkErrors();
        }

        public static void GoToTitle(string message)
        {
            Instance.isTryGoToTitle = true;
            Tutorial.Abort(); // 튜토리얼 강제 중지

            UI.ConfirmPopup(message, LoadIntroScene); // 타이틀 화면으로 이동
        }

        public static void Quit(string message)
        {
            Instance.isTryApplicationQuit = true;
            Tutorial.Abort(); // 튜토리얼 강제 중지

            UI.ConfirmPopup(message, QuitApplication); // 게임 종료
        }

        private static void LoadIntroScene()
        {
            Instance.isTryGoToTitle = false;
            SceneLoader.LoadIntro();
        }

        private static void QuitApplication()
        {
            Instance.isTryApplicationQuit = false;

            Application.Quit();
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

#if UNITY_EDITOR
        private string GetLastConnectIp()
        {
            return PlayerPrefs.GetString(Config.CONNECT_IP, string.Empty);
        }

        private void SaveLastConnectIp(string ip)
        {
            PlayerPrefs.SetString(Config.CONNECT_IP, ip);
            PlayerPrefs.Save();
        }
#endif

        private void SetSmartBeatId()
        {
            // SmartBeat id 셋팅
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = loginManager.GetUuid(); // 유저 고유번호
            }
            else
            {
                // uuid가 변경되었을 때
                if (!uuid.Equals(loginManager.GetUuid()))
                {
                    uuid = loginManager.GetUuid(); // 유저 고유번호
                }
            }
        }

        /// <summary>
        /// 대만 여부
        /// </summary>
        public bool IsTaiwan()
        {
            return GameServerConfig.IsGlobal() && gameServerConfig.zoneIndex != 0 && gameServerConfig.zoneIndex != 1; // 0:글로벌섭, 1:한국섭
        }

        /// <summary>
        /// 온버프 여부
        /// </summary>
        public bool IsOnBuff()
        {
            return isOnBuff;
        }

        /// <summary>
        /// 약관동의 GDPR 여부
        /// </summary>
        public bool IsAgreeGDPR()
        {
            return isAgreeGDPR;
        }
    }
}