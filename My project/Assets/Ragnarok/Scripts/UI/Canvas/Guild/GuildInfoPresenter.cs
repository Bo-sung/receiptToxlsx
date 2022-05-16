namespace Ragnarok
{
    public class GuildInfoPresenter : ViewPresenter
    {
        public interface IView
        {
            void SetGuildName(string name);
            void SetGuildLevel(int level);
            void SetGuildMaster(string name);
            void SetGuildMember(int count, int max);
            void SetGuildExp(int exp);
            void SetGuildIntroduction(string name);
            void SetMasterView(GuildPosition guildPosition);
            void SetGuildExpProgress(float value);
            void SetGuildExpLabel(string name);
            void SetTodayCount(int count);
            void SetBtnGetRewardEnabled(bool isAttendReward);
            void SetBtnCheckEnabled(bool isTodayAttend);
            void SetEmblem(int background, int frame, int icon);
        }

        private readonly IView view;
        private readonly GuildModel guild;

        bool IsGetAttendReward => guild.IsGetAttendReward || guild.YesterdayMemberCount < BasisType.GUILD_ATTEND_COUNT_REWARD.GetKeyList()[0];

        public GuildInfoPresenter(IView view)
        {
            this.view = view;
            guild = Entity.player.Guild;
        }

        public override void AddEvent()
        {
            guild.OnUpdateGuildEmblem += OnUpdateGuildEmblem;
            guild.OnUpdateGuildName += OnUpdateGuildName;
        }

        public override void RemoveEvent()
        {
            guild.OnUpdateGuildEmblem -= OnUpdateGuildEmblem;
            guild.OnUpdateGuildName -= OnUpdateGuildName;
        }

        public async void SetView()
        {
            SetGuildInfoView(); // 응답 오기 전 호출 한 번 필요

            await guild.RequestMyGuildInfo();
            await guild.RequestAttendInfo();
            SetGuildInfoView();
        }

        private void SetGuildInfoView()
        {
            view.SetGuildName(guild.GuildName);
            view.SetGuildLevel(guild.GuildLevel);
            view.SetGuildMaster(guild.MasterName);
            view.SetGuildMember(guild.MemberCount, guild.MaxMemberCount);
            view.SetGuildExp(guild.ExpPoint);
            view.SetGuildIntroduction(guild.Introduction);
            view.SetMasterView(guild.GuildPosition);
            if (guild.IsMaxLevel)
            {
                view.SetGuildExpProgress(1f);
                view.SetGuildExpLabel(LocalizeKey._33067.ToText()); // MAX
            }
            else
            {
                view.SetGuildExpProgress(guild.CurLevelExp / (float)guild.CurNeedLevelExp);
                view.SetGuildExpLabel($"{guild.CurLevelExp}/{guild.CurNeedLevelExp}"); // MAX
            }
            view.SetTodayCount(guild.TodayMemberCount);
            view.SetBtnGetRewardEnabled(!IsGetAttendReward);
            view.SetBtnCheckEnabled(!guild.IsTodayAttend);
            view.SetEmblem(guild.EmblemBg, guild.EmblemFrame, guild.EmblemIcon);
        }

        void OnUpdateGuildEmblem()
        {
            view.SetEmblem(guild.EmblemBg, guild.EmblemFrame, guild.EmblemIcon);
        }

        /// <summary>
        /// 길드명 업데이트
        /// </summary>
        void OnUpdateGuildName()
        {
            view.SetGuildName(guild.GuildName);
        }

        #region 프로토콜

        public async void RequestChangeGuildIntroduction(string introduction)
        {
            await guild.RequestChangeGuildIntroduction(introduction);
            view.SetGuildIntroduction(guild.Introduction);
        }

        public async void RequestAttendReward()
        {
            await guild.RequestAttendReward();
            view.SetBtnGetRewardEnabled(!IsGetAttendReward);
        }

        public async void RequestCheckAttend()
        {
            await guild.RequestCheckAttend();
            SetGuildInfoView();
        }

        public void RequestGuildOut()
        {
            if (guild.GuildOutRemainTime.ToRemainTime() > 0f) // 길드 탈퇴 쿨타임이 남아있는 경우
            {
                UI.ConfirmPopup(LocalizeKey._33100.ToText().Replace(ReplaceKey.TIME, guild.GuildOutRemainTime.ToStringTime())); // 길드 가입 후 일정 시간이 지나야 가능합니다.\n\n남은 시간 : {TIME}
                return;
            }

            guild.RequestGuildOut().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드 이름 변경 팝업 표시
        /// </summary>
        public void ShowEditGuildNname()
        {
            int needCatCoin = 0;
            // 길드명 유료 변경
            if(guild.FreeGuildNameChangeCount == 0)
            {
                needCatCoin = BasisType.GUILD_NAME_CHANGE_CATCOIN.GetInt();
            }

            UI.Show<UIEditGuildName>().Set(guild.GuildName, needCatCoin, RequestEditGuildName);
        }

        /// <summary>
        /// 길드명 변경 요청
        /// </summary>
        void RequestEditGuildName(string guildName)
        {
            guild.RequestEditGuildName(guildName).WrapNetworkErrors();
        }

        #endregion
    }
}
