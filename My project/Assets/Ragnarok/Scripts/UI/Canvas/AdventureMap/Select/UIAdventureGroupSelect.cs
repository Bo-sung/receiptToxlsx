using UnityEngine;

namespace Ragnarok
{
    public sealed class UIAdventureGroupSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] AdventureGroupElement[] elements;
        [SerializeField] UIButtonHelper btnConfirm;

        private AdventureGroupSelectPresenter presenter;

        private TaskAwaiter<int> awaiter;
        private int result;

        protected override void OnInit()
        {
            presenter = new AdventureGroupSelectPresenter();

            foreach (var item in elements)
            {
                item.OnSelect += OnSelect;
            }

            EventDelegate.Add(btnConfirm.OnClick, CloseUI);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);

            foreach (var item in elements)
            {
                item.OnSelect -= OnSelect;
            }

            Complete(CloseUIException.Default); // UI 강제 닫기
        }

        protected override void OnShow(IUIData data = null)
        {
            result = 0;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._48283; // 지도
            btnConfirm.LocalKey = LocalizeKey._4100; // 닫기
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Complete(CloseUIException.Default); // UI 강제 닫기
        }

        void OnSelect(int groupId)
        {
            result = groupId;
            CloseUI();
        }

        public TaskAwaiter<int> AsyncShow()
        {
            Complete(DuplicateUIException.Default); // UI 중복

            Show();
            awaiter = new TaskAwaiter<int>();

            AdventureGroupElement.IInput[] inputs = presenter.GetData();
            int length = inputs == null ? 0 : inputs.Length;
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].SetData(i < length ? inputs[i] : null);
            }

            return awaiter;
        }

        private void CloseUI()
        {
            Complete(null);
            UI.Close<UIAdventureGroupSelect>();
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
    }
}