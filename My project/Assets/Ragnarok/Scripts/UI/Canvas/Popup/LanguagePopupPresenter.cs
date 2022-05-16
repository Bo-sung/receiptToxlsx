namespace Ragnarok
{
    /// <summary>
    /// <see cref="UILanguagePopup"/>
    /// </summary>
    public sealed class LanguagePopupPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;

        public LanguagePopupPresenter(IView view)
        {
            this.view = view;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public LanguageType GetCurrentLanguage()
        {
            return Language.Current;
        }

        public void SelectLanguage(LanguageType type)
        {
            Language.SetLanguageType(type);
            view.Refresh();
        }
    }
}