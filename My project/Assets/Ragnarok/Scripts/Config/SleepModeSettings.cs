using UnityEngine;

namespace Ragnarok
{
    public sealed class SleepModeSettings : Singleton<SleepModeSettings>
    {
        private const string KEY = "SleepModeSettings";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SleepModeTime sleepTime = Instance.GetSleepModeTime();
            Instance.SetSleepModeTime(sleepTime);
        }

        public SleepModeTime SleepTime
        {
            get { return GetSleepModeTime(); }
            set
            {
                if (GetSleepModeTime() == value)
                    return;

                SetSleepModeTime(value);
            }
        }

        protected override void OnTitle()
        {
        }

        private SleepModeTime GetSleepModeTime()
        {
            int sleepTime = PlayerPrefs.GetInt(KEY, (int)SleepModeTime.FiveMinutes);
            return sleepTime.ToEnum<SleepModeTime>();
        }

        private void SetSleepModeTime(SleepModeTime sleepModeTime)
        {
            PlayerPrefs.SetInt(KEY, (int)sleepModeTime);

            // TODO 절전모드 처리
            switch (sleepModeTime)
            {
                case SleepModeTime.OneMinute:
                    break;

                case SleepModeTime.FiveMinutes:
                    break;

                case SleepModeTime.TenMinutes:
                    break;

                default:
                    Debug.Log($"[올바르지 않은 {nameof(sleepModeTime)}] {nameof(sleepModeTime)} = {sleepModeTime}");
                    break;
            }
        }
    }
}