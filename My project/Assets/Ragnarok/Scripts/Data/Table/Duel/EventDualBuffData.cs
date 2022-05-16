using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="EventDualBuffDataManager"/>
    /// </summary>
    public class EventDualBuffData : IData, UIDuelBuffReward.IInput
    {
        public const int PERFECT_RANK_TYPE = 1; // 완전승리보상
        public const int NORMAL_RANK_TYPE = 2; // 일반순위보상

        [System.Flags]
        public enum BuffType
        {
            /// <summary>
            /// 일반경험치 버프
            /// </summary>
            BaseExp = 1 << 0,
            /// <summary>
            /// 직업경험치 버프
            /// </summary>
            JobExp = 1 << 1,
            /// <summary>
            /// 아이템 버프
            /// </summary>
            Item = 1 << 2,
            /// <summary>
            /// 제니 버프
            /// </summary>
            Zeny = 1 << 3,
        }

        public readonly ObscuredInt id;
        public readonly ObscuredInt rank_type; // RankType 참조
        public readonly ObscuredInt rank; // rank_type 이 일반순위보상(2) 경우에 순위 보상
        public readonly ObscuredInt day; // 버프 지속 시간
        public readonly ObscuredInt buff_type; // 버프 타입
        public readonly ObscuredInt buff_value; // 버프 적용 값 (100분율)

        public int Rank => rank;

        public EventDualBuffData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            rank_type = data[index++].AsInt32();
            rank = data[index++].AsInt32();
            day = data[index++].AsInt32();
            buff_type = data[index++].AsInt32();
            buff_value = data[index++].AsInt32();
        }

        public string GetTitle(bool isResult)
        {
            if (rank_type == PERFECT_RANK_TYPE)
            {
                if (isResult)
                    return LocalizeKey._47852.ToText(); // 완전 승리를 축하합니다!

                return LocalizeKey._47909.ToText(); // 완전 승리 보상
            }

            if (rank > 0)
            {
                if (isResult)
                    return LocalizeKey._47853.ToText().Replace(ReplaceKey.RANK, rank); // {RANK}위를 축하합니다!

                return LocalizeKey._47910.ToText().Replace(ReplaceKey.RANK, rank); // {RANK}위 보상
            }

            if (isResult)
                return LocalizeKey._47854.ToText(); // 참여해주셔서 감사합니다.

            return LocalizeKey._47911.ToText(); // 참여 보상
        }

        public string GetDescription()
        {
            return LocalizeKey._47855.ToText() // {DAYS}일간 {NAME} 획득량 [c][F66095]{VALUE}% 증가[-][/c]
                .Replace(ReplaceKey.DAYS, day)
                .Replace(ReplaceKey.NAME, GetBuffText())
                .Replace(ReplaceKey.VALUE, buff_value);
        }

        private string GetBuffText()
        {
            BuffType buffType = buff_type.ToEnum<BuffType>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (BuffType item in System.Enum.GetValues(typeof(BuffType)))
            {
                if (!buffType.HasFlag(item))
                    continue;

                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(GetBuffText(item));
            }

            return sb.ToString();
        }

        private string GetBuffText(BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.BaseExp:
                    return LocalizeKey._47856.ToText(); // 일반 경험치

                case BuffType.JobExp:
                    return LocalizeKey._47857.ToText(); // 직업 경험치

                case BuffType.Item:
                    return LocalizeKey._47858.ToText(); // 아이템

                case BuffType.Zeny:
                    return LocalizeKey._47859.ToText(); // 제니
            }

            return string.Empty;
        }
    }
}