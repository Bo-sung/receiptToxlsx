using Ragnarok.View;

using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildAttack : UICanvas
    {
        private const int TAB_GUILD_ATTACK = 0;

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] GameObject guildAttackPopupBase;
        [SerializeField] PopupView popupView;
        [SerializeField] UITabHelper tab;
        [SerializeField] GuildAttackView guildAttackView;
        [SerializeField] GuildAttackChangeTimeHelpView guildAttackChangeTimeHelpView;
        [SerializeField] GuildAttackDonationHelpView guildAttackDonationHelpView;

        [SerializeField] GameObject guildAttackCreatePopupBase;
        [SerializeField] PopupView createPopupView;
        [SerializeField] EmperiumBuffView emperiumBuffView;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButtonWithIconValueHelper btnCreate;

        GuildAttackPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildAttackPresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;
            tab.OnSelect += OnSelectTab;
            guildAttackView.OnChangeStartTime += OnChangeStartTime;
            guildAttackView.OnChangeStartTimeHelp += OnChangeStartTimeHelp;
            guildAttackView.OnDonation += OnDonation;
            guildAttackView.OnDonationHelp += OnDonationHelp;

            createPopupView.OnConfirm += OnBack;
            createPopupView.OnExit += OnBack;
            EventDelegate.Add(btnCreate.OnClick, OnClickedBtnCreate);

            presenter.OnUpdateRefineEmperium += UpdateRefinEmperium;
            presenter.OnUpdateGuildAttackStartTime += UpdateChangeStartTimeView;
            presenter.OnUpdateGuildAttackStartTime += UpdateRemainTimeView;
            presenter.OnUpdateCreateEmperium += UpdateView;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
            tab.OnSelect -= OnSelectTab;
            guildAttackView.OnChangeStartTime -= OnChangeStartTime;
            guildAttackView.OnChangeStartTimeHelp -= OnChangeStartTimeHelp;
            guildAttackView.OnDonation -= OnDonation;
            guildAttackView.OnDonationHelp -= OnDonationHelp;

            createPopupView.OnConfirm -= OnBack;
            createPopupView.OnExit -= OnBack;
            EventDelegate.Remove(btnCreate.OnClick, OnClickedBtnCreate);

            presenter.OnUpdateRefineEmperium -= UpdateRefinEmperium;
            presenter.OnUpdateGuildAttackStartTime -= UpdateChangeStartTimeView;
            presenter.OnUpdateGuildAttackStartTime -= UpdateRemainTimeView;
            presenter.OnUpdateCreateEmperium -= UpdateView;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            int level = presenter.GetEmperiumLevel();
            if (level == 0)
            {
                guildAttackPopupBase.SetActive(false);
                guildAttackCreatePopupBase.SetActive(true);
                GuildAttackCreateView();
            }
            else
            {
                guildAttackPopupBase.SetActive(true);
                guildAttackCreatePopupBase.SetActive(false);
                GuildAttackView();
            }
        }

        private void GuildAttackView()
        {
            tab.Value = TAB_GUILD_ATTACK;
            guildAttackChangeTimeHelpView.Hide();
            guildAttackDonationHelpView.Hide();

            UdpateEmperiumLevel();
            UpdateBuffView();
            UpdateChangeStartTimeView();
            UpdateDonationView();
            UpdateDonationHelpView();
            UpdateRemainTimeView();
            UpdateChangeTimeHelpView();
        }

        private void GuildAttackCreateView()
        {
            emperiumBuffView.SetData(presenter.GetBattleOptions(1));
            btnCreate.SetValue(presenter.GetCreateZeny().ToString("N0"));
            btnCreate.SetActive(presenter.IsGuildMaster());
            createPopupView.SetActiveConfirm(!presenter.IsGuildMaster());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._38400; // 길드 습격
            popupView.ConfirmLocalKey = LocalizeKey._38413; // 확인
            tab[0].LocalKey = LocalizeKey._38401; // 길드 습격
            tab[1].LocalKey = LocalizeKey._38402; // 길드 축제

            createPopupView.MainTitleLocalKey = LocalizeKey._38500; // 길드 습격
            createPopupView.ConfirmLocalKey = LocalizeKey._38503; // 확인
            labelDescription.LocalKey = LocalizeKey._38501; // 길드 마스터가 엠펠리움을 생성 할 수 있습니다.
            btnCreate.LocalKey = LocalizeKey._38502; // 생성
        }

        private void OnSelectTab(int index)
        {
            guildAttackView.SetActive(index == TAB_GUILD_ATTACK);
        }

        private void OnChangeStartTime()
        {
            presenter.RequestChangeStartTime();
        }

        private void OnChangeStartTimeHelp()
        {
            guildAttackChangeTimeHelpView.Show();
        }

        private void OnDonation()
        {
            presenter.RequestDonation();
        }

        private void OnDonationHelp()
        {
            guildAttackDonationHelpView.Show();
        }

        protected override void OnBack()
        {
            if (guildAttackChangeTimeHelpView.IsShow)
            {
                guildAttackChangeTimeHelpView.Hide();
                return;
            }

            if (guildAttackDonationHelpView.IsShow)
            {
                guildAttackDonationHelpView.Hide();
                return;
            }

            base.OnBack();
        }

        void UdpateEmperiumLevel()
        {
            int level = presenter.GetEmperiumLevel();

            if (presenter.IsMaxEmperiumLevel())
            {
                string text = LocalizeKey._38403.ToText()
                    .Replace(ReplaceKey.LEVEL, level); // [FF6C7B]MAX[-] [4595F5]Lv.{LEVEL}[-]

                guildAttackView.SetEmperiumLevel(text);
            }
            else
            {
                string text = LocalizeKey._38404.ToText()
                    .Replace(ReplaceKey.LEVEL, level); // [4595F5]Lv.{LEVEL}[-]

                guildAttackView.SetEmperiumLevel(text);
            }
        }

        void UpdateBuffView()
        {
            guildAttackView.SetBuffOption(presenter.GetBattleOptions(presenter.GetEmperiumLevel()));
        }

        void UpdateChangeStartTimeView()
        {
            guildAttackView.SetActiveBtnChangeStartTime(presenter.IsGuildMaster());
            guildAttackView.SetChangeTimeCoin(presenter.GetChangeTimeCoin());
            guildAttackView.SetStartTime(presenter.GetStartTime());
        }

        void UpdateDonationView()
        {
            UpdateRefinEmperium();
            guildAttackView.SetDonationItem(presenter.GetDonationItemIconName(), presenter.GetDonationNeedCount());
        }

        private void UpdateRefinEmperium()
        {
            guildAttackView.SetOwnedRefineEmperium(presenter.GetRefineEmperiumCount());
        }

        void UpdateDonationHelpView()
        {
            guildAttackDonationHelpView.SetData(presenter.GetDonationReward());
        }

        void UpdateRemainTimeView()
        {
            guildAttackView.SetRemainTime(presenter.GetStartTime());
        }

        void UpdateChangeTimeHelpView()
        {
            string text = LocalizeKey._38415.ToText()
                .Replace(ReplaceKey.TIME, presenter.GetGuildAttackStartTimes()); // 길드 마스터는 기부를 통해 모인 정제된 엠펠리움 조각을 사요하여\n길드 습격 시간이 변경됩니다.\n\n변경 시에 [76C5FE]요일[-]과 [76C5FE]{TIME}[-] 중\n무작위로 길드 습격 시간이 변경됩니다.
            guildAttackChangeTimeHelpView.SetText(text);
        }

        void OnClickedBtnCreate()
        {
            presenter.RequestEmperiumCreate();
        }
    }
}