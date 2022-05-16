namespace Ragnarok
{
    public enum AgentType
    {
        None = 0,
        CombatAgent = 1,
        ExploreAgent = 2,
        /// <summary>
        /// 오토셰어링 (클라전용)
        /// </summary>
        AutoSharingAgent = 3,
    }

    public static class AgentTypeExtensions
    {
        public static string GetIconName(this AgentType type)
        {
            switch (type)
            {
                case AgentType.CombatAgent:
                    return "Ui_Common_Info";
                case AgentType.ExploreAgent:
                    return "Ui_Common_Agent";
            }

            return string.Empty;
        }
    }
}
