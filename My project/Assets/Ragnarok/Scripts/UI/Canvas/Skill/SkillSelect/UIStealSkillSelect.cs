using Ragnarok.View;
using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIStealSkillSelect : UICanvas, IStealSkillSelectCanvas
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
        [SerializeField] JobSelectView jobSelectView;
        [SerializeField] SkillSelectView skillSelectView;
        [SerializeField] SkillInfoPreview skillInfoPreview;

        StealSkillSelectPresenter presenter;

        private System.Action<int> onConfirm;
        private int selectedSkillId;

        protected override void OnInit()
        {
            presenter = new StealSkillSelectPresenter(this);

            popupView.OnExit += OnExit;
            popupView.OnConfirm += OnConfirm;

            jobSelectView.AddListener(this);
            skillSelectView.AddListener(this);
            skillInfoPreview.AddListener(this);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            popupView.OnExit -= OnExit;
            popupView.OnConfirm -= OnConfirm;

            jobSelectView.RemoveListener(this);
            skillSelectView.RemoveListener(this);
            skillInfoPreview.RemoveListener(this);

            presenter.RemoveEvent();
            presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            jobSelectView.ClearItem();
            if (data is Input input)
            {
                JobSelectView.IInfo[] jobInfos = presenter.GetJobInfos(input.skillId);
                jobSelectView.AddItems(jobInfos);
                onConfirm = input.onConfirm;
            }

            popupView.MainTitleLocalKey = LocalizeKey._39100; // 스킬 선택
            popupView.ConfirmLocalKey = LocalizeKey._39101; // 결정
            skillSelectView.NoticeLocalKey = LocalizeKey._39103; // 훔칠 스킬을 선택하세요.

            Refresh();
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

        void JobSelectView.IListener.OnSelect(Job job)
        {
            skillSelectView.Show(presenter.GetSkills(job)); // 첫 번째 OnSelect 알아서 호출
        }

        void SkillSelectView.IListener.OnSelect(int skillId)
        {
            selectedSkillId = skillId;
            Refresh();
        }
    }
}