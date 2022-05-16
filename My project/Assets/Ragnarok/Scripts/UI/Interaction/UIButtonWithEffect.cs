using UnityEngine;

namespace Ragnarok
{
    public class UIButtonWithEffect : UIButtonHelper
    {
        [SerializeField] GameObject goEffect;

        public override bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            set
            {
                base.IsEnabled = value;
                SetActiveEffect(value);
            }
        }

        public void SetActiveEffect(bool isShow)
        {
            NGUITools.SetActive(goEffect, isShow);
        }
    }
}