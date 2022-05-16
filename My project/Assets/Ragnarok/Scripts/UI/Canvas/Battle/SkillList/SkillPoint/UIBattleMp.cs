using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleMp : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIAniProgressBar mp;
        [SerializeField] UILabelValue labelMp;
        [SerializeField] UILabelHelper labelMax;

        BattleMpPresenter presenter;

        protected override void OnInit()
        {
            presenter = new BattleMpPresenter();

            presenter.OnChangeMP += UpdateMp;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.OnChangeMP -= UpdateMp;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMp.Title = LocalizeKey._2400.ToText(); // MP
            labelMax.Text = LocalizeKey._2401.ToText() // MAX:{VALUE}
                .Replace(ReplaceKey.VALUE, presenter.GetMaxMp());
        }

        public void Refresh()
        {
            int cur = presenter.GetCurMp();
            int max = presenter.GetMaxMp();
            mp.Set(cur, max);
            Refresh(cur);
        }

        private void UpdateMp(int cur, int max)
        {
            mp.Tween(cur, max);
            Refresh(cur);
        }

        private void Refresh(int cur)
        {
            labelMp.Value = cur.ToString();
        }
    }
}