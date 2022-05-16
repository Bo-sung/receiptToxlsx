using UnityEngine;

namespace Ragnarok
{
    public sealed class SkillEffectPoolManager : PoolManager<SkillEffectPoolManager>
    {
        IEffectContainer effectContainer;

        protected override void Awake()
        {
            base.Awake();

            effectContainer = AssetManager.Instance;
        }

        protected override Transform GetOriginal(string key)
        {
            GameObject go = effectContainer.Get(key);

            if (go == null)
                Debug.LogError($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return go.transform;
        }

        protected override PoolObject Create(string key)
        {
            GameObject go = effectContainer.Get(key);

            if (go == null)
                Debug.LogError($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return Instantiate(go).AddMissingComponent<SkillEffect>();
        }

        public SkillEffect Spawn(ElementType elementType, bool isCritical)
        {
            return Spawn(GetKey(elementType, isCritical)) as SkillEffect;
        }

        /// <summary>
        /// 전직 이펙트 출력
        /// </summary>
        public SkillEffect SpawnJobChangeEffect(Transform parent)
        {
            SkillEffect effect = Spawn("FX_09_Character_JobChange", parent, false) as SkillEffect;
            effect.SetDuration(Constants.Battle.JOB_CHANGE_EFFECT_DURATION);

            return effect;
        }

        /// <summary>
        /// 돌진 이펙트 출력
        /// </summary>
        public (SkillEffect frontEffect, SkillEffect backEffect) SpawnRushEffect(Transform parent)
        {
            SkillEffect frontEffect = Spawn("VFX_ChaSkill_Spear", parent, false, Constants.Battle.RushEffect_Front_StartPosition) as SkillEffect;
            frontEffect?.SetScale(Constants.Battle.RushEffect_Front_Scale);
            frontEffect?.SetDuration((int)(Constants.Battle.RushEffect_Front_Duration * 100));

            SkillEffect backEffect = Spawn("VFX_ChaSkill_SpeedRush", parent, false) as SkillEffect;
            backEffect?.SetDuration((int)(Constants.Battle.RushEffect_Back_Duration * 100));

            return (frontEffect, backEffect);
        }

        /// <summary>
        /// 레벨업 이펙트 출력
        /// </summary>
        public SkillEffect SpawnLevelUpEffect(Transform parent)
        {
            return Spawn("FX_10_Character_BaseLevelUp", parent, worldPositionStays: false) as SkillEffect;
        }

        /// <summary>
        /// 직업레벨업 이펙트 출력
        /// </summary>
        public SkillEffect SpawnJobLevelUpEffect(Transform parent)
        {
            return Spawn("FX_09_Character_JobLevelUp", parent, worldPositionStays: false) as SkillEffect;
        }

        private string GetKey(ElementType elementType, bool isCritical)
        {
            switch (elementType)
            {
                case ElementType.Neutral:
                    return isCritical ? "VFX_Hit_NormalCri" : "VFX_Hit_Normal";

                case ElementType.Fire:
                    return isCritical ? "VFX_Hit_FireCri" : "VFX_Hit_Fire";

                case ElementType.Water:
                    return isCritical ? "VFX_Hit_IceCri" : "VFX_Hit_Ice";

                case ElementType.Wind:
                    return isCritical ? "VFX_Hit_WindCri" : "VFX_Hit_Wind";

                case ElementType.Earth:
                    return isCritical ? "VFX_Hit_EarthCri" : "VFX_Hit_Earth";

                case ElementType.Poison:
                    return isCritical ? "VFX_Hit_LeafCri" : "VFX_Hit_Leaf";

                case ElementType.Holy:
                    return isCritical ? "VFX_Hit_LightCri" : "VFX_Hit_Light";

                case ElementType.Shadow:
                    return isCritical ? "VFX_Hit_DarkCri" : "VFX_Hit_Dark";

                case ElementType.Ghost:
                    return isCritical ? "VFX_Hit_WaterCri" : "VFX_Hit_Water";

                case ElementType.Undead:
                    return isCritical ? "VFX_Hit_ThunderCri" : "VFX_Hit_Thunder";
            }

            return string.Empty;
        }
    }
}