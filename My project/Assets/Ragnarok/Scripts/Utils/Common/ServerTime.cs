using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public static class ServerTime
    {
        private static long serverTime;
        private static float savedTime;

        public static bool IsInitialize { get; private set; }

        public static void Initialize(long ticks)
        {
            IsInitialize = true;

            serverTime = ticks;
            savedTime = Time.realtimeSinceStartup;
        }

        public static System.DateTime Now => UtcNow.ToLocalTime();

        public static System.DateTime UtcNow
        {
            get
            {
                if (!IsInitialize)
                {
                    Debug.LogWarning("Initialize 필요");
                    return System.DateTime.UtcNow;
                }

                float progressTime = (Time.realtimeSinceStartup - savedTime); // 진행 시간 (초)
                return new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
                    .AddMilliseconds(serverTime)
                    .AddSeconds(progressTime);
            }
        }

        /// <summary>
        /// 서버 리셋 시간
        /// </summary>
        public static int ResetHour
        {
            get
            {
                if (int.TryParse(GamePotUnity.GamePot.getConfig("ResetHour"), out int hour))
                {
                    return new System.DateTime(1970, 1, 1, hour, 0, 0, System.DateTimeKind.Utc)
                        .ToLocalTime()
                        .Hour;
                }
                return 1;
            }
        }
    }

    public static class ObscuredServerTime
    {
        private static ObscuredLong serverTime;
        private static ObscuredFloat savedTime;

        public static bool IsInitialize { get; private set; }

        public static void Initialize(long ticks)
        {
            IsInitialize = true;

            serverTime = ticks;
            savedTime = Time.realtimeSinceStartup;
        }

        public static System.DateTime Now => UtcNow.ToLocalTime();

        public static System.DateTime UtcNow
        {
            get
            {
                if (!IsInitialize)
                {
                    Debug.LogWarning("Initialize 필요");
                    return System.DateTime.UtcNow;
                }

                float progressTime = (Time.realtimeSinceStartup - savedTime); // 진행 시간 (초)
                return new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
                    .AddMilliseconds(serverTime)
                    .AddSeconds(progressTime);
            }
        }
    }
}