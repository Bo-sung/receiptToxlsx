using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class BattleBookOptionInfo : List<BattleOption>
    {
        public struct Settings
        {
            public IEnumerable<BattleOption> battleOptions;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Settings settings)
        {
            Clear(); // 리셋

            if (settings.battleOptions != null)
                AddRange(settings.battleOptions);
        }
    }
}
