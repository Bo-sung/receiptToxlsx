namespace Ragnarok
{
    public sealed class GateMultiMazePacket : IPacket<Response>, IBattleInput
    {
        public BattleMazeCharacterPacket[] arrMultiMazePlayerPacket;
        public int gateId;
        public ForestMazeMonsterPacket[] arrMazeMonsterPacket;

        public long currentTime;
        public long endTime;
        public long worldBossHp;

        void IInitializable<Response>.Initialize(Response response)
        {
            int channelId = response.GetInt("1"); // 채널id
            arrMultiMazePlayerPacket = response.GetPacketArray<BattleMazeCharacterPacket>("2"); // 다른 유저 정보
            gateId = response.GetInt("3"); // 던전id
            currentTime = response.GetLong("4"); // 현재시각
            arrMazeMonsterPacket = response.GetPacketArray<ForestMazeMonsterPacket>("5"); // 몬스터 정보
            endTime = response.GetLong("6"); // 종료시각
            worldBossHp = response.GetLong("7");
        }
    }
}