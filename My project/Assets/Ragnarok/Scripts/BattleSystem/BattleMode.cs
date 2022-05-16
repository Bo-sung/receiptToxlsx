namespace Ragnarok
{
    /// <summary>
    /// Entry 설정 필수 <see cref="BattleEntry.Factory"/>
    /// </summary>
    public enum BattleMode
    {
        Stage = 1, // 모험
        Defence,   // 비공정 습격(디펜스)
        WorldBoss, // 무한의 공간(월드보스)
        League,    // 대전
        Lobby,     // 거래소
        CentralLab, // 중앙실험실
        ClickerDungeon, // 클리커던전 (제니,경험치 던전)
        SpecialDungeon, // 미로-탑 (길드 미로)
        Challenge, // 챌린지 (더미)
        Siege, // 공성전 (더미)
        TamingMaze, // 테이밍 미로
        MultiMazeLobby, // 멀티미로대기방
        MultiMaze, // 멀티미로
        MultiBossMaze, // 멀티미로보스
        ScenarioMaze, // 시나리오 미로
        ScenarioMazeBoss, // 시나리오 미로 - 보스전
        Duel, // 듀얼
        MatchMultiMaze, // 매칭-멀티미로
        MatchMultiMazeBoss, // 매칭-멀티미로보스
        Prologue, // 프롤로그
        FreeFight, // 난전
        GuildLobby, // 길드 로비
        GuildAttack, // 길드 습격
        ChristmasMatchMultiMaze, // 크리스마스 매칭-멀티미로
        ChristmasFreeFight, // 크리스마스 난전
        WaterBombFreeFight, // 물폭탄 난전
        EndlessTower, // 엔들리스타워
        ForestMaze, // 미궁숲
        ForestMazeBoss, // 미궁숲보스
        TimePatrol, // 타임패트롤
        GuildBattle, // 길드전
        DarkMaze, // 이벤트미로-암흑
        GateMaze, // 게이트 미로
        GateBoss, // 게이트 보스
        GateWorldBoss, // 게이트 월드보스

#if UNITY_EDITOR
        Test,
#endif
    }

    public static class BattleModeExtensions
    {
        /// <summary>
        /// 재접속이 일어날 수 있는 BattleMode
        /// </summary>
        public static bool IsReconnectableMode(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.Stage:
                case BattleMode.Lobby:
                case BattleMode.MultiMazeLobby:
                case BattleMode.FreeFight:
                case BattleMode.GuildLobby:
                case BattleMode.WaterBombFreeFight:
                case BattleMode.TimePatrol:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 재접속이 일어났을 때 저장된 셰어 사용하는 BattleMode
        /// </summary>
        public static bool IsReuseSavedSharingCharacter(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.Stage:
                case BattleMode.Lobby:
                case BattleMode.MultiMazeLobby:
                case BattleMode.GuildLobby:
                    return true;
            }

            // FreeFight 의 경우 서버가 바뀌었더라도 셰어캐릭터 재사용을 하지 않는다.
            return false;
        }

        /// <summary>
        /// 재접속 진입을 위한 프로토콜
        /// </summary>
        public static bool IsEnterProtocol(this Protocol protocol)
        {
            // 게임에 입장
            if (protocol == Protocol.JOIN_GAME_MAP)
                return true;

            // 자동사냥 스테이지 입장 요청
            if (protocol == Protocol.REQUEST_AUTO_STAGE_ENTER)
                return true;

            // 개인 상점 로비 입장
            if (protocol == Protocol.REQUEST_TRADEPRIVATE_ENTERROOM)
                return true;

            // 멀티 미로 대기방 입장
            if (protocol == Protocol.REQUEST_MULMAZE_WAITINGROOM_JOIN)
                return true;

            // 난전 입장
            if (protocol == Protocol.REQUEST_FF_ROOM_JOIN)
                return true;

            // 길드 로비 입장
            if (protocol == Protocol.REQUEST_GUILDUSER_ENTERROOM)
                return true;

            // 이벤트난전 입장
            if (protocol == Protocol.REQUEST_FF_EVENTROOM_JOIN)
                return true;

            // 타임패트롤 입장
            if (protocol == Protocol.REQUEST_TP_STAGE_ENTER)
                return true;

            return false;
        }

        /// <summary>
        /// 던전일 경우 영웅 UI, 스킬 UI 진입 불가능 처리
        /// </summary>
        public static bool IsDungeon(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.Stage:
                case BattleMode.Lobby:
                case BattleMode.MultiMazeLobby:
                case BattleMode.GuildLobby:
                case BattleMode.TimePatrol:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 스킬 거리 제한 없음
        /// </summary>
        public static bool IsAntiChaseSkill(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.MultiBossMaze:
                case BattleMode.ScenarioMazeBoss:
                case BattleMode.MatchMultiMazeBoss:
                case BattleMode.ForestMazeBoss:
                case BattleMode.GateBoss:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 스킬 수동 여부
        /// </summary>
        public static bool IsAntiAutoSkill(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.SpecialDungeon:
                case BattleMode.WorldBoss:
                case BattleMode.Defence:
                case BattleMode.FreeFight:
                case BattleMode.GuildAttack:
                case BattleMode.ChristmasFreeFight:
                case BattleMode.WaterBombFreeFight:
                case BattleMode.GuildBattle:
                case BattleMode.GateWorldBoss:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 대미지 독립 처리
        /// </summary>
        public static bool IsIndependenceDamage(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.SpecialDungeon:
                case BattleMode.FreeFight:
                case BattleMode.GuildAttack:
                case BattleMode.ChristmasFreeFight:
                case BattleMode.WaterBombFreeFight:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 고정 최대 체력
        /// </summary>
        public static bool IsFixedMaxHp(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.SpecialDungeon:
                case BattleMode.ScenarioMaze:
                case BattleMode.FreeFight:
                case BattleMode.GuildAttack:
                case BattleMode.ChristmasFreeFight:
                case BattleMode.WaterBombFreeFight:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 스킬 포인트 사용
        /// </summary>
        public static bool IsUseSkillPoint(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.ScenarioMaze:
                case BattleMode.ScenarioMazeBoss:
                case BattleMode.MultiMaze:
                case BattleMode.MultiBossMaze:
                case BattleMode.WorldBoss:
                case BattleMode.Defence:
                case BattleMode.MatchMultiMaze:
                case BattleMode.MatchMultiMazeBoss:
                case BattleMode.Stage:
                case BattleMode.FreeFight:
                case BattleMode.GuildAttack:
                case BattleMode.ChristmasFreeFight:
                case BattleMode.WaterBombFreeFight:
                case BattleMode.EndlessTower:
                case BattleMode.TimePatrol:
                case BattleMode.ForestMaze:
                case BattleMode.ForestMazeBoss:
                case BattleMode.GuildBattle:
                case BattleMode.GateMaze:
                case BattleMode.GateBoss:
                case BattleMode.GateWorldBoss:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 출석 체크 표시할 모드
        /// </summary>
        public static bool IsDailyCheckMode(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.Stage:
                case BattleMode.Lobby:
                case BattleMode.MultiMazeLobby:
                case BattleMode.TimePatrol:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 멀티로비에서 다른 Mode 로 변경될 때, Exit 스킵하는 mode
        /// </summary>
        public static bool IsSkipServerChangeFromMultiLobby(this BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.ScenarioMaze:
                case BattleMode.MultiMaze:
                case BattleMode.MultiMazeLobby:
                case BattleMode.MatchMultiMaze:
                case BattleMode.FreeFight:
                case BattleMode.ChristmasMatchMultiMaze:
                case BattleMode.ChristmasFreeFight:
                case BattleMode.WaterBombFreeFight:
                case BattleMode.ForestMaze:
                case BattleMode.DarkMaze:
                case BattleMode.GateMaze:
                    return true;
            }
            return false;
        }
    }
}