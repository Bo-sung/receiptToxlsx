using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildDismissal"/>
    /// </summary>
    public class GuildDismissalPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GuildModel guildModel;

        // <!-- Repositories --!>
        public readonly int guildMasterDismissalDays;

        // <!-- Events --!>
        public event System.Action OnSuccessGuildMasterGet;

        public GuildDismissalPresenter()
        {
            guildModel = Entity.player.Guild;
            guildMasterDismissalDays = BasisType.GUILD_MASTER_NOCONNECTION_DAYS.GetInt();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void RequestMasterDismissal()
        {
            RequestGuildMasterGet().WrapNetworkErrors();
        }

        public bool CanMasterDismissal()
        {
            return guildModel.CanMasterDismissal;
        }

        private async Task RequestGuildMasterGet()
        {
            bool isSuccess = await guildModel.RequestGuildMasterGet(); // 길드 권한 가져오기
            if (!isSuccess)
                return;

            OnSuccessGuildMasterGet?.Invoke();
        }
    }
}