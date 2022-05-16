using UnityEngine;

namespace Ragnarok
{
    public class UIWarpCenterOnClick : MonoBehaviour, IInspectorFinder
    {
        private Transform myTransform;
        private Vector2 localOffset;
        [SerializeField] private UIScrollView sv;
        [SerializeField] private bool ignoreClick;

        private void Awake()
        {
            myTransform = transform;
        }

        public async void Run()
        {
            if (sv == null)
                return;

            await Awaiters.NextFrame;

            Vector2 extents = sv.bounds.extents;
            var viewExtents = sv.panel.GetViewSize() * 0.5f;
            var size = extents - viewExtents;
            localOffset = sv.panel.cachedTransform.InverseTransformPoint(myTransform.position);
            localOffset = new Vector2(Mathf.Clamp(localOffset.x, -size.x, size.x), Mathf.Clamp(-localOffset.y, -size.y, size.y));
            var x = (localOffset.x + size.x) / (size.x * 2f);
            var y = (localOffset.y + size.y) / (size.y * 2f);
            sv.SetDragAmount(x, y, false);
        }

        void OnClick()
        {
            if (!ignoreClick)
                Run();
        }

        bool IInspectorFinder.Find()
        {
            var panel = NGUITools.FindInParents<UIPanel>(gameObject);

            if (panel == null)
                return false;

            sv = panel.GetComponent<UIScrollView>();
            return true;
        }
    }
}
