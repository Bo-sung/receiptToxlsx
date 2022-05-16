using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CatCoinRewardInfo : DataInfo<RewardData>
    {
        public enum CatCoinCompleteType
        {
            /// <summary>
            /// 보상 대기 중
            /// </summary>
            StandByReward = 1,

            /// <summary>
            /// 진행중
            /// </summary>
            InProgress = 2,

            /// <summary>
            /// 보상 완료
            /// </summary>
            ReceivedReward = 3,
        }

        /// <summary>
        /// 누적 시간
        /// </summary>
        private CumulativeTime cumulativeTime;

        /// <summary>
        /// 보상 여부
        /// </summary>
        private bool isReceived;

        /// <summary>
        /// 보상 수령 시간(초)
        /// </summary>
        public int RewardTime { get; }

        public string TotalMinute => TimeSpan.FromSeconds(RewardTime).TotalMinutes.ToString();

        /// <summary>
        /// 보상정보
        /// </summary>
        public RewardData reward => data;

        float remainTime => (RewardTime * 1000f) - cumulativeTime.ToCumulativeTime();

        bool IsRemainTime => remainTime > 0f;

        bool IsStandByReward()
        {
            return !IsRemainTime;
        }

        /// <summary>
        /// 보상 상태 타입
        /// </summary>
        public CatCoinCompleteType CompleteType => GetCompleteType();

        private CatCoinCompleteType GetCompleteType()
        {
            if (isReceived)
                return CatCoinCompleteType.ReceivedReward;

            return IsRemainTime ? CatCoinCompleteType.InProgress : CatCoinCompleteType.StandByReward;
        }


        public CatCoinRewardInfo(int time)
        {
            RewardTime = time;
        }

        public void SetCumulativeTime(CumulativeTime time)
        {
            cumulativeTime = time;
            InvokeEvent();
            CheckReaminTime().AsIEnumerator();
        }

        public void SetReceived(bool isReceived)
        {
            this.isReceived = isReceived;
            InvokeEvent();
        }

        async Task CheckReaminTime()
        {
            if (isReceived)
                return;

            await new WaitUntil(IsStandByReward);
            InvokeEvent();
        }
    }
}
