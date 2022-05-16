using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICentralLabPause : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        public enum SelectResult
        {
            Resume = 1,
            Exit,
        }

        [SerializeField] UIButton blind;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UISkillLevelInfoSelect[] skills;
        [SerializeField] UIButtonHelper btnExit, btnContinue;

        private TaskAwaiter<SelectResult> awaiter;
        private SelectResult result;

        protected override void OnInit()
        {
            EventDelegate.Add(blind.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnContinue.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(blind.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(btnContinue.OnClick, CloseUI);

            Complete(CloseUIException.Default); // UI 강제 닫기
        }

        protected override void OnShow(IUIData data = null)
        {
            result = SelectResult.Resume;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._48319; // 현재 보유 스킬
            btnExit.LocalKey = LocalizeKey._48320; // 나가기
            btnContinue.LocalKey = LocalizeKey._48321; // 계속하기
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Complete(DestroyUIException.Default); // UI 강제 제거
        }

        public TaskAwaiter<SelectResult> Show(UISkillLevelInfoSelect.IInfo[] arrData)
        {
            Complete(DuplicateUIException.Default); // UI 중복

            Show();
            awaiter = new TaskAwaiter<SelectResult>();

            int dataCount = arrData == null ? 0 : arrData.Length;
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i].Show(i < dataCount ? arrData[i] : null);
            }

            return awaiter;
        }

        void OnClickedBtnExit()
        {
            result = SelectResult.Exit;
            CloseUI();
        }

        private void CloseUI()
        {
            Complete(null);
            UI.Close<UICentralLabPause>();
        }

        private void Complete(UIException exception)
        {
            // Awaiter 음슴
            if (awaiter == null)
                return;

            if (!awaiter.IsCompleted)
                awaiter.Complete(result, exception);

            awaiter = null;
        }

        protected override void OnBack()
        {
            CloseUI();
        }

        public override bool Find()
        {
            base.Find();

            skills = GetComponentsInChildren<UISkillLevelInfoSelect>();
            return true;
        }
    }
}