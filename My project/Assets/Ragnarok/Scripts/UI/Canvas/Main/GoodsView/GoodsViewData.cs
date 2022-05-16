using System;
using UnityEngine;
using static Ragnarok.GoodsViewData;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="GoodsViewSlot"/>
    /// 제니, 경험치 뷰 슬롯 데이터
    /// </summary>
    public class GoodsViewData : IInfo
    {
        public enum GoodsViewDataType
        {
            NONE = 0,

            /// <summary>
            /// 제니
            /// </summary>
            Zeny,

            /// <summary>
            /// 경험치
            /// </summary>
            LevelExp,

            /// <summary>
            /// 직업 경험치
            /// </summary>
            JobExp,
        }

        public bool IsInvalidData => default;
        public event Action OnUpdateEvent;

        /// <summary>
        /// Zeny
        /// LevelExp
        /// JobExp  
        /// ...
        /// </summary>
        public GoodsViewDataType type;
        public long value;

        public GoodsViewData(GoodsViewDataType type, long value)
        {
            this.type = type;
            this.value = value;
        }
    }

    public static class GoodsViewDataTypeExtension
    {
        public static string GetIconName(this GoodsViewDataType type)
        {
            string iconName = string.Empty;
            switch (type)
            {
                case GoodsViewDataType.NONE:
                    break;
                case GoodsViewDataType.Zeny:
                    iconName = "Zeny";
                    break;
                case GoodsViewDataType.LevelExp:
                    iconName = "BaseExp";
                    break;
                case GoodsViewDataType.JobExp:
                    iconName = "JobExp";
                    break;
                default:
                    break;
            }
            return iconName;
        }
    }
}