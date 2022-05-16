using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class UICenterOnSelectSlot : MonoBehaviour
    {
        static UIScrollView sv;
        static UIPanel panel;
        static UIGrid grid;

        /// <summary>
        /// 특정 슬롯을 스크롤 뷰의 센터로 변경.. Grid 필요함.. ㅠ
        /// </summary>
        /// <param name="slotObject"></param>
        /// <param name="instant"></param>
        public static void InitCenter(GameObject slotObject, bool instant = true)
        {
            grid = NGUITools.FindInParents<UIGrid>(slotObject);
            if (grid == null) return;
            sv = NGUITools.FindInParents<UIScrollView>(slotObject);
            if (sv == null) return;
            panel = sv.GetComponent<UIPanel>();

            Vector3 offset = -panel.cachedTransform.InverseTransformPoint(slotObject.transform.position);            
            if (!sv.canMoveHorizontally)
            {
                offset = RestrictPos(offset, false, true); // 경계 체크
                offset.x = panel.cachedTransform.localPosition.x;
            }
            if (!sv.canMoveVertically)
            {
                offset = RestrictPos(offset, true, false); // 경계 체크
                offset.y = panel.cachedTransform.localPosition.y;
            }

            if (instant) // OnShow의 경우 딜레이..
                DelaySpringPanel(offset, instant);
            else
                SpringPanel.Begin(panel.cachedGameObject, offset, 6f);
        }

        static async Task DelaySpringPanel(Vector3 offset, bool instant)
        {
            await Awaiters.Seconds(0.1f);

            SpringPanel.Begin(panel.cachedGameObject, offset, 100f);
        }

        static Vector3 RestrictPos(Vector3 pos, bool isHorizon, bool isVertical)
        {
            // min 체크
            Vector4 clipRegion = -panel.baseClipRegion;
            if(isHorizon)
            {
                var offsetMinX = (clipRegion.z + grid.cellWidth) * 0.5f;
                if (pos.x > offsetMinX) pos.x = offsetMinX;
            }
            if(isVertical)
            {
                var offsetMinY = (clipRegion.w + grid.cellHeight) * 0.5f;
                if (pos.y > offsetMinY) pos.y = offsetMinY;
            }

            // max 체크
            Vector3 boundsSize = -sv.bounds.size;
            if(isHorizon)
            {
                var offsetMaxX = boundsSize.x - (clipRegion.z - grid.cellWidth) * 0.5f;
                if (pos.x < offsetMaxX) pos.x = offsetMaxX;
            }
            if(isVertical)
            {
                var offsetMaxY = boundsSize.y - (clipRegion.w - grid.cellHeight) * 0.5f;
                if (pos.y < offsetMaxY) pos.y = offsetMaxY;
            }

            return pos;
        }
    }
}