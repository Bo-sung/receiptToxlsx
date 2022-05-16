using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPowerSaving : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        public override int layer => Layer.UI_Empty;

        [SerializeField] UITextureHelper logo;
        [SerializeField] UITimePressButton btnPowerSaving;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelTime;
        [SerializeField] UILabelHelper labelBattery;
        [SerializeField] UIProgressBar progressBar;

        PowerSavingPresenter presenter;

        protected override void OnInit()
        {
            presenter = new PowerSavingPresenter();

            EventDelegate.Add(btnPowerSaving.onClick, CloseUI);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnPowerSaving.onClick, CloseUI);

            presenter.RemoveEvent();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            presenter.Dispose();
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.Initialize();

            Refresh();
        }

        protected override void OnHide()
        {
            presenter.Dispose();
        }

        protected override void OnLocalize()
        {
            labelDescription.LocalKey = LocalizeKey._10701; // 잠금을 해제하려면\n절전모드 아이콘을 2초간 누르세요.
        }

        private void Refresh()
        {
            logo.Set(presenter.GetLogo());
            Timing.RunCoroutineSingleton(YieldRefresh().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void CloseUI()
        {
            UI.Close<UIPowerSaving>();
        }

        IEnumerator<float> YieldRefresh()
        {
            float batteryLevel;
            while (true)
            {
                labelTime.Text = presenter.GetNowTime();
                batteryLevel = MathUtils.Abs(presenter.GetBatteryLevel());
                labelBattery.Text = MathUtils.GetPercentText(batteryLevel);
                progressBar.value = batteryLevel;

                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}