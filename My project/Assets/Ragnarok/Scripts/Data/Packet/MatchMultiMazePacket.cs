namespace Ragnarok
{
    public class MatchMultiMazePacket : IPacket<Response>, IBattleInput
    {
        private const long COUNT_DOWN_TIME = 5000L; // 서버 카운트 다운 시간은 5초로 고정

        public int multiMazeId;
        public int channelId;
        public MultiMazePlayerPacket[] arrMultiMazePlayerPacket;
        public MatchMazeMonsterPacket[] arrMazeMonsterPacket;
        public int mapId;
        public byte monsterSpeedType;
        public MazeCubePacket[] arrMazeCubePacket;
        public RemainTime remainTime;
        public RemainTime countdownTime;
        public MazeCubePacket[] arrMazeItemPacket; // 아이템 정보 (일반 멀티미로에서만 사용)
        public MazeCubePacket[] arrMazeItem1Packet; // 아이템 정보 (이벤트 멀티미로에서만 사용)
        public MazeCubePacket[] arrMazeItem2Packet; // 아이템 정보 (이벤트 멀티미로에서만 사용)

        public void SetMultiMazeId(int multiMazeId)
        {
            this.multiMazeId = multiMazeId;
        }

        public void Initialize(Response response)
        {
            channelId = response.GetInt("1"); // 채널
            arrMultiMazePlayerPacket = response.GetPacketArray<MultiMazePlayerPacket>("2"); // 다른 유저 정보
            arrMazeMonsterPacket = response.GetPacketArray<MatchMazeMonsterPacket>("3"); // 몬스터 정보
            mapId = response.GetInt("6"); // 맵 id
            monsterSpeedType = response.GetByte("7"); // 몬스터 속도디버프 여부 (1이 오면 모든 몬스터를 70% 속도로 변경)

            if (response.ContainsKey("8"))
            {
                arrMazeCubePacket = response.GetPacketArray<MazeCubePacket>("8"); // 퀘스트 코인 정보
            }

            long startTime = response.GetLong("9"); // 현재시각
            long endTime = response.GetLong("10"); // 종료시각
            remainTime = endTime - startTime;
            countdownTime = COUNT_DOWN_TIME;

            if (response.ContainsKey("12"))
            {
                arrMazeItemPacket = response.GetPacketArray<MazeCubePacket>("12"); // 아이템 정보
            }

            if (response.ContainsKey("13"))
            {
                arrMazeItem1Packet = response.GetPacketArray<MazeCubePacket>("13"); // 아이템 정보
            }

            if (response.ContainsKey("14"))
            {
                arrMazeItem2Packet = response.GetPacketArray<MazeCubePacket>("14"); // 아이템 정보
            }
        }
    }
}