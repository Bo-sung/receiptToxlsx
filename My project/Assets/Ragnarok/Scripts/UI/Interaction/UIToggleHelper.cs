using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIToggle))]
    public class UIToggleHelper : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UIToggle toggle;
        [SerializeField] UILabelHelper label;
        [SerializeField] string sound;
        [SerializeField] GameObject notice;
        [SerializeField] UIWidget widget;

        public List<EventDelegate> OnChange => toggle.onChange;
        public UILabelHelper Label => label;

        public int LocalKey
        {
            set { Text = value.ToText(); }
        }

        public string Text
        {
            set
            {
                if (label)
                    label.Text = value;
            }
        }

        public bool Value
        {
            get { return toggle.value; }
            set { toggle.value = value; }
        }

        public bool IsEnable
        {
            get
            {
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			    Collider col = collider;
#else
                Collider col = gameObject.GetComponent<Collider>();
#endif
                if (col && col.enabled) return true;
                Collider2D c2d = GetComponent<Collider2D>();
                return (c2d && c2d.enabled);
            }
            set
            {
                if (IsEnable != value)
                {
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
				    Collider col = collider;
#else
                    Collider col = gameObject.GetComponent<Collider>();
#endif
                    if (col != null)
                    {
                        col.enabled = value;
                    }
                    else
                    {
                        Collider2D c2d = GetComponent<Collider2D>();
                        if (c2d != null)
                        {
                            c2d.enabled = value;
                        }
                        else enabled = value;
                    }
                }
            }
        }

        public int Group
        {
            set { toggle.group = value; }
        }

        /// <summary>
        /// 현재 토글 여부
        /// </summary>
        public bool IsCurrentToggle()
        {
            return UIToggle.current == toggle;
        }

        public void Set(bool state, bool notify = true)
        {
            toggle.Set(state, notify);
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
            {
                notice.SetActive(isActive);
            }
        }

        public void SetLabelOutline(bool isActive)
        {
            if (label)
            {
                label.IsOutline = isActive;
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SetAlpha(float value)
        {
            if (widget)
                widget.alpha = value;
        }

        public UIWidget GetWidget()
        {
            return widget;
        }

        public virtual bool Find()
        {
            toggle = GetComponent<UIToggle>();
            var sprite = transform.Find("Highlight");
            if (sprite != null)
            {
                toggle.activeSprite = sprite.GetComponent<UISprite>();
            }
            var lab = transform.Find("Label");
            if (lab != null)
            {
                label = lab.GetComponent<UILabelHelper>();
            }
            var widget = GetComponent<UIWidget>();
            if (widget)
                this.widget = widget;

            return true;
        }
    }
}