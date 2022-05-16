using UnityEngine;

namespace Ragnarok.View
{
    public class UIGuildAttackBuffResult : UIView, IInspectorFinder
    {
        [SerializeField] UIWidget widget;
        [SerializeField] UILabelHelper labelTitle, labelValue;
        [SerializeField] UILabelHelper labelAddValue;
        [SerializeField] GameObject goNew;

        protected override void OnLocalize()
        {
        }

        public void SetTitle(string text)
        {
            labelTitle.Text = text;
        }

        public void SetValue(string text)
        {
            labelValue.Text = text;
        }

        public void SetAddValue(string text)
        {
            labelAddValue.Text = text;
        }

        public void SetActiveNew(bool isActive)
        {
            goNew.SetActive(isActive);
        }

        public void SetAlpha(float alpha)
        {
            widget.alpha = alpha;
        }

        bool IInspectorFinder.Find()
        {
            widget = GetComponent<UIWidget>();
            return true;
        }
    }
}