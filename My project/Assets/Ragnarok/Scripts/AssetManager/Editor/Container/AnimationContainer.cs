using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/Animation")]
    public sealed class AnimationContainer : StringAssetContainer<AnimationClip>
    {
        protected override string ConvertKey(AnimationClip t)
        {
            return t.name;
        }

#if UNITY_EDITOR
        [ContextMenu("클립보드에 복사")]
        private void CopyToClipboard() => CopyAllItems();
#endif
    }
}