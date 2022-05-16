using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using GamePotUnity.SimpleJSON;
using System.IO;
using System.Text;
using System.Linq;
using Realtime.LITJson;


#if VUPLEX
using Vuplex.WebView;
using Vuplex.WebView.Demos;
#endif

#if UNITY_STANDALONE
using GamePotUnity.Standalone.Networking;



namespace GamePotUnity.Standalone
{

    public class GamePotUnityPluginStandalone
    {
        #region Public Properties
        public static string MEMBER_ID { get; set; }
        public static string ADID { get { return _ADID; } }

        public static string PROJECT_ID { get; set; }
        public static string TOKEN { get; set; }
        public static string STORE { get; set; }
        #endregion

        #region Private Properties
        private static bool _initialized = false;
        private static string _ADID = "";
        private static bool _Beta = false;
        #endregion


        public static void initPlugin()
        {
            //Read GAMEPOT Configuration info from JSON
            var filePath = Path.Combine(Application.dataPath, "GamePotStandalone_Config.json");
            var json = File.ReadAllText(filePath);
            JSONNode config = JSONNode.Parse(json);

            PROJECT_ID = config["PROJECT_ID"];
            STORE = config["STORE"];
            GraphQLRequest.setup(config["API_URL"]);

            //Config 환경변수 confirm
            if (ReferenceEquals(PROJECT_ID, null) || string.IsNullOrEmpty(PROJECT_ID))
            {
                Debug.LogError("[GamePotStandalone] initPlugin : Project ID needs to set in GamePotConfig!!");
                return;
            }

            if (ReferenceEquals(STORE, null) || string.IsNullOrEmpty(STORE))
            {
                Debug.LogError("[GamePotStandalone] initPlugin : STORE Value  needs to set in GamePotConfig!!");
                return;
            }

            if (GamePotEventListener.s_instance != null) return;
            Debug.Log("GamePot - Creating GamePot Standalone Native Bridge Receiver");
            new GameObject("GamePotStandaloneManager", typeof(GamePotEventListener));

            //necessary instance initialized
            if (GamePotWebViewManager.s_instance == null)
            {
                Debug.Log("GamePot - Creating GamePotWebViewManager");
                GamePotWebViewManager.initialize();
            }

            //get ADID (async)
            Application.RequestAdvertisingIdentifierAsync((string adid, bool trackingEnabled, string error) =>
            {
                _ADID = adid;
            });

            GamePotChat.initialize();

            //먼저 shared 탐색. -> 없으면 포워딩
            // string server_url = _Beta ? PlayerPrefs.GetString("gamepot_domain_beta", "") : PlayerPrefs.GetString("gamepot_domain", "");
            // string socket_url = _Beta ? PlayerPrefs.GetString("gamepot_socket_beta", "") : PlayerPrefs.GetString("gamepot_socket", "");
            string server_url = "";
            string socket_url = "";

            if (string.IsNullOrEmpty(server_url.Trim()) || string.IsNullOrEmpty(socket_url.Trim()))
            {
                //캐싱된 주소가 없을 때
                RefreshForwardURL((bool success) =>
                {
                    if (success)
                    {
                        server_url = _Beta ? PlayerPrefs.GetString("gamepot_domain_beta", "") : PlayerPrefs.GetString("gamepot_domain", "");
                        socket_url = _Beta ? PlayerPrefs.GetString("gamepot_socket_beta", "") : PlayerPrefs.GetString("gamepot_socket", "");
                        GraphQLRequest.setup(string.IsNullOrEmpty(config["API_URL"]) ? server_url : (string)config["API_URL"]);
                        _initialized = true;
                    }
                    else
                    {
                        Debug.LogError("[GamePotUnityStandalone] Failed to initPlugin()!");
                    }
                });
            }
            else
            {
                //string opt_server_url = string.IsNullOrEmpty(config["API_URL"]) ? server_url : (string)config["API_URL"];
                GraphQLRequest.setup(string.IsNullOrEmpty(config["API_URL"]) ? server_url : (string)config["API_URL"]);

                //Refresh Forward Url Again
                RefreshForwardURL((bool success) =>
                {
                    if (success)
                    {
                        string _f_url = _Beta ? PlayerPrefs.GetString("gamepot_domain_beta", "") : PlayerPrefs.GetString("gamepot_domain", "");

                        if (string.IsNullOrEmpty(config["API_URL"]) && (_f_url != server_url))
                        {
                            GraphQLRequest.setup(_f_url);
                            Debug.Log("[GamePotUnityStandalone] RefreshForwardURL- Setup Recall to initial Server URL - " + _f_url);
                        }
                    }
                    else
                    {
                        Debug.LogError("[GamePotUnityStandalone] Failed to Refresh Forward Url!");
                    }
                });

                _initialized = true;
            }

#if UNITY_EDITOR
            UnityInterConnector interconnector = GamePotChat.s_unityInterconnector;
            interconnector.mainThread = System.Threading.Thread.CurrentThread;
#endif

        }


