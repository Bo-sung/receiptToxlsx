using UnityEngine;

namespace Ragnarok.View
{
    public class PackAchieveSlotState : UIView
    {
        [SerializeField] UILabelHelper labelStep;
        [SerializeField] UILabelHelper labelTitle;

        protected override void OnLocalize()
        {
            
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Set(string step, string title)
        {
            labelStep.Text = step;
            labelTitle.Text = title;
        }
    }
}