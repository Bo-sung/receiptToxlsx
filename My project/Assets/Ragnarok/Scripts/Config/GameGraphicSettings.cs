using UnityEngine;

namespace Ragnarok
{
    public sealed class GameGraphicSettings : Singleton<GameGraphicSettings>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GraphicQuality qualityLevel = Instance.GetGraphicQuality();
            Instance.SetGraphicQuality(qualityLevel);
        }

        private enum Key
        {
            GameGraphicSettings,
            ShadowMultiPlayerSettings,
        }

        public GraphicQuality QualityLevel
        {
            get { return GetGraphicQuality(); }
            set
            {
                if (GetGraphicQuality() == value)
                    return;

                SetGraphicQuality(value);
            }
        }

        public ShadowMultiPlayerQuality ShadowMultiPlayerQualityLevel
        {
            get { return GetShadowMultiPlayerQuality(); }
            set
            {
                if (GetShadowMultiPlayerQuality() == value)
                    return;

                SetShadowMultiPlayer(value);
            }
        }

        private bool isShadowMode;
        public bool IsShadowMode
        {
            set
            {
                if (isShadowMode == value)
                    return;

                ShadowMultiPlayerQuality savedShadowState = CurrentShadowState;
                isShadowMode = value;

                if (savedShadowState != CurrentShadowState)
                {
                    OnShadowState?.Invoke();
                }
            }
        }

        public ShadowMultiPlayerQuality CurrentShadowState
        {
            get { return isShadowMode ? GetShadowMultiPlayerQuality() : ShadowMultiPlayerQuality.NonShadow; }
        }

        public event System.Action OnShadowState;

        protected override void OnTitle()
        {
        }

        private GraphicQuality GetGraphicQuality()
        {
            int qualityLevel = PlayerPrefs.GetInt(nameof(Key.GameGraphicSettings), (int)GraphicQuality.Medium);
            return qualityLevel.ToEnum<GraphicQuality>();
        }

        private void SetGraphicQuality(GraphicQuality quality)
        {
            PlayerPrefs.SetInt(nameof(Key.GameGraphicSettings), (int)quality);

            switch (quality)
            {
                case GraphicQuality.Low:
                    QualitySettings.SetQualityLevel(0);
                    break;

                case GraphicQuality.Medium:
                    QualitySettings.SetQualityLevel(1);
                    break;

                case GraphicQuality.High:
                    QualitySettings.SetQualityLevel(2);
                    break;

                default:
                    Debug.Log($"[올바르지 않은 {nameof(quality)}] {nameof(quality)} = {quality}");
                    break;
            }
        }

        private ShadowMultiPlayerQuality GetShadowMultiPlayerQuality()
        {
            int value = PlayerPrefs.GetInt(nameof(Key.ShadowMultiPlayerSettings), (int)ShadowMultiPlayerQuality.Shadow);
            return value.ToEnum<ShadowMultiPlayerQuality>();
        }

        private void SetShadowMultiPlayer(ShadowMultiPlayerQuality quality)
        {
            ShadowMultiPlayerQuality savedShadowState = CurrentShadowState;
            PlayerPrefs.SetInt(nameof(Key.ShadowMultiPlayerSettings), (int)quality);

            if (savedShadowState != CurrentShadowState)
            {
                OnShadowState?.Invoke();
            }
        }
    }
}