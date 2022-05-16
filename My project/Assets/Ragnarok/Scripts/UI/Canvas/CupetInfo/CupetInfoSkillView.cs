using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public class CupetInfoSkillView : UISubCanvas<CupetInfoPresenter>, IAutoInspectorFinder
    {
        [SerializeField] CupetSkillSimpleView cupetSkillSimpleView;
        [SerializeField] CupetSkillInfoView skillInfoView;

        protected override void OnInit()
        {
            cupetSkillSimpleView.OnClickEvent = OnClickedBtnSkill;
        }

        protected override void OnHide()
        {

        }

        protected override void OnClose()
        {

        }

        protected override void OnLocalize()
        {

        }

        protected override void OnShow()
        {
            cupetSkillSimpleView.SetData(presenter, presenter.Cupet);
            presenter.SkillViewSelectSkill(0);
            Refresh();
        }


        public void Refresh()
        {
            cupetSkillSimpleView.RefreshView();
            skillInfoView.Refresh();
        }

        /// <summary>
        /// 스킬 선택 이벤트
        /// </summary>
        void OnClickedBtnSkill(int index)
        {
            presenter.SkillViewSelectSkill(index);
        }

        /// <summary>
        /// 해당 스킬을 선택처리하고 해당 스킬의 정보를 출력.
        /// </summary>
        /// <param name="index"></param>
        public void SelectSkill(int index)
        {
            cupetSkillSimpleView.SelectSkill(index);
            ISkillViewInfo skillInfo = presenter.GetSkillInfo(index);
            skillInfoView.Show(skillInfo);
        }
    }
}