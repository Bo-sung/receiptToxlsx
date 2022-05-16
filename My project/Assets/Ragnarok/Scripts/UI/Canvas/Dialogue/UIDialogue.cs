using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDialogue : UICanvas
    {
        private const string TAG = nameof(UIDialogue);

        public enum Pivot
        {
            Left = 1,
            Center,
            Right,
        }

        public enum Talker
        {
            Character = 1,
            Deviruchi,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIButton btnNext;
        [SerializeField] UITutorialDialog tutorialDialogView;
        [SerializeField] UIButtonHelper btnSkip;
        [SerializeField] UILabelHelper labelNext;

        DialoguePresenter presenter;

        private bool isNextStep;
        private bool isHide;

        protected override void OnInit()
        {
            presenter = new DialoguePresenter();

            EventDelegate.Add(btnNext.onClick, OnClickedBtnNext);
            EventDelegate.Add(btnSkip.OnClick, OnClickedBtnSkip);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnNext.onClick, OnClickedBtnNext);
            EventDelegate.Remove(btnSkip.OnClick, OnClickedBtnSkip);

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            SetActiveBtnSkip(false);
            tutorialDialogView.Hide();

            isHide = false;
        }

        protected override void OnHide()
        {
            isNextStep = false;
            isHide = true;

            SetActiveBtnSkip(false);
            tutorialDialogView.Hide();
        }

        protected override void OnLocalize()
        {
            btnSkip.LocalKey = LocalizeKey._22100; // SKIP
            labelNext.LocalKey = LocalizeKey._22101; // NEXT
        }

        void OnClickedBtnNext()
        {
            if (tutorialDialogView.isActiveAndEnabled && tutorialDialogView.Finish())
                return;

            isNextStep = true;
        }

        void OnClickedBtnSkip()
        {
            Hide();
        }

        public void SetActiveBtnSkip(bool isActive)
        {
            btnSkip.SetActive(isActive);
        }

        public void Show(Npc npc, string dialog)
        {
            Show(npc.imageName, npc.nameLocalKey.ToText(), dialog);
        }

        public void Show(string spriteName, string name, string dialog)
        {
            tutorialDialogView.SetNpc(name, spriteName);
            tutorialDialogView.Show(dialog, UIWidget.Pivot.Center);
        }

        public float UntilShow(Npc npc, string dialog)
        {
            return UntilShow(npc.imageName, npc.nameLocalKey.ToText(), dialog);
        }

        public float UntilShow(string spriteName, string name, string dialog)
        {
            Show(spriteName, name, dialog);
            return Timing.WaitUntilTrue(IsNextStep);
        }

        public float UntilShow(Npc npc, string[] dialogs)
        {
            return UntilShow(npc.imageName, npc.nameLocalKey.ToText(), dialogs);
        }

        public float UntilShow(string spriteName, string name, string[] dialogs)
        {
            tutorialDialogView.SetNpc(name, spriteName);
            return Timing.WaitUntilDone(ShowTutorial(dialogs), TAG);
        }

        private IEnumerator<float> ShowTutorial(string[] dialogs)
        {
            foreach (var dialog in dialogs)
            {
                tutorialDialogView.Show(dialog, UIWidget.Pivot.Center);
                yield return Timing.WaitUntilTrue(IsNextStep);
            }
        }

        private bool IsNextStep()
        {
            // 꺼져있을 경우에는 완료
            if (isHide)
                return true;

            if (isNextStep)
            {
                isNextStep = false;
                return true;
            }

            return false;
        }
    }
}