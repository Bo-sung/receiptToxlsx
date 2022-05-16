using Ragnarok.View;
using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISkillTooltip : UICanvas, ISkillTooltipCanvas
    {
        public class Input : IUIData
        {
            public readonly int skillId;
            public readonly int skillLevel;

            public Input(int skillId, int skillLevel)
            {
                this.skillId = skillId;
                this.skillLevel = skillLevel;
            }
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] SkillTooltipView skillTooltipView;

        SkillTooltipPresenter presenter;
        private int skillId;
        private int skillLevel;

        protected override void OnInit()
        {
            presenter = new SkillTooltipPresenter(this);

            popupView.OnExit += OnExit;
            popupView.OnConfirm += OnConfirm;

            skillTooltipView.AddListener(this);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            popupView.OnExit -= OnExit;
            popupView.OnConfirm -= OnConfirm;

            skillTooltipView.RemoveListener(this);

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is Input input)
            {
                skillId = input.skillId;
                skillLevel = input.skillLevel;
            }

            popupView.MainTitleLocalKey = LocalizeKey._39200; // 스킬 정보
            popupView.ConfirmLocalKey = LocalizeKey._39201; // 닫기

            Refresh();
        }

        protected override void OnHide()
        {
            skillId = 0;
            skillLevel = 0;
        }

        protected override void OnLocalize()
        {
        }

        public void Refresh()
        {
            ISkillViewInfo skillInfos = presenter.GetSkill(skillId, skillLevel);
            skillTooltipView.Show(skillInfos); // 첫 번째 OnSelect 알아서 호출
        }

        void OnConfirm()
        {
            OnBack();
        }

        void OnExit()
        {
            OnBack();
        }
    }
}