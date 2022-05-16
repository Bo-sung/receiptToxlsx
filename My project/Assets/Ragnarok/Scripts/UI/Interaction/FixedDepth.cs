using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// Depth 자동 조절에 영향을 받지 않기 위해 만듦
    /// (특별한 기능이 있는 것은 아님)
    /// (UIMenuItems 의 AutoAdjestWidgetDepth)
    /// </summary>
    public class FixedDepth : MonoBehaviour
    {
        public int depth;
    }
}