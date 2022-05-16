using UnityEngine;

namespace Ragnarok
{
    public class GuildMemberBaseView : UISubCanvas<GuildMainPresenter>, IAutoInspectorFinder
    {
        [SerializeField] UITabHelper tab;
        [SerializeField] UILabelHelper labelGuildMember;
        [SerializeField] GuildMemberSubView memberView;
        [SerializeField] GuildJoinSubmitSubView joinSubmitView;

        UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            memberView.Initialize(presenter);
            joinSubmitView.Initialize(presenter);
            EventDelegate.Add(tab[0].OnChange, ShowGuildMemverView);
            EventDelegate.Add(tab[1].OnChange, ShowJoinSubmitView);
        }
        protected override void OnClose()
        {
            EventDelegate.Remove(tab[0].OnChange, ShowGuildMemverView);
            EventDelegate.Remove(tab[1].OnChange, ShowJoinSubmitView);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            tab[0].LocalKey = LocalizeKey._33031; // 길드원
            labelGuildMember.LocalKey = LocalizeKey._33031; // 길드원
        }

        public void Refresh()
        {
            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Show();
            SetTabLocalize();
        }

        void SetTabLocalize()
        {
            if (presenter.IsShowJoinMemberList())
            {
                NGUITools.SetActive(tab.gameObject, true);
                labelGuildMember.SetActive(false);

                tab[1].Text = LocalizeKey._33044.ToText() // 가입신청({COUNT})
                    .Replace("{COUNT}", presenter.GuildJoinSubmitUserCount.ToString());
            }
            else
            {
                NGUITools.SetActive(tab.gameObject, false);
                labelGuildMember.SetActive(true);
            }
        }

        private void ShowGuildMemverView()
        {
            ShowSubCanvas(memberView);
        }

        private void ShowJoinSubmitView()
        {
            ShowSubCanvas(joinSubmitView);
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            if (!UIToggle.current.value)
                return;

            currentSubCanvas = subCanvas;
            HideAllSubCanvas();
            Refresh();
        }

        private void HideAllSubCanvas()
        {
            memberView.Hide();
            joinSubmitView.Hide();
        }
    }
}
