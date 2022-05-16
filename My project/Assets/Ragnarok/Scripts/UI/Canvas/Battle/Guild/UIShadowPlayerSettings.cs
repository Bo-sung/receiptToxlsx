using UnityEngine;

namespace Ragnarok
{
    public sealed class UIShadowPlayerSettings : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIButtonWithIcon btnShadow;

        GameGraphicSettings graphicSettings;

        protected override void OnInit()
        {
            graphicSettings = GameGraphicSettings.Instance;

            EventDelegate.Add(btnShadow.OnClick, OnClickedBtnShadow);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnShadow.OnClick, OnClickedBtnShadow);
        }

        protected override void OnShow(IUIData data = null)
        {
            graphicSettings.IsShadowMode = true;
            Refresh();
        }

        protected override void OnHide()
        {
            graphicSettings.IsShadowMode = false;
        }

        protected override void OnLocalize()
        {
            UpdateText();
        }

        void OnClickedBtnShadow()
        {
            switch (graphicSettings.ShadowMultiPlayerQualityLevel)
            {
                case ShadowMultiPlayerQuality.Shadow:
                    graphicSettings.ShadowMultiPlayerQualityLevel = ShadowMultiPlayerQuality.HalfShadow;
                    break;

                case ShadowMultiPlayerQuality.HalfShadow:
                    graphicSettings.ShadowMultiPlayerQualityLevel = ShadowMultiPlayerQuality.NonShadow;
                    break;

                case ShadowMultiPlayerQuality.NonShadow:
                    graphicSettings.ShadowMultiPlayerQualityLevel = ShadowMultiPlayerQuality.Shadow;
                    break;
            }

            Refresh();
        }

        private void Refresh()
        {
            const string SHADOW = "Ui_Common_Icon_Shadow_Low";
            const string HALF_SHADOW = "Ui_Common_Icon_Shadow_Medium";
            const string NON_SHADOW = "Ui_Common_Icon_Shadow_High";

            switch (graphicSettings.ShadowMultiPlayerQualityLevel)
            {
                case ShadowMultiPlayerQuality.Shadow:
                    btnShadow.SetIconName(SHADOW);
                    break;

                case ShadowMultiPlayerQuality.HalfShadow:
                    btnShadow.SetIconName(HALF_SHADOW);
                    break;

                case ShadowMultiPlayerQuality.NonShadow:
                    btnShadow.SetIconName(NON_SHADOW);
                    break;
            }

            UpdateText();
        }

        private void UpdateText()
        {
            switch (graphicSettings.ShadowMultiPlayerQualityLevel)
            {
                case ShadowMultiPlayerQuality.Shadow:
                    btnShadow.LocalKey = LocalizeKey._38102; // Low
                    break;

                case ShadowMultiPlayerQuality.HalfShadow:
                    btnShadow.LocalKey = LocalizeKey._38103; // Medium
                    break;

                case ShadowMultiPlayerQuality.NonShadow:
                    btnShadow.LocalKey = LocalizeKey._38104; // High
                    break;
            }
        }
    }
}