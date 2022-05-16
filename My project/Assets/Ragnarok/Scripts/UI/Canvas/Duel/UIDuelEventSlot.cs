using UnityEngine;

namespace Ragnarok
{
    public class UIDuelEventSlot : UIButtonWithIconHelper
    {
        public void SetSelection(bool value)
        {
            GetIcon().cachedTransform.localScale = value ? new Vector3(1.25f, 1.25f, 1.0f) : Vector3.one;
        }
    }
}