using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICentralLabSkill : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UISkillInfoToggleSimple[] skills;
        [SerializeField] UISkillLevel selectedSkillLevel;
        [SerializeField] UILabelHelper labelSkillName, labelSkillDescription;
        [SerializeField] UILabelHelper labelSkillList;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UIButtonHelper btnSelect;

        CentralLabSkillPresenter presenter;

        public event System.Action<int> OnSelect;

        private UISkillLevelInfoSelect.IInfo[] selectSkills;
        private UISkillLevelInfoSelect.IInfo[] hasSkills;
        private int skillId;
        private int skillLevel;

        protected override void OnInit()
        {
            presenter = new CentralLabSkillPresenter();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            foreach (var item in skills)
            {
                item.OnSelect += SelectSkill;
            }

            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            foreach (var item in skills)
            {
                item.OnSelect -= SelectSkill;
            }

            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
            selectSkills = null;
            hasSkills = null;
            skillId = 0;
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._48311; // 포링의 선물
            labelDescription.LocalKey = LocalizeKey._48312; // 스킬을 선택하세요.
            labelSkillList.LocalKey = LocalizeKey._48313; // 현재 보유 스킬
            labelNoData.LocalKey = LocalizeKey._48314; // 보유한 스킬이 없습니다.
            btnSelect.LocalKey = LocalizeKey._48315; // 선택하기

            UpdateSkillText();
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UISkillLevelInfoSelect ui = go.GetComponent<UISkillLevelInfoSelect>();
            ui.Show(hasSkills[index]);
        }

        void OnClickedBtnSelect()
        {
            OnSelect?.Invoke(skillId);
            HideUI();
        }

        public void Show(UISkillLevelInfoSelect.IInfo[] selectSkills, UISkillLevelInfoSelect.IInfo[] hasSkills)
        {
            this.selectSkills = selectSkills;
            this.hasSkills = hasSkills;

            Show();

            if (this.selectSkills == null)
                return;

            int selectSkillCount = this.selectSkills.Length;
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i].Show(i < selectSkillCount ? this.selectSkills[i] : null);
            }

            int hasSkillCount = this.hasSkills == null ? 0 : this.hasSkills.Length;
            wrapper.Resize(hasSkillCount);
            labelNoData.SetActive(hasSkillCount == 0);

            SelectFirstSkill(); // 첫번째 스킬 선택
        }

        private void SelectFirstSkill()
        {
            int firstSkillId = selectSkills.Length > 0 ? selectSkills[0].SkillId : 0;
            SelectSkill(firstSkillId);
        }

        private void SelectSkill(int skillId)
        {
            this.skillId = skillId;
            UISkillLevelInfoSelect.IInfo find = System.Array.Find(selectSkills, a => a.SkillId == this.skillId);
            skillLevel = find == null ? 0 : find.SkillLevel;

            selectedSkillLevel.SetData(find);

            // Toggle Refresh
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i].SetSelect(this.skillId);
            }

            UpdateSkillText();
        }

        private void UpdateSkillText()
        {
            labelSkillName.LocalKey = presenter.GetSkillNameId(skillId, skillLevel);
            labelSkillDescription.LocalKey = presenter.GetSkillDescriptionId(skillId, skillLevel);
        }

        private void HideUI()
        {
            Hide();
        }

        public override bool Find()
        {
            base.Find();

            skills = GetComponentsInChildren<UISkillInfoToggleSimple>();
            return true;
        }
    }
}