        private static void RefreshForwardURL(System.Action<bool> callback = null)
        {
            Debug.Log("[GamePotUnityStandalone] initPlugin() - Goto Forwarding Table..");

            string licenseUrl = _Beta ? "https://fw.gamepot.beta.ntruss.com/v1" : "https://gamepot.apigw.ntruss.com/fw/v1";
            //forwarding table
            WebRequest.RequestObject www
                = new WebRequest.RequestObject()
                {
                    mode = UnityWebRequest.kHttpVerbGET,
                    uri = licenseUrl + "/config",
                    header = new Dictionary<string, string>() { { "projectid", PROJECT_ID } },
                    callback = (long responseCode, string result) =>
                    {
#if GAMECHAT_DEBUG
                            Debug.LogFormat("Return from forwading Table : {0}", result);
#endif

                        if (responseCode != 200)
                        {
                            Debug.LogError("[GamePotUnityStandalone] RefreshForwardURL - Failed to Visit forwading Table - " + result);
                            if (callback != null) callback(false);
                            return;
                        }

                        JSONNode forwarding_url = JSONNode.Parse(result);
                        string server_url = "";
                        string socket_url = "";

                        try
                        {
                            server_url = forwarding_url["domain"];
                            socket_url = forwarding_url["socket"];
                            socket_url = socket_url.Replace("https://", "wss://");

                            Debug.Log("[GamePotUnityStandalone] refresh server_url : " + server_url);
                            Debug.Log("[GamePotUnityStandalone] refresh socket_url : " + socket_url);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("[GamePotUnityStandalone] RefreshForwardURL - " + ex.ToString());
                            if (callback != null) callback(false);
                            return;
                        }

                        if (string.IsNullOrEmpty(server_url) || string.IsNullOrEmpty(socket_url))
                        {
                            Debug.LogErrorFormat("[GamePotUnityStandalone] RefreshForwardURL -  Failed to Refresh. server_url or socket_url is empty. ServerUrl : {0}, SocketUrl : {1}", server_url, socket_url);
                            if (callback != null) callback(false);
                            return;
                        }

                        //Refresh Caching
                        if (_Beta)
                        {
                            PlayerPrefs.SetString("gamepot_domain_beta", server_url);
                            PlayerPrefs.SetString("gamepot_socket_beta", socket_url);
                        }
                        else
                        {
                            PlayerPrefs.SetString("gamepot_domain", server_url);
                            PlayerPrefs.SetString("gamepot_socket", socket_url);
                        }

                        Debug.Log("[GamePotUnityStandalone] RefreshForwardURL - Forwarded Completed!");
                        if (callback != null) callback(true);
                    }
                };
            WebRequest.Http(www);
        }


        public static void login(NCommon.LoginType loginType)
        {
            //TODO LoginType별 분기 처리가 필요하면 여기에서 case 처리.

            checkAppStatus((bool success, NError err) =>
            {
                GamePotEventListener listener = GamePotEventListener.s_instance;
                if (success)
                {
                    GraphQLRequest.setMember(PROJECT_ID, TOKEN, ADID, GamePotDeviceInfo.DeviceModel, GamePotDeviceInfo.NetworkType, GamePotDeviceInfo.AppVersion, GamePotDeviceInfo.DeviceModel);

                    ////Todo : need code renewal
                    string socket_url = _Beta ? PlayerPrefs.GetString("gamepot_socket_beta", "") : PlayerPrefs.GetString("gamepot_socket", "");
                    GamePotChat.setValue(PROJECT_ID, TOKEN, STORE);
                    GamePotChat.setup(socket_url);    //CCU

                    listener.onLoginSuccess(GamePotSettings.MemberInfo.ToJson());
                }
                else
                {
                    listener.onLoginFailure(err.ToJson());
                }
            });
        }


