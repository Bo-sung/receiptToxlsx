//#define LOAD_LOCAL_DATA

using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIIntro"/>
    /// </summary>
    public sealed class IntroPresenter : ViewPresenter
    {
        private readonly ConnectionManager connectionManager;
        private readonly DataManager dataManager;
        private readonly AssetManager assetManager;
        private readonly UserModel userModel;
        private readonly LoginManager loginManager;
        private readonly CharacterListModel characterListModel;
        private readonly BattleManager battleManager;
        private readonly DungeonModel dungeonModel;
        private readonly IGamePotImpl gamePotImpl;
        private readonly CharacterModel characterModel;

        public event System.Action<UIIntro.State> OnUpdateState;

        public event System.Action<float> OnUpdateTotalProgress
        {
            add
            {
                dataManager.OnDownloadTotalProgress += value;
                assetManager.OnDownloadTotalProgress += value;
                dataManager.OnLoadTotalProgress += value;
                assetManager.OnLoadTotalProgress += value;
            }
            remove
            {
                dataManager.OnDownloadTotalProgress -= value;
                assetManager.OnDownloadTotalProgress -= value;
                dataManager.OnLoadTotalProgress -= value;
                assetManager.OnLoadTotalProgress -= value;
            }
        }

        public event System.Action<float> OnUpdateDetailProgress
        {
            add
            {
                dataManager.OnDownloadDetailProgress += value;
                assetManager.OnDownloadDetailProgress += value;
                dataManager.OnLoadDetailProgress += value;
                assetManager.OnLoadDetailProgress += value;
            }
            remove
            {
                dataManager.OnDownloadDetailProgress -= value;
                assetManager.OnDownloadDetailProgress -= value;
                dataManager.OnLoadDetailProgress -= value;
                assetManager.OnLoadDetailProgress -= value;
            }
        }

        public event System.Action<int, int> OnUpdateDownloadCountProgress
        {
            add
            {
                dataManager.OnDownloadCountProgress += value;
                assetManager.OnDownloadCountProgress += value;
                dataManager.OnLoadCountProgress += value;
                assetManager.OnLoadCountProgress += value;
            }
            remove
            {
                dataManager.OnDownloadCountProgress -= value;
                assetManager.OnDownloadCountProgress -= value;
                dataManager.OnLoadCountProgress -= value;
                assetManager.OnLoadCountProgress -= value;
            }
        }

        /// <summary>
        /// 계정 약관 동의 완료
        /// </summary>
        public event System.Action OnAgreedTerm;

        /// <summary>
        /// 게임 시작 준비 완료
        /// </summary>
        public event System.Action OnAllReady;

        /// <summary>
        /// Intro 종료
        /// </summary>
        public event System.Action OnFinished;

        /// <summary>
        /// 로드 이벤트
        /// </summary>
        public event System.Action OnStartLoad, OnFinishedLoad;

        public event System.Action<long> OnDownloadSpeed
        {
            add
            {
                dataManager.OnDownloadSpeed += value;
                assetManager.OnLoadloadSpeed += value;
            }
            remove
            {
                dataManager.OnDownloadSpeed -= value;
                assetManager.OnLoadloadSpeed -= value;
            }
        }

        public IntroPresenter()
        {
            connectionManager = ConnectionManager.Instance;
            dataManager = DataManager.Instance;
            assetManager = AssetManager.Instance;
            loginManager = LoginManager.Instance;
            battleManager = BattleManager.Instance;
            gamePotImpl = GamePotSystem.Instance;

            userModel = Entity.player.User;
            characterListModel = Entity.player.CharacterList;
            characterModel = Entity.player.Character;
            dungeonModel = Entity.player.Dungeon;
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
            gamePotImpl.OnAgreeDialogSuccess += OnAgreeDialogSuccess;
            connectionManager.OnDuplicateLoginDetected += RetryServerConnect; // 서버 접속 중에 중복 디바이스 로그인 발생되었을 경우 호출 (서버 재접속 시도)
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
            gamePotImpl.OnAgreeDialogSuccess -= OnAgreeDialogSuccess;
            connectionManager.OnDuplicateLoginDetected -= RetryServerConnect;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (mode == BattleMode.Prologue)
                return;

            OnFinished?.Invoke(); // 종료
        }

        void OnAgreeDialogSuccess(NAgreeResultInfo info)
        {
            OnAgreedTerm?.Invoke(); // 계정 약관 동의 완료
        }

        /// <summary>
        /// 버전 정보 반환
        /// </summary>
        public string GetBuildVersion()
        {
            return connectionManager.GetBuildVersion();
        }

        /// <summary>
        /// 로고 반환
        /// </summary>
        public string GetLogoTextureName()
        {
            return connectionManager.GetLogoName();
        }

        /// <summary>
        /// 마지막 로그인 타입
        /// </summary>
        public NCommon.LoginType GetLastLoginType()
        {
            return loginManager.GetLastLoginType();
        }

        /// <summary>
        /// 이용 약관
        /// </summary>
        public void ShowAgreeDialog()
        {
            gamePotImpl.ShowAgreeDialog();
        }

        /// <summary>
        /// 서버 로그인
        /// </summary>
        public void ConnectServer(bool isNeedLogin)
        {
            ServerLoginAsync(isNeedLogin).WrapNetworkErrors();
        }

        /// <summary>
        /// 보유 캐릭터 존재 유무
        /// </summary>
        public bool HasCharacter()
        {
            return characterListModel.HasCharacter();
        }

        /// <summary>
        /// 게임 맵 입장
        /// </summary>
        public void JoinGameMap()
        {
            AsyncJoinGameMap().WrapNetworkErrors();
        }

        /// <summary>
        /// 서버 재접속 시도
        /// </summary>
        private void RetryServerConnect()
        {
            ConnectServer(isNeedLogin: false); // 서버 연결 (이미 로그인하여 로그인 필요 음슴)
        }

        /// <summary>
        /// 서버로그인
        /// </summary>
        private async Task ServerLoginAsync(bool isNeedLogin)
        {
            // 다른 캐릭터로 즉시 접속
            if (IntroScene.IsBackToTitle)
            {
                IntroScene.IsBackToTitle = false;
                battleManager.StartFadeIn(duration: 0f);

                // 캐릭터 목록 정보 조회
                OnUpdateState?.Invoke(UIIntro.State.ReqCharacterList); // 캐릭터 목록 정보를 조회합니다.
                await characterListModel.RequestCharacterList();
            }
            else
            {
                if (isNeedLogin)
                {
                    // 계정 로그인 (GamePot으로 로그인 시에 점검 등을 알 수 있다.)
                    OnUpdateState?.Invoke(UIIntro.State.ReqAccountLogin); // 계정 로그인 중입니다.
                    await loginManager.LastAutoLogin(); // 자동로그인
                }

                // 지역코드 정보 요청
                OnUpdateState?.Invoke(UIIntro.State.ReqContryCode); // 서버 정보를 가져오는 중입니다.
                connectionManager.RequestDetectCountry();

                // 인증 서버 접속
                OnUpdateState?.Invoke(UIIntro.State.ReqAuthServerConnect); // 인증 요청을 시도합니다.
                await connectionManager.AsyncAuthServerConnect();

                // 인증 서버 로그인
                OnUpdateState?.Invoke(UIIntro.State.ReqAuthServerLogin); // 인증 요청 중입니다.
                await connectionManager.AsyncAuthServerLogin();

                // 게임 서버 접속
                OnUpdateState?.Invoke(UIIntro.State.ReqGameServerConnect); // 게임 서버에 연결 중입니다
                await connectionManager.AsyncGameServerConnect();

                // 게임 서버 로그인
                OnUpdateState?.Invoke(UIIntro.State.ReqGameServerLogin); // 게임에 접속 중입니다.
                await connectionManager.AsyncGameServerLogin();

                // 캐릭터 목록 정보 조회
                OnUpdateState?.Invoke(UIIntro.State.ReqCharacterList); // 캐릭터 목록 정보를 조회합니다.
                long deleteRemainTime = await characterListModel.RequestCharacterList();

                // 탈퇴 신청 계정
                if (deleteRemainTime > 0)
                {
                    RemainTime time = deleteRemainTime;
                    string description = LocalizeKey._13.ToText()
                        .Replace(ReplaceKey.TIME, time.ToRemainTime().ToStringTimeConatinsDayLocal()); // 탈퇴 신청중인 계정입니다.\n남은시간: {TIME}\n탈퇴를 철회하시겠습니까?
                    string confirmText = LocalizeKey._14.ToText(); // 탈퇴철회
                    string cancelText = LocalizeKey._15.ToText(); // 로그아웃

                    bool? result = await UI.SelectClosePopup(description, confirmText, cancelText, ConfirmButtonType.None);

                    if (!result.HasValue || !result.Value) // 로그아웃 처리
                    {
                        await loginManager.AsyncLogOut();
                        return;
                    }
                    else // 탈퇴 철회
                    {
                        await userModel.RequestAccountWithdawalCancel(); // 게임 탈퇴 철회
                    }
                }

#if LOAD_LOCAL_DATA
                // 로컬 데이터 로드
                OnUpdateState?.Invoke(UIIntro.State.LoadLocalData); // 데이터 파일을 로드 중입니다.
                await dataManager.LoadLocalData(); // 데이터 로드
#endif

                // 데이터 버전 체크
                OnUpdateState?.Invoke(UIIntro.State.CheckDataVersion); // 데이터 버전을 확인 중입니다.
                long dataDownSize = await dataManager.GetResoureceVersion();

                // 어셋번들 패치리스트 다운로드
                OnUpdateState?.Invoke(UIIntro.State.DownloadAssetBundlePatchList); // 리소스 버전을 확인 중입니다.
                long assetBundleDownloadSize = await assetManager.DownloadPatchList();

                bool isShowAssetBundleDownload = assetManager.IsShowDownload();
                long size = isShowAssetBundleDownload ? (dataDownSize + assetBundleDownloadSize) : dataDownSize;

                if (size > 0L)
                {
                    // {SIZE} 의 추가 데이터를 다운로드 받습니다.\n계속하시겠습니까?
                    string description = LocalizeKey._9.ToText().Replace(ReplaceKey.SIZE, size.FormatBytes());
                    if (!await UI.SelectPopup(LocalizeKey._5.ToText(), description))
                    {
                        Application.Quit();
#if UNITY_EDITOR
                        if (UnityEditor.EditorApplication.isPlaying)
                        {
                            UnityEditor.EditorApplication.isPlaying = false;
                        }
#endif
                        throw new System.Exception("다운로드 취소");
                    }
                }

                OnStartLoad?.Invoke(); // 리소스 로드 시작

#region 프롤로그 체크
                await Awaiters.NextFrame; // 다운로드 팝업이 닫히는 시간..
                if (!HasCharacter())
                {
                    await Awaiters.Seconds(1f); // 시놉시스 로딩 잠깐 보고 넘어감..
                    battleManager.StartBattle(BattleMode.Prologue, false);
                }
#endregion

                if (dataDownSize > 0L)
                {
                    OnUpdateState?.Invoke(UIIntro.State.DownloadData); // 데이터 파일을 다운로드 중입니다.
                    await dataManager.Download(); // 데이터 리소스 다운로드
                }

                OnUpdateState?.Invoke(UIIntro.State.LoadData); // 데이터 파일을 로드 중입니다.
                await dataManager.Load(); // 데이터 로드
                dataManager.InitializeData(); // 데이터 초기화

                if (assetBundleDownloadSize > 0L)
                {
                    // (리소스 파일을 다운로드 중입니다.) or (리소스 파일을 로드 중입니다.)
                    UIIntro.State state = isShowAssetBundleDownload ? UIIntro.State.DownloadAssetBundle : UIIntro.State.LoadAssetBundle;
                    OnUpdateState?.Invoke(state);
                    await assetManager.Download(); // 어셋번들 다운로드
                }

                OnUpdateState?.Invoke(UIIntro.State.LoadAssetBundle); // 리소스 파일을 로드 중입니다.
                await assetManager.Load(); // 어셋번들 로드

                OnFinishedLoad?.Invoke(); // 리소스 로드 완료
            }

            OnUpdateState?.Invoke(UIIntro.State.ReqUserInfo); // 유저 정보를 조회합니다.
            await userModel.RequestUserInfo(); // 유저 정보 조회
            await userModel.RequestOnBuffLogin(); // 온버프 로그인

            OnAllReady?.Invoke(); // 완료
        }

        /// <summary>
        /// 게임 맵 입장
        /// </summary>
        private async Task AsyncJoinGameMap()
        {
            ResultCode resultCode = await characterListModel.RequestJoinGame();

            if (resultCode != ResultCode.SUCCESS)
                return;

            characterModel.SendLogCharacter(); // 게임팟 유저 로그 전송
            dungeonModel.StartBattle(); // 맵 입장
        }
    }
}