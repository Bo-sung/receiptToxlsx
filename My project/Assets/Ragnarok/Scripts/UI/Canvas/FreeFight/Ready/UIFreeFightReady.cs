using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIFreeFightReady : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] FreeFightReadyBrowseView browseView;
        [SerializeField] FreeFightReadyEntryView entryView;

        public event System.Action<Vector2> OnMove;
        public event System.Action OnEnter;

        protected override void OnInit()
        {
            browseView.OnMove += OnMoveBrowser;
            entryView.OnSelectEnter += OnSelectEnter;
            entryView.OnFinished += HideUI;
        }

        protected override void OnClose()
        {
            browseView.OnMove -= OnMoveBrowser;
            entryView.OnSelectEnter -= OnSelectEnter;
            entryView.OnFinished -= HideUI;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnMoveBrowser(Vector2 offset)
        {
            OnMove?.Invoke(offset);
        }

        void OnSelectEnter()
        {
            OnEnter?.Invoke();
        }

        private void HideUI()
        {
            Hide();
        }

        public void SetAutoEnter(float remainTicks)
        {
            entryView.Run(remainTicks);
        }
    }
}