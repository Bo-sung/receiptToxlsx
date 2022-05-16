using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIOption"/>
    /// </summary>
    public sealed class OptionPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void CloseChangeNamePopup();
        }

        public interface IViewAccount
        {
            void SetBtnGoogleView(bool isLinked);
            void SetBtnFacebookView(bool isLinked);
            void SetBtnAppleView(bool isLinked);
        }

        private readonly IView view;
        private readonly IViewAccount viewAccount;

        private readonly GameSoundSettings gameSoundSettings;
        private readonly GameGraphicSettings gameGraphicSettings;
        private readonly PlayerTrackingSettings playerTrackingSettings;
        private readonly SleepModeSettings sleepModeSettings;
        private readonly CharacterModel characterModel;
        private readonly LoginManager loginManager;
        private readonly IGamePotImpl gamePotImpl;
        private readonly UserModel userModel;
        private readonly GoodsModel goodsModel;
        private readonly ConnectionManager connectionManager;
        private readonly GameAdSettings gameAdSettings;

        private readonly int freeNameChangeCount, nameChangeCatCoin;

        public event System.Action OnUpdateOptionSetting
        {
            add { userModel.OnUpdateOptionSetting += value; }
            remove { userModel.OnUpdateOptionSetting -= value; }
        }

        public event System.Action OnUpdateAdAppCustom
        {
            add { gameAdSettings.OnChange += value; }
            remove { gameAdSettings.OnChange -= value; }
        }

        public event System.Action OnBuffAccountLink
        {
            add { userModel.OnBuffAccountLink += value; }
            remove { userModel.OnBuffAccountLink -= value; }
        }

        public event System.Action OnBuffAccountUnLink
        {
            add { userModel.OnBuffAccountUnLink += value; }
            remove { userModel.OnBuffAccountUnLink -= value; }
        }

        public OptionPresenter(IView view, IViewAccount viewAccount)
        {
            this.view = view;
            this.viewAccount = viewAccount;

            gameSoundSettings = GameSoundSettings.Instance;
            gameGraphicSettings = GameGraphicSettings.Instance;
            playerTrackingSettings = PlayerTrackingSettings.Instance;
            sleepModeSettings = SleepModeSettings.Instance;
            characterModel = Entity.player.Character;
            loginManager = LoginManager.Instance;
            gamePotImpl = GamePotSystem.Instance;
            userModel = Entity.player.User;
            goodsModel = Entity.player.Goods;
            connectionManager = ConnectionManager.Instance;
            gameAdSettings = GameAdSettings.Instance;

            freeNameChangeCount = BasisType.FREE_NAME_CHANGE_CNT.GetInt();
            nameChangeCatCoin = BasisType.NAME_CHANE_CAT_COIN.GetInt();
        }

        public override void AddEvent()
        {
            gamePotImpl.OnUpdateLinked += SetView;
            gamePotImpl.OnUpdatePush += OnUpdatePushState;
            gamePotImpl.OnUpdateNightPush += OnUpdateNightPushState;
            goodsModel.OnUpdateOnBuffMyPoint += OnUpdateOnBuffMyPoint;
            UI.AddEventLocalize(OnChangeLanguage);
        }

        public override void RemoveEvent()
        {
            gamePotImpl.OnUpdateLinked -= SetView;
            gamePotImpl.OnUpdatePush -= OnUpdatePushState;
            gamePotImpl.OnUpdateNightPush -= OnUpdateNightPushState;
            goodsModel.OnUpdateOnBuffMyPoint -= OnUpdateOnBuffMyPoint;
            UI.RemoveEventLocalize(OnChangeLanguage);
        }

        public string GetMemberId()
        {
            return gamePotImpl.GetMemberId();
        }

        public string GetCharacterHexId()
        {
            return characterModel.CidHex;
        }

        public string GetCharacterName()
        {
            return characterModel.Name;
        }

        public int GetFreeNameChangeCount()
        {
            return freeNameChangeCount;
        }

        public int GetNameChangeCatCoin()
        {
            // 무료 변경 가능할 경우
            if (characterModel.NameChangeCount < freeNameChangeCount)
                return 0;

            return nameChangeCatCoin;
        }

        public void RequestChangeName(string name)
        {
            RequestChangeNameAsync(name).WrapNetworkErrors();
        }

        public async void Logout()
        {
            if (!await UI.SelectPopup(LocalizeKey._90176.ToText())) // 로그아웃 하시겠습니까?\n(게스트 로그인 중이라면 플레이 데이터가 사라질 수 있습니다)
                return;

            loginManager.AsyncLogOut().WrapNetworkErrors(); // 로그 아웃
        }

        public bool GetBgmMute()
        {
            return gameSoundSettings.bgm.IsMute;
        }

        public void SetBgmMute(bool isMute)
        {
            gameSoundSettings.bgm.IsMute = isMute;
        }

        public float GetBgmVolume()
        {
            return gameSoundSettings.bgm.Volume;
        }

        public void SetBgmVolume(float volume)
        {
            gameSoundSettings.bgm.Volume = volume;
        }

        public bool GetSfxMute()
        {
            return gameSoundSettings.sfx.IsMute;
        }

        public void SetSfxMute(bool isMute)
        {
            gameSoundSettings.sfx.IsMute = isMute;
        }

        public float GetSfxVolume()
        {
            return gameSoundSettings.sfx.Volume;
        }

        public void SetSfxVolume(float volume)
        {
            gameSoundSettings.sfx.Volume = volume;
        }

        public GraphicQuality GetGraphicQualityLevel()
        {
            return gameGraphicSettings.QualityLevel;
        }

        public void SetGraphicQualityLevel(GraphicQuality graphicQuality)
        {
            gameGraphicSettings.QualityLevel = graphicQuality;
        }

        public PlayerTrackingType GetPlayerTrackingType()
        {
            return playerTrackingSettings.TrackingType;
        }

        public void SetPlayerTrackingType(PlayerTrackingType playerTrackingType)
        {
            playerTrackingSettings.TrackingType = playerTrackingType;
        }

        public SleepModeTime GetSleepModeTime()
        {
            return sleepModeSettings.SleepTime;
        }

        public void SetSleepModeTime(SleepModeTime sleepModeTime)
        {
            sleepModeSettings.SleepTime = sleepModeTime;
        }

        public bool IsAdAppCustom()
        {
            return gameAdSettings.GameAdAppCustomType == AdAppCustomType.On;
        }

        public void ToggleAdAppCustom()
        {
            // TODO 팝업 처리
            if (!IsAdAppCustom())
            {
                UI.Show<UIAdConsent>();
            }
            else
            {
                gameAdSettings.GameAdAppCustomType = AdAppCustomType.Off;
            }
        }

        private async Task RequestChangeNameAsync(string name)
        {
            await characterModel.RequestChangeName(name);
            view.CloseChangeNamePopup();
            view.Refresh();
        }

        public void SetView()
        {
            viewAccount.SetBtnGoogleView(gamePotImpl.IsLinkedLogin(NCommon.LinkingType.GOOGLE));
            viewAccount.SetBtnFacebookView(gamePotImpl.IsLinkedLogin(NCommon.LinkingType.FACEBOOK));
            viewAccount.SetBtnAppleView(gamePotImpl.IsLinkedLogin(NCommon.LinkingType.APPLE));
        }

        public void ShowCSWebView()
        {
            gamePotImpl.ShowCSWebView();
        }

        /// <summary>
        /// 게임 이용 약관
        /// </summary>
        public void ShowTerms()
        {
            if (GameServerConfig.IsKorea())
            {
                BasisUrl.KoreanTerms.OpenUrl();
            }
            else
            {
                gamePotImpl.ShowTerms();
            }
        }

        /// <summary>
        /// 개인 정보 처리 방침
        /// </summary>
        public void ShowPrivacy()
        {
            if (GameServerConfig.IsKorea())
            {
                BasisUrl.KoreanPrivacy.OpenUrl();
            }
            else
            {
                gamePotImpl.ShowPrivacy();
            }
        }

        /// <summary>
        /// 계정 연동 구글
        /// </summary>
        public void LinkGoogle()
        {
            gamePotImpl.ToggleLinked(NCommon.LinkingType.GOOGLE);
        }

        /// <summary>
        /// 계정 연동 페이스북
        /// </summary>
        public void LinkFaceBook()
        {
            gamePotImpl.ToggleLinked(NCommon.LinkingType.FACEBOOK);
        }

        /// <summary>
        /// 계정 연동 애플
        /// </summary>
        public void LinkApple()
        {
            gamePotImpl.ToggleLinked(NCommon.LinkingType.APPLE);
        }

        public bool IsPush()
        {
            return gamePotImpl.IsPush();
        }

        public bool IsNightPush()
        {
            return gamePotImpl.IsNightPush();
        }

        public bool IsSharePush()
        {
            return userModel.IsSharePush();
        }

        public void TogglePush()
        {
            gamePotImpl.TogglePush();
        }

        public void ToggleNightPush()
        {
            gamePotImpl.ToggleNightPush();
        }

        public void ToggleSharePush()
        {
            userModel.RequestOptionSetting(OptionSettingType.SharePush, (!userModel.IsSharePush()).ToByteValue()).WrapNetworkErrors();
        }

        void OnUpdatePushState()
        {
            userModel.RequestOptionSetting(OptionSettingType.NightPush, IsPush().ToByteValue()).WrapNetworkErrors();
        }

        void OnUpdateNightPushState()
        {
            userModel.RequestOptionSetting(OptionSettingType.NightPush, IsNightPush().ToByteValue()).WrapNetworkErrors();
        }

        void OnChangeLanguage()
        {
            userModel.RequestOptionSetting(OptionSettingType.Language, Language.Current.ToByteValue()).WrapNetworkErrors();
        }

        public void ShowNotice()
        {
            gamePotImpl.ShowNotice(false);
        }

        /// <summary>
        /// 검수 서버 여부
        /// </summary>
        /// <returns></returns>
        public bool IsStageServer()
        {
            string serverPosition = connectionManager.GetServerPosition();
            return serverPosition.Equals("stage");
        }

        /// <summary>
        /// 게임탈퇴 버튼 이벤트
        /// </summary>
        public void OnClickedBtnDeleteMember()
        {
            string title = LocalizeKey._14067.ToText(); // 게임 탈퇴 신청
            string description = LocalizeKey._90332.ToText(); // 게임신청 내용
            string confirmText = LocalizeKey._14068.ToText(); // 탈퇴

            UI.ConfirmPopupLong(title, description, confirmText, RequestDeleteMember); // 게임탈퇴 신청 확인 팝업
        }

        /// <summary>
        /// 게임탈퇴 신청
        /// </summary>
        private async void RequestDeleteMember()
        {
            string description = LocalizeKey._90322.ToText(); // 게임탈퇴 내용
            string confirmText = LocalizeKey._14065.ToText(); // 게임탈퇴
            string cancelText = LocalizeKey._2.ToText();

            bool? result = await UI.SelectClosePopup(description, confirmText, cancelText, ConfirmButtonType.Red);

            if (!result.HasValue || !result.Value)
                return;

            userModel.RequestDeleteMember().WrapNetworkErrors();
        }

        public bool IsOnBuffAccountLink()
        {
            return userModel.IsOnBuffAccountLink();
        }

        public string GetInnoUID()
        {
            return userModel.InnoUID;
        }

        /// <summary>
        /// 온버프 연동 요청
        /// </summary>
        public void RequestOnBuffLink(string innoUID)
        {
            userModel.RequestOnBuffAccountLink(innoUID.Trim()).WrapNetworkErrors();
        }

        /// <summary>
        /// 온버프 해제 요청
        /// </summary>
        public void RequestOnBuffUnLink()
        {
            userModel.RequestOnBuffAccountUnLink().WrapNetworkErrors();
        }

        /// <summary>
        /// 온버프 포인트 동기화 요청
        /// </summary>
        public void RequestOnBuffMyPoint()
        {
            goodsModel.RequestOnBuffMyPoint().WrapNetworkErrors();
        }

        /// <summary>
        /// 온버프 포인트 동기화 완료 이벤트
        /// </summary>
        private void OnUpdateOnBuffMyPoint()
        {
            UI.ShowToastPopup(LocalizeKey._90331.ToText()); // OnBuff 포인트가 동기화되었습니다.
        }
    }
}