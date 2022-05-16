namespace Ragnarok
{
    public class UILongPressButton : UIButton
    {
        public event System.Action OnSelectLongPress;

        private void OnLongPress()
        {
            OnSelectLongPress?.Invoke();
        }
    }
}