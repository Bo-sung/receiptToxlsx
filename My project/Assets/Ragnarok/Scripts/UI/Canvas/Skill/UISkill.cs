using Ragnarok.View;
using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISkill : UICanvas, ISkillCanvas, TutorialSkillLearn.ISkillImpl
    {
        public enum SkillSelectType
        {
            /// <summary>
            /// 스킬 훔쳐 배우기
            /// </summary>
            Steal = 1,

            /// <summary>
            /// 서몬 볼
            /// </summary>
            SummonBall,

            /// <summary>
            /// 룬 마스터리
            /// </summary>
            RuneMastery,
        }

        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] SkillModelView skillModelView;
        [SerializeField] SkillListView skillListView;
        [SerializeField] SkillInfoView skillInfoView;
        [SerializeField] SkillEquipView skillEquipView;
        [SerializeField] SkillUnselectView skillUnselectView;

        SkillPresenter presenter;

        private int selectedSkillId;
        private int selectedSkillSlotIndex; // 인덱스는 1부터 시작 (0일 경우 선택한 슬롯이 없는 경우)

        protected override void OnInit()
        {
            presenter = new SkillPresenter(this);

            skillModelView.AddListener(this);
            skillListView.AddListener(this);
            skillInfoView.AddListener(this);
            skillEquipView.AddListener(this);
            skillUnselectView.AddListener(this);

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            skillModelView.RemoveListener(this);
            skillListView.RemoveListener(this);
            skillInfoView.RemoveListener(this);
            skillEquipView.RemoveListener(this);
            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_Skill(); // 신규 컨텐츠 플래그 제거
            Refresh();
        }

        protected override void OnHide()
        {
            selectedSkillId = 0;
            selectedSkillSlotIndex = 0;

            presenter.RemoveHasNewSkillPoint(); // 새로운 스킬 포인트 존재 여부 플래그 제거
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._39000.ToText()); // 스킬
        }

        /// <summary>
        /// 새로고침
        /// </summary>
        public void Refresh()
        {
            UpdateZeny(presenter.GetZeny());
            UpdateCatCoin(presenter.GetCatCoin());
            UpdateJobLevel(presenter.GetJobLevel());

            skillModelView.ShowJob(presenter.GetJobIcon(), presenter.GetJobName());
            skillModelView.ShowSkillPoint(presenter.GetSkillPoint());
            skillModelView.SetHasLevelUpSkill(presenter.HasLevelUpSkill());

            skillListView.Show(presenter.GetSkillInfos());
            skillEquipView.Show(presenter.GetSkillEquipInfos());

            skillListView.SetSelect(selectedSkillId);
            skillInfoView.SetSkillPoint(presenter.GetSkillPoint());
            skillInfoView.Show(presenter.GetSkillInfo(selectedSkillId));

            skillListView.ResetParent();
            skillListView.SetDraggable(true);
            skillUnselectView.SetActive(false);

            skillEquipView.SetActivePlus(selectedSkillId != 0);
            skillEquipView.SetPassiveLock(IsSelectPassiveSkill());
            UpdateHasNewSkillPoint();
        }

        /// <summary>
        /// 스킬 선택 해제 (스킬 훔쳐 배우기로 인하여 id가 변경될 수 있음)
        /// </summary>
        public void UnSelectSkill()
        {
            selectedSkillId = 0; // 선택한 스킬 초기화
            Refresh();
        }

        /// <summary>
        /// 제니 업데이트
        /// </summary>
        public void UpdateZeny(long zeny)
        {
            titleView.ShowZeny(zeny);
        }

        /// <summary>
        /// 캣코인 업데이트
        /// </summary>
        public void UpdateCatCoin(long catCoin)
        {
            titleView.ShowCatCoin(catCoin);
        }

        /// <summary>
        /// 직업 레벨 업데이트
        /// </summary>
        public void UpdateJobLevel(int jobLevel)
        {
            skillModelView.ShowJobLevel(presenter.GetJobLevel());
        }

        /// <summary>
        /// 새로운 스킬포인트 존재 여부 업데이트
        /// </summary>
        public void UpdateHasNewSkillPoint()
        {
            //skillModelView.SetHasNewSkillPoint(presenter.GetSkillPoint() > 0);
            skillModelView.SetHasNewSkillPoint(presenter.HasNewSkillPoint());
        }

        void SkillModelView.IListener.OnInitSkillPoint()
        {
            presenter.RequestSkillInitialize();
        }

        void SkillListView.IListener.OnSelect(int skillId)
        {
            selectedSkillId = skillId;
            Refresh();
        }

        void SkillListView.IListener.OnUnselect()
        {
            UnSelectSkill();
        }

        void SkillInfoView.IListener.OnLevelUp(int skillId, int plusLevel)
        {
            presenter.RequestSkillLevelUp(skillId, plusLevel);
        }

        void SkillInfoView.IListener.OnSelectChangeSkill(int skillId)
        {
            SkillSelectType selectType = presenter.GetSelectType(skillId);

            switch (selectType)
            {
                case SkillSelectType.Steal:
                    UI.Show<UIStealSkillSelect>(new UIStealSkillSelect.Input(skillId, OnChangeSkill));
                    break;

                case SkillSelectType.SummonBall:
                    UI.Show<UISkillSelect>(new UISkillSelect.Input(skillId, OnChangeSkill));
                    break;
            }
        }

        void SkillEquipView.IListener.OnUnlock()
        {
            presenter.RequestBuySkillSlot();
        }

        void SkillEquipView.IListener.OnEmptySlotClick(int slotIndex)
        {
            if (selectedSkillId != 0)
            {
                var skill = presenter.GetSkillInfo(selectedSkillId);

                if (skill.GetSkillLevel() > 0)
                {
                    if (skill.GetSkillData(skill.GetSkillLevel()).SkillType != SkillType.Passive)
                    {
                        presenter.RequestEquipSkillSlot(slotIndex, selectedSkillId);
                        selectedSkillId = 0;
                        Refresh();
                    }
                    else
                    {
                        UI.ShowToastPopup(LocalizeKey._39020.ToText()); // 패시브 스킬은 장착할 수 없습니다.
                    }
                }
                else
                {
                    UI.ShowToastPopup(LocalizeKey._39021.ToText()); // 레벨이 0인 스킬은 장착할 수 없습니다.
                }
            }
        }

        void SkillEquipView.IListener.OnEquippedSlotClick(int slotIndex)
        {
            if (selectedSkillId != 0)
            {
                if (presenter.GetSkillIDInSlot(slotIndex) == selectedSkillId)
                    return;

                var skill = presenter.GetSkillInfo(selectedSkillId);

                if (skill.GetSkillLevel() > 0)
                {
                    if (skill.GetSkillData(skill.GetSkillLevel()).SkillType != SkillType.Passive)
                    {
                        presenter.RequestEquipSkillSlot(slotIndex, selectedSkillId);
                        selectedSkillId = 0;
                        Refresh();
                    }
                    else
                    {
                        UI.ShowToastPopup(LocalizeKey._39020.ToText()); // 패시브 스킬은 장착할 수 없습니다.
                    }
                }
                else
                {
                    UI.ShowToastPopup(LocalizeKey._39021.ToText()); // 레벨이 0인 스킬은 장착할 수 없습니다.
                }
            }
            else
            {
                selectedSkillId = presenter.GetSkillIDInSlot(slotIndex);
                Refresh();
            }
        }

        void SkillEquipView.IListener.OnUnequipButtonClick(int slotIndex)
        {
            presenter.RequestUnequipSkillSlot(slotIndex);
        }

        void SkillUnselectView.IListener.OnUnselect()
        {
            selectedSkillSlotIndex = 0;
            Refresh();
        }

        private void OnChangeSkill(int changeSkillId)
        {
            presenter.RequestChangeSkill(selectedSkillId, changeSkillId);
        }

        private bool IsSelectPassiveSkill()
        {
            if (selectedSkillId == 0)
                return false;

            var skill = presenter.GetSkillInfo(selectedSkillId);
            if (skill == null)
                return false;

            int skillLevel = skill.GetSkillLevel() == 0 ? 1 : skill.GetSkillLevel();
            SkillData.ISkillData skillData = skill.GetSkillData(skillLevel);
            if (skillData == null)
                return false;

            return skillData.SkillType == SkillType.Passive;
        }

        protected override void OnBack()
        {
            // 선택한 스킬 슬롯 존재
            if (selectedSkillSlotIndex > 0)
            {
                selectedSkillSlotIndex = 0;
                Refresh();
                return;
            }

            // 선택한 스킬 존재
            if (selectedSkillId > 0)
            {
                selectedSkillId = 0;
                Refresh();
                return;
            }

            base.OnBack();
        }

        private void HideUI()
        {
            UI.Close<UISkill>();
        }

        #region Tutorial
        TutorialSkillLearn.ISelectActiveSkillImpl TutorialSkillLearn.ISkillImpl.GetSelectActiveSkillImpl()
        {
            return skillListView;
        }

        TutorialSkillLearn.ILevelUpSkillImpl TutorialSkillLearn.ISkillImpl.GetLevelUpSkillImpl()
        {
            return skillInfoView;
        }

        TutorialSkillEquip.IEquipSkillImpl TutorialSkillLearn.ISkillImpl.GetEquipSkillImpl()
        {
            return skillEquipView;
        }
        #endregion
    }
}