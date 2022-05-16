using UnityEngine;

namespace Ragnarok
{
    public class UIBattleSkillList : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIBattleSkillSlot[] slots;
        [SerializeField] UIButtonHelper btnAuto;
        [SerializeField] TweenRotation tweenIcon;
        [SerializeField] UIButtonHelper btnSkill;
        [SerializeField] UISprite mpBar;
        [SerializeField] UILabel mpLabel;

        public event System.Action<SkillInfo, UIBattleNormalSkillSlot.SlotType> OnSelect;
        public event System.Action OnAuto;
        public event System.Action OnToggleSkill;

        protected CharacterEntity entity;

        protected override void OnInit()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].OnSelect += OnSelectSkill;
            }

            if (btnAuto)
                EventDelegate.Add(btnAuto.OnClick, OnClickedBtnAuto);

            if (btnSkill)
                EventDelegate.Add(btnSkill.OnClick, OnClickedBtnSkill);
        }

        protected override void OnClose()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].OnSelect -= OnSelectSkill;
            }

            if (btnAuto)
                EventDelegate.Remove(btnAuto.OnClick, OnClickedBtnAuto);

            if (btnSkill)
                EventDelegate.Remove(btnSkill.OnClick, OnClickedBtnSkill);

            RemoveEvent();
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

        private void AddEvent()
        {
            if (entity == null)
                return;

            entity.OnChangeMP += UpdateMP;
            entity.OnReloadStatus += RefreshSkill;

            UnitActor actor = entity.GetActor();
            if (actor == null)
            {
                entity.OnSpawnActor += AddActorEvent;
            }
            else
            {
                AddActorEvent(actor);
            }

            LastAddEvent();
        }

        protected virtual void LastAddEvent()
        {
        }

        private void RemoveEvent()
        {
            if (entity == null)
                return;

            entity.OnChangeMP -= UpdateMP;
            entity.OnReloadStatus -= RefreshSkill; // 중간에 스탯이 변하면서 스킬이 초월될 수 있다.

            UnitActor actor = entity.GetActor();
            if (actor == null)
            {
                entity.OnSpawnActor -= AddActorEvent;
            }
            else
            {
                RemoveActorEvent(actor);
            }

            LastRemoveEvent();

            entity = null;
        }

        protected virtual void LastRemoveEvent()
        {
        }

        private void AddActorEvent(UnitActor actor)
        {
            actor.Entity.OnSpawnActor -= AddActorEvent;
            actor.AI.externalInput.OnSetSkill += OnSetSkill;
        }

        private void RemoveActorEvent(UnitActor actor)
        {
            actor.AI.externalInput.OnSetSkill -= OnSetSkill;
        }

        void OnSelectSkill(SkillInfo info, UIBattleNormalSkillSlot.SlotType slotType)
        {
            OnSelect?.Invoke(info, slotType);
        }

        void OnClickedBtnAuto()
        {
            OnAuto?.Invoke();
        }

        void OnClickedBtnSkill()
        {
            OnToggleSkill?.Invoke();
        }

        public virtual void SetCharacter(CharacterEntity entity)
        {
            RemoveEvent();
            this.entity = entity;
            AddEvent();

            RefreshSkill();

            PlayTweenIcon(entity.Skill.IsAutoSkill);
            UpdateMP(entity.CurMp, entity.MaxMp);
        }

        /// <summary>
        /// 스킬 예약 이벤트
        /// </summary>
        private void OnSetSkill(SkillInfo skillInfo)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].RefreshSkillInput(skillInfo);
            }
        }

        public void SetActivePlus(bool isActive)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].GetSlotType() == UIBattleNormalSkillSlot.SlotType.Empty)
                {
                    slots[i].SetActivePlus(isActive);
                }
                else
                {
                    slots[i].SetActivePlus(false);
                }
            }
        }

        private void PlayTweenIcon(bool isPlay)
        {
            if (tweenIcon)
            {
                tweenIcon.value = Quaternion.identity;
                tweenIcon.enabled = isPlay;
            }
        }

        private void UpdateMP(int current, int max)
        {
            float progress = MathUtils.GetProgress(current, max);

            mpBar.fillAmount = progress;
            mpLabel.text = MathUtils.GetPercentText(progress);
        }

        private void RefreshSkill()
        {
            if (entity == null)
                return;

            int skillSlotCount = entity.Skill.SkillSlotCount;
            for (int i = 0; i < slots.Length; i++)
            {
                SkillModel.ISlotValue slotData = entity.Skill.GetSlotInfo(i);
                slots[i].SetDie(entity.IsDie);
                slots[i].SetLock(i >= skillSlotCount);
                slots[i].SetWeaponType(entity.battleItemInfo.WeaponType);
                slots[i].SetData(slotData == null ? null : entity.Skill.GetSkill(slotData.SkillNo, isBattleSkill: true));
            }
        }

        public override bool Find()
        {
            base.Find();

            slots = GetComponentsInChildren<UIBattleSkillSlot>();
            tweenIcon = GetComponentInChildren<TweenRotation>();
            return true;
        }
    }
}