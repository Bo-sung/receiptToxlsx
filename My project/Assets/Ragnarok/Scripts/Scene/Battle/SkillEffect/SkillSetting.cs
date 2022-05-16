using UnityEngine;

namespace Ragnarok
{
    public class SkillSetting : ScriptableObject
    {
        [System.Serializable]
        public class Vfx
        {
            public int time;
            public string name;
            public int duration;
            public bool toTarget;
            public string node;
            public bool isAttach;
            public Vector3 offset;
            public Vector3 rotate;
        }

        [System.Serializable]
        public class Sound
        {
            public int time;
            public string name;
            public int duration;
        }

        [System.Serializable]
        public class Projectile
        {
            public int time;
            public string name;
            public int duration;
            public string node;
            public Vector3 offset;
            public Vector3 rotate;
        }

        public int id;
        public string aniName;
        public string castingAniName;

        [SkillInfo("대미지 상세 정보", SkillInfoAttribute.SkillInfoType.Damage)]
        public int hitTime;

        [SkillInfo("이펙트 상세 정보", SkillInfoAttribute.SkillInfoType.Vfx)]
        public Vfx[] arrVfx;

        [SkillInfo("사운드 상세 정보", SkillInfoAttribute.SkillInfoType.Sound)]
        public Sound[] arrSound;

        [SkillInfo("발사체 상세 정보", SkillInfoAttribute.SkillInfoType.Projectile)]
        public Projectile[] arrProjectile;

#if UNITY_EDITOR
        public void SortByFrame()
        {
            if (arrVfx != null)
                System.Array.Sort(arrVfx, (a, b) => a.time.CompareTo(b.time));

            if (arrSound != null)
                System.Array.Sort(arrSound, (a, b) => a.time.CompareTo(b.time));

            if (arrProjectile != null)
                System.Array.Sort(arrProjectile, (a, b) => a.time.CompareTo(b.time));
        }
#endif
    }
}