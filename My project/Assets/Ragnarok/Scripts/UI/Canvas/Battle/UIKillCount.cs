using UnityEngine;

namespace Ragnarok
{
    public sealed class UIKillCount : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelCount;
        [SerializeField] int maxKillCount = 999;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
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

        public void SetKillCount(int killCount)
        {
            killCount = Mathf.Clamp(killCount, 0, maxKillCount);
            labelCount.Text = killCount.ToString();
        }
    }
}