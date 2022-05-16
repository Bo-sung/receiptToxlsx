using UnityEngine;

namespace Ragnarok
{
    public class UIAutoSleepModeSettings : UIOptionEtcSettings
    {
        GameObject myGameObject;

        protected override void Awake()
        {
            base.Awake();

            myGameObject = gameObject;
        }

        protected override void Start()
        {
            base.Start();

            // TODO 추후 절전 UI 작업 필요
            myGameObject.SetActive(false);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelTitle.LocalKey = LocalizeKey._14011; // 자동 절전 모드

            for (int i = 0; i < toggles.Length; i++)
            {
                SleepModeTime sleepTime = i.ToEnum<SleepModeTime>();
                toggles[i].Text = LocalizeKey._14012.ToText() // {VALUE} 분
                    .Replace("{VALUE}", GetMinutes(sleepTime).ToString());
            }
        }

        protected override void OnChange(int index)
        {
            SleepModeTime sleepTime = index.ToEnum<SleepModeTime>();
            presenter.SetSleepModeTime(sleepTime);
        }

        protected override void Refresh()
        {
            SleepModeTime sleepTime = presenter.GetSleepModeTime();
            int index = (int)sleepTime;

            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].Value = index == i;
            }
        }

        private int GetMinutes(SleepModeTime sleepModeTime)
        {
            switch (sleepModeTime)
            {
                case SleepModeTime.OneMinute:
                    return 1; // 1분

                case SleepModeTime.FiveMinutes:
                    return 5; // 5분

                case SleepModeTime.TenMinutes:
                    return 10; // 10분

                default:
                    Debug.Log($"[올바르지 않은 {nameof(sleepModeTime)}] {nameof(sleepModeTime)} = {sleepModeTime}");
                    return 0;

            }
        }
    }
}