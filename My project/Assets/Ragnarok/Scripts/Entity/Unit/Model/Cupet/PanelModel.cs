namespace Ragnarok
{
    public class PanelModel : UnitModel<CupetEntity>
    {
        PanelBuffInfo panelBuffInfo;

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Reset()
        {
            panelBuffInfo = null;
        }

        /// <summary>
        /// 패널 기본 세팅
        /// </summary>
        public void Initialize(PanelBuffInfo panelBuffInfo)
        {
            this.panelBuffInfo = panelBuffInfo;
        }

        /// <summary>
        /// 패널 정보 반환
        /// </summary>
        public PanelBuffInfo GetPanelBuffInfo()
        {
            return panelBuffInfo;
        }

        /// <summary>
        /// 패널 버프 정보 반환
        /// </summary>
        public BattleOption GetBattleOption()
        {
            return panelBuffInfo.GetBattleOption();
        }
    }
}