using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ragnarok
{
    public class FacebookManager : GameObjectSingleton<FacebookManager>
    {
        enum FBCallbackType
        {
            Login,
            Friends,
            InviteFriends,
        }

        [SerializeField] List<string> AppLabels;
        [SerializeField] List<string> AppIds;

        const string PUBLIC_PROFILE = "public_profile";
        const string EMAIL = "email";
        const string USER_FRIENDS = "user_friends";
        const string APP_NON_USERS = "app_non_users";
        List<string> permissions;
        List<object> filter;
        List<string> friendIds;
        List<string> inviteIds;

        public event System.Action<string[]> OnCheckAccept;
        public event System.Action<string[]> OnCheckInvite;

        protected override void Awake()
        {
            base.Awake();

            permissions = new List<string>() { PUBLIC_PROFILE, EMAIL, USER_FRIENDS };
            filter = new List<object>() { APP_NON_USERS };
            friendIds = new List<string>();
            inviteIds = new List<string>();

            // 페이스북 앱 이벤트
            SetAppEvent();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnTitle()
        {
        }

        #region 앱 이벤트
        void OnApplicationPause(bool pauseStatus)
        {
            // Check the pauseStatus to see if we are in the foreground
            // or background
            if (!pauseStatus)
            {
                SetAppEvent();
            }
        }

        /// <summary>
        /// 페이스북 앱 이벤트
        /// </summary>
        void SetAppEvent()
        {
            // log when a user launches your app.
            if (FB.IsInitialized)
            {
                SendFacebookSettings();
            }
            else
            {
                FB.Init(() => {
                    SendFacebookSettings();
                });
            }
        }

        void SendFacebookSettings()
        {
            // GDPR
            var activeTracking = GameAdSettings.Instance.GameAdAppCustomType == AdAppCustomType.On;
            SetTracking(activeTracking);

            // 페이스북 CCPA 지원 관련
            FB.Mobile.SetDataProcessingOptions(new string[] { "LDU" }, 0, 0);

            // 앱 이벤트
            FB.ActivateApp();
        }

        public void SetTracking(bool activeTracking)
        {
            if (FB.IsInitialized)
            {
                FB.Mobile.SetAdvertiserIDCollectionEnabled(activeTracking);
            }
            else
            {
                FB.Init(() => {
                    FB.Mobile.SetAdvertiserIDCollectionEnabled(activeTracking);
                });
            }
        }
        #endregion

        #region 친구 초대
        // 게임 접속 시
        // 1. 접속 시에 내가 초대한 페북 친구 리스트를 서버로 부터 받음
        //    초대완료 친구들은 카운트만, 아닌 친구들은 리스트를 받음

        // UI 활성화 시
        // 1. 친구 리스트 중 페북설치 유저를 찾아 서버로 보상 요청을 보냄(완료시 리스트에서 삭제)

        // UI 초대하기 눌렀을 때    
        // 1. 친구 초대를 하면, 친구리스트에 없는 친구만 서버로 보냄

        // 친구초대 권한 체크
        public bool HasPermissionUserFriend()
        {
            // 초기화 체크
            Debug.LogError("_______________Init Check");
            if (!FB.IsInitialized)
                return false;

            // 로그인 체크
            Debug.LogError("_______________Login Check");
            if (!FB.IsLoggedIn)
                return false;

            // 토큰 체크
            if (AccessToken.CurrentAccessToken == null)
                return false;

            // 권한 체크
            Debug.LogError("_______________Permissions Check");
            foreach (string perm in AccessToken.CurrentAccessToken.Permissions)
            {
                if (USER_FRIENDS.Equals(perm))
                {
                    // 친구 권한이 있음.
                    return true;
                }
            }

            // 친구 권한 없음.
            Debug.LogError("_______________Permissions None");
            return false;
        }

        /// <summary>
        /// 친구초대 권한 요청 로그인
        /// </summary>
        public void LoginWithUserFriendPermission()
        {
            // 페북 초기화 체크
            Debug.LogError($"-{FB.IsInitialized}-_______________fb init");
            if (!FB.IsInitialized)
            {
                FB.Init(this.OnInitComplete, this.OnHideUnity);
                return;
            }

            // 권한요청 로그인
            Debug.LogError("_______________fb login");
            FB.LogInWithReadPermissions(permissions, this.LoginResult);
        }

        /// <summary>
        /// 친구 리스트 요청
        /// </summary>
        public void RequestFacebookFriends()
        {
            FB.API("/me/friends?limit=5000", HttpMethod.GET, FriendsResult);
        }

        /// <summary>
        /// 페이스북 친구초대 표시
        /// </summary>
        public void ShowFacebookInvite()
        {
            List<object> filter = new List<object>() { "app_non_users" };
            FB.AppRequest(LocalizeKey._6608.ToText(), // 초대
                null, filter, null, 0,
                LocalizeKey._6609.ToText(), // RO:Labyrinth
                LocalizeKey._6609.ToText(), // RO:Labyrinth
                InviteFriendsResult);
        }

        private void OnInitComplete()
        {
            Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);

            LoginWithUserFriendPermission();
        }

        private void OnHideUnity(bool isGameShown)
        {
            Debug.Log("Is game showing? " + isGameShown);
        }

        private void LoginResult(IResult result)
        {
            HandleResult(result, FBCallbackType.Login);
        }

        private void FriendsResult(IResult result)
        {
            HandleResult(result, FBCallbackType.Friends);
        }

        private void InviteFriendsResult(IResult result)
        {
            HandleResult(result, FBCallbackType.InviteFriends);
        }

        private void HandleResult(IResult result, FBCallbackType type)
        {
            if (result == null)
            {
                UI.ConfirmPopup("Null Response");
                return;
            }

            // Some platforms return the empty string instead of null.
            if (!string.IsNullOrEmpty(result.Error))
            {
                // "Error - Check log for details";
                UI.ConfirmPopup("Error Response:\n" + result.Error);
            }
            else if (result.Cancelled)
            {
                // "Cancelled - Check log for details";
                UI.ConfirmPopup(LocalizeKey._530.ToText()); // 로그인을 취소하였습니다.
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                // "Success - Check log for details";
                switch (type)
                {
                    case FBCallbackType.Login:
                        // 로그인 완료
                        // 서버로 내 정보(ID 정도?) 보냄; 저장 안함.
                        UI.ConfirmPopup(LocalizeKey._6611); // 이제 초대하기 기능을 사용할 수 있습니다.
                        break;
                    case FBCallbackType.Friends:
                        // 친구 리스트 받음
                        // 서버로 완료된 친구 리스트 정보 보냄
                        OnGetFriendsComplete(result.RawResult);
                        break;
                    case FBCallbackType.InviteFriends:
                        // 초대한 친구 리스트
                        // 서버로 초대한 친구 리스트 정보 보냄
                        CheckOnAppRequestComplete(result.RawResult);
                        break;
                    default:
                        Debug.LogError("___________________ 타입 추가해야함..");
                        break;
                }

                Debug.LogError($"type___________________{type}");
                Debug.LogError("Success Response:\n" + result.RawResult);
                return;
            }
            else
            {
                UI.ConfirmPopup("Empty Response\n");
            }
        }

        void OnGetFriendsComplete(string json)
        {
            friendIds.Clear();

            try
            {
                Hashtable Ht_Parent = Procurios.Public.JSON.JsonDecode(json) as Hashtable;
                if (Ht_Parent != null)
                {
                    if (Ht_Parent["data"] != null)
                    {
                        ArrayList arrayList = (ArrayList)Ht_Parent["data"];
                        if (arrayList != null)
                        {
                            for (int i = 0; i < arrayList.Count; i++)
                            {
                                Hashtable ht = (Hashtable)arrayList[i];
                                if (ht != null)
                                {
                                    friendIds.Add(ht["id"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                friendIds.Clear();
            }

            // 완료한 친구 상관없이 무조건 체크
            OnCheckAccept?.Invoke(friendIds.ToArray());
        }

        void CheckOnAppRequestComplete(string json)
        {
            inviteIds.Clear();

            try
            {
                Hashtable hashtable = Procurios.Public.JSON.JsonDecode(json) as Hashtable;
                Debug.LogError($"1_______________{json}");
#if UNITY_IOS
                string[] strIDs = (string[])(((ArrayList)hashtable["to"]).ToArray(typeof(string)));
#else
                string[] strIDs = hashtable["to"].ToString().Split(',');
#endif
                Debug.LogError($"2_______________{strIDs}");
                if (strIDs != null && strIDs.Length > 0)
                {
                    Debug.LogError($"3_______________");
                    for (int i = 0; i < strIDs.Length; i++)
                    {
                        Debug.LogError($"4_______________{strIDs[i]}");
                        if (!Regex.IsMatch(strIDs[i], @"^\d+$"))
                        {
                            Debug.LogError($"5_______________");
                            Debug.LogWarning("페이스북 아이디는 숫자로 구성되어있어야..  fb_id : " + strIDs[i]);
                            continue;
                        }

                        inviteIds.Add(strIDs[i]);
                    }
                }
            }
            catch
            {
                inviteIds.Clear();
            }

            // 초대한 친구가 있으면.. 체크
            if (inviteIds.Count > 0)
            {
                OnCheckInvite?.Invoke(inviteIds.ToArray());
            }
        }
        #endregion

        public List<string> GetAppLabels()
        {
            return AppLabels;
        }

        public List<string> GetAppIds()
        {
            return AppIds;
        }
    }
}
