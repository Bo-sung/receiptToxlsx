using UnityEngine;

namespace Ragnarok.View
{
    public class FreeFightReadyBrowseView : UIView
    {
        [SerializeField] UIScrollView scrollView;
        [SerializeField] UIWidget bound;

        public event System.Action<Vector2> OnMove;

        protected override void Start()
        {
            base.Start();

            scrollView.panel.onClipMove += OnMovePanel;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            scrollView.panel.onClipMove -= OnMovePanel;
        }

        protected override void OnLocalize()
        {
        }

        public void SetBounds(int width, int height)
        {
            bound.width = width;
            bound.height = height;
        }

        void OnMovePanel(UIPanel panel)
        {
            OnMove?.Invoke(panel.clipOffset);
        }
    }
}