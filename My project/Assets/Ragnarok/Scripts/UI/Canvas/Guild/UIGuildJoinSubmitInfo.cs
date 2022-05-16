using UnityEngine;

namespace Ragnarok
{
    public class UIGuildJoinSubmitInfo : UIBaseGuildInfo<GuildJoinPresenter>
    {
        [SerializeField] UIButtonHelper btnCancel;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
        }

        protected override void OnLocalize()
        {
            btnCancel.LocalKey = LocalizeKey._33029; // 취소
        }

        /// <summary>
        /// 길드 가입 신청 취소 버튼
        /// </summary>
        void OnClickedBtnCancel()
        {
            presenter.RequstGuildJoinCancel(info);
        }
    }
}