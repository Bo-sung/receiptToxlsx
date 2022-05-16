namespace Ragnarok
{
    public class BattleMazeCharacterPacket : BattleCharacterPacket
    {
        public override void Initialize(Response response)
        {
            base.Initialize(response);

            PosX = response.GetFloat("26");
            PosZ = response.GetFloat("27");
            State = response.GetByte("28");
            HasMaxHp = response.ContainsKey("29");
            MaxHp = response.GetInt("29");
        }
    }
}