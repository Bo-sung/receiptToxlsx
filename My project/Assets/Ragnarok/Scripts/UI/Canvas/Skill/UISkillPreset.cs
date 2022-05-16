using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class UISkillPreset : MonoBehaviour, IInspectorFinder
    {
        public delegate void SlotUnlockEvent();
        public delegate void SlotEvent(int slotIndex);

        public interface IInfo
        {
            bool IsUnlock();
            bool HasSkill();
            bool HasEquipSkillSlot();
            UISkillInfo.IInfo SkillInfo { get; }
        }

        [SerializeField] int slotIndex;
        [SerializeField] GameObject lockBase;
        [SerializeField] GameObject unlockBase;
        [SerializeField] UISkillInfo skillInfo;
        [SerializeField] GameObject skillBlock;
        [SerializeField] UIButtonHelper unequipButton;
        [SerializeField] GameObject plusIcon;

        public event SlotUnlockEvent OnUnlock;
        public event SlotEvent OnEmptySlotClick;
        public event SlotEvent OnEquippedSlotClick;
        public event SlotEvent OnUnequipButtonClick;

        private IInfo info;

        private void Start()
        {
            EventDelegate.Add(unequipButton.OnClick, OnClickUnequip);
        }

        void OnClick()
        {
            if (info == null)
                return;

            if (info.IsUnlock())
            {
                if (info.HasSkill())
                {
                    OnEquippedSlotClick?.Invoke(slotIndex); // 장착해제
                }
                else
                {
                    OnEmptySlotClick?.Invoke(slotIndex); // 장착준비
                }
            }
            else
            {
                OnUnlock?.Invoke(); // 스킬슬롯 잠금해제
            }
        }

        public void Show(IInfo info)
        {
            this.info = info;
            Refresh();
        }

        public void SetActivePlus(bool value)
        {
            plusIcon.SetActive(info.IsUnlock() && !info.HasSkill() && value);
        }

        private void Refresh()
        {
            bool isUnlock = info.IsUnlock();
            bool hasSkill = info.HasSkill();
            lockBase.SetActive(!isUnlock);
            unlockBase.SetActive(isUnlock);
            skillInfo.Show(info.SkillInfo);

            bool isInvalidSkill = false; // 사용불가한 스킬인지.

            unequipButton.gameObject.SetActive(hasSkill);

            if (hasSkill)
            {
                // 무기와 스킬이 어울리는지 체크.
                EquipmentClassType weaponType = Entity.player.battleItemInfo.WeaponType;
                isInvalidSkill = info.SkillInfo != null && !info.SkillInfo.IsAvailableWeapon(weaponType);
            }

            skillInfo.SetSkillBlock(isInvalidSkill);
            skillInfo.SetSpriteMode(isInvalidSkill ? UIGraySprite.SpriteMode.Grayscale : UIGraySprite.SpriteMode.None);
        }

        bool IInspectorFinder.Find()
        {
            int.TryParse(name, out slotIndex);
            return true;
        }

        void OnClickUnequip()
        {
            OnUnequipButtonClick?.Invoke(slotIndex);
        }
    }
}