using UnityEngine;

namespace Ragnarok
{
    public sealed class GameSoundSettings : Singleton<GameSoundSettings>
    {
        public interface ISoundVolumeSettings
        {
            event System.Action OnChange;

            bool IsMute { get; set; }
            float Volume { get; set; }
        }

        public readonly ISoundVolumeSettings bgm;
        public readonly ISoundVolumeSettings sfx;

        public GameSoundSettings()
        {
            bgm = new SoundVolumeSettings("SoundVolumeSettings.Bgm.Mute", "SoundVolumeSettings.Bgm.Volume");
            sfx = new SoundVolumeSettings("SoundVolumeSettings.Sfx.Mute", "SoundVolumeSettings.Sfx.Volume");
        }

        protected override void OnTitle()
        {
        }

        private class SoundVolumeSettings : ISoundVolumeSettings
        {
            private const int FALSE = 0;
            private const int TRUE = 1;

            private readonly string muteKey;
            private readonly string volumeKey;

            private System.Action onChange;

            public event System.Action OnChange
            {
                add { onChange += value; }
                remove { onChange -= value; }
            }

            public bool IsMute
            {
                get { return GetMute(); }
                set
                {
                    if (GetMute() == value)
                        return;

                    SetMute(value);
                }
            }

            public float Volume
            {
                get { return GetVolume(); }
                set
                {
                    if (GetVolume() == value)
                        return;

                    SetVolume(value);
                }
            }

            public SoundVolumeSettings(string muteKey, string volumeKey)
            {
                this.muteKey = muteKey;
                this.volumeKey = volumeKey;
            }

            private bool GetMute()
            {
                int muteValue = PlayerPrefs.GetInt(muteKey, FALSE);
                return muteValue == TRUE;
            }

            private void SetMute(bool isMute)
            {
                PlayerPrefs.SetInt(muteKey, isMute ? TRUE : FALSE);
                InvokeChangeEvent();
            }

            private float GetVolume()
            {
                return PlayerPrefs.GetFloat(volumeKey, 0.5f);
            }

            private void SetVolume(float volume)
            {
                PlayerPrefs.SetFloat(volumeKey, volume);
                InvokeChangeEvent();
            }

            private void InvokeChangeEvent()
            {
                onChange?.Invoke();
            }
        }
    }
}