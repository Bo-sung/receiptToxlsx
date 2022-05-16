using UnityEngine;

namespace Ragnarok
{
    public class GuildInfoSubView : UISubCanvas<GuildInfoPresenter>, GuildInfoPresenter.IView
    {
        // 기본 정보
        [SerializeField] UILabelHelper labelGuildName;
        [SerializeField] UITextureHelper emblemBackground;
        [SerializeField] UITextureHelper emblemFrame;
        [SerializeField] UITextureHelper emblemIcon;
        [SerializeField] UILabelHelper labelGuildLevel;
        [SerializeField] UILabelValue labelGuildMaster;
        [SerializeField] UILabelValue labelGuildMember;
        [SerializeField] UILabelValue labelGuildExp;
        [SerializeField] UISlider progressExp;
        [SerializeField] UILabelHelper labelExp;
        [SerializeField] UIButtonHelper btnRank;
        [SerializeField] UILabelHelper labelIntroductionTitle;
        [SerializeField] UIInput inputIntroduction;
        [SerializeField] UIButtonHelper btnEdit;
        [SerializeField] UILabelHelper labelDailyCheckTitle;
        [SerializeField] UILabelHelper labelNoti;
        [SerializeField] UILabelHelper labelNoti1;
        [SerializeField] UILabelHelper labelTodayCount;
        [SerializeField] UIButtonHelper btnRewardInfo;
        [SerializeField] UIButtonHelper btnGetReward;
        [SerializeField] UIButtonHelper btnCheck;
        [SerializeField] UIGrid gridBtn;
        [SerializeField] UIButtonHelper btnLeave;
        [SerializeField] UIButtonHelper btnSetting;
        [SerializeField] UIButtonHelper btnMasterDismissal;
        [SerializeField] UIButtonHelper btnQuest;
        [SerializeField] UIButtonHelper btnEmblemChange;
        [SerializeField] GameObject masterBase;
        [SerializeField] UIButtonHelper btnEditGuildName;

        protected override void OnInit()
        {
            presenter = new GuildInfoPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnEdit.OnClick, OnClickedBtnEdit);
            EventDelegate.Add(inputIntroduction.onSubmit, OnSubmitIntroduction);
            EventDelegate.Add(btnGetReward.OnClick, OnClickedBtnGetReward);
            EventDelegate.Add(btnCheck.OnClick, OnClickedBtnCheck);
            EventDelegate.Add(btnLeave.OnClick, OnClickedBtnLeave);
            EventDelegate.Add(btnRewardInfo.OnClick, OnClickedBtnRewardInfo);
            EventDelegate.Add(btnEmblemChange.OnClick, OnClickedBtnEmblemChange);
            EventDelegate.Add(btnSetting.OnClick, OnClickedBtnSetting);
            EventDelegate.Add(btnMasterDismissal.OnClick, OnClickedBtnMasterDismissal);
            EventDelegate.Add(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Add(btnQuest.OnClick, OnClickedBtnQuest);
            EventDelegate.Add(btnEditGuildName.OnClick, OnClickedBtnEditGuildName);
        }
        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnEdit.OnClick, OnClickedBtnEdit);
            EventDelegate.Remove(inputIntroduction.onSubmit, OnSubmitIntroduction);
            EventDelegate.Remove(btnGetReward.OnClick, OnClickedBtnGetReward);
            EventDelegate.Remove(btnCheck.OnClick, OnClickedBtnCheck);
            EventDelegate.Remove(btnLeave.OnClick, OnClickedBtnLeave);
            EventDelegate.Remove(btnRewardInfo.OnClick, OnClickedBtnRewardInfo);
            EventDelegate.Remove(btnEmblemChange.OnClick, OnClickedBtnEmblemChange);
            EventDelegate.Remove(btnSetting.OnClick, OnClickedBtnSetting);
            EventDelegate.Remove(btnMasterDismissal.OnClick, OnClickedBtnMasterDismissal);
            EventDelegate.Remove(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Remove(btnQuest.OnClick, OnClickedBtnQuest);
            EventDelegate.Remove(btnEditGuildName.OnClick, OnClickedBtnEditGuildName);
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
            labelGuildMaster.TitleKey = LocalizeKey._33036; // 길드장 :
            labelGuildMember.TitleKey = LocalizeKey._33037; // 인원 :
            labelGuildExp.TitleKey = LocalizeKey._33038; // 길드 누적 경험치 :
            btnRank.LocalKey = LocalizeKey._33035; // 길드 랭킹
            labelIntroductionTitle.LocalKey = LocalizeKey._33007; // 길드 소개
            labelDailyCheckTitle.LocalKey = LocalizeKey._33034; // 출석 체크
            labelNoti.LocalKey = LocalizeKey._33039; // 전날 출석인원에 따라 보상이 달라집니다.
            labelNoti1.LocalKey = LocalizeKey._33040; // 받기 버튼을 터치 시 출석 보상이 지급됩니다.
            btnRewardInfo.LocalKey = LocalizeKey._33042; // 보상 보기
            btnGetReward.LocalKey = LocalizeKey._33043; // 보상 받기
            btnCheck.LocalKey = LocalizeKey._33034; // 출석 체크
            btnLeave.LocalKey = LocalizeKey._33045; // 길드 탈퇴
            btnSetting.LocalKey = LocalizeKey._33046; // 길드 관리
            btnMasterDismissal.LocalKey = LocalizeKey._33118; // 길드장 해임
            btnQuest.LocalKey = LocalizeKey._33047; // 길드 퀘스트
            inputIntroduction.defaultText = LocalizeKey._33008.ToText(); // 길드 소개글을 입력해주세요.
        }

