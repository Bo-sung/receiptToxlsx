namespace Ragnarok
{
    public class EmblemIconSubView : EmblemSubView
    {
        public override void Localize()
        {
            labelTitle.LocalKey = LocalizeKey._33016; // 아이콘
        }

        protected override void OnChangeToggle()
        {
            if (!UIToggle.current.value)
                return;

            presenter.SetEmblemIcon(byte.Parse(UIToggle.current.name));
        }
    } 
}
