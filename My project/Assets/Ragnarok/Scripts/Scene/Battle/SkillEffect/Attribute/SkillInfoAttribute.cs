using UnityEngine;

namespace Ragnarok
{
    public class SkillInfoAttribute : PropertyAttribute
    {
        public enum SkillInfoType { Damage, Vfx, Sound, Projectile, }

        public readonly string title;
        public readonly SkillInfoType skillInfoType;

        public SkillInfoAttribute(string title, SkillInfoType skillInfoType)
        {
            this.title = title;
            this.skillInfoType = skillInfoType;
        }
    }
}