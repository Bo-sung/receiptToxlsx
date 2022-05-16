using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class UITabHelper : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] int group;
        [SerializeField] UIToggleHelper[] toggles;

        GameObject myGameObject;

        public List<EventDelegate>[] OnChange => toggles.Select(x => x.OnChange).ToArray();

        public int Value
        {
            set
            {
                for (int i = 0; i < toggles.Length; i++)
                {
                    if (toggles[i] == null)
                        continue;

                    toggles[i].Value = i == value;
                }
            }
        }

        public UIToggleHelper this[int index]
        {
            get
            {
                return toggles[index];
            }
        }

        public int Count => toggles.Length;

        public event System.Action<int> OnSelect;
        public event System.Action<int> OnUnselect;

        void Awake()
        {
            foreach (var item in toggles)
            {
                EventDelegate.Add(item.OnChange, OnChangedToggle);
            }
        }

        void OnDestroy()
        {
            foreach (var item in toggles)
            {
                EventDelegate.Remove(item.OnChange, OnChangedToggle);
            }
        }

        public void ResetToggle()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] == null)
                    continue;

                toggles[i].Set(false, notify: true);
            }
        }

        void OnChangedToggle()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (!toggles[i].IsCurrentToggle())
                    continue;

                if (UIToggle.current.value)
                {
                    OnSelect?.Invoke(i);
                }
                else
                {
                    OnUnselect?.Invoke(i);
                }

                break;
            }
        }

        public void SetValue(int value)
        {
            for (int i = 0; i < Count; i++)
            {
                toggles[i].SetAlpha(i < value ? 1f : 0f);
            }
        }

        public int GetSelectIndex()
        {
            for (int i = 0; i < Count; i++)
            {
                if (toggles[i].Value)
                    return i;
            }
            return 0;
        }

        public void SetActive(bool isActive)
        {
            if (myGameObject == null) myGameObject = gameObject;

            myGameObject.SetActive(isActive);
        }

        public void SetToggleWidgetSize(int idx, int width = -1, int height = -1)
        {
            var toggle = toggles[idx];
            if (toggle == null || toggle.GetWidget() == null)
            {
                return;
            }

            var widget = toggle.GetWidget();
            if (width > 0)
            {
                widget.width = width;
            }

            if (height > 0)
            {
                widget.height = height;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (toggles == null)
                return;

            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].Group = group;
            }
        }
#endif

        bool IInspectorFinder.Find()
        {
            toggles = GetComponentsInChildren<UIToggleHelper>();
            return true;
        }
    }
}