using UnityEngine;

namespace Ragnarok
{
    public class UIGuildMemberInfoSlot : UIInfo<GuildMainPresenter, GuildMemberInfo>
    {
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelID;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UILabelHelper labelDonate;
        [SerializeField] UILabelHelper labelPosition;
        [SerializeField] UILabelHelper labelOffline;
        [SerializeField] UILabelHelper labelOnline;
        [SerializeField] GameObject masterIcon;
        [SerializeField] UIButtonHelper btnMemberInfo;
        [SerializeField] GameObject goNewMemberIcon; // NEW 아이콘

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnMemberInfo.OnClick, OnClickedBtnMemberInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnMemberInfo.OnClick, OnClickedBtnMemberInfo);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            jobIcon.Set(info.Job.GetJobIcon());
            labelName.Text = info.Name;
            labelDonate.Text = info.DonatePoint.ToString("N0");
            labelPosition.Text = info.GuildPosition.ToText();
            masterIcon.SetActive(info.GuildPosition == GuildPosition.Master);
            labelLevel.Text = info.JobLevel.ToString();
            labelID.Text = LocalizeKey._33052.ToText()
                .Replace("{VALUE}", info.ID); // ID : {VALUE}

            goNewMemberIcon.SetActive(info.GuildOutRemainTime.ToRemainTime() > 0f); // 탈퇴쿨타임이 남아있다면 NEW 표시

            if (info.IsOnline)
            {
                labelOnline.LocalKey = LocalizeKey._33057; // 접속중
                labelOnline.SetActive(true);
                labelOffline.SetActive(false);
            }
            else
            {
                labelOnline.SetActive(false);
                labelOffline.SetActive(true);
                labelOffline.Text = info.OfflineTime.ToStringOnlineTime();
            }
        }

        /// <summary>
        /// 맴버 정보 보기
        /// </summary>
        void OnClickedBtnMemberInfo()
        {
            presenter.ShowGuildMemberInfo(info);
        }
    }
}
