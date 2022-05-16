using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UILabel))]
    public class UILabelHelper : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UILabel label;
        public UILabel uiLabel => label;

        public int LocalKey
        {
            set
            {
                Text = value.ToText();
            }
        }

        public string Text
        {
            get
            {
                return label.text;
            }
            set
            {
                label.text = value;
            }
        }

        public void SetActive(bool isActive)
        {
            NGUITools.SetActive(label.cachedGameObject, isActive);
        }

        public Color Outline
        {
            get
            {
                return label.effectColor;
            }
            set
            {
                label.effectColor = value;
            }
        }

        public Color Color
        {
            get => label.color;
            set => label.color = value;
        }

        public bool IsOutline
        {
            set
            {
                label.effectStyle = value == true ? UILabel.Effect.Outline8 : UILabel.Effect.None;
            }
        }

        bool IInspectorFinder.Find()
        {
            label = GetComponent<UILabel>();
            return true;
        }
    }
}