using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleAgentBookOptionInfo : List<BattleOption>
    {
        public struct Settings
        {
            public IEnumerable<AgentBookState> enabledBooks;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Settings settings)
        {
            Clear(); // 리셋

            if (settings.enabledBooks != null)
                foreach (var each in settings.enabledBooks)
                    AddRange(each.BookData.GetBattleOptions());
        }
    }
}