using UnityEngine;

namespace Ragnarok
{
    public class UILabelValue : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelTitle, labelValue;

        private GameObject myGameObject;

        public int TitleKey { set { Title = value.ToText(); } }

        public int ValueKey { set { Value = value.ToText(); } }

        public string Title
        {
            set
            {
                if (labelTitle != null)
                    labelTitle.Text = value;
            }
        }

        public string Value
        {
            get
            {
                if (labelValue != null)
                    return labelValue.Text;

                return string.Empty;
            }
            set
            {
                if (labelValue != null)
                    labelValue.Text = value;
            }
        }

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            if (myGameObject.activeSelf == isActive)
                return;

            myGameObject.SetActive(isActive);
        }

        protected void SetActiveValue(bool isActive)
        {
            if (labelValue == null)
                return;

            labelValue.SetActive(isActive);
        }
    }
}