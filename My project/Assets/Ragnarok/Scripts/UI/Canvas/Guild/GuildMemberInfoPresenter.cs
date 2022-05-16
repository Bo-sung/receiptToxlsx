namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildMemberInfo"/>
    /// </summary>
    public class GuildMemberInfoPresenter : ViewPresenter
    {
        public interface IView
        {
            void SetUserIcon(string name);
            void SetJobIcon(string name);
            void SetUserName(string name);
            void SetJobName(string name);
            void SetBtnAppointName(string name);
            void SetEnabledBtnAppoint(bool isEnable);
            void SetEnabledBtnHandOver(bool isEnable);
            void SetEnabledBtnKick(bool isEnable);
            void CloseUI();
        }

        private readonly IView view;
        private readonly GuildModel guild;

        private GuildMemberInfo info => guild.SelectGuildMemberInfo;

        public GuildMemberInfoPresenter(IView view)
        {
            this.view = view;
            guild = Entity.player.Guild;
        }

        public override void AddEvent()
        {        
            
        }

        public override void RemoveEvent()
        {
            
        }

        public void SetView()
        {
            view.SetUserIcon(info.ProfileName);
            view.SetJobIcon(info.Job.GetJobIcon());
            string name = LocalizeKey._33080.ToText()
                .Replace("{LEVEL}",info.JobLevel.ToString())
                .Replace("{NAME}",info.Name)
                .Replace("{ID}",info.ID); // Lv. {LEVEL} {NAME} (ID:{ID})
            view.SetUserName(name);
            string jobName = LocalizeKey._33081.ToText()
                .Replace("{JOB_NAME}",info.Job.GetJobName()); // 직업 : {JOB_NAME}
            view.SetJobName(jobName);

            if(guild.GuildPosition == GuildPosition.Master)
            {
                view.SetEnabledBtnAppoint(true);
                view.SetEnabledBtnHandOver(true);
                view.SetEnabledBtnKick(true);
                if (info.GuildPosition == GuildPosition.Member)
                {
                    view.SetBtnAppointName(LocalizeKey._33082.ToText()); // 부길드장\n임명
                }
                else
                {
                    view.SetBtnAppointName(LocalizeKey._33083.ToText()); // 부길드장\n해임
                }
            }
            else if(guild.GuildPosition == GuildPosition.PartMaster)
            {
                view.SetEnabledBtnAppoint(false);
                view.SetEnabledBtnHandOver(false);
                view.SetBtnAppointName(LocalizeKey._33082.ToText()); // 부길드장\n임명
                if (info.GuildPosition == GuildPosition.Member)
                {
                    view.SetEnabledBtnKick(true);
                }
                else
                {
                    view.SetEnabledBtnKick(false);
                }
            }
            else
            {
                view.SetEnabledBtnAppoint(false);
                view.SetEnabledBtnHandOver(false);
                view.SetEnabledBtnKick(false);
                view.SetBtnAppointName(LocalizeKey._33082.ToText()); // 부길드장\n임명
            }
        }

        /// <summary>
        /// 부길드장 임명/해임
        /// </summary>
        public async void OnClickedBtnAppoint()
        {
            await guild.RequestGuildGrantPartMaster(info);
            SetView();
        }

        /// <summary>
        /// 길드 양도
        /// </summary>
        public async void OnClickedBtnHandOver()
        {
            await guild.RequestGuildMasterChange(info);
            view.CloseUI();
        }

        /// <summary>
        /// 길드 추방
        /// </summary>
        public async void OnClickedBtnKick()
        {
            if (info.GuildOutRemainTime.ToRemainTime() > 0f) // 길드 탈퇴 쿨타임이 남아있는 경우
            {
                UI.ConfirmPopup(LocalizeKey._33100.ToText().Replace(ReplaceKey.TIME, info.GuildOutRemainTime.ToStringTime())); // 길드 가입 후 일정 시간이 지나야 가능합니다.\n\n남은 시간 : {TIME}
                return;
            }

            await guild.RequestGuildKick(info);
            view.CloseUI();
        }
    }
}