        void OnClickedBtnEdit()
        {
            inputIntroduction.isSelected = true;
        }

        void OnSubmitIntroduction()
        {
            inputIntroduction.isSelected = false;

            string text = inputIntroduction.value;
            if (!string.IsNullOrEmpty(text))
                presenter.RequestChangeGuildIntroduction(text);
        }

        /// <summary>
        /// 출석 보상 받기
        /// </summary>
        void OnClickedBtnGetReward()
        {
            presenter.RequestAttendReward();
        }

        /// <summary>
        /// 출석 체크
        /// </summary>
        void OnClickedBtnCheck()
        {
            presenter.RequestCheckAttend();
        }

        /// <summary>
        /// 길드 탈퇴
        /// </summary>
        void OnClickedBtnLeave()
        {
            presenter.RequestGuildOut();
        }

        /// <summary>
        /// 길드 보상 보기
        /// </summary>
        void OnClickedBtnRewardInfo()
        {
            UI.Show<UIGuildRewardInfo>();
        }

        /// <summary>
        /// 길드 엠블렘 변경
        /// </summary>
        void OnClickedBtnEmblemChange()
        {
            UI.Show<UIGuildEmblem>();
        }

        /// <summary>
        /// 길드 관리 버튼
        /// </summary>
        void OnClickedBtnSetting()
        {
            UI.Show<UIGuildSet>();
        }

        /// <summary>
        /// 길드장 해임 버튼
        /// </summary>
        void OnClickedBtnMasterDismissal()
        {
            UI.Show<UIGuildDismissal>();
        }

        /// <summary>
        /// 길드 랭킹 버튼
        /// </summary>
        void OnClickedBtnRank()
        {
            UI.Show<UIGuildRank>().Set(UIGuildRank.GuildRankType.HasGuild);
        }

        /// <summary>
        /// 길드 퀘스트 버튼
        /// </summary>
        void OnClickedBtnQuest()
        {
            UIQuest.view = UIQuest.ViewType.Guild;
            UI.ShortCut<UIQuest>();
        }

        /// <summary>
        /// 길드 이름 변경 버튼
        /// </summary>
        void OnClickedBtnEditGuildName()
        {
            presenter.ShowEditGuildNname();
        }

        void GuildInfoPresenter.IView.SetGuildName(string name)
        {
            labelGuildName.Text = name;
        }

        void GuildInfoPresenter.IView.SetGuildLevel(int level)
        {
            labelGuildLevel.Text = $"Lv.{level}";
        }

        void GuildInfoPresenter.IView.SetGuildMaster(string name)
        {
            labelGuildMaster.Value = name;
        }

        void GuildInfoPresenter.IView.SetGuildMember(int count, int max)
        {
            labelGuildMember.Value = $"{count}/{max}";
        }

        void GuildInfoPresenter.IView.SetGuildExp(int exp)
        {
            labelGuildExp.Value = exp.ToString("N0");
        }

        void GuildInfoPresenter.IView.SetGuildIntroduction(string name)
        {
            inputIntroduction.defaultText = name;
        }

        void GuildInfoPresenter.IView.SetMasterView(GuildPosition guildPosition)
        {
            masterBase.SetActive(guildPosition == GuildPosition.Master);
            btnSetting.SetActive(guildPosition == GuildPosition.Master);
            btnMasterDismissal.SetActive(guildPosition == GuildPosition.PartMaster);
            btnEditGuildName.SetActive(guildPosition == GuildPosition.Master);

            gridBtn.Reposition();
        }

        void GuildInfoPresenter.IView.SetGuildExpProgress(float value)
        {
            progressExp.value = value;
        }

        void GuildInfoPresenter.IView.SetGuildExpLabel(string name)
        {
            labelExp.Text = name;
        }

        void GuildInfoPresenter.IView.SetTodayCount(int count)
        {
            labelTodayCount.Text = LocalizeKey._33041.ToText() // [908F90]오늘 출석 : [-][7AB0F0]{COUNT}[-]
                .Replace("{COUNT}", count.ToString());
        }

        void GuildInfoPresenter.IView.SetBtnGetRewardEnabled(bool isEnabled)
        {
            btnGetReward.IsEnabled = isEnabled;
        }

        void GuildInfoPresenter.IView.SetBtnCheckEnabled(bool isEnabled)
        {
            btnCheck.IsEnabled = isEnabled;
        }

        void GuildInfoPresenter.IView.SetEmblem(int background, int frame, int icon)
        {
            emblemBackground.SetGuildEmblem($"background_{background}");
            emblemFrame.SetGuildEmblem($"frame_{frame}");
            emblemIcon.SetGuildEmblem($"icon_{icon}");
        }
    }
}