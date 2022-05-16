#define SHOW_ONLY_COSTUME_WEAPON

using UnityEngine;

namespace Ragnarok
{
    public class BookDummyCharacterActor : CharacterActor
    {
        protected override void UpdateWeapon()
        {
            EquipmentClassType type;
            string prefabName;
            string effectName;

            // 코스튬 무기
            EquipmentClassType costumeWeaponType = characterEntity.GetCostumeWeaponType().ToWeaponType();
            string weaponCostumePrefabName = characterEntity.GetCostumePrefabName(ItemEquipmentSlotType.CostumeWeapon);
            string weaponCostumeEffectName = characterEntity.GetEffectName(ItemEquipmentSlotType.CostumeWeapon);

#if SHOW_ONLY_COSTUME_WEAPON
            type = costumeWeaponType;
            prefabName = weaponCostumePrefabName;
            effectName = weaponCostumeEffectName;
#else
            // 일반 무기
            EquipmentClassType weaponType = characterEntity.GetWeaponType();
            string weaponPrefabName = characterEntity.GetWeaponPrefabName();
            string weaponEffectName = characterEntity.GetEffectName(ItemEquipmentSlotType.Weapon);

            if (string.IsNullOrEmpty(weaponCostumePrefabName))
            {
                type = weaponType;
                prefabName = weaponPrefabName;
                effectName = weaponEffectName;
            }
            else
            {
                type = costumeWeaponType;
                prefabName = weaponCostumePrefabName;
                effectName = weaponCostumeEffectName;
            }
#endif
            if (type == default)
                type = EquipmentClassType.OneHandedSword;

            // 무기 세팅
            animator.SetWeaponType(type);
            appearance.SetWeaponType(prefabName, type, effectName);
        }
    }
}