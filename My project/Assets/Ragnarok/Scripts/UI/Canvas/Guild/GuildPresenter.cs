using System;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuild"/>
    /// </summary>
    public sealed class GuildPresenter : ViewPresenter
    {
        public interface IView
        {
        }

        private readonly CharacterModel characterModel;
        private readonly GuildModel guildModel;

        private readonly IView view;

        public GuildPresenter(IView view)
        {
            this.view = view;
            characterModel = Entity.player.Character;
            guildModel = Entity.player.Guild;
        }

        public override void AddEvent()
        {
            guildModel.OnUpdateGuildState += OnUpdateGuildState;
        }

        public override void RemoveEvent()
        {
            guildModel.OnUpdateGuildState -= OnUpdateGuildState;
        }

        /// <summary>
        /// 길드 생성 버튼 클릭
        /// </summary>
        public void OnClickedBtnCreate()
        {
            string title = LocalizeKey._5.ToText(); // 알람
            // 길드생성을 위한 직업레벨 부족
            if(characterModel.JobLevel < BasisType.GUILD_CREATE_JOB_LEVEL.GetInt())
            {                
                string description = LocalizeKey._90054.ToText() // 길드 생성은 직업레벨 {JOB_LEVEL}이상부터 가능합니다.
                    .Replace("{JOB_LEVEL}", BasisType.GUILD_CREATE_JOB_LEVEL.GetString());
                UI.ConfirmPopup(title, description);
                return;
            }

            // 길드 생성 가능 시간 체크         
            if (guildModel.RejoinTime.ToRemainTime() > 0)
            {
                TimeSpan span = TimeSpan.FromMilliseconds(BasisType.GUILD_REJOIN_TIME.GetInt());
                string description = LocalizeKey._90064.ToText() // 탈퇴 후 {TIME}시간 동안 길드를 생성할 수 없습니다./n남은시간 : {REMAIN_TIME}
                    .Replace("{TIME}", span.TotalHours.ToString("N0"))
                    .Replace("{REMAIN_TIME}", guildModel.RejoinTime.ToStringTime());
                UI.ConfirmPopup(title, description);
                return;
            }

            // 이미 길드에 가입되어있는지 다시 체크
            if (guildModel.HaveGuild)
            {
                UI.Close<UIGuild>();
                return;
            }

            UI.Close<UIGuild>();
            UI.Show<UIGuildCreate>();
        }

        /// <summary>
        /// 길드 가입 버튼 클릭
        /// </summary>
        public async  void OnClickedBtnJoin()
        {
            // 길드 가입 가능 시간 체크
            if (guildModel.RejoinTime.ToRemainTime() > 0)
            {
                TimeSpan span = TimeSpan.FromMilliseconds(BasisType.GUILD_REJOIN_TIME.GetInt());
                string title = LocalizeKey._5.ToText(); // 알람
                string description = LocalizeKey._90063.ToText() // 탈퇴 후 {TIME}시간 동안 길드에 가입할 수 없습니다./n남은시간 : {REMAIN_TIME}
                    .Replace("{TIME}", span.TotalHours.ToString("N0"))
                    .Replace("{REMAIN_TIME}", guildModel.RejoinTime.ToStringTime());
                UI.ConfirmPopup(title, description);
                return;
            }

            // 이미 길드에 가입되어있는지 다시 체크
            if (guildModel.HaveGuild)
            {
                UI.Close<UIGuild>();
                return;
            }

            await guildModel.RequestJoinSubmitGuildList();
            await guildModel.RequestGuildRandom();

            UI.Close<UIGuild>();
            UI.Show<UIGuildJoin>();
        }

        void OnUpdateGuildState()
        {
            if (guildModel.HaveGuild)
                UI.Close<UIGuild>();
        }
    } 
}
