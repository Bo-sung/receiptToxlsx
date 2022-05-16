using UnityEngine;

namespace Ragnarok
{
    public sealed class GameAdSettings : Singleton<GameAdSettings>
    {
        private const string KEY = nameof(AdAppCustomType);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            AdAppCustomType adAppCustomType = Instance.GetAdAppCustomType();
            Instance.SetAdAppCustomType(adAppCustomType);
        }

        public event System.Action OnChange;        

        public AdAppCustomType GameAdAppCustomType
        {
            get { return GetAdAppCustomType(); }
            set
            {
                if (GetAdAppCustomType() == value)
                {
                    OnChange?.Invoke();
                    return;
                }

                SetAdAppCustomType(value);
                OnChange?.Invoke();

                // 광고사에 트래킹 설정값 셋팅
                var activeTracking = value == AdAppCustomType.On;
                IronSourceManager.Instance.SetTracking(activeTracking);
            }
        }

        protected override void OnTitle()
        {
        }

        private AdAppCustomType GetAdAppCustomType()
        {
            int adAppCustomType = PlayerPrefs.GetInt(KEY, defaultValue: (int)AdAppCustomType.None);
            return adAppCustomType.ToEnum<AdAppCustomType>();
        }

        private void SetAdAppCustomType(AdAppCustomType adAppCustomType)
        {
            PlayerPrefs.SetInt(KEY, (int)adAppCustomType);

            switch (adAppCustomType)
            {
                case AdAppCustomType.None:
                    break;

                case AdAppCustomType.Off:
                    break;

                case AdAppCustomType.On:
                    break;

                default:
                    Debug.Log($"[올바르지 않은 {nameof(adAppCustomType)}] {nameof(adAppCustomType)} = {adAppCustomType}");
                    break;
            }
        }        
    }
}