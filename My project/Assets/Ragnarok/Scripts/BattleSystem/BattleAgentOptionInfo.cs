using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleAgentOptionInfo : List<BattleOption>
    {
        public struct Settings
        {
            public IEnumerable<IAgent> agents;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Settings settings)
        {
            Clear(); // 리셋

            if (settings.agents != null)
                foreach(var each in settings.agents)
                    AddRange(each.AgentData.GetBattleOptions());
        }
    }
}