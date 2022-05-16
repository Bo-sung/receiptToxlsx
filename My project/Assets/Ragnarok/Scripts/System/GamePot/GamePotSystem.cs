using CodeStage.AntiCheat.ObscuredTypes;
using GamePotUnity;
using System;
using System.Collections.Generic;
#if UNITY_IOS
using System.Runtime.InteropServices; 
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public class GamePotSystem : Singleton<GamePotSystem>, IGamePot, IGamePotImpl
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        public static extern void d(string apikey, string version, string appName, string username); 
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.Equals(SceneLoader.PRELOAD))
                return;

            GamePot.initPlugin();
            GamePot.setListener(Instance);
        }

        protected override void OnTitle()
        {
        }

        private bool isLogined = false;

        public event Action<NAgreeResultInfo> OnAgreeDialogSuccess;
        public event Action OnUpdateLinked;
        public event Action OnLogoutSuccess;
        public event Action OnUpdatePush;
        public event Action OnUpdateNightPush;
        public event Action OnPurchaseSuccess;
        public event Action<NPurchaseItem[]> OnPurchaseItemListSuccess;
        public event Action OnDeleteMeberSuccess;

        public void ShowAgreeDialog()
        {
            if (Application.isEditor)
            {
                OnAgreeDialogSuccess?.Invoke(null);
            }
            else
            {
                NAgreeInfo info = new NAgreeInfo();
                info.showNightPush = true;

                GamePot.showAgreeDialog(info);
            }
            GamePot.getLinkedList();
        }

        public string GetLoginTypeString()
        {
            NCommon.LoginType type = GamePot.getLastLoginType();

            switch (type)
            {
                case NCommon.LoginType.GOOGLE:
                    return LocalizeKey._201.ToText();

                case NCommon.LoginType.FACEBOOK:
                    return LocalizeKey._200.ToText();

                case NCommon.LoginType.GUEST:
                    return LocalizeKey._202.ToText();
            }
            return default;
        }

        public NCommon.LoginType GetLastLoginType()
        {
            return GamePot.getLastLoginType();
        }

        public void AutoLogin()
        {
            NCommon.LoginType type = GamePot.getLastLoginType();
            if (type != NCommon.LoginType.NONE)
                GamePot.login(type);
        }

        public void Login(NCommon.LoginType type)
        {
            // 약관이 GDPR 버전이 아닐경우 약관 팝업 커스터 세팅
            if (!Cheat.IsAgreeGDPR)
            {
                NAgreeInfo bulider = new NAgreeInfo();
                // 야간 관고성 수신도의 버튼 노출
                bulider.showNightPush = true;
                GamePot.setAutoAgreeBuilder(bulider);
            }

            UI.ShowIndicator(0f);
            isLogined = false;
            GamePot.login(type);
        }

        public void Logout()
        {
            GamePot.logout();
        }

        public bool IsLogin()
        {
            return isLogined;
        }

        public AuthLoginType GetAuthLoginType()
        {
            return AuthLoginType.GAME_POT;
        }

        public string GetUuid()
        {
            return SystemInfo.deviceUniqueIdentifier.Replace("-", "").Substring(0, 10).ToLower();
        }

        public string GetAccountKey()
        {
            return GetUuid();
        }

        public string GetAccountPassword()
        {
            return string.Empty;
        }

        public bool IsLinkedLogin(NCommon.LinkingType linkingType)
        {
            List<NLinkingInfo> linkedList = GamePot.getLinkedList();

            if (linkedList != null)
            {
                foreach (NLinkingInfo item in linkedList)
                {
                    if (linkingType == item.provider)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsGuestLogin()
        {
            List<NLinkingInfo> linkedList = GamePot.getLinkedList();
            return linkedList == null || linkedList.Count == 0;
        }

        public void ToggleLinked(NCommon.LinkingType linkingType)
        {
            if (IsLinkedLogin(linkingType))
            {
                Debug.Log($"연동 해제 요청 = {linkingType}");
                GamePot.deleteLinking(linkingType);
            }
            else
            {
                Debug.Log($"연동 요청 = {linkingType}");
                GamePot.createLinking(linkingType);
            }
        }

        /// <summary>
        /// 고객센터
        /// </summary>
        public void ShowCSWebView()
        {
            GamePot.showCSWebView();
        }

        /// <summary>
        /// 공지사항
        /// </summary>
        public void ShowNotice(bool showTodayButton)
        {
            if (!Application.isEditor)
            {
                GamePot.showNotice(showTodayButton);
            }
        }

        /// <summary>
        /// 이용약관
        /// </summary>
        public void ShowTerms()
        {
            GamePot.showTerms();
        }

        /// <summary>
        /// 개인정보취급방침
        /// </summary>
        public void ShowPrivacy()
        {
            GamePot.showPrivacy();
        }

        public void TogglePush()
        {
            GamePot.setPushStatus(!IsPush());
        }

        public void ToggleNightPush()
        {
            GamePot.setPushNightStatus(!IsNightPush());
        }

        public bool IsPush()
        {
            if (Application.isEditor)
                return true;

            return GamePot.getPushStatus().enable;
        }

        public bool IsNightPush()
        {
            if (Application.isEditor)
                return true;

            return GamePot.getPushStatus().night;
        }

        public void SendLogCharacter(GamePotSendLogCharacter sendLogCharacter)
        {
            if (Application.isEditor)
                return;

            bool result = GamePot.characterInfo(sendLogCharacter);

            if (!result)
            {
                Debug.LogError("GamePotSendLogCharacter Failed");
            }
        }

        /// <summary>
        /// 결제 상품 목록
        /// </summary>
        public NPurchaseItem[] GetPurchaseItems()
        {
            return GamePot.getPurchaseItems();
        }

        /// <summary>
        /// 결제 상품 목록 요청
        /// </summary>
        public void GetPurchaseDetailListAsync()
        {
            GamePot.getPurchaseDetailListAsync();
        }

        /// <summary>
        /// 결제 요청
        /// </summary>
        /// <param name="productId">마켓에 등록된 상품ID</param>
        /// <param name="uniqueId">서버인덱스_채널인덱스_uid_cid_상점테이블id_serverPosition</param>
        public void Purchase(string productId, string uniqueId)
        {
            if (Application.isEditor)
                return;

            // 결제 처리시 인디케이터 딜레이 제거
            UI.ShowIndicator(0f);
            GamePot.purchase(productId, uniqueId);
        }

        /// <summary>
        /// 쿠폰 번호 사용
        /// </summary>
        /// <param name="couponNumber">쿠폰 번호</param>
        /// <param name="userData">서버인덱스_채널인덱스_uid_cid_serverPosition_couponNumber</param>
        public void Coupon(string couponNumber, string userData)
        {
            if (Application.isEditor)
                return;

            // 쿠폰 처리시 인디케이터 딜레이 제거
            UI.ShowIndicator(0f);
            GamePot.coupon(couponNumber, userData);
        }

        /// <summary>
        /// 게임팟 멤버 ID
        /// </summary>
        public string GetMemberId()
        {
            if (GamePotSettings.MemberInfo == null)
            {
                return string.Empty;
            }

            return GamePotSettings.MemberInfo.memberid;
        }

        /// <summary>
        /// 회원 탈퇴
        /// </summary>
        public void DeleteMember()
        {
            GamePot.deleteMember();
        }

        #region 이벤트

        /// <summary>
        /// 로그인 성공
        /// </summary>
        void IGamePot.onLoginSuccess(NUserInfo userInfo)
        {
            UI.HideIndicator();
            GamePotSettings.MemberInfo = userInfo;
            isLogined = true;

            PerftestManager.SetId(userInfo.memberid);
#if UNITY_IOS
            string version = BuildSettings.Instance.BuildVersion;
            var sb = StringBuilderPool.Get();
            sb.Append("Labyrinth");
            if (GameServerConfig.IsGlobal())
            {
                sb.Append("Global");
            }
            d("zwjXddSHynKpQCVv", version, sb.Release(), userInfo.memberid);
#endif

            NCommon.LoginType type = GamePot.getLastLoginType();
            Debug.Log($"마지막 로그인 타입 = {type}");

            switch (type)
            {
                case NCommon.LoginType.GOOGLE:
                    Analytics.TrackEvent(TrackType.GoogleLogin);
                    break;

                case NCommon.LoginType.FACEBOOK:
                    Analytics.TrackEvent(TrackType.FacebookLogin);
                    break;

                case NCommon.LoginType.APPLE:
                    Analytics.TrackEvent(TrackType.AppleLogin);
                    break;
            }
        }

        /// <summary>
        /// 로그인 실패
        /// </summary>
        void IGamePot.onLoginFailure(NError error)
        {
            UI.HideIndicator();
            // 로그인을 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// 로그인 취소
        /// 사용자가 임의로 로그인을 취소한 경우
        /// </summary>
        void IGamePot.onLoginCancel()
        {
            UI.HideIndicator();
            UI.ConfirmPopup(LocalizeKey._530.ToText()); // 로그인을 취소하였습니다.
        }

        void IGamePot.onLoginExit()
        {

        }

        /// <summary>
        /// 강제 업데이트(스토어 버전과 클라이언트 버전이 다를 경우 호출)
        /// </summary>
        void IGamePot.onNeedUpdate(NAppStatus status)
        {
            // TODO: 파라미터로 넘어온 status 정보를 토대로 팝업을 만들어 사용자에게 알려줘야 합니다.
            // TODO: 아래 두 가지 방식 중 한 가지를 선택하세요.
            // case 1: 인게임 팝업을 통해 개발사에서 직접 UI 구현
            // case 2: SDK의 팝업을 사용(이 경우에는 아래 코드를 호출해 주세요.)
            GamePot.showAppStatusPopup(status.ToJson());
        }

        /// <summary>
        /// 점검(대시보드에 점검이 활성화되어 있는 경우 호출)
        /// </summary>
        void IGamePot.onMainternance(NAppStatus status)
        {
            // TODO: 파라미터로 넘어온 status 정보를 토대로 팝업을 만들어 사용자에게 알려줘야 합니다.
            // TODO: 아래 두 가지 방식 중 한 가지를 선택하세요.
            // case 1: 인게임 팝업을 통해 개발사에서 직접 UI 구현
            // case 2: SDK의 팝업을 사용(이 경우에는 아래 코드를 호출해 주세요.)
            GamePot.showAppStatusPopup(status.ToJson());
        }

        /// <summary>
        /// 앱 종료
        /// </summary>
        void IGamePot.onAppClose()
        {
            // TODO: 강제 업데이트나 점검 기능을 case 2 방식으로 구현하는 경우
            // TODO: 앱을 강제 종료할 수 있기 때문에 이 곳에 앱을 종료할 수 있도록 구현하세요.
            Application.Quit();
        }

        /// <summary>
        /// 로그아웃 성공
        /// </summary>
        void IGamePot.onLogoutSuccess()
        {
            isLogined = false;
            OnLogoutSuccess?.Invoke();
        }

        /// <summary>
        /// 로그아웃 실패
        /// </summary>
        void IGamePot.onLogoutFailure(NError error)
        {
            // 로그아웃을 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// 회원 탈퇴 성공
        /// </summary>
        void IGamePot.onDeleteMemberSuccess()
        {
            Debug.Log("회원 탈퇴 성공");
            OnDeleteMeberSuccess?.Invoke();
        }

        /// <summary>
        /// 회원 탈퇴 실패
        /// </summary>
        void IGamePot.onDeleteMemberFailure(NError error)
        {
            // 회원 탈퇴를 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// 계정 연동 성공
        /// </summary>
        void IGamePot.onCreateLinkingSuccess(NUserInfo userInfo)
        {
            Debug.Log("계정 연동 성공");
            OnUpdateLinked?.Invoke();
        }

        /// <summary>
        /// 계정 연동 실패
        /// </summary>
        void IGamePot.onCreateLinkingFailure(NError error)
        {
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// 계정 연동 취소
        /// </summary>
        void IGamePot.onCreateLinkingCancel()
        {
            Debug.Log("계정 연동 취소");
            // 유저가 계정연동을 취소한 경우
        }

        /// <summary>
        /// 계정 연동 해제 성공
        /// </summary>
        void IGamePot.onDeleteLinkingSuccess()
        {
            Debug.Log("계정 해제 성공");
            OnUpdateLinked?.Invoke();
        }

        /// <summary>
        /// 계정 연동 해제 실패
        /// </summary>
        void IGamePot.onDeleteLinkingFailure(NError error)
        {
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// 결제 상품 정보 목록 요청 성공
        /// </summary>
        void IGamePot.onPurchaseDetailListSuccess(NPurchaseItem[] purchaseInfoList)
        {
            OnPurchaseItemListSuccess?.Invoke(purchaseInfoList);
        }

        /// <summary>
        /// 인앱 상품 정보 목록 요청 실패
        /// </summary>
        void IGamePot.onPurchaseDetailListFailure(NError error)
        {
            const string SEPARATOR = "msg = ";

            string msg = error.message;
            if (msg.Contains(SEPARATOR))
            {
                int start = msg.IndexOf(SEPARATOR) + SEPARATOR.Length;
                msg = msg.Substring(start);
            }

            UI.ConfirmPopup(msg);
            UI.HideIndicator();
        }

        /// <summary>
        /// 인앱 결제 성공
        /// </summary>
        void IGamePot.onPurchaseSuccess(NPurchaseInfo purchaseInfo)
        {
            OnPurchaseSuccess?.Invoke();

            Debug.Log($"[onPurchaseSuccess] productId={purchaseInfo.productId}");
            Debug.Log($"[onPurchaseSuccess] price={purchaseInfo.price}");
            Debug.Log($"[onPurchaseSuccess] currency={purchaseInfo.currency}");

            if (double.TryParse(purchaseInfo.price.Replace(',', '.'), out double result))
            {
                Analytics.TrackEvent(TrackType.InAppPurchase);
                Analytics.TrackEvent(TrackType.FirstInAppPurchase, isUnique: true);
                Analytics.TrackPurchaseEvent(result, purchaseInfo.currency); // 구매 매출 값
            }
        }

        /// <summary>
        /// 인앱 결제 실패
        /// </summary>
        void IGamePot.onPurchaseFailure(NError error)
        {
            // 결제를 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            const string SEPARATOR = "msg = ";

            string msg = error.message;
            if (msg.Contains(SEPARATOR))
            {
                int start = msg.IndexOf(SEPARATOR) + SEPARATOR.Length;
                msg = msg.Substring(start);
            }

            UI.ConfirmPopup(msg);
            UI.HideIndicator();
        }

        /// <summary>
        /// 인앱 결제 취소
        /// </summary>
        void IGamePot.onPurchaseCancel()
        {
            UI.ConfirmPopup(LocalizeKey._90202.ToText()); // 결제를 취소하였습니다.
            UI.HideIndicator();
        }

        /// <summary>
        /// 쿠폰 사용 성공
        /// </summary>
        void IGamePot.onCouponSuccess()
        {
            UI.ConfirmPopup(LocalizeKey._90203.ToText()); // 쿠폰 사용을 성공하였습니다.
            UI.HideIndicator();
        }

        /// <summary>
        /// 쿠폰 사용 실패
        /// </summary>
        void IGamePot.onCouponFailure(NError error)
        {
            // 쿠폰 사용을 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
            UI.HideIndicator();
        }

        /// <summary>
        /// 푸시 상태 변경에 대한 서버 통신 성공
        /// </summary>
        void IGamePot.onPushSuccess()
        {
            OnUpdatePush?.Invoke();
        }

        /// <summary>
        /// 푸시 상태 변경에 대한 서버 통신 실패
        /// </summary>
        void IGamePot.onPushFailure(NError error)
        {
            // 푸시 상태 변경을 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// 야간 푸시 상태 변경에 대한 서버 통신 성공
        /// </summary>
        void IGamePot.onPushNightSuccess()
        {
            OnUpdateNightPush?.Invoke();
        }

        /// <summary>
        /// 야간 푸시 상태 변경에 대한 서버 통신 실패
        /// </summary>
        void IGamePot.onPushNightFailure(NError error)
        {
            // 야간 푸시 상태 변경을 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// 광고 푸시 상태 변경에 대한 서버 통신 성공
        /// </summary>
        void IGamePot.onPushAdSuccess()
        {
        }

        /// <summary>
        /// 광고 푸시 상태 변경에 대한 서버 통신 실패
        /// </summary>
        void IGamePot.onPushAdFailure(NError error)
        {
            // 광고 푸시 상태 변경을 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// 푸시 상태 변경에 대한 서버 통신 성공
        /// </summary>
        void IGamePot.onPushStatusSuccess()
        {
        }

        /// <summary>
        /// 푸시 상태 변경에 대한 서버 통신 실패
        /// </summary>
        void IGamePot.onPushStatusFailure(NError error)
        {
            // 푸시 상태 변경을 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
        }

        /// <summary>
        /// GAMEPOT 대시보드에서 클릭액션을 SCHEME으로 설정한 경우 해당 이미지를 클릭 시 SCHEME값을 전달드립니다.
        /// </summary>
        void IGamePot.onReceiveScheme(string scheme)
        {
        }

        /// <summary>
        /// 약관에 동의한 경우
        /// </summary>
        void IGamePot.onAgreeDialogSuccess(NAgreeResultInfo info)
        {
            // 약관 동의함..
            ObscuredPrefs.SetBool(Config.CONFIRM_AGREE, true);

            OnAgreeDialogSuccess?.Invoke(info);

            Debug.Log($"약관={info.ToJson()}");
            GamePot.setPushStatus(info.agreePush);
            GamePot.setPushNightStatus(info.agreeNight);
            // info.agree : 필수 약관을 모두 동의한 경우 true
            // info.agreeNight : 야간 광고성 수신 동의를 체크한 경우 true, 그렇지 않으면 false
            // agreeNight 값은 로그인 완료 후 setPushNightStatus api를 통해 전달하세요.
        }

        /// <summary>
        /// 약관 동의 실패
        /// </summary>
        void IGamePot.onAgreeDialogFailure(NError error)
        {
            // error.message를 팝업 등으로 유저에게 알려주세요.
            UI.ConfirmPopup(error.message);
        }

        void IGamePot.onLoadAchievementSuccess(List<NAchievementInfo> info)
        {
        }

        void IGamePot.onLoadAchievementFailure(NError error)
        {
        }

        void IGamePot.onLoadAchievementCancel()
        {
        }

        void IGamePot.onWebviewClose(string result)
        {
        }

        void IGamePot.onRequestTrackingAuthorization(NResultTrackingAuthorization resultState)
        {
        }

        void IGamePot.onCheckAppStatusSuccess()
        {
        }

        void IGamePot.onCheckAppStatusFailure(NError error)
        {
        }

        void IGamePot.onSendAgreeEmailSuccess(NAgreeSendEmailInfo resultInfo)
        {
        }

        void IGamePot.onSendAgreeEmailFailure(NError error)
        {
        }

        void IGamePot.onCheckAgreeEmailSuccess(NAgreeCheckEmailInfo resultInfo)
        {
        }

        void IGamePot.onCheckAgreeEmailFailure(NError error)
        {
        }

        void IGamePot.onSetAgreeInfoSuccess()
        {
        }

        void IGamePot.onSetAgreeInfoFailure(NError error)
        {
        }

        #endregion
    }
}