        public static void loginByThirdPartySDK(string providerId)
        {
            if (_initialized)
            {
                GraphQLRequest.signInV3(PROJECT_ID, STORE, NCommon.LoginType.THIRDPARTYSDK.ToString().ToLower(), providerId,
                    "", "",
                    (bool success, JSONNode result) =>
                    {
                        Debug.Log("[GamePotStandalone] signInV3 - " + result.ToString());
                        GamePotEventListener listener = GamePotEventListener.s_instance;
                        if (listener == null)
                        {
                            Debug.LogError("[GamePotStandalone] signInV3 - GamePotEventListener not initialized");
                            return;
                        }

                        if (success)
                        {
                            //GamePotSettings.MemberInfo = new NUserInfo();
                            GamePotSettings.MemberInfo = JsonMapper.ToObject<NUserInfo>(result.ToString());
                            //Debug.Log("[GamePotStandalone] MemberInfo - " + GamePotSettings.MemberInfo.ToJson());
                            login(NCommon.LoginType.THIRDPARTYSDK);
                        }
                        else
                        {
                            NError err = new NError(result);
                            listener.onLoginFailure(err.ToJson());
                        }
                    });
            }
        }



        public static void checkAppStatus(GamePotCallbackDelegate.CB_Common callback)
        {
            if (_initialized)
            {
                NUserInfo user_info = GamePotSettings.MemberInfo;
                MEMBER_ID = user_info.memberid;
                TOKEN = user_info.token;

                Debug.LogFormat("[GamePotStandalone] checkAppStatus - Member id : {0}", MEMBER_ID);
                Debug.LogFormat("[GamePotStandalone] checkAppStatus - TOKEN  : {0}", TOKEN);

                GraphQLRequest.maintenanceAppV2(PROJECT_ID, TOKEN, STORE,
                    (bool success, JSONNode result) =>
                    {
                        GamePotEventListener listener = GamePotEventListener.s_instance;
                        if (listener == null)
                        {
                            Debug.LogError("[GamePotStandalone] maintenanceAppV2 - GamePotEventListener not initialized");
                            return;
                        }

                        if (success)
                        {
                            //ON_MAINTENANCE
                            if (result != null)
                            {
                                //return app_status
                                listener.onMainternance(result.ToString());
                                return;
                            }
                            callback(true);
                        }
                        else
                        {
                            callback(false, new NError(result));
                        }
                    });
            }
        }

        public static void showWebView(string _url, Dictionary<string, string> _header = null)
        {
            if (GamePotWebViewManager.s_instance == null)
            {
                Debug.LogError("[GamePotUnityStandalone] showCSWebView - GamePotUnityStandalone is not initialized!!");
                return;
            }

#if VUPLEX
            GamePotWebViewManager webview_mgr = GamePotWebViewManager.s_instance;
            webview_mgr._webview = CanvasWebViewPrefab.Instantiate().gameObject;

            GameObject _frame = webview_mgr.transform.GetChild(0).gameObject;
            webview_mgr._webview.transform.parent = _frame.transform;
            webview_mgr._webview.transform.localPosition = new Vector3(0, 0, 0);
            webview_mgr._webview.transform.localScale = new Vector3(1, 1, 1);
            RectTransform rt = webview_mgr._webview.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);


            CanvasWebViewPrefab curWebView  = webview_mgr._webview.gameObject.GetComponent<CanvasWebViewPrefab>();

            if (_header != null)
            {
                curWebView.Initialized += (initializedSender, initializedEventArgs) =>
                {
                    curWebView.WebView.LoadUrl(_url, _header);
                    curWebView.WebView.MessageEmitted += (object sender, EventArgs<string> eventArgs)=>
                    {
                        var json = eventArgs.Value;
                        Debug.Log("Received Json :" + json);
                    };
                };
            }
            else
            {
                curWebView.Initialized += (initializedSender, initializedEventArgs) =>
                {
                    curWebView.WebView.LoadUrl(_url);
                    curWebView.WebView.MessageEmitted += (object sender, EventArgs<string> eventArgs) =>
                    {
                        var json = eventArgs.Value;
                        Debug.Log("Received Json :" + json);
                    };
                };
            }
#endif

