using System;

namespace Ragnarok
{
    /// <summary>
    /// 길드 출석체크 인원별 보상정보
    /// </summary>
    public class GuildAttendRewardInfo : IInfo
    {
        /// <summary>
        /// 필요 출석체크 인원
        /// </summary>
        public readonly int count;

        public readonly RewardData reward;

        bool IInfo.IsInvalidData => false;

        event Action IInfo.OnUpdateEvent
        {
            add { }
            remove { }
        }

        public GuildAttendRewardInfo(int count, int itemId)
        {
            this.count = count;
            reward = new RewardData(6, itemId, 1);
        }
    } 
}
