using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/Sound")]
    public sealed class SoundContainer : StringAssetContainer<AudioClip>
    {
        protected override string ConvertKey(AudioClip t)
        {
            return t.name;
        }
    }
}