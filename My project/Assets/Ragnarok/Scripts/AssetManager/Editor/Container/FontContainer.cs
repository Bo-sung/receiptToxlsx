using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/Font")]
    public class FontContainer : StringAssetContainer<Font>
    {
        protected override string ConvertKey(Font t)
        {
            return t.name;
        }
    } 
}
