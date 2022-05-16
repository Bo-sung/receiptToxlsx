using UnityEngine;

namespace Ragnarok
{
    public abstract class UIOptionSoundSettings : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField]
        protected UILabelHelper labelTitle;

        [SerializeField] UISlider slider;
        [SerializeField] UIToggleHelper toggle;
        [SerializeField] BoxCollider2D sliderCollider;

        protected bool IsMute;
        protected float? Volume;

        protected OptionPresenter presenter;

        protected virtual void Awake()
        {
            slider.onDragFinished += OnChangedSlider;
            EventDelegate.Add(toggle.OnChange, OnChangedToggle);         
            
            UI.AddEventLocalize(OnLocalize);
        }

        protected virtual void Start()
        {
            OnLocalize();
        }

        protected virtual void OnDestroy()
        {
            UI.RemoveEventLocalize(OnLocalize);
            slider.onDragFinished -= OnChangedSlider;
            EventDelegate.Remove(toggle.OnChange, OnChangedToggle);

            presenter = null;
        }

        public void Initialize(OptionPresenter presenter)
        {
            this.presenter = presenter;
        }

        private void OnChangedToggle()
        {
            OnChangeMute(!toggle.Value);
            Refresh();
        }

        private void OnChangedSlider()
        {
            OnChangeVolume(slider.value);
            Refresh();
        }

        protected virtual void OnLocalize()
        {
            Refresh();
        }

        protected abstract void Refresh();

        protected virtual void OnChangeMute(bool isMute)
        {
            this.IsMute = isMute;
        }

        protected virtual void OnChangeVolume(float volume)
        {
            this.Volume = volume;
        }

        protected void SetSoundSetting(bool isMute, float volume)
        {
            toggle.Value = !isMute;
            slider.value = volume;
            sliderCollider.enabled = volume != 0;
        }
    }
}