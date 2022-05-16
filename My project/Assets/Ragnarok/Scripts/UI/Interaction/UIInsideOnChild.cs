using UnityEngine;

namespace Ragnarok
{
    public class UIInsideOnChild : MonoBehaviour
    {
        Transform myTransform;
        Vector2 localOffset;

        [SerializeField, Tooltip("Anchor 또는 Center 가 정확하게 (0, 0)이 아닐 경우에 대한 처리")]
        Vector2 maxOffset;

        [SerializeField, Tooltip("Anchor 또는 Center 가 정확하게 (0, 0)이 아닐 경우에 대한 처리")]
        Vector2 minOffset;

        void Awake()
        {
            myTransform = transform;
        }

        public void Run()
        {
            UIPanel panel = NGUITools.FindInParents<UIPanel>(gameObject);

            if (panel == null)
                return;

            Bounds absolute = NGUIMath.CalculateAbsoluteWidgetBounds(myTransform);

            // 위 아래가 동시에 안 보일 경우가 있을 수 있음
            // 정석대로라면 ScrollView 를 찾아 정렬방식으로 처리하는 것이 맞지만
            // 아무래도 Left-Top 정렬이 많기 때문에 무조건 min 부터 처리하기로 한다
            if (!panel.IsVisible(absolute.min))
            {
                Vector2 offset = -panel.cachedTransform.InverseTransformPoint(absolute.center);
                Vector2 viewExtents = panel.GetViewSize() * 0.5f;
                Vector2 clipSoftness = panel.clipSoftness;
                Vector2 extents = NGUIMath.CalculateRelativeWidgetBounds(myTransform).extents;
                localOffset = offset - viewExtents + clipSoftness + extents;
                localOffset += minOffset;
            }
            else if (!panel.IsVisible(absolute.max))
            {
                Vector2 offset = -panel.cachedTransform.InverseTransformPoint(absolute.center);
                Vector2 viewExtents = panel.GetViewSize() * 0.5f;
                Vector2 clipSoftness = panel.clipSoftness;
                Vector2 extents = NGUIMath.CalculateRelativeWidgetBounds(myTransform).extents;
                localOffset = offset + viewExtents - clipSoftness - extents;
                localOffset += maxOffset;
            }
            else
            {
                // 가려지지 않고 잘 보이고 있음
                return;
            }

            UIScrollView sv = panel.GetComponent<UIScrollView>();
            if (sv != null)
            {
                if (!sv.canMoveHorizontally)
                    localOffset.x = panel.cachedTransform.localPosition.x;

                if (!sv.canMoveVertically)
                    localOffset.y = panel.cachedTransform.localPosition.y;

                SpringPanel.Begin(panel.cachedGameObject, localOffset, 8f);
            }
        }
    }
}