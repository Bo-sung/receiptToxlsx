using UnityEngine;

namespace Ragnarok
{
    public class UIGuildRewardInfo : UICanvas, GuildRewardInfoPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        GuildRewardInfoPresenter presenter;
        GuildAttendRewardInfo[] arrayInfo;

        protected override void OnInit()
        {
            presenter = new GuildRewardInfoPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnExit.OnClick, OnBack);
            EventDelegate.Add(btnConfirm.OnClick, OnBack);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnExit.OnClick, OnBack);
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.Refresh();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._33073; // 출석 보상 목록
            labelTitle.LocalKey = LocalizeKey._33074; // 어제 출석 인원
            btnConfirm.LocalKey = LocalizeKey._33077; // 확 인
        }


        private void OnItemRefresh(GameObject go, int index)
        {
            UIGuildRewardInfoSlot ui = go.GetComponent<UIGuildRewardInfoSlot>();
            ui.SetData(arrayInfo[index]);
        }

        void GuildRewardInfoPresenter.IView.SetScrollView()
        {
            arrayInfo = presenter.GetGuildAttendRewardInfos();
            wrapper.Resize(arrayInfo.Length);
        }

        void GuildRewardInfoPresenter.IView.SetAttendCount(int count)
        {
            labelCount.Text = LocalizeKey._33075.ToText()
                .Replace("{COUNT}", count.ToString()); // {COUNT}명
        }
    }
}
