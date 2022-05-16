using UnityEngine;

namespace Ragnarok
{
    public class UIGuildJoinSubmitUserInfo : UIInfo<GuildMainPresenter, GuildJoinSubmitInfo>
    {
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelID;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UILabelHelper labelDate;
        [SerializeField] UIButtonHelper btnReject;
        [SerializeField] UIButtonHelper btnAccept;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnReject.OnClick, OnClickedBtnReject);
            EventDelegate.Add(btnAccept.OnClick, OnClickedBtnAccept);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnReject.OnClick, OnClickedBtnReject);
            EventDelegate.Remove(btnAccept.OnClick, OnClickedBtnAccept);
        }

        protected override void Refresh()
        {
            jobIcon.Set(info.Job.GetJobIcon());
            labelName.Text = info.Name;
            labelID.Text = LocalizeKey._33052.ToText() // ID : {VALUE}
                .Replace("{VALUE}", info.Id);
            labelLevel.Text = info.Level.ToString();
            labelDate.Text = info.Date.ToShortDateString(); // 신청일 (연-월-일)
            btnReject.LocalKey = LocalizeKey._33053; // 거절
            btnAccept.LocalKey = LocalizeKey._33054; // 승인
        }

        void OnClickedBtnAccept()
        {
            presenter.RequestGuildJoinSumitUserProc(info, 1).WrapNetworkErrors();
        }

        void OnClickedBtnReject()
        {
            presenter.RequestGuildJoinSumitUserProc(info, 0).WrapNetworkErrors();
        }
    }
}