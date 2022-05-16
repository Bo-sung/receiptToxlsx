using UnityEngine;

namespace Ragnarok
{
    public class GuildBoardSubView : UISubCanvas<GuildBoardPresenter>, GuildBoardPresenter.IView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelNoticeTitle;
        [SerializeField] UIInput input;
        [SerializeField] UIButtonHelper btnEdit;
        [SerializeField] UIButtonHelper btnWrite;

        GuildBoardInfo[] arrayInfo;

        protected override void OnInit()
        {
            presenter = new GuildBoardPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.ScrollView.onDragFinished = presenter.OnDragFinished;
            wrapper.SpawnNewList(prefab, 0, 0);
            EventDelegate.Add(btnEdit.OnClick, presenter.OnClickedBtnEdit);
            EventDelegate.Add(input.onSubmit, presenter.OnSubmitInput);
            EventDelegate.Add(btnWrite.OnClick, OnClickedBtnWrite);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnEdit.OnClick, presenter.OnClickedBtnEdit);
            EventDelegate.Remove(input.onSubmit, presenter.OnSubmitInput);
            EventDelegate.Remove(btnWrite.OnClick, OnClickedBtnWrite);
        }

        protected override void OnShow()
        {
            presenter.SetView();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33068; // 길드 게시판
            labelNoticeTitle.LocalKey = LocalizeKey._33069; // 공지 사항
            input.defaultText = LocalizeKey._33070.ToText(); // 공지사항을 입력해주세요.
            btnWrite.LocalKey = LocalizeKey._33071; // 글쓰기
        }

        private void OnItemRefresh(GameObject go, int dataIndex)
        {
            UIGuildBoardInfo ui = go.GetComponent<UIGuildBoardInfo>();
            ui.SetData(presenter, presenter.GetInfo(dataIndex));
        }

        void OnClickedBtnWrite()
        {
            UI.Show<UIGuildBoardWrite>();
        }

        void GuildBoardPresenter.IView.SetBtnEdit(bool isActive)
        {
            btnEdit.SetActive(isActive);
        }

        void GuildBoardPresenter.IView.SetResize(int size)
        {
            wrapper.Resize(size);
        }

        bool GuildBoardPresenter.IView.IsEndDrag()
        {
            // UIScrollView 408줄 참고
            Bounds b = wrapper.ScrollView.bounds;
            Vector3 constraint = wrapper.Panel.CalculateConstrainOffset(b.min, b.max);
            constraint.x = 0f;

            // 최하단에서 드래그 끝났을 때
            return constraint.sqrMagnitude > 0.1f && constraint.y < 0f;
        }

        void GuildBoardPresenter.IView.SetInputSelect(bool isSelect)
        {
            input.isSelected = isSelect;
        }

        string GuildBoardPresenter.IView.GetInputValue()
        {
            return input.value;
        }

        void GuildBoardPresenter.IView.SetGuildNotice(string notice)
        {
            input.value = notice;
        }
    }
}
