using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleShareForceStatusOptionInfo : List<BattleOption>
    {
        public struct Settings
        {
            public IEnumerable<ShareStatBuildUpData> shareForces;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Settings settings)
        {
            Clear(); // 리셋

            if (settings.shareForces == null)
                return;

            foreach (var item in settings.shareForces)
            {
                Add(item.GetBattleOption());
            }
        }
    }
}