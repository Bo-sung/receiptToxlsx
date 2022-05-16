using UnityEngine;

namespace Ragnarok
{
    public class UIButtonWithToggle : UIButtonHelper
    {
        [SerializeField] GameObject onState;
        [SerializeField] GameObject offState;

        public void SetToggle(bool isOnState)
        {
            NGUITools.SetActive(onState, isOnState);
            NGUITools.SetActive(offState, !isOnState);
        }
    }
}