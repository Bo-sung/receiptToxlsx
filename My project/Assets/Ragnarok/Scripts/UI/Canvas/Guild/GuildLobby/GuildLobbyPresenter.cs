namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildLobby"/>
    /// </summary>
    public sealed class GuildLobbyPresenter : ViewPresenter
    {
        // <!-- Managers --!>
        private readonly GuildSquareManager guildSquareManager;

        // <!-- Event --!>
        public event System.Action OnUpdateGuildAttackStartTime
        {
            add { guildSquareManager.OnUpdateGuildAttackStartTime += value; }
            remove { guildSquareManager.OnUpdateGuildAttackStartTime -= value; }
        }

        public event System.Action OnUpdateCreateEmperium
        {
            add { guildSquareManager.OnUpdateCreateEmperium += value; }
            remove { guildSquareManager.OnUpdateCreateEmperium -= value; }
        }

        public GuildLobbyPresenter()
        {
            guildSquareManager = GuildSquareManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 길드습격 시작 시간
        /// </summary>
        public System.DateTime GetStartTime()
        {
            return guildSquareManager.GuildAttackStartTime;
        }
    }
}