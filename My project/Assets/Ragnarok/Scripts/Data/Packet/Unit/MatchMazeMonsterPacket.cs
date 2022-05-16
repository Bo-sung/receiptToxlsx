namespace Ragnarok
{
    public class MatchMazeMonsterPacket : MazeMonsterPacket
    {
        [System.Flags]
        public enum MatchMazeMonsterType : byte
        {
            Suicide = 1 << 0,
            LosePlayerHp = 1 << 1,
            LosePlayerCoin = 1 << 2,
            FreezePlayer = 1 << 3,
        }

        public MatchMazeMonsterType matchMazeMonsterType;

        public override void Initialize(Response response)
        {
            base.Initialize(response);

            matchMazeMonsterType = response.GetByte("7").ToEnum<MatchMazeMonsterType>();
        }
    }
}