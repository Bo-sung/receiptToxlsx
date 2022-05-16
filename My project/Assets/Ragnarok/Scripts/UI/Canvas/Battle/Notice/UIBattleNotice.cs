using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleNotice : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIPlayTween background;
        [SerializeField] UIMoveNotice moveNotice;

        private Queue<string> queue;
        private bool isShowNotice;

        protected override void OnInit()
        {
            queue = new Queue<string>();

            EventDelegate.Add(background.onFinished, OnFinishedBackground);
            moveNotice.OnFinish += OnFinishedMoveNotice;

            moveNotice.Hide();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(background.onFinished, OnFinishedBackground);
            moveNotice.OnFinish -= OnFinishedMoveNotice;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Show(string text)
        {
            Show();

            queue.Enqueue(text);
            ShowBackground();
        }

        void OnFinishedBackground()
        {
            if (isShowNotice)
            {
                moveNotice.Show();
                ShowNotice();
            }
            else
            {
                moveNotice.Hide();
                Hide();
            }
        }

        void OnFinishedMoveNotice()
        {
            ShowNotice();
        }

        private void ShowBackground()
        {
            if (isShowNotice)
                return;

            isShowNotice = true;
            background.Play(forward: true);
        }

        private void HideBackground()
        {
            if (!isShowNotice)
                return;

            isShowNotice = false;
            background.Play(forward: false);
        }

        private void ShowNotice()
        {
            if (queue.Count > 0)
            {
                moveNotice.ShowNotice(queue.Dequeue());
                return;
            }

            HideBackground();
        }
    }
}