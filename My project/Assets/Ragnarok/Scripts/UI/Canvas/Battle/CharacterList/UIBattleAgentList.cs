using Ragnarok.View.BattleStage;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleAgentList : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIBattleStageAgentSlot[] agnetSlots;
        [SerializeField] UIButtonWithToggleAdvanced btnFollowBinding;
        [SerializeField] UIButtonWithToggleAdvanced btnAutoSkill;

        public event System.Action<CharacterEntity> OnSelectAgent;
        public event System.Action<bool> OnSelectMoveBind;
        public event System.Action<bool> OnSelectSkillAuto;

        public bool IsFollowBinding { get; private set; } = false;
        public bool IsAutoSkill { get; private set; } = false;

        protected override void OnInit()
        {
            foreach (var item in agnetSlots)
            {
                item.OnSelect += OnSelectAgentSlot;
            }

            EventDelegate.Add(btnFollowBinding.OnClick, OnClickedBtnFollowBinding);
            EventDelegate.Add(btnAutoSkill.OnClick, OnClickedBtnAutoSkill);
        }

        protected override void OnClose()
        {
            foreach (var item in agnetSlots)
            {
                item.OnSelect -= OnSelectAgentSlot;
            }

            EventDelegate.Remove(btnFollowBinding.OnClick, OnClickedBtnFollowBinding);
            EventDelegate.Remove(btnAutoSkill.OnClick, OnClickedBtnAutoSkill);
        }

        protected override void OnShow(IUIData data = null)
        {
            RefreshBtnFollowBinding();
            RefreshBtnAutoSkill();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnFollowBinding.LocalKey = LocalizeKey._39400; // 이동
            btnFollowBinding.OffLocalKey = LocalizeKey._39400; // 이동
            btnAutoSkill.LocalKey = LocalizeKey._39401; // AUTO
            btnAutoSkill.OffLocalKey = LocalizeKey._39401; // AUTO
        }

        void OnSelectAgentSlot(CharacterEntity characterEntity)
        {
            OnSelectAgent?.Invoke(characterEntity);
        }

        void OnClickedBtnFollowBinding()
        {
            IsFollowBinding = !IsFollowBinding;
            RefreshBtnFollowBinding();

            OnSelectMoveBind?.Invoke(IsFollowBinding);
        }

        void OnClickedBtnAutoSkill()
        {
            IsAutoSkill = !IsAutoSkill;
            RefreshBtnAutoSkill();

            OnSelectSkillAuto?.Invoke(IsAutoSkill);
        }

        public void SetCharacters(CharacterEntity[] array)
        {
            int size = array == null ? 0 : array.Length;
            for (int i = 0; i < agnetSlots.Length; i++)
            {
                agnetSlots[i].SetData(i < size ? array[i] : null);
            }
        }

        public void SetSelect(int index)
        {
            for (int i = 0; i < agnetSlots.Length; i++)
            {
                agnetSlots[i].SetSelect(i == index);
            }
        }

        private void RefreshBtnFollowBinding()
        {
            btnFollowBinding.SetToggle(IsFollowBinding);
        }

        private void RefreshBtnAutoSkill()
        {
            btnAutoSkill.SetToggle(IsAutoSkill);
        }

        public override bool Find()
        {
            base.Find();

            agnetSlots = GetComponentsInChildren<UIBattleStageAgentSlot>();
            return true;
        }
    }
}