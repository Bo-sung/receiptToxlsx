using UnityEngine;

namespace Ragnarok
{
    public abstract class UIOptionEtcSettings : MonoBehaviour, IInspectorFinder
    {
        [SerializeField]
        protected UILabelHelper labelTitle;

        [SerializeField]
        protected UIToggleHelper[] toggles;

        protected OptionPresenter presenter;

        protected virtual void Awake()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                EventDelegate.Add(toggles[i].OnChange, OnChangedToggle);
            }

            UI.AddEventLocalize(OnLocalize);
        }

        protected virtual void Start()
        {
            OnLocalize();
        }

        protected virtual void OnDestroy()
        {
            UI.RemoveEventLocalize(OnLocalize);
            for (int i = 0; i < toggles.Length; i++)
            {
                EventDelegate.Remove(toggles[i].OnChange, OnChangedToggle);
            }

            presenter = null;
        }

        public void Initialize(OptionPresenter presenter)
        {
            this.presenter = presenter;
            Refresh();
        }

        private void OnChangedToggle()
        {
            if (!UIToggle.current.value)
                return;

            int index = GetToggle();
            OnChange(index);
            Refresh();
        }

        private int GetToggle()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i].IsCurrentToggle())
                    return i;
            }

            return -1;
        }

        protected virtual void OnLocalize()
        {
            Refresh();
        }

        protected abstract void OnChange(int index);

        protected abstract void Refresh();

        bool IInspectorFinder.Find()
        {
            toggles = GetComponentsInChildren<UIToggleHelper>(includeInactive: true);
            return true;
        }
    }
}