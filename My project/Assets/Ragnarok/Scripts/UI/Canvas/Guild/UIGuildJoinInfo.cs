using UnityEngine;

namespace Ragnarok
{
    public class UIGuildJoinInfo : UIBaseGuildInfo<GuildJoinPresenter>
    {
        [SerializeField] UIButtonHelper btnJoin;
        [SerializeField] UILabelHelper labelJoined;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnJoin.OnClick, OnClickedBtnJoin);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnJoin.OnClick, OnClickedBtnJoin);
        }

        protected override void OnLocalize()
        {
            labelJoined.LocalKey = LocalizeKey._33028; // 신청완료
        }

        protected override void Refresh()
        {
            base.Refresh();

            if (info.IsSubmitJoin)
            {
                btnJoin.SetActive(false);
                labelJoined.SetActive(true);
            }
            else
            {
                btnJoin.SetActive(true);
                labelJoined.SetActive(false);
            }

            btnJoin.LocalKey = info.IsAutoJoin ? LocalizeKey._33098 : LocalizeKey._33024; // 가입 : 신청
        }

        /// <summary>
        /// 길드 가입 신청 버튼 클릭
        /// </summary>
        void OnClickedBtnJoin()
        {
            presenter.RequestGuildJoin(info);
        }
    }
}