namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleMp"/>
    /// </summary>
    public class BattleMpPresenter : ViewPresenter
    {
        PlayerEntity playerEntity;

        // <!-- Event --!>
        public event UnitEntity.ChangeHPEvent OnChangeMP
        {
            add { playerEntity.OnChangeMP += value; }
            remove { playerEntity.OnChangeMP -= value; }
        }

        public BattleMpPresenter()
        {
            playerEntity = Entity.player;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public int GetCurMp()
        {
            return playerEntity.CurMp;
        }

        public int GetMaxMp()
        {
            return playerEntity.MaxMp;
        }
    }
}