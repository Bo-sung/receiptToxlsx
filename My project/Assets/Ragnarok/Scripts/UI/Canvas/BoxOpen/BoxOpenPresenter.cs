namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBoxOpen"/>
    /// </summary>
    public sealed class BoxOpenPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;

        public BoxOpenPresenter(IView view)
        {
            this.view = view;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }
    }
}