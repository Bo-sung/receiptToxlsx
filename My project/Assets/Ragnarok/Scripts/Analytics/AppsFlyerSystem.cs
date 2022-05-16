using AppsFlyerSDK;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class AppsFlyerSystem : MonoBehaviour, IAppsFlyerConversionData
    {
        public static  bool IsEnable;

        [SerializeField] bool isEnable;
        [SerializeField] string devKey;
        [SerializeField] string appID;
        [SerializeField] bool isDebug;

        private void Awake()
        {
            IsEnable = isEnable;

            if (!IsEnabled())
                return;

            AppsFlyer.setIsDebug(isDebug);
            AppsFlyer.initSDK(devKey, appID);
            AppsFlyer.startSDK();
        }

        private static bool IsEnabled()
        {
#if UNITY_EDITOR
            return false;
#endif
            return IsEnable;
        }

        public static void trackEvent(string key, Dictionary<string, string> eventValues)
        {
            if (!IsEnabled())
                return;

            AppsFlyer.sendEvent(key, eventValues);
        }

        public void onConversionDataSuccess(string conversionData)
        {
            Debug.Log("onConversionDataSuccess");
        }

        public void onConversionDataFail(string error)
        {
            Debug.Log($"onConversionDataFail={error}");
        }

        public void onAppOpenAttribution(string attributionData)
        {
            Debug.Log("onConversionDataSuccess");
        }

        public void onAppOpenAttributionFailure(string error)
        {
            Debug.Log($"onAppOpenAttributionFailure={error}");
        }
    }
}
