using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattlePause : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIButton btnPause;

        public event System.Action OnPause;

        protected override void OnInit()
        {
            EventDelegate.Add(btnPause.onClick, Pause);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnPause.onClick, Pause);
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

        private void Pause()
        {
            OnPause?.Invoke();
        }
    }
}