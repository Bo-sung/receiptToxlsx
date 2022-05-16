using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public enum PassState
    {
        /// <summary>
        /// 구매 가능
        /// </summary>
        Avalable,
        /// <summary>
        /// 비활성화
        /// </summary>
        Disable,
        /// <summary>
        /// 이미 구매함.
        /// </summary>
        Purchased,
        /// <summary>
        /// 이미 활성화 됨
        /// </summary>
        OnActivate,
        /// <summary>
        /// 레벨 제한
        /// </summary>
        LevelLimit,
        /// <summary>
        /// 기간이 아님
        /// </summary>
        NotTime
    }
}
