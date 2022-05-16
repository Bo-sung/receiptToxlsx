using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIDiceEventElement : UIElement<string>
    {
        [SerializeField] UITextureHelper texture;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            texture.SetEvent(info);
        }
    }
}