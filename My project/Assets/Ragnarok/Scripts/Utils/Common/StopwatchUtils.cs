using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public class RemainTimeStopwatch
    {
        private ObscuredFloat serverTime;
        private float lastRealTime;
        private float runningTime;
        private bool isPause;

        private bool isFinished;

        public RemainTimeStopwatch()
        {
            Set(0);
        }

        public void Set(float serverTime)
        {
            this.serverTime = serverTime;
            lastRealTime = Time.realtimeSinceStartup;
            runningTime = 0f;
            isPause = false;
            isFinished = false;
        }

        public void Pause()
        {
            Process();
            isPause = true;
        }

        public void Resume()
        {
            isPause = false;
            Process();
        }

        /// <summary>
        /// 남은 시간
        /// </summary>
        public float ToRemainTime()
        {
            if (isFinished)
                return 0f;

            Process();

            float remainTime = serverTime - runningTime;
            if (remainTime <= 0f)
            {
                isFinished = true;
                return 0f;
            }

            return remainTime;
        }

        public string ToStringTime(string format = @"hh\:mm\:ss")
        {
            return ToRemainTime().ToStringTime(format);
        }

        public bool IsFinished()
        {
            ToRemainTime();
            return isFinished;
        }

        private void Process()
        {
            if (isPause)
            {
                // runningTime 이 가지 않는다.
            }
            else
            {
                runningTime += (Time.realtimeSinceStartup - lastRealTime) * 1000; // 진행 시간
            }

            lastRealTime = Time.realtimeSinceStartup;
        }
    }

    public class CumulativeTimeStopwatch
    {
        private ObscuredFloat serverTime;
        private float lastRealTime;
        private float runningTime;
        private bool isPause;

        public CumulativeTimeStopwatch()
        {
            Set(0);
        }

        public void Set(float serverTime)
        {
            this.serverTime = serverTime;
            lastRealTime = Time.realtimeSinceStartup;
            runningTime = 0f;
            isPause = false;
        }

        public void Pause()
        {
            Process();
            isPause = true;
        }

        public void Resume()
        {
            isPause = false;
            Process();
        }

        public float ToCumulativeTime()
        {
            Process();
            return serverTime + runningTime;
        }

        public string ToStringTime(string format = @"hh\:mm\:ss")
        {
            return ToCumulativeTime().ToStringTime(format);
        }

        public string ToStringOnlineTime()
        {
            return ToCumulativeTime().ToTimeSpan().ToStringOnlineTime();
        }

        private void Process()
        {
            if (isPause)
            {
                // runningTime 이 가지 않는다.
            }
            else
            {
                runningTime += (Time.realtimeSinceStartup - lastRealTime) * 1000; // 진행 시간
            }

            lastRealTime = Time.realtimeSinceStartup;
        }
    }
}