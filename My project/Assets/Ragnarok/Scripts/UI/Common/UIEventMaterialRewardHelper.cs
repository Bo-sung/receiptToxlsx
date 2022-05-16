namespace Ragnarok
{
    public sealed class UIEventMaterialRewardHelper : UIMaterialRewardHelper
    {
        protected override void RefreshCount()
        {
            long ownValue = GetOwnValue();
            int value = GetValue() * amount;
            IsLackAmount = ownValue < value;

            if (IsLackAmount)
            {
                button.Text = StringBuilderPool.Get()
                    .Append("[c][D76251]").Append(ownValue)
                    .Append("/")
                    .Append(value).Append("[-][/c]").Release();

                icon.Mode = UIGraySprite.SpriteMode.Grayscale;
            }
            else
            {
                button.Text = StringBuilderPool.Get()
                    .Append(ownValue)
                    .Append("/")
                    .Append(value).Release();

                icon.Mode = UIGraySprite.SpriteMode.None;
            }
        }
    }
}