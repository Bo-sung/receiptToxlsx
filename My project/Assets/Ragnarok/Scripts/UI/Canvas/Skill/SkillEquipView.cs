using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class SkillEquipView : UIView<SkillEquipView.IListener>, IInspectorFinder, TutorialSkillEquip.IEquipSkillImpl
    {
        public interface IListener
        {
            void OnUnlock();
            void OnEmptySlotClick(int slotIndex);
            void OnEquippedSlotClick(int slotIndex);
            void OnUnequipButtonClick(int slotIndex);
        }

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UISkillPreset[] presets;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] GameObject goPassiveLock;

        protected override void Awake()
        {
            base.Awake();

            foreach (var item in presets)
            {
                item.OnUnlock += OnUnlock;
                item.OnEmptySlotClick += OnEmptySlotClick;
                item.OnEquippedSlotClick += OnEquippedSlotClick;
                item.OnUnequipButtonClick += OnUnequipButtonClick;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in presets)
            {
                item.OnUnlock -= OnUnlock;
                item.OnEmptySlotClick -= OnEmptySlotClick;
                item.OnEquippedSlotClick -= OnEquippedSlotClick;
                item.OnUnequipButtonClick -= OnUnequipButtonClick;
            }
        }

        private void OnUnequipButtonClick(int slotIndex)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnUnequipButtonClick(slotIndex);
            }
        }

        void OnUnlock()
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnUnlock();
            }
        }

        void OnEmptySlotClick(int slotIndex)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnEmptySlotClick(slotIndex);
            }

            isSelectedSkillSlot = true;
        }

        void OnEquippedSlotClick(int slotIndex)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnEquippedSlotClick(slotIndex);
            }

            isSelectedSkillSlot = true;
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._39017; // 스킬 프리셋
            labelNotice.LocalKey = LocalizeKey._39018; // 등록하거나 해제할 스킬 슬롯을 선택하세요
        }

        public void Show(UISkillPreset.IInfo[] equipInfos)
        {
            for (int i = 0; i < presets.Length; i++)
            {
                presets[i].Show(i < equipInfos.Length ? equipInfos[i] : null);
            }
        }

        public void SetActivePlus(bool value)
        {
            for (int i = 0; i < presets.Length; i++)
                presets[i].SetActivePlus(value);
        }

        public void SetPassiveLock(bool value)
        {
            NGUITools.SetActive(goPassiveLock, value);
        }

        bool IInspectorFinder.Find()
        {
            presets = GetComponentsInChildren<UISkillPreset>();
            return true;
        }

        #region Tutorial
        bool isSelectedSkillSlot;

        UIWidget TutorialSkillEquip.IEquipSkillImpl.GetSkillSlot()
        {
            return presets[0].GetComponent<UIWidget>();
        }

        bool TutorialSkillEquip.IEquipSkillImpl.IsSelectedSkillSlot()
        {
            if (isSelectedSkillSlot)
            {
                isSelectedSkillSlot = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}