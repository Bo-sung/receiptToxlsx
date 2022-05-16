using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UITitleContent : MonoBehaviour
    {
        [SerializeField] UILabelHelper title;
        [SerializeField] UILabelHelper content;
        [SerializeField] UIButtonHelper button;

        public List<EventDelegate> OnClick => button.OnClick;

        public int TitleLocalKey
        {
            set
            {
                TitleText = value.ToText();
            }
        }

        public string TitleText
        {
            set
            {
                if (title)
                    title.Text = value;
            }
        }

        public int ContentLocalKey
        {
            set
            {
                ContentText = value.ToText();
            }
        }

        public string ContentText
        {
            set
            {
                if (content)
                    content.Text = value;
            }
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
            gameObject.SetActive(isActive);
        }

        public UIButtonHelper GetButton()
        {
            return button;
        }
    }
}