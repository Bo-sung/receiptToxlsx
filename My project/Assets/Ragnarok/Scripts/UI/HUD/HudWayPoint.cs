using UnityEngine;

namespace Ragnarok
{
    public class HudWayPoint : HUDObject, IAutoInspectorFinder
    {
        [SerializeField] UILabel labelName;

        public void Initialize(string text)
        {
            labelName.text = text;
        }
    } 
}
