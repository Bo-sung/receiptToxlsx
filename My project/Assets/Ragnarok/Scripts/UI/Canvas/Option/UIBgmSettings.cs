namespace Ragnarok
{
    public class UIBgmSettings : UIOptionSoundSettings
    {      
        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelTitle.LocalKey = LocalizeKey._14003; // 배경음
        }

        protected override void OnChangeMute(bool isMute)
        {          
            base.OnChangeMute(isMute);
            presenter.SetBgmMute(isMute);
        }

        protected override void OnChangeVolume(float volume)
        {
            if (IsMute)
                return;

            base.OnChangeVolume(volume);

            if (Volume == 0)
                return;

            presenter.SetBgmVolume(volume);
        }

        protected override void Refresh()
        {
            bool isMute = presenter.GetBgmMute();
            float volume = presenter.GetBgmVolume();

            if (IsMute)
            {
                volume = 0;
            }           
            else if(Volume.HasValue && Volume.Value == 0)
            {
                volume = Volume.Value;
                isMute = true;
            }           
            Volume = null;

            SetSoundSetting(isMute, volume);
        }
    }
}