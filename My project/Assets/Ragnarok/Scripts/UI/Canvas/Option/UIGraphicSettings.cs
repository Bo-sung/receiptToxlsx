using UnityEngine;

namespace Ragnarok
{
    public class UIGraphicSettings : UIOptionEtcSettings
    {
        [SerializeField] UILabelHelper[] offLabels;

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelTitle.LocalKey = LocalizeKey._14007; // 그래픽 품질

            for (int i = 0; i < toggles.Length; i++)
            {
                GraphicQuality graphicQuality = i.ToEnum<GraphicQuality>();
                string name = GetName(graphicQuality);
                toggles[i].Text = name;
                offLabels[i].Text = name;
            }
        }

        protected override void OnChange(int index)
        {
            GraphicQuality graphicQuality = index.ToEnum<GraphicQuality>();
            presenter.SetGraphicQualityLevel(graphicQuality);
        }

        protected override void Refresh()
        {
            GraphicQuality graphicQuality = presenter.GetGraphicQualityLevel();
            int index = (int)graphicQuality;

            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].Value = index == i;
            }
        }

        private string GetName(GraphicQuality graphicQuality)
        {
            switch (graphicQuality)
            {
                case GraphicQuality.Low:
                    return LocalizeKey._14008.ToText(); // 하

                case GraphicQuality.Medium:
                    return LocalizeKey._14009.ToText(); // 중

                case GraphicQuality.High:
                    return LocalizeKey._14010.ToText(); // 상

                default:
                    Debug.Log($"[올바르지 않은 {nameof(graphicQuality)}] {nameof(graphicQuality)} = {graphicQuality}");
                    return default;
            }
        }
    }
}