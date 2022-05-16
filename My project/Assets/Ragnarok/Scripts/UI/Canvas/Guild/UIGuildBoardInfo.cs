using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIGuildBoardInfo : UIInfo<GuildBoardPresenter, GuildBoardInfo>
    {
        [SerializeField] UILabelHelper labelDay;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnDelete;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnDelete.OnClick, OnClickedBtnDelete);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnDelete.OnClick, OnClickedBtnDelete);
        }

        protected override void Refresh()
        {
            labelDay.Text = info.InsertTime;
            labelName.Text = info.Name;
            labelDesc.Text = FilterUtils.ReplaceChat(info.Message);
            btnDelete.SetActive(presenter.CanDeleteBoard(info));
        }

        void OnClickedBtnDelete()
        {
            presenter.RequestRemoveGuildBoard(info);
        }
    }
}
