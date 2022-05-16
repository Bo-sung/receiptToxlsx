using Ragnarok.View;
using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICupetSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPopupView selectPopupView;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UICupetSelectElement element;
        [SerializeField] UILabelHelper labelNoData;

        private SuperWrapContent<UICupetSelectElement, UICupetSelectElement.IInput> wrapContent;
        private Action<int[]> onSelectedIds;

        CupetSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CupetSelectPresenter();

            wrapContent = wrapper.Initialize<UICupetSelectElement, UICupetSelectElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelectLongPress += presenter.ShowCupetInfo;
            }

            selectPopupView.OnExit += CloseUI;
            selectPopupView.OnCancel += CloseUI;
            selectPopupView.OnConfirm += OnClickedBtnConfirm;
            presenter.OnUpdateList += RefreshAllItems;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            foreach (var item in wrapContent)
            {
                item.OnSelectLongPress -= presenter.ShowCupetInfo;
            }

            selectPopupView.OnExit -= CloseUI;
            selectPopupView.OnCancel -= CloseUI;
            selectPopupView.OnConfirm -= OnClickedBtnConfirm;
            presenter.OnUpdateList -= RefreshAllItems;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            selectPopupView.MainTitleLocalKey = LocalizeKey._34300; // 큐펫 선택
            selectPopupView.ConfirmLocalKey = LocalizeKey._34301; // 확인
            selectPopupView.CancelLocalKey = LocalizeKey._2; // 취소
            labelNoData.LocalKey = LocalizeKey._34302; // 사용 가능한 큐펫이 없습니다.
        }

        private void CloseUI()
        {
            UI.Close<UICupetSelect>();
        }

        private void SetData(UICupetSelectElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);

            int length = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(length == 0);
        }

        private void RefreshAllItems()
        {
            wrapContent.RefreshAllItems();
        }

        /// <summary>
        /// 큐셋 선택 세팅
        /// </summary>
        /// <param name="selectedIds">미리 선택 한 큐펫 ID 목록</param>
        /// <param name="onSelectedIds">선택 모두 완료 후 목록</param>
        /// <param name="filterIds">필터 해야할 목록</param>
        public void Set(int[] selectedIds, Action<int[]> onSelectedIds, int[] filterIds = null)
        {
            presenter.SetSelectedIds(selectedIds);
            SetData(presenter.GetCupetArray(filterIds));
            this.onSelectedIds = onSelectedIds;
        }

        private void OnClickedBtnConfirm()
        {
            int[] ids = presenter.GetSelectedIds();
            onSelectedIds?.Invoke(ids);
            onSelectedIds = null;
            CloseUI();
        }
    }
}