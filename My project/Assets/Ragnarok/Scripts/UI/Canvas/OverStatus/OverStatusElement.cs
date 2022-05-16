using UnityEngine;

namespace Ragnarok.View
{
    public class OverStatusElement : UIView, IInspectorFinder
    {
        [SerializeField] UILabelValue overStat;
        [SerializeField] GameObject effect;

        protected override void OnLocalize()
        {
        }

        public UILabelValue Get()
        {
            return overStat;
        }

        public void SetEffect(bool isActive)
        {
            effect.SetActive(isActive);
        }

        bool IInspectorFinder.Find()
        {
            overStat = GetComponent<UILabelValue>();
            return true;
        }
    }
}