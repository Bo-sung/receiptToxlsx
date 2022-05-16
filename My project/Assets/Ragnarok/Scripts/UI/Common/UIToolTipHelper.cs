using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class UIToolTipHelper : MonoBehaviour
    {
        private int localizeKey;

        public void SetToolTipLocalizeKey(int localizeKey)
        {
            this.localizeKey = localizeKey;
        }

        void OnTooltip(bool show)
        {
            if (show && localizeKey != 0)
            {
                UITooltip.Show(localizeKey.ToText());
            }
        }
    }
}