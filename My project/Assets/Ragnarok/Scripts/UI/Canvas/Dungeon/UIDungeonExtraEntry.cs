using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDungeonExtraEntry : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        public enum SelectResult
        {
            Cancel = 1,
            Confirm,
        }

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelValue remainCount;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UICostButtonHelper btnConfirm;

        DungeonExtraEntryPresenter presenter;

        private TaskAwaiter<SelectResult> awaiter;
        private SelectResult result;
        private int needCoin;

        protected override void OnInit()
        {
            presenter = new DungeonExtraEntryPresenter();

            EventDelegate.Add(btnExit.onClick, CloseUI);
            EventDelegate.Add(btnCancel.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedConfirm);

            presenter.OnUpdateCatCoin += UpdateCatCoin;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateCatCoin -= UpdateCatCoin;

            EventDelegate.Remove(btnExit.onClick, CloseUI);
            EventDelegate.Remove(btnCancel.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedConfirm);

            Complete(CloseUIException.Default); // UI 강제 닫기
        }

        protected override void OnShow(IUIData data = null)
        {
            result = SelectResult.Cancel;
            UpdateCatCoin(presenter.GetCatCoin());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._90027; // 유료 던전 입장
            labelDescription.LocalKey = LocalizeKey._90028; // 냥다래를 소모하여 던전에 입장하시겠습니까?
            remainCount.TitleKey = LocalizeKey._7101; // 오늘 남은 입장 횟수
            btnCancel.LocalKey = LocalizeKey._2; // 취소
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Complete(DestroyUIException.Default); // UI 강제 제거
        }

        public TaskAwaiter<SelectResult> Show(int count, int max, int needCoin)
        {
            this.needCoin = needCoin;
            Complete(DuplicateUIException.Default); // UI 중복

            Show();
            awaiter = new TaskAwaiter<SelectResult>();

            remainCount.Value = StringBuilderPool.Get()
                .Append(count).Append("/").Append(max)
                .Release();

            btnConfirm.SetCostCount(needCoin);
            return awaiter;
        }

        void OnClickedConfirm()
        {
            result = SelectResult.Confirm;
            CloseUI();
        }

        private void CloseUI()
        {
            Complete(null);
            UI.Close<UIDungeonExtraEntry>();
        }

        private void UpdateCatCoin(long catCoin)
        {
            btnConfirm.IsEnabled = needCoin <= catCoin;
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