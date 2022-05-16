using UnityEngine;
using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ProgressAction : MonoBehaviour
    {
        public enum TimeType
        {
            Time,
            RealTime,
        }

        public System.Action onStart;
        public System.Action<float> onProgress;
        public System.Action onEnd;

        private bool isPause;

        protected virtual void OnDestroy()
        {
            Stop();
        }

        public void Play(float duration, TimeType timeType = TimeType.RealTime)
        {
            Timing.RunCoroutine(TimeProcess(duration, timeType), gameObject);
        }

        public void Stop()
        {
            Timing.KillCoroutines(gameObject);
        }

        public void Pause()
        {
            isPause = true;
        }

        public void Resume()
        {
            isPause = false;
        }

        IEnumerator<float> TimeProcess(float duration, TimeType timeType)
        {
            onStart?.Invoke();
            onProgress?.Invoke(0f);

            float lastTime = GetTime(timeType);
            float runningTime = 0f;
            float percentage = 0f;
            float timeRate = 1f / duration;

            while (percentage < 1f)
            {
                if (!isPause)
                {
                    runningTime = GetTime(timeType) - lastTime;
                    percentage = runningTime * timeRate;
                    onProgress?.Invoke(percentage);
                }

                yield return Timing.WaitForOneFrame;
            }

            onProgress?.Invoke(1f);
            onEnd?.Invoke();
        }

        private float GetTime(TimeType timeType)
        {
            switch (timeType)
            {
                default:
                case TimeType.Time:
                    return Time.time;

                case TimeType.RealTime:
                    return Time.realtimeSinceStartup;
            }
        }
    }
}