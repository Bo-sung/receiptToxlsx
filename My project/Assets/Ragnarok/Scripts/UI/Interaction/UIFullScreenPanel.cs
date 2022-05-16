using UnityEngine;

namespace Ragnarok
{
    public class UIFullScreenPanel : UIPanel
    {
        protected override Camera GetAnchorCamera()
        {
            return NGUITools.FindCameraForLayer(cachedGameObject.layer);
        }
    }
}