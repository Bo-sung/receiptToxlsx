using com.adjust.sdk;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public static class Analytics
    {
        private static readonly Dictionary<string, string> tempEvent = new Dictionary<string, string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            TrackEvent(TrackType.FirstAppOpen, isUnique: true);
            TrackEvent(TrackType.AppOpen);
        }

        /// <summary>
        /// 통계 측정
        /// </summary>
        public static void TrackEvent(string type, bool isUnique = false)
        {
            if (!System.Enum.IsDefined(typeof(TrackType), type))
                return;

            TrackEvent(type.ToEnum<TrackType>(), isUnique);
        }

        /// <summary>
        /// 통계 측정
        /// </summary>
        public static void TrackEvent(TrackType type, bool isUnique = false)
        {
            string eventKey = AdjustSettings.trankingKeys[type];
            if (string.IsNullOrEmpty(eventKey))
                return;

            if (isUnique)
            {
                string key = type.ToString();
                if (PlayerPrefs.HasKey(key))
                {
                    Debug.Log($"중복 트랙킹 방지: {nameof(TrackType)} = {type}, {nameof(eventKey)} = {eventKey}");
                    return;
                }

                PlayerPrefs.SetInt(key, 0);
            }

            if (Adjust.IsEnable)
            {
                AdjustEvent adjustEvent = new AdjustEvent(eventKey);
                Adjust.trackEvent(adjustEvent);
            }

            if (AppsFlyerSystem.IsEnable)
            {
                AppsFlyerSystem.trackEvent(type.ToString(), tempEvent);
            }

            Debug.Log($"트래킹: {nameof(TrackType)} = {type}, {nameof(eventKey)} = {eventKey}");
        }

        /// <summary>
        /// 통계 측정 (인앱매출)
        /// </summary>
        public static void TrackPurchaseEvent(double amount, string currency)
        {
            string eventKey = AdjustSettings.trankingKeys[TrackType.InAppPurchaseRevenue];
            if (string.IsNullOrEmpty(eventKey))
                return;

            if (Adjust.IsEnable)
            {
                AdjustEvent adjustEvent = new AdjustEvent(eventKey);
                adjustEvent.setRevenue(amount, currency);
                Adjust.trackEvent(adjustEvent);
            }

            if (AppsFlyerSystem.IsEnable)
            {
                Dictionary<string, string> purchaseEvent = new Dictionary<string, string>();
                purchaseEvent.Add(AFInAppEvents.CURRENCY, currency);
                purchaseEvent.Add(AFInAppEvents.REVENUE, amount.ToString());
                purchaseEvent.Add(AFInAppEvents.QUANTITY, "1");
                AppsFlyerSystem.trackEvent(AFInAppEvents.PURCHASE, purchaseEvent);
            }

            Debug.Log($"인앱매출 트래킹: {nameof(eventKey)} = {eventKey}, {nameof(amount)} = {amount}, {nameof(currency)} = {currency}");
        }
    }
}