using UnityEngine;

namespace Ragnarok
{
    public sealed class SoundManager : GameObjectSingleton<SoundManager>
    {
        GameSoundSettings soundSettings;
        IBgmContainer bgmContainer;
        ISfxContainer sfxContainer;
        IUiSfxContainer uiSfxContainer;

        private AudioSource bgmAudioSource, sfxAudioSource;

        protected override void Awake()
        {
            base.Awake();

            soundSettings = GameSoundSettings.Instance;

            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.loop = true;

            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;

            AddEvent();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RemoveEvent();
        }

        void Start()
        {
            bgmContainer = AssetManager.Instance;
            sfxContainer = AssetManager.Instance;
            uiSfxContainer = AssetManager.Instance;

            ApplyBgmSettings();
            ApplySfxSettings();
        }

        protected override void OnTitle()
        {
            PlayBgm("Title");
        }

        public void PlayBgm(string bgm)
        {
            if (string.IsNullOrEmpty(bgm))
                return;

            // 같은 곡의 경우
            if (bgmAudioSource.clip != null && bgmAudioSource.clip.name.Equals(bgm))
                return;

            bgmAudioSource.Stop(); // Stop

            AudioClip clip = bgmContainer.Get(bgm);
            if (clip == null)
            {
                Debug.LogError($"해당 bgm을 찾을 수 없습니다: {nameof(bgm)} = {bgm}");
                return;
            }

            bgmAudioSource.clip = clip; // Add AudioClip
            bgmAudioSource.Play(); // Play
        }

        public void PlaySfx(string sfx, float duration = 0f)
        {
            AudioClip clip = sfxContainer.Get(sfx);
            if (clip == null)
            {
                Debug.LogError($"해당 sfx을 찾을 수 없습니다: {nameof(sfx)} = {sfx}");
                return;
            }

            sfxAudioSource.PlayOneShot(clip); // Play
        }

        public void PlayButtonSfx(Sfx.Button button)
        {
            PlayUISfx(button.ToString(), 0f);
        }

        public void PlayUISfx(Sfx.UI ui)
        {
            PlayUISfx(ui.ToString(), 0f);
        }

        public void PlayLevelUpSfx()
        {
            PlaySfx("battle_level_up");
        }

        private void PlayUISfx(string sfx, float duration = 0f)
        {
            AudioClip clip = uiSfxContainer.Get(sfx);
            if (clip == null)
            {
                Debug.LogError($"해당 UI_Sfx을 찾을 수 없습니다: {nameof(sfx)} = {sfx}");
                return;
            }

            sfxAudioSource.PlayOneShot(clip); // Play
        }

        private void ApplyBgmSettings()
        {
            bgmAudioSource.mute = soundSettings.bgm.IsMute;
            bgmAudioSource.volume = soundSettings.bgm.Volume;
        }

        private void ApplySfxSettings()
        {
            sfxAudioSource.mute = soundSettings.sfx.IsMute;
            sfxAudioSource.volume = soundSettings.sfx.Volume;
        }

        private void AddEvent()
        {
            if (soundSettings != null)
            {
                soundSettings.bgm.OnChange += ApplyBgmSettings;
                soundSettings.sfx.OnChange += ApplySfxSettings;
            }
        }

        private void RemoveEvent()
        {
            if (soundSettings != null)
            {
                soundSettings.bgm.OnChange -= ApplyBgmSettings;
                soundSettings.sfx.OnChange -= ApplySfxSettings;
            }
        }
    }
}