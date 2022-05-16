using Ragnarok.View;
using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISkillSelect : UICanvas, ISkillSelectCanvas
    {
        public class Input : IUIData
        {
            public readonly int skillId;
            public readonly System.Action<int> onConfirm; // 확인 이벤트

            public Input(int skillId, System.Action<int> onConfirm)
            {
                this.skillId = skillId;
                this.onConfirm = onConfirm;
            }
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] SkillSelectView skillSelectView;
        [SerializeField] SkillInfoPreview skillInfoPreview;

        SkillSelectPresenter presenter;

        private System.Action<int> onConfirm;
        private int selectedSkillId;

        protected override void OnInit()
        {
            presenter = new SkillSelectPresenter(this);

            popupView.OnExit += OnExit;
            popupView.OnConfirm += OnConfirm;

            skillSelectView.AddListener(this);
            skillInfoPreview.AddListener(this);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            popupView.OnExit -= OnExit;
            popupView.OnConfirm -= OnConfirm;

            skillSelectView.RemoveListener(this);
            skillInfoPreview.RemoveListener(this);

            presenter.RemoveEvent();
            presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is Input input)
            {
                UISkillInfoSelect.IInfo[] skillInfos = presenter.GetSkills(input.skillId);
                skillSelectView.Show(skillInfos); // 첫 번째 OnSelect 알아서 호출
                onConfirm = input.onConfirm;
            }

            popupView.MainTitleLocalKey = LocalizeKey._39100; // 스킬 선택
            popupView.ConfirmLocalKey = LocalizeKey._39101; // 결정
            skillSelectView.NoticeLocalKey = LocalizeKey._39104; // 원하는 스킬을 선택하세요.
        }

        protected override void OnHide()
        {
            onConfirm = null;
            selectedSkillId = 0;
        }

        protected override void OnLocalize()
        {
        }

        public void Refresh()
        {
            skillSelectView.SetSelect(selectedSkillId);
            skillInfoPreview.Show(presenter.GetSkillInfo(selectedSkillId));
        }

        void OnConfirm()
        {
            onConfirm?.Invoke(selectedSkillId);
        }

        void OnExit()
        {
            OnBack();
        }

        void SkillSelectView.IListener.OnSelect(int skillId)
        {
            selectedSkillId = skillId;
            Refresh();
        }
    }
}