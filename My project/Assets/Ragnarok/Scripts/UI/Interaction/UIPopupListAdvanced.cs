using UnityEngine;

namespace Ragnarok
{
    public class UIPopupListAdvanced : UIPopupList
    {
        [SerializeField] UILabelHelper labelCategory;

        public string CategoryText
        {
            set { labelCategory.Text = value; }
        }

        public int CategoryLocalKey
        {
            set { labelCategory.LocalKey = value; }
        }

        public override void Show()
        {
            base.Show();

            if (mChild == null)
                return;
            
            if (separatePanel)
            {
                UIPanel panel = mChild.GetComponent<UIPanel>();

                if (panel == null)
                    return;

                panel.useSortingOrder = mPanel.useSortingOrder; // useSortingOrder 세팅 누락 수정
            }
        }
    }
}