namespace Ragnarok
{
    public class EmblemFrameSubView : EmblemSubView
    {
        public override void Localize()
        {
            labelTitle.LocalKey = LocalizeKey._33015; // 프레임
        }

        protected override void OnChangeToggle()
        {
            if (!UIToggle.current.value)
                return;

            presenter.SetEmblemFrame(byte.Parse(UIToggle.current.name));
        }
    } 
}
