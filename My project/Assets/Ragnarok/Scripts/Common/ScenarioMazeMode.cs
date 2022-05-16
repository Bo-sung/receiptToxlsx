namespace Ragnarok
{
    public enum ScenarioMazeMode
    {
        /// <summary>
        /// NPC 대화 후, 멀티 미로와 같은 게임을 시작 (퀘스트 코인 수집 후 보스전투 가능)
        /// </summary>
        QuestCoin = 1,
        
        /// <summary>
        /// NPC 대화 후, 게임 클리어
        /// </summary>
        Dialog = 2,

        /// <summary>
        /// 제니 큐브 3개 격파 후 데비루치 접촉 시 클리어 (보스몹 O, 쫄몹 X)
        /// </summary>
        ZenyCube = 3,

        /// <summary>
        /// Zeny타입과 동일 
        /// </summary>
        ExpCube = 4,

        /// <summary>
        /// Battle과 동일, 이속 포션 획득 시 이속버프
        /// </summary>
        SpeedItem = 5,

        /// <summary>
        /// 첫번째 튜토리얼 (연출 및 죽지 않음, 나가기버튼 없음)
        /// </summary>
        FirstTutorial = 6,

        /// <summary>
        /// 두번째 튜토리얼 (죽었을 때 다시 시작, 나가기버튼 없음)
        /// </summary>
        SecondTutorial = 7,

        /// <summary>
        /// 상태이상 시나리오 미로 (대화 후 배회하는 몬스터와 퀘스트 코인 등장, 몬스터는 4가지 속성 보유, 코인 전부 수집하면 즉시 보스전 입장)
        /// </summary>
        CrowdControl = 8,
    }

    public static class ScenarioMazeModeExtensions
    {
        /// <summary>
        /// 시나리오미로 타입에 해당하는 오픈컨텐츠 연출 타입
        /// </summary>
        public static ContentType GetOpenContentType(this ScenarioMazeMode mode)
        {
            switch (mode)
            {
            }

            return default;
        }
    }
}