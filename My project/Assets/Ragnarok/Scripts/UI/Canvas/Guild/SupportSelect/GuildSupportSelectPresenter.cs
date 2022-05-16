namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildSupportSelect"/>
    /// </summary>
    public class GuildSupportSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GuildModel guildModel;
        private readonly CharacterModel characterModel;

        // <!-- DATA --!>
        private readonly Buffer<GuildSupportSelectElement> supportSelectElements;

        // <!-- Event --!>
        public event System.Action OnUpdateGuildMember
        {
            add { guildModel.OnUpdateGuildMember += value; }
            remove { guildModel.OnUpdateGuildMember -= value; }
        }

        public GuildSupportSelectPresenter()
        {
            guildModel = Entity.player.Guild;
            characterModel = Entity.player.Character;

            supportSelectElements = new Buffer<GuildSupportSelectElement>();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void RequestGuildMeber()
        {
            guildModel.RequestGuildMemberList().WrapNetworkErrors();
        }

        public UIGuildSupportSelectElement.IInput[] GetArray()
        {
            GuildMemberInfo[] members = guildModel.GetGuildMemberInfos();
            foreach (var item in members)
            {
                // 자기 자신 제외
                if (item.CID == characterModel.Cid)
                    continue;

                // 사용한 길드원 필터
                if (guildModel.UsedGuildBattleSupportAgents.Contains(item.CID))
                    continue;

                supportSelectElements.Add(new GuildSupportSelectElement(item));
            }

            supportSelectElements.Sort(SortSupportAgent);

            return supportSelectElements.GetBuffer(isAutoRelease: true);
        }

        private int SortSupportAgent(GuildSupportSelectElement a, GuildSupportSelectElement b)
        {
            return b.BattleScore.CompareTo(a.BattleScore); // 전투력 높은 순
        }

        private class GuildSupportSelectElement : UIGuildSupportSelectElement.IInput
        {
            public int Cid { get; private set; }
            public int Uid { get; private set; }
            public string ProfileName { get; private set; }
            public string JobIconName { get; private set; }
            public int JobLevel { get; private set; }
            public string CharacterName { get; private set; }
            public int BattleScore { get; private set; }

            public GuildSupportSelectElement(GuildMemberInfo info)
            {
                Cid = info.CID;
                Uid = info.UID;
                ProfileName = info.ProfileName;
                JobIconName = info.Job.GetJobIcon();
                JobLevel = info.JobLevel;
                CharacterName = info.Name;
                BattleScore = info.BattleScore;
            }
        }
    }
}