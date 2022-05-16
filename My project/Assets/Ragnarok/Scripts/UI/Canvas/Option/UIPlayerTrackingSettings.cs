using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIPlayerTrackingSettings : UIOptionEtcSettings
    {
        [SerializeField] UILabelHelper[] offLabels;

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelTitle.LocalKey = LocalizeKey._14042; // 플레이어 이동 시 카메라 효과

            for (int i = 0; i < toggles.Length; i++)
            {
                PlayerTrackingType trackingType = i.ToEnum<PlayerTrackingType>();
                string name = GetName(trackingType);
                toggles[i].Text = name;
                offLabels[i].Text = name;
            }
        }

        protected override void OnChange(int index)
        {
            PlayerTrackingType trackingType = index.ToEnum<PlayerTrackingType>();
            presenter.SetPlayerTrackingType(trackingType);
        }

        protected override void Refresh()
        {
            PlayerTrackingType trackingType = presenter.GetPlayerTrackingType();
            int index = (int)trackingType;

            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].Value = (index == i);
            }
        }

        private string GetName(PlayerTrackingType trackingType)
        {
            switch (trackingType)
            {
                case PlayerTrackingType.ZoomIn:
                    return LocalizeKey._14043.ToText(); // 확대

                case PlayerTrackingType.None:
                    return LocalizeKey._14044.ToText(); // 없음

                case PlayerTrackingType.ZoomOut:
                    return LocalizeKey._14045.ToText(); // 축소

                default:
                    Debug.Log($"[올바르지 않은 {nameof(trackingType)}] {nameof(trackingType)} = {trackingType}");
                    return default;
            }
        }
    }
}