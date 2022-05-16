using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 캐릭터 기본 정보
    /// </summary>
    public class DummyCharacterModel : CharacterModel
    {
        public delegate void ChangeStyleEvent(Gender gender, Job job);
        public delegate void ChangeWeaponEvent();
        public delegate void ChangeCostumeEvent();

        private readonly ItemDataManager itemDataRepo;
        private readonly CostumeDataManager costumeDataRepo;

        Gender gender = Gender.Male;
        Job job = Job.Novice;

        public override Job Job => job;
        public override Gender Gender => gender;
        public string WeaponPrefabName { get; private set; }
        public EquipmentClassType WeaponType { get; private set; } = EquipmentClassType.OneHandedSword;
        public string CostumePrefabName { get; private set; }
        public string CostumeWeaponPrefabName { get; private set; }
        public string CostumeHatPrefabName { get; private set; }
        public string CostumeFacePrefabName { get; private set; }
        public string CostumeCapePrefabName { get; private set; }
        public string CostumePetPrefabName { get; private set; }
        public string CostumeBodyPrefabName { get; private set; }
        public CostumeType CostumeWeaponType { get; private set; } = CostumeType.OneHandedSword;
        public int CostumeTitleId { get; private set; } // 코스튬 칭호 언어 ID
        public CostumeBodyType CostumeBodyType { get; private set; } // 몸 코스튬 타입

        private int costumeDataId; // 코스튬 테이블 ID ( 코스튬 머리 위치 직업별로 오프셋 정보 )

        public event ChangeStyleEvent OnChangedStyle;
        public event ChangeWeaponEvent OnChangedWeapon;
        public event ChangeCostumeEvent OnChangeCostumeEvent;

        public DummyCharacterModel()
        {
            itemDataRepo = ItemDataManager.Instance;
            costumeDataRepo = CostumeDataManager.Instance;
        }

        /// <summary>
        /// 성별 세팅
        /// </summary>
        public override void SetGender(Gender gender)
        {
            base.SetGender(gender);
            this.gender = gender;
            CallbackChangeStyle();
        }

        public override void SetJob(Job job)
        {
            base.SetJob(job);
            this.job = job;
            CallbackChangeStyle();
        }

        public void Set(Job job, Gender gender)
        {
            base.SetGender(gender);
            base.SetJob(job);
            this.job = job;
            this.gender = gender;
            CallbackChangeStyle();
        }

        public void Set(int weaponItemID)
        {
            ItemData weapon = GetItem(weaponItemID);

            if (weapon == null)
            {
                WeaponType = EquipmentClassType.OneHandedSword;
                WeaponPrefabName = Constants.Appearance.EMPTY_WEAPON_PREFAB_NAME;
            }
            else
            {
                WeaponType = weapon.class_type.ToEnum<EquipmentClassType>();
                WeaponPrefabName = weapon.prefab_name;
            }

            OnChangedWeapon?.Invoke();
        }

        public void SetCostume(ItemInfo.IEquippedItemValue[] equippedItem, bool isHidePet = false)
        {
            ResetCostume();
            foreach (var item in equippedItem.OrEmptyIfNull())
            {
                ItemData costume = GetItem(item.ItemId);

                if (costume == null)
                    continue;

                ItemGroupType itemGroupType = costume.ItemGroupType;
                if (itemGroupType == ItemGroupType.Costume)
                {
                    ItemEquipmentSlotType slotType = costume.GetCostumeSlotType();

                    if (isHidePet && slotType == ItemEquipmentSlotType.CostumePet)
                        continue;

                    switch (slotType)
                    {
                        case ItemEquipmentSlotType.CostumeWeapon:
                            CostumeWeaponPrefabName = costume.prefab_name;
                            CostumeWeaponType = costume.class_type.ToEnum<CostumeType>();
                            break;

                        case ItemEquipmentSlotType.CostumeHat:
                            {
                                CostumeHatPrefabName = costume.prefab_name;
                                costumeDataId = costume.event_id;
                            }
                            break;

                        case ItemEquipmentSlotType.CostumeFace:
                            CostumeFacePrefabName = costume.prefab_name;
                            break;

                        case ItemEquipmentSlotType.CostumeCape:
                            CostumeCapePrefabName = costume.prefab_name;
                            break;

                        case ItemEquipmentSlotType.CostumePet:
                            CostumePetPrefabName = costume.prefab_name;
                            break;

                        case ItemEquipmentSlotType.CostumeBody:
                            CostumeBodyPrefabName = costume.prefab_name;
                            CostumeBodyType = costume.element_type.ToEnum<CostumeBodyType>();
                            break;

                        case ItemEquipmentSlotType.CostumeTitle:
                            CostumeTitleId = costume.skill_id;
                            break;
                    }
                }
            }

            OnChangeCostumeEvent?.Invoke();
        }

        public void SetCostume(int[] costumeItemIds)
        {
            ResetCostume();

            if (costumeItemIds == null)
                return;

            for (int i = 0; i < costumeItemIds.Length; i++)
            {
                ItemData costume = GetItem(costumeItemIds[i]);

                if (costume != null)
                {
                    ItemEquipmentSlotType slotType = costume.GetCostumeSlotType();
                    switch (slotType)
                    {
                        case ItemEquipmentSlotType.CostumeWeapon:
                            CostumeWeaponPrefabName = costume.prefab_name;
                            break;

                        case ItemEquipmentSlotType.CostumeHat:
                            {
                                CostumeHatPrefabName = costume.prefab_name;
                                costumeDataId = costume.event_id;
                            }
                            break;

                        case ItemEquipmentSlotType.CostumeFace:
                            CostumeFacePrefabName = costume.prefab_name;
                            break;

                        case ItemEquipmentSlotType.CostumeCape:
                            CostumeCapePrefabName = costume.prefab_name;
                            break;

                        case ItemEquipmentSlotType.CostumePet:
                            CostumePetPrefabName = costume.prefab_name;
                            break;

                        case ItemEquipmentSlotType.CostumeBody:
                            CostumeBodyPrefabName = costume.prefab_name;
                            CostumeBodyType = costume.element_type.ToEnum<CostumeBodyType>();
                            break;

                        case ItemEquipmentSlotType.CostumeTitle:
                            CostumeTitleId = costume.skill_id;
                            break;
                    }
                }
            }

            OnChangeCostumeEvent?.Invoke();
        }

        private void ResetCostume()
        {
            costumeDataId = 0;
            CostumeWeaponPrefabName = string.Empty;
            CostumeHatPrefabName = string.Empty;
            CostumeFacePrefabName = string.Empty;
            CostumeCapePrefabName = string.Empty;
            CostumePetPrefabName = string.Empty;
            CostumeBodyPrefabName = string.Empty;
            CostumeWeaponType = CostumeType.OneHandedSword;
            CostumeTitleId = 0;
        }

        public void Set(string custumePrefabName)
        {
            CostumePrefabName = custumePrefabName;
            OnChangeCostumeEvent?.Invoke();
        }

        /// <summary>
        /// 이벤트 호출
        /// </summary>
        private void CallbackChangeStyle()
        {
            OnChangedStyle?.Invoke(gender, job);
        }

        private ItemData GetItem(int itemID)
        {
            if (itemID == 0)
                return null;

            ItemData item = itemDataRepo.Get(itemID);

            if (item == null)
                Debug.LogError($"아이템 데이터가 존재하지 않습니다: {nameof(itemID)} = {itemID}");

            return item;
        }

        public CostumeData GetCostumeData()
        {
            if (costumeDataId == 0)
                return null;

            CostumeData data = costumeDataRepo.Get(costumeDataId);

            if (data == null)
                Debug.LogError($"코스튬 데이터가 존재하지 않습니다: {nameof(costumeDataId)} = {costumeDataId}");

            return data;
        }
    }
}