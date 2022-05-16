using System.Collections.Generic;

namespace Ragnarok
{
    public class BattlePanelInfo : List<BattleOption>
    {
        public struct Settings
        {
            public PanelBuffInfo panelBuffInfo;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Settings settings)
        {
            Clear(); // 리셋

            if (settings.panelBuffInfo != null)
            {
                Add(settings.panelBuffInfo.GetBattleOption());
            }
        }
    }
}