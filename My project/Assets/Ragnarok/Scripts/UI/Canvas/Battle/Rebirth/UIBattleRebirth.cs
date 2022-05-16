using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleRebirth : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper labelSingleDesc;
        [SerializeField] UICostButtonHelper btnRebirth;
        [SerializeField] UISprite icon;

        public event System.Action OnConfirm;
        public event System.Action OnAutoConfirm;

        private int titleLocalKey = LocalizeKey._38200; // 캐릭터가 사망했습니다.
        private int descLocalKey = LocalizeKey._38201; // {TIME}초 후 자동으로 부활합니다.
        private int btnLocalKey = LocalizeKey._38202; // 바로 부활

        private int needZeny;

        BattleRebirthPresenter presenter;

        protected override void OnInit()
        {
            presenter = new BattleRebirthPresenter();

            EventDelegate.Add(btnRebirth.OnClick, OnClickedBtnRebirth);

            presenter.OnUpdateZeny += Refresh;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnRebirth.OnClick, OnClickedBtnRebirth);

            presenter.OnUpdateZeny -= Refresh;
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
            UpdateText();
        }

        void OnClickedBtnRebirth()
        {
            OnConfirm?.Invoke();
        }

        /// <summary>
        /// Title, Desc, Icon
        /// </summary>
        public void Initialize(int titleLocalKey, int descLocalKey, int btnLocalKey)
        {
            this.titleLocalKey = titleLocalKey;
            this.descLocalKey = descLocalKey;
            this.btnLocalKey = btnLocalKey;

            labelTitle.SetActive(true);
            labelDesc.SetActive(true);
            labelSingleDesc.SetActive(false);
            NGUITools.SetActive(icon.cachedGameObject, true);

            UpdateText();
        }

        /// <summary>
        /// SingleDesc (only)
        /// </summary>
        public void Initialize(int descLocalKey, int btnLocalKey)
        {
            this.descLocalKey = descLocalKey;
            this.btnLocalKey = btnLocalKey;

            labelTitle.SetActive(false);
            labelDesc.SetActive(false);
            labelSingleDesc.SetActive(true);
            NGUITools.SetActive(icon.cachedGameObject, false);

            UpdateText();
        }

        public void Show(int needZeny, int duration)
        {
            this.needZeny = needZeny;

            Show();

            btnRebirth.SetCostCount(this.needZeny);
            Timing.RunCoroutineSingleton(YieldAutoConfirm(duration).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldAutoConfirm(int duration)
        {
            do
            {
                string message = descLocalKey.ToText().Replace(ReplaceKey.TIME, duration);
                labelDesc.Text = message;
                labelSingleDesc.Text = message;
                yield return Timing.WaitForSeconds(1f);
            } while (--duration > 0);

            Hide();
            OnAutoConfirm?.Invoke();
        }

        private void UpdateText()
        {
            labelTitle.LocalKey = titleLocalKey;
            btnRebirth.LocalKey = btnLocalKey;
        }

        private void Refresh()
        {
            bool isEnable = presenter.CanUseZeny(needZeny);
            btnRebirth.IsEnabled = isEnable;
            btnRebirth.SetCostColor(isEnable);
        }
    }
}