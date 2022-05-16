using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIOptionSystem : UISubCanvas<OptionPresenter>
    {
        [SerializeField] UIBgmSettings bgm;
        [SerializeField] UISfxSettings sfx;
        [SerializeField] UIGraphicSettings graphic;
        [SerializeField] UIAutoSleepModeSettings sleepMode;
        [SerializeField] UIPlayerTrackingSettings playerTracking;
        [SerializeField] AlarmSettingView alarmSettingView;
        [SerializeField] UIToggleHelper cameraShake;

        public override void Initialize(OptionPresenter presenter)
        {
            base.Initialize(presenter);

            bgm.Initialize(presenter);
            sfx.Initialize(presenter);
            graphic.Initialize(presenter);
            playerTracking.Initialize(presenter);
            sleepMode.Initialize(presenter);

            EventDelegate.Add(cameraShake.OnChange, OnChangedCameraShake);
        }

        public override void Close()
        {
            base.Close();

            EventDelegate.Remove(cameraShake.OnChange, OnChangedCameraShake);
        }

        protected override void OnInit()
        {
            alarmSettingView.OnSelectPush += OnSelectPush;
            alarmSettingView.OnSelectNightPush += OnSelectNightPush;
            alarmSettingView.OnSelectSharePush += OnSelectSharePush;
        }

        protected override void OnClose()
        {
            alarmSettingView.OnSelectPush -= OnSelectPush;
            alarmSettingView.OnSelectNightPush -= OnSelectNightPush;
            alarmSettingView.OnSelectSharePush -= OnSelectSharePush;
        }

        protected override void OnShow()
        {
            alarmSettingView.Set(presenter.IsPush(), presenter.IsNightPush(), presenter.IsSharePush());
            cameraShake.Set(LocalValue.IsCameraShake, notify: false);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            cameraShake.LocalKey = LocalizeKey._14056; // 카메라 흔들림
        }

        void OnSelectPush()
        {
            presenter.TogglePush();
        }

        void OnSelectNightPush()
        {
            presenter.ToggleNightPush();
        }

        void OnSelectSharePush()
        {
            presenter.ToggleSharePush();
        }

        void OnChangedCameraShake()
        {
            if (!cameraShake.IsCurrentToggle())
                return;

            LocalValue.IsCameraShake = cameraShake.Value;
        }
    }
}