using UnityEngine;

namespace Ragnarok
{
    public class PlayerTrackingSettings : Singleton<PlayerTrackingSettings>
    {
        private const string KEY = nameof(PlayerTrackingType);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            PlayerTrackingType trackingType = Instance.GetPlayerTrackingType();
            Instance.SetPlayerTrackingType(trackingType);
        }

        public PlayerTrackingType TrackingType
        {
            get { return GetPlayerTrackingType(); }
            set
            {
                if (GetPlayerTrackingType() == value)
                    return;

                SetPlayerTrackingType(value);
            }
        }

        protected override void OnTitle()
        {
        }

        private PlayerTrackingType GetPlayerTrackingType()
        {
            int trackingType = PlayerPrefs.GetInt(KEY, defaultValue: (int)PlayerTrackingType.ZoomOut);
            return trackingType.ToEnum<PlayerTrackingType>();
        }

        private void SetPlayerTrackingType(PlayerTrackingType trackingType)
        {
            PlayerPrefs.SetInt(KEY, (int)trackingType);

            switch (trackingType)
            {
                case PlayerTrackingType.ZoomIn:
                    break;

                case PlayerTrackingType.None:
                    break;

                case PlayerTrackingType.ZoomOut:
                    break;

                default:
                    Debug.Log($"[올바르지 않은 {nameof(trackingType)}] {nameof(trackingType)} = {trackingType}");
                    break;
            }
        }
    }
}