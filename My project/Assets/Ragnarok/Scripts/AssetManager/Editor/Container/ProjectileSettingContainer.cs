using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/ProjectileSetting")]
    public sealed class ProjectileSettingContainer : StringAssetContainer<ProjectileSetting>
    {
        protected override string ConvertKey(ProjectileSetting t)
        {
            return t.name;
        }
    }
}