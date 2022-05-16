namespace Ragnarok
{
    public class UISfxSettings : UIOptionSoundSettings
    {
        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelTitle.LocalKey = LocalizeKey._14004; // 효과음
        }

        protected override void OnChangeMute(bool isMute)
        {
            base.OnChangeMute(isMute);
            presenter.SetSfxMute(isMute);
        }

        protected override void OnChangeVolume(float volume)
        {
            if (IsMute)
                return;

            base.OnChangeVolume(volume);

            if (Volume == 0)
                return;

            presenter.SetSfxVolume(volume);
        }

        protected override void Refresh()
        {
            bool isMute = presenter.GetSfxMute();
            float volume = presenter.GetSfxVolume();

            if (IsMute)
            {
                volume = 0;
            }
            else if (Volume.HasValue && Volume.Value == 0)
            {
                volume = Volume.Value;
                isMute = true;
            }
            Volume = null;

            SetSoundSetting(isMute, volume);
        }
    }
}