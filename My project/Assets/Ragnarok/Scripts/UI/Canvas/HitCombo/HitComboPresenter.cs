namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIHitCombo"/>
    /// </summary>
    public class HitComboPresenter : ViewPresenter
    {
        public interface IView
        {
            void Show(int combo);
        }

        private readonly IView view;

        public HitComboPresenter(IView view)
        {
            this.view = view;           
        }     

        public override void AddEvent()
        {
             Entity.player.OnHitCombo += OnHitCombo;
        }

        public override void RemoveEvent()
        {
             Entity.player.OnHitCombo -= OnHitCombo;
        }

        private void OnHitCombo(int combo)
        {
            view.Show(combo);
        }
    }
}
