using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class CupetSkillSimpleView : UIInfo<CupetInfoPresenter, CupetEntity>, IAutoInspectorFinder
    {
        [SerializeField] CupetSkillSimpleProfile[] skillProfile;

        public delegate void OnClick(int index);
        public OnClick OnClickEvent { private get; set; } = null;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(skillProfile[0].OnClick, OnClickedBtnSkill0);
            EventDelegate.Add(skillProfile[1].OnClick, OnClickedBtnSkill1);
            EventDelegate.Add(skillProfile[2].OnClick, OnClickedBtnSkill2);
            EventDelegate.Add(skillProfile[3].OnClick, OnClickedBtnSkill3);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(skillProfile[0].OnClick, OnClickedBtnSkill0);
            EventDelegate.Remove(skillProfile[1].OnClick, OnClickedBtnSkill1);
            EventDelegate.Remove(skillProfile[2].OnClick, OnClickedBtnSkill2);
            EventDelegate.Remove(skillProfile[3].OnClick, OnClickedBtnSkill3);
        }

        void OnClickedBtnSkill0() { OnClickedBtnSkill(0); }
        void OnClickedBtnSkill1() { OnClickedBtnSkill(1); }
        void OnClickedBtnSkill2() { OnClickedBtnSkill(2); }
        void OnClickedBtnSkill3() { OnClickedBtnSkill(3); }

        private void OnClickedBtnSkill(int skillIndex)
        {
            if (OnClickEvent == null)
                return;

            if (skillProfile[skillIndex].IsInvalid())
                return;

            OnClickEvent(skillIndex);
        }

        /// <summary>
        /// public Refresh
        /// </summary>
        public void RefreshView()
        {
            Refresh();
        }

        protected override void Refresh()
        {
            if (info == null)
                return;


            CupetSkillInfo cupetSkillInfo = presenter.GetCupetSkillInfo(info.Cupet.CupetID);
            int rank = presenter.GetCurrentCupetRank();
            List<SkillInfo> skillInfos = cupetSkillInfo.GetCupetSkillList(rank, isPreview: true);
            for (int i = 0; i < CupetData.CUPET_SKILL_SIZE; ++i)
            {
                skillProfile[i].SetData(i < skillInfos.Count ? skillInfos[i] : null);

                bool isPreviewSkill = !cupetSkillInfo.HasSkill(i, rank);
                skillProfile[i].SetLock(isPreviewSkill);
            }
        }

        public void SelectSkill(int index)
        {
            for (int i = 0; i < skillProfile.Length; ++i)
            {
                skillProfile[i].SetSelect(i == index);
            }
        }
    }
}