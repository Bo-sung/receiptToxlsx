using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public static class BattleTime
    {
        /// <summary>
        /// 일시정지 여부
        /// </summary>
        public static bool IsPause
        {
            get => isPause;
            set
            {
                if (value)
                {
                    TimePause();
                }
                else
                {
                    TimeResume();
                }
            }
        }

        public static float realtimeSinceStartup => GetRealtimeSinceStartup();

        private static bool isPause; // 일시정지 유무
        private static float pauseRunningTime; // 일시정지 지속시간

        public static event System.Action OnPause;
        public static event System.Action OnResume;

        /// <summary>
        /// 일시정지
        /// </summary>
        private static void TimePause()
        {
            if (isPause)
                return;

            isPause = true;

            SetTimeScale(0f);
            Timing.RunCoroutine(YieldCheckPauseTime());

            OnPause?.Invoke();
        }

        /// <summary>
        /// 일시정지 해제
        /// </summary>
        private static void TimeResume()
        {
            if (!isPause)
                return;

            SetTimeScale(1f);
            isPause = false;

            OnResume?.Invoke();
        }

        private static float GetRealtimeSinceStartup()
        {
            return Time.realtimeSinceStartup - pauseRunningTime;
        }

        private static void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = 0.02f * timeScale; // Unity 권장사항: Time.timeScale을 바꾸었을 때는 Time.fixedDeltaTime도 수정
        }

        private static IEnumerator<float> YieldCheckPauseTime()
        {
            float lastTime = Time.realtimeSinceStartup;
            while (isPause)
            {
                pauseRunningTime += Time.realtimeSinceStartup - lastTime;
                lastTime = Time.realtimeSinceStartup;
                yield return Timing.WaitForOneFrame;
            }
        }
    }
}