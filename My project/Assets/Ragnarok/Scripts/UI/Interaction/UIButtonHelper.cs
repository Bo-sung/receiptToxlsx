using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIButton))]
    public class UIButtonHelper : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UIButton button;
        [SerializeField] UILabelHelper label;
        [SerializeField] string sound;
        [SerializeField] protected Color32 normal = new Color32(63, 136, 188, 255);
        [SerializeField] protected Color32 disabled = new Color32(125, 125, 125, 255);
        [SerializeField] GameObject notice;

        public List<EventDelegate> OnClick => button.onClick;

        public virtual bool IsEnabled
        {
            get
            {
                return button.isEnabled;
            }
            set
            {
                if (label)
                    label.Outline = value ? normal : disabled;
                button.isEnabled = value;
            }
        }

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
                if (label)
                    return label.Text;

                return string.Empty;
            }
            set
            {
                if (label)
                    label.Text = value;
            }
        }

        public string SpriteName
        {
            set
            {
                button.normalSprite = value;
            }
        }

        public UIBasicSprite.Flip Flip
        {
            set
            {
                UISprite spr = GetComponent<UISprite>();
                if (spr is null)
                    return;

                spr.flip = value;
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

        void OnPress(bool isPress)
        {
            // TODO 임시 사운드 재생 (나중에 UIButtonSound 로 변경해보자)
            if (!isPress)
                SoundManager.Instance.PlayButtonSfx(Sfx.Button.Yes);
        }

        public void SetNotice(bool isActive)
        {
            if (notice)
                notice.SetActive(isActive);
        }

        public bool IsActiveNotice()
        {
            if (!notice)
                return false;

            return notice.activeSelf;
        }

        public void SetActiveLabel(bool isActive)
        {
            if (label == null)
                return;

            label.SetActive(isActive);
        }

        public void SetAsLastSibling()
        {
            transform.SetAsLastSibling();
        }

        public void SetOutlineColor(Color32 color)
        {
            if (label)
            {
                normal = color;
                label.Outline = normal;
            }
        }

        public virtual bool Find()
        {
            button = GetComponent<UIButton>();
            if (label)
                normal = label.Outline;

            return true;
        }
    }
}