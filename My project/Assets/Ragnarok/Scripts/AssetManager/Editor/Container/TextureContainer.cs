using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/Texture")]
    public sealed class TextureContainer : StringAssetContainer<Texture2D>
    {
        protected override string ConvertKey(Texture2D t)
        {
            return t.name;
        }
    }
}
