using System.Runtime.InteropServices;
using AOT;
using CodeStage.AntiCheat.Detectors;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    [System.Serializable]
    public class AppGuardJsonAndroid
    {
        [System.Serializable]
        public class Info
        {
            public int type;
            public string data;
        }

        public Info info;
    }

    public class AntiCheatManager : GameObjectSingleton<AntiCheatManager>
    {

#if UNITY_IOS
        public delegate void NativeDelegateNotification(string param);
        //native
        [DllImport("__Internal")]
        public static extern void c(NativeDelegateNotification aa, bool useAlert);

        [MonoPInvokeCallback(typeof(NativeDelegateNotification))]
        public static void AppguardHandler(string aa)
        {
            Debug.Log("Unity Callback : " + aa);
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            PerftestManager.SetCallback("AntiCheatManager", "OnAppGuard", false);
#if UNITY_IOS
            c(AppguardHandler, true);
#endif

            SetNewCrytoKey();

            ObscuredCheatingDetector.StartDetection(OnObscuredCheatingDetected);
            ObscuredPrefs.onAlterationDetected = OnAlterationDetected;
            SpeedHackDetector.StartDetection(OnSpeedHackDetected);
        }

        protected override void OnTitle()
        {
        }

        void OnAppGuard(string msg)
        {
            byte[] arr = System.Convert.FromBase64String(msg);
            string json = System.Text.Encoding.UTF8.GetString(arr);
            AppGuardJsonAndroid data = JsonUtility.FromJson<AppGuardJsonAndroid>(json);
            if (data != null && data.info != null)
            {
                // 루팅 단말 탐지
                if (data.info.type == 4 && data.info.data.Equals("300"))
                {
                    UI.ConfirmPopup(LocalizeKey._543.ToText(), ProcessDetection); // 루팅한 기기는 해킹 방지를 위해 강제 종료됩니다.
                }
            }
        }

        /// <summary>
        /// 앱이 켜질때 새로운 비밀키 랜덤 세팅
        /// int,short,long,bool,byte,string 사용하고있다.
        /// 추가적 데이터형 필요하면 여기에 구현
        /// </summary>
        void SetNewCrytoKey()
        {
            int intCryptoKey;
            do
            {
                intCryptoKey = Random.Range(int.MinValue, int.MaxValue);
            }
            while (intCryptoKey == 0);
            ObscuredInt.SetNewCryptoKey(intCryptoKey);

            short shortCryptoKey;
            do
            {
                shortCryptoKey = (short)Random.Range(short.MinValue, short.MaxValue);
            }
            while (shortCryptoKey == 0);
            ObscuredShort.SetNewCryptoKey(shortCryptoKey);

            long longCryptoKey;
            do
            {
                longCryptoKey = Random.Range(int.MinValue, int.MaxValue);
            }
            while (longCryptoKey == 0);
            ObscuredLong.SetNewCryptoKey(longCryptoKey);

            ObscuredBool.SetNewCryptoKey((byte)Random.Range(1, 150));
            ObscuredByte.SetNewCryptoKey((byte)Random.Range(1, 255));
            ObscuredFloat.SetNewCryptoKey(Random.Range(100000, 999999));
            ObscuredString.SetNewCryptoKey(Random.Range(int.MinValue, int.MaxValue).ToString());
        }

        /// <summary>
        /// 메모리 변조 감지
        /// </summary>
        private void OnObscuredCheatingDetected()
        {
            Debug.LogError("OnObscuredCheatingDetected");
            ProcessDetection();
        }

        /// <summary>
        /// 로컬 저장데이터 변경 감지
        /// </summary>
        private void OnAlterationDetected()
        {
            Debug.LogError("OnAlterationDetected");
            ProcessDetection();
        }

        /// <summary>
        /// 스피드핵 감지
        /// </summary>
        private void OnSpeedHackDetected()
        {
            Debug.LogError("OnSpeedHackDetected");
            ProcessDetection();
        }

        /// <summary>
        /// dll 주입 감지
        /// </summary>
        private void OnInjectionDetected(string msg)
        {
            Debug.LogError("OnInjectionDetected");
            Debug.LogError(msg);
            ProcessDetection();
        }

        /// <summary>
        /// 해킹 감지에 대한 처리
        /// </summary>
        private void ProcessDetection()
        {
            Application.Quit();
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
