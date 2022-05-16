using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICenterOnClick"/>
    /// 참조: http://www.tasharen.com/forum/index.php?topic=13086.0
    /// </summary>
    public class UILimitCenter : MonoBehaviour
    {
        UIPanel mPanel;
        UIScrollView mScrollView;

        public void Execute()
        {
            if (!FindPanel())
                return;

            if (!IsShouldMove())
                return;

            Transform panelTrans = mPanel.cachedTransform;

            Vector3[] corners = mPanel.worldCorners;
            Vector3 panelCenter = (corners[2] + corners[0]) * 0.5f;
            Vector3 cp = panelTrans.InverseTransformPoint(transform.position);
            Vector3 cc = panelTrans.InverseTransformPoint(panelCenter);
            Vector3 localOffset = cp - cc;

            Vector3 move = panelTrans.localPosition - panelTrans.localPosition - localOffset;
            mScrollView.MoveRelative(move);

            Vector3 ofs = mPanel.CalculateConstrainOffset(mScrollView.bounds.min, mScrollView.bounds.max);
            Vector3 targetPos = ofs + panelTrans.localPosition;

            mScrollView.MoveRelative(-move);

            SpringPanel.Begin(mPanel.cachedGameObject, targetPos, 6f);
        }

        private bool FindPanel()
        {
            if (mPanel)
                return true;

            UIPanel panel = NGUITools.FindInParents<UIPanel>(gameObject);
            if (panel == null || panel.clipping == UIDrawCall.Clipping.None)
                return false;

            mPanel = panel;
            mScrollView = mPanel.GetComponent<UIScrollView>();
            return true;
        }

        private bool IsShouldMove()
        {
            if (!mScrollView.disableDragIfFits)
                return true;

            Vector4 clip = mPanel.finalClipRegion;
            Bounds b = mScrollView.bounds;

            float hx = (clip.z == 0f) ? Screen.width : clip.z * 0.5f;
            float hy = (clip.w == 0f) ? Screen.height : clip.w * 0.5f;

            if (mScrollView.canMoveHorizontally)
            {
                if (b.min.x + 0.001f < clip.x - hx) return true;
                if (b.max.x - 0.001f > clip.x + hx) return true;
            }

            if (mScrollView.canMoveVertically)
            {
                if (b.min.y + 0.001f < clip.y - hy) return true;
                if (b.max.y - 0.001f > clip.y + hy) return true;
            }
            return false;
        }
    }
}