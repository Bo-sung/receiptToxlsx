namespace Ragnarok
{
    public class EmblemBackgroundSubView : EmblemSubView
    {
        public override void Localize()
        {
            labelTitle.LocalKey = LocalizeKey._33014; // 배경
        }

        protected override void OnChangeToggle()
        {
            if (!UIToggle.current.value)
                return;

            presenter.SetEmblemBackground(byte.Parse(UIToggle.current.name));
        }
    } 
}
