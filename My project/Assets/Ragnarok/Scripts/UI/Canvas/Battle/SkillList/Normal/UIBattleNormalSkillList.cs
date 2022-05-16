using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleNormalSkillList : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIBattleNormalSkillSlot[] slots;

        public event System.Action<SkillInfo, UIBattleNormalSkillSlot.SlotType> OnSelect;

        protected override void OnInit()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].OnSelect += OnSelectSkill;
            }
        }

        protected override void OnClose()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].OnSelect -= OnSelectSkill;
            }
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnSelectSkill(SkillInfo info, UIBattleNormalSkillSlot.SlotType slotType)
        {
            OnSelect?.Invoke(info, slotType);
        }

        public void SetCharacter(CharacterEntity entity)
        {
            int skillSlotCount = entity.Skill.SkillSlotCount;
            for (int i = 0; i < slots.Length; i++)
            {
                SkillModel.ISlotValue slotData = entity.Skill.GetSlotInfo(i);
                slots[i].SetLock(i >= skillSlotCount);
                slots[i].SetWeaponType(entity.battleItemInfo.WeaponType);
                slots[i].SetData(slotData == null ? null : entity.Skill.GetSkill(slotData.SkillNo, isBattleSkill: true));
            }
        }

        public override bool Find()
        {
            base.Find();

            slots = GetComponentsInChildren<UIBattleNormalSkillSlot>();
            return true;
        }
    }
}