            GamePotWebViewManager.showWebView();
        }

        public static void showCSWebView()
        {
            // if (GamePotWebViewManager.s_instance == null)
            // {
            //     Debug.LogError("[GamePotUnityStandalone] showCSWebView GamePotUnityStandalone is not initialized!!");
            //     return;
            // }
            // GamePotWebViewManager.s_instance.showWebView("https://gsrpkjibrmls4086645.gcdn.ntruss.com/demo/cs/question?projectid=ab2775b4-cf09-4794-9480-decd607a7f8a&store=google&memberid=4e125b06-462f-4c9f-8dbe-b1447bc9e370&device=android&sdkversion=2.1.2&language=ko");
        }

        public static void showFaq()
        {
            // if (GamePotWebViewManager.s_instance == null)
            // {
            //     Debug.LogError("[GamePotUnityStandalone] showCSWebView GamePotUnityStandalone is not initialized!!");
            //     return;
            // }
            // GamePotWebViewManager.s_instance.showWebView("https://gsrpkjibrmls4086645.gcdn.ntruss.com/demo/cs/faq?projectid=ab2775b4-cf09-4794-9480-decd607a7f8a&store=google&memberid=4e125b06-462f-4c9f-8dbe-b1447bc9e370&device=android&sdkversion=2.1.2&language=ko");
        }

        public static void showNoticeWebView()
        {
            if (GamePotWebViewManager.s_instance == null)
            {
                Debug.LogError("[GamePotUnityStandalone] showCSWebView GamePotUnityStandalone is not initialized!!");
                return;
            }
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("projectid", PROJECT_ID);
            header.Add("store", STORE);
            showWebView(GraphQLRequest.BASE_URL + "/notice", header);
            //Debug.Log(GraphQLRequest.BASE_URL + "/notice");
        }

        public static void coupon(string couponNumber, string userData)
        {
            if (_initialized)
            {
                GraphQLRequest.useCoupon(PROJECT_ID, TOKEN, couponNumber, STORE, userData,
                    (bool success, JSONNode result) =>
                    {
                        GamePotEventListener listener = GamePotEventListener.s_instance;
                        if (listener == null)
                        {
                            Debug.LogError("[GamePotStandalone] maintenanceAppV2 - GamePotEventListener not initialized");
                            return;
                        }

                        if (success)
                        {
                            listener.onCouponSuccess(result.ToString());
                        }
                        else
                        {
                            NError err = new NError(result);
                            listener.onCouponFailure(err.ToJson());
                        }
                    });
            }
        }

        public static bool characterInfo(string _data, GamePotCallbackDelegate.CB_Common callback = null)
        {
            if (_initialized)
            {
                JSONNode parsed = JSONNode.Parse(_data);
                parsed.Add("project_id", PROJECT_ID);
                parsed.Add("user_id", MEMBER_ID);

                //Wrapping JSON Object
                JSONNode _body = new JSONObject();
                _body.Add("body", parsed);
                JSONArray _arr_body = new JSONArray();
                _arr_body.Add(_body);
                JSONNode _opt_body = JSONNode.Parse(_arr_body.ToString());
                //Debug.Log("characterInfo ToJson : " + _opt_body.ToString());

                Dictionary<string, string> _header = new Dictionary<string, string>();
                _header.Add("Content-Type", "application/json");
                _header.Add("x-api-key", TOKEN);

                string end_point = GraphQLRequest.BASE_URL + "/v1/objects/GamePlayerProfile";
                Debug.Log("endpoint : " + end_point);

                NetworkListener networkListener = new NetworkListener()
                {
                    onNetworkSuccess = (JSONNode data) =>
                    {
                        if (data["status"] != null && data["status"] == 1)
                        {
                            if (callback != null) callback(true);
                            return;
                        }
                        //status code 1 반환 안됐을 때,
                        if (callback != null) callback(false, new NError { code = NError.CODE_UNKNOWN_ERROR, message = data.ToString() });
                    },
                    onNetworkFailure = (JSONNode error) =>
                    {
                        if (callback != null) callback(false, new NError(error));
                    }
                };

                WebRequest.LoadRequest(end_point, 2, networkListener, _header, _opt_body);
                return true;
            }
            return false;
        }

    }
}
#endif