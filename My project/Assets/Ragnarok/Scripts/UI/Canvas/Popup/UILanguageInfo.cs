using UnityEngine;

namespace Ragnarok
{
    public class UILanguageInfo : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] LanguageType type;
        [SerializeField] UIButtonHelper button;
        [SerializeField] GameObject check;
        [SerializeField] UILabel labelLanguageName;
        [SerializeField] Color32 normal = new Color32(76, 74, 77, 255);
        [SerializeField] Color32 disabled = new Color32(255, 255, 255, 255);

        LanguagePopupPresenter presenter;

        void Awake()
        {
            EventDelegate.Add(button.OnClick, OnClickdBtn);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(button.OnClick, OnClickdBtn);
        }

        public void Initialize(LanguagePopupPresenter presenter)
        {
            this.presenter = presenter;
            SetIsEnabled(type != LanguageType.None);
        }

        public void Refresh()
        {
            check.SetActive(type == presenter.GetCurrentLanguage());
        }

        private void OnClickdBtn()
        {
            presenter.SelectLanguage(type);
        }

        private void SetIsEnabled(bool isEnabled)
        {
            button.IsEnabled = isEnabled;
            labelLanguageName.color = isEnabled ? normal : disabled;
        }

        bool IInspectorFinder.Find()
        {
            button = GetComponent<UIButtonHelper>();
            return true;
        }
    }
}