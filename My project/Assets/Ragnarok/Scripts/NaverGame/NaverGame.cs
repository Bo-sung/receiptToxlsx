using UnityEngine;

namespace Ragnarok
{
    public class NaverGame : MonoBehaviour
    {
        [Header("NAVER GAME Lounge ID")]
        public string LoungeId = "RO_Labyrinth";

        [Header("NAVER GAME ClientId")]
        public string NaverLoginClientId = "MyXMa8lYFgL3Qx4mniwp";

        [Header("NAVER GAME ClientSecret")]
        public string NaverLoginClientSecret = "daVMUW2qAP";

        void Start()
        {
            if (Issue.NAVER_LOUNGE)
            {
                Debug.Log($"[NAVER GAME] Init");
#if !UNITY_EDITOR
                GLink.sharedInstance().init(LoungeId, NaverLoginClientId, NaverLoginClientSecret);
#endif
            }
        }

        public static void HomeBanner()
        {
            if (Issue.NAVER_LOUNGE)
            {
                Debug.Log($"[NAVER GAME] HomeBanner");
#if !UNITY_EDITOR
                GLink.sharedInstance().executeHomeBanner();
#endif
            }
        }
    }
}