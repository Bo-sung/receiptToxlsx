using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// Time.timeScale에 영향이 없는 Real 남은 시간
    /// </summary>
    public struct ObscuredRemainTime
    {
        private readonly ObscuredFloat serverRemainTime;
        private readonly ObscuredFloat savedTime;

        private ObscuredRemainTime(float serverRemainTime)
        {
            this.serverRemainTime = serverRemainTime;
            savedTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 남은 시간 (밀리초)
        /// </summary>
        public float ToRemainTime()
        {
            float progressTime = (Time.realtimeSinceStartup - savedTime) * 1000; // 진행 시간
            return Mathf.Max(0, serverRemainTime - progressTime);
        }

        public string ToStringTime(string format = @"hh\:mm\:ss")
        {
            return ToRemainTime().ToStringTime(format);
        }

        public static implicit operator ObscuredRemainTime(float serverlastTime) => new ObscuredRemainTime(serverlastTime);
        public static implicit operator float(ObscuredRemainTime serverlastTime) => serverlastTime.ToRemainTime();
    }

    /// <summary>
    /// Time.timeScale에 영향이 없는 Real 남은 시간
    /// </summary>
    public struct RemainTime
    {
        private readonly float serverRemainTime;
        private readonly float savedTime;

        private RemainTime(float serverRemainTime)
        {
            this.serverRemainTime = serverRemainTime;
            savedTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 남은 시간 (밀리초)
        /// </summary>
        public float ToRemainTime()
        {
            float progressTime = (Time.realtimeSinceStartup - savedTime) * 1000; // 진행 시간
            return Mathf.Max(0, serverRemainTime - progressTime);
        }        

        public string ToStringTime(string format = @"hh\:mm\:ss")
        {
            return ToRemainTime().ToStringTime(format);
        }

        public static implicit operator RemainTime(float serverlastTime) => new RemainTime(serverlastTime);
        public static implicit operator float(RemainTime serverlastTime) => serverlastTime.ToRemainTime();
    }

    /// <summary>
    /// Time.timeScale에 영향을 받는 남은 시간
    /// </summary>
    public struct RelativeRemainTime
    {
        private readonly float endTime; // 종료 시간
        private bool hasRemainTime;

        private RelativeRemainTime(float time)
        {
            endTime = Time.time + time;
            hasRemainTime = true;
        }

        /// <summary>
        /// 남은 시간
        /// </summary>
        public float GetRemainTime()
        {
            if (hasRemainTime)
            {
                float remainTime = endTime - Time.time;

                // 남은 시간 존재
                if (remainTime > 0f)
                    return remainTime;

                hasRemainTime = false;
            }

            return 0f;
        }

        /// <summary>
        /// 남은 시간 텍스트
        /// </summary>
        public string GetRemainTimeText()
        {
            RemainTime remainTime = GetRemainTime() * 1000;
            return remainTime.ToStringTime(@"hh\:mm");
        }

        public static implicit operator RelativeRemainTime(float endTime) => new RelativeRemainTime(endTime);
        public static implicit operator float(RelativeRemainTime time) => time.GetRemainTime();
    }

    public struct LoopingRelativeRemainTime
    {
        private float endTime;
        private float loopTime;

        public LoopingRelativeRemainTime(float remainTime, float loopTime)
        {
            endTime = remainTime + Time.time;
            this.loopTime = loopTime;
        }

        public float GetRemainTime()
        {
            float time = endTime;
            while (time < Time.time)
                time += loopTime;
            endTime = time;
            return time - Time.time;
        }

        public static implicit operator float(LoopingRelativeRemainTime time) => time.GetRemainTime();
    }

    public struct CumulativeTime
    {
        private readonly float serverCumulativeTime;
        private readonly float savedTime;

        private CumulativeTime(int serverCumulativeTime)
        {
            this.serverCumulativeTime = serverCumulativeTime * 1000f;
            savedTime = Time.realtimeSinceStartup;
        }

        private CumulativeTime(long serverCumulativeTime)
        {
            this.serverCumulativeTime = serverCumulativeTime;
            savedTime = Time.realtimeSinceStartup;
        }

        public float ToCumulativeTime()
        {
            float progressTime = (Time.realtimeSinceStartup - savedTime) * 1000; // 진행 시간
            return serverCumulativeTime + progressTime;
        }

        public string ToStringTime(string format = @"hh\:mm\:ss")
        {
            return ToCumulativeTime().ToStringTime(format);
        }

        public string ToStringOnlineTime()
        {
            return ToCumulativeTime().ToTimeSpan().ToStringOnlineTime();
        }

        public static implicit operator CumulativeTime(int serverlastTime) => new CumulativeTime(serverlastTime);
        public static implicit operator CumulativeTime(long serverlastTime) => new CumulativeTime(serverlastTime);
        public static implicit operator float(CumulativeTime serverlastTime) => serverlastTime.ToCumulativeTime();
    }

    /// <summary>
    /// Time.timeScale에 영향이 없는 Real 남은 시간 (단, TimePause 체크 가능, Pause 있는 전투에서 사용)
    /// </summary>
    public struct BattleRemainTime
    {
        private readonly float serverRemainTime;
        private readonly float lastRealTime;

        private BattleRemainTime(float serverRemainTime)
        {
            this.serverRemainTime = serverRemainTime;
            lastRealTime = BattleTime.realtimeSinceStartup;
        }

        /// <summary>
        /// 남은 시간 (밀리초)
        /// </summary>
        public float ToRemainTime()
        {
            float progressTime = (BattleTime.realtimeSinceStartup - lastRealTime) * 1000; // 진행 시간
            return Mathf.Max(0, serverRemainTime - progressTime);
        }

        public string ToStringTime(string format = @"hh\:mm\:ss")
        {
            return ToRemainTime().ToStringTime(format);
        }

        public static implicit operator BattleRemainTime(float serverlastTime) => new BattleRemainTime(serverlastTime);
        public static implicit operator float(BattleRemainTime serverlastTime) => serverlastTime.ToRemainTime();
    }
}