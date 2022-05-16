using UnityEngine;

namespace Ragnarok
{
    public sealed class UITutorial : UICanvas
    {
        public enum ClickType
        {
            /// <summary>
            /// 어떤 곳을 터치하여도 상관 없음
            /// </summary>
            All,

            /// <summary>
            /// Mask 된 부분만 클릭 가능
            /// </summary>
            OnlyMask,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] GameObject blind;
        [SerializeField] UIButton btnNext;
        [SerializeField] UITutorialDialog tutorialDialog;
        [SerializeField] UITutorialTouch tutorialTouch;
        [SerializeField] UIButtonHelper btnSkip;
        [SerializeField] UILabelHelper labelNext;

        bool isNextStep;
        bool isEmpty;

        protected override void OnInit()
        {
            EventDelegate.Add(btnNext.onClick, OnClickedBtnNext);
            EventDelegate.Add(btnSkip.OnClick, OnClickedBtnSkip);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnNext.onClick, OnClickedBtnNext);
            EventDelegate.Remove(btnSkip.OnClick, OnClickedBtnSkip);
        }

        protected override void OnShow(IUIData data = null)
        {
            ShowEmpty();
        }

        protected override void OnHide()
        {
            isNextStep = false;
        }

        protected override void OnLocalize()
        {
            btnSkip.LocalKey = LocalizeKey._26157; // SKIP
            labelNext.LocalKey = LocalizeKey._26158; // NEXT
        }

        void OnClickedBtnNext()
        {
            if (isEmpty)
            {
                UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
                return;
            }

            if (tutorialDialog.Finish())
                return;

            isNextStep = true;
        }

        void OnClickedBtnSkip()
        {

        }

        public void SetNpc(Npc npc)
        {
            tutorialDialog.SetNpc(npc);
        }

        /// <summary>
        /// 연출 중에 다른 UI가 터치되는 것을 막음
        /// </summary>
        public void ShowEmpty()
        {
            isEmpty = true;
            HideBlind();
            HideDialog();
            HideMask();
            btnNext.isEnabled = true;
        }

        public void ShowDialog(string dialog, UIWidget.Pivot pivot)
        {
            isEmpty = false;
            ShowBlind();
            HideMask();
            btnNext.isEnabled = true;
            tutorialDialog.ForceNextActivation(null);
            tutorialDialog.Show(dialog, pivot);
        }

        public void ShowMask(string dialog, UIWidget.Pivot pivot, UIWidget maskArea, UIWidget fingerArea, ClickType clickType, bool isActiveFinger)
        {
            isEmpty = false;
            ShowBlind();
            btnNext.isEnabled = clickType == ClickType.All;
            tutorialDialog.ForceNextActivation(false);
            tutorialDialog.Show(dialog, pivot);
            tutorialTouch.Show(maskArea, fingerArea, clickType == ClickType.OnlyMask, isActiveFinger);
        }

        public void ShowMask(UIWidget maskArea, UIWidget fingerArea, ClickType clickType, bool isActiveFinger)
        {
            isEmpty = false;
            ShowBlind();
            btnNext.isEnabled = clickType == ClickType.All;
            HideDialog();
            tutorialTouch.Show(maskArea, fingerArea, clickType == ClickType.OnlyMask, isActiveFinger);
        }

        public void SetActiveBtnSkip(bool isActive)
        {
            btnSkip.SetActive(isActive);
        }

        public bool IsNextStep()
        {
            if (isNextStep)
            {
                isNextStep = false;
                return true;
            }

            return false;
        }

        private void HideDialog()
        {
            tutorialDialog.Hide();
        }

        private void HideMask()
        {
            tutorialTouch.Hide();
        }

        public void ShowBlind()
        {
            NGUITools.SetActive(blind, true);
        }

        public void HideBlind()
        {
            NGUITools.SetActive(blind, false);
        }
    }
}