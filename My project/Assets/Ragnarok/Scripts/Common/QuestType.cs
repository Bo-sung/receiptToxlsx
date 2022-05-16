using UnityEngine;

namespace Ragnarok
{
    public enum QuestType : short
    {
        /// <summary>
        /// 몬스터 사냥 수
        /// </summary>
        MONSTER_KILL = 1,

        /// <summary>
        /// 특정 몬스터 사냥 수 (보스전용로 사용함)
        /// <see cref="MonsterData.id"/>
        /// </summary>
        MONSTER_KILL_TARGET = 2,

        /// <summary>
        /// 몬스터 크기별 사냥 수
        /// <see cref="UnitSizeType"/>
        /// </summary>
        MONSTER_KILL_SIZE = 3,

        /// <summary>
        /// 몬스터 속성별 사냥 수
        /// <see cref="ElementType"/>
        /// </summary>
        MONSTER_KILL_ELEMENT = 4,

        /// <summary>
        /// 일일 퀘스트 클리어 (오직 하나)
        /// </summary>
        DAILY_QUEST_CLEAR = 9,

        /// <summary>
        /// 특정퀘스트 완료 횟수
        /// <see cref="QuestCategory"/>
        /// </summary>
        QUESTE_TYPE_CLEAR_COUNT = 10,

        /// <summary>
        /// 제니 획득
        /// </summary>
        ZENY_GAIN = 11,

        /// <summary>
        /// 제니 소모
        /// </summary>
        ZENY_USE = 12,

        /// <summary>
        /// 특정 아이템 획득
        /// <see cref="ItemData.id"/>
        /// </summary>
        ITEM_GAIN = 14,

        /// <summary>
        /// 아이템 종류 획득 개수
        /// <see cref="ItemGroupType"/>
        /// </summary>
        ITEM_GAIN_TYPE = 15,

        /// <summary>
        /// 특정 아이템 사용 횟수
        /// <see cref="ItemData.id"/>
        /// </summary>
        ITEM_USE = 17,

        /// <summary>
        /// 아이템 종류 사용 횟수
        /// <see cref="ItemGroupType"/>
        /// </summary>
        ITEM_USE_TYPE = 18,

        /// <summary>
        /// 아이템 제작 횟수
        /// </summary>
        ITEM_MAKING = 19,

        /// <summary>
        /// 장비템 제련 횟수
        /// </summary>
        ITEM_UPGRADE = 20,

        /// <summary>
        /// 장비템 제련 성공 횟수
        /// </summary>
        [System.Obsolete]
        ITEM_UPGRADE_YES = 21,

        /// <summary>
        /// 장비템 제련 실패 횟수
        /// </summary>
        [System.Obsolete]
        ITEM_UPGRADE_NO = 22,

        /// <summary>
        /// 장비템 특정 제련도 도달
        /// </summary>
        ITEM_UPGRADE_MAX = 23,

        /// <summary>
        /// 장비템 특정 성급 제작 횟수
        /// <see cref="ItemData.rating"/>
        /// </summary>
        ITEM_STAR_UPGRADE = 24,

        /// <summary>
        /// 장비템 분해 횟수
        /// </summary>
        ITEM_BREAK = 25,

        /// <summary>
        /// 특정 아이템 제작 횟수
        /// <see cref="ItemData.id"/>
        /// </summary>
        MAKE_ITEM_COUNT = 26,

        /// <summary>
        /// 특정 큐펫 획득 체크
        /// </summary>
        [System.Obsolete]
        CUPET_FOUND = 27,

        /// <summary>
        /// 큐펫 수집수 체크
        /// </summary>
        [System.Obsolete]
        CUPET_MAX = 28,

        /// <summary>
        /// 특정 성급 큐펫 수집수 체크
        /// </summary>
        [System.Obsolete]
        CUPET_MAX_STAR = 29,

        /// <summary>
        /// 큐펫 성급 업그레이드 성공 횟수
        /// </summary>
        [System.Obsolete]
        CUPET_STAR_UPGRADE = 30,

        /// <summary>
        /// 아이템 종류 제작 개수
        /// <see cref="ItemGroupType"/>
        /// </summary>
        ITEM_MAKING_TYPE = 31,

        /// <summary>
        /// 직업 레벨 도달
        /// </summary>
        CHARACTER_MAX_JOB_LEVEL = 34,

        /// <summary>
        /// 일반 레벨 도달
        /// </summary>
        CHARACTER_MAX_BASIC_LEVEL = 35,

        /// <summary>
        /// 일반 레벨업 횟수
        /// </summary>
        CHARACTER_BASIC_LEVEL_MAX = 36,

        /// <summary>
        /// 전승 횟수
        /// </summary>
        CHARACTER_RETURN = 37,

        /// <summary>
        /// 특정 타입 던전 클리어 횟수
        /// <see cref="DungeonType"/>
        /// </summary>
        DUNGEON_TYPE_CLEAR_COUNT = 38,

        /// <summary>
        /// 특정 몬스터 그룹 처치 수
        /// <see cref="MonsterData.monster_group"/>
        /// </summary>
        MONSTER_GROUP_KILL_COUNT = 42,

        /// <summary>
        /// 일일 퀘스트 모두 완료 횟수
        /// </summary>
        QUESTE_TYPE_CLEAR_COUNT_2 = 45,

        /// <summary>
        /// 장비템 특정 제련도 도달 횟수
        /// </summary>
        ITEM_LEVEL_UPGRADE = 46,

        /// <summary>
        /// 장비 장착 1회
        /// </summary>
        ITEM_EQUIP = 51,

        /// <summary>
        /// 해당 차수 전직 도달
        /// </summary>
        JOB_DEGREE = 52,

        /// <summary>
        /// 비밀 상점 구매 횟수
        /// </summary>
        SECRET_SHOP_BUY_COUNT = 53,

        /// <summary>
        /// 스킬 투자 횟수
        /// </summary>
        SKILL_LEVEL_UP_COUNT = 58,

        /// <summary>
        /// 장비에 카드 인챈트 횟수
        /// </summary>
        CARD_ENCHANT_COUNT = 59,

        /// <summary>
        /// 특정 월드 보스 처치(전투 참여)
        /// <see cref="WorldBossData.id"/>
        /// </summary>
        WORLD_BOSS_TYPE_COUNT = 60,

        /// <summary>
        /// 대전 입장 횟수
        /// </summary>
        PVE_COUNT = 61,

        /// <summary>
        /// 특정 시나리오 미로 클리어
        /// <see cref="ScenarioMazeData.id"/>
        /// </summary>
        SCENARIO_MAZE_ID_CLEAR_COUNT = 62,

        /// <summary>
        /// 특정 멀티 미로 클리어 횟수
        /// <see cref="MultiMazeData.chapter"/>
        /// </summary>
        MULTI_MAZE_CHAPTER_CLEAR_COUNT = 63,

        /// <summary>
        /// MVP 처치 횟수
        /// </summary>
        MVP_CLEAR_COUNT = 64,

        /// <summary>
        /// 특정 필드에서 MVP 처치 횟수
        /// <see cref="StageData.id"/>
        /// </summary>
        FIELD_ID_MVP_CLEAR_COUNT = 65,

        /// <summary>
        /// 특정 필드에서 rare_type3번 MVP 처치 횟수
        /// <see cref="StageData.id"/>
        /// <see cref="MvpData.rare_type"/>
        /// </summary>
        FILED_ID_RARE_3_MVP_CLEAR_COUNT = 66,

        /// <summary>
        /// 셰어 캐릭터 고용 횟수
        /// </summary>
        SHARE_CHAR_USE_COUNT = 67,

        /// <summary>
        /// 특정 필드에서 셰어+동료 집결 횟수
        /// <see cref="StageData.id"/>
        /// </summary>
        FIELD_ID_ASSEMBLE_COUNT = 68,

        /// <summary>
        /// 카드 레벨업 횟수
        /// </summary>
        CARD_LEVEL_COUNT = 69,

        /// <summary>
        /// 카드 레벨
        /// </summary>
        CARD_LEVEL = 70,

        /// <summary>
        /// 동료 획득 횟수
        /// </summary>
        AGENT_COUNT = 71,

        /// <summary>
        /// 동료 합성 횟수
        /// </summary>
        AGENT_COMPOSE_COUNT = 72,

        /// <summary>
        /// 동료 파견 보내기 횟수
        /// </summary>
        AGENT_EXPLORE_COUNT = 73,

        /// <summary>
        /// 특정던전 입장 횟수
        /// <see cref="DungeonType"/>
        /// </summary>
        DUNGEON_TYPE_COUNT = 74,

        /// <summary>
        /// 듀얼 도전 횟수
        /// </summary>
        DUEL_COUNT = 75,

        /// <summary>
        /// 특정 챕터 듀얼 보상 받기 횟수
        /// </summary>
        CHAPTER_DUEL_CLEAR_COUNT = 76,

        /// <summary>
        /// 1랭크 특정 장비 강화도 도달 횟수
        /// </summary>
        ITEM_RANK_1_LEVEL_COUNT = 77,

        /// <summary>
        /// 1랭크 특정 장비 강화도 도달 횟수
        /// </summary>
        ITEM_RANK_2_LEVEL_COUNT = 78,

        /// <summary>
        /// 1랭크 특정 장비 강화도 도달 횟수
        /// </summary>
        ITEM_RANK_3_LEVEL_COUNT = 79,

        /// <summary>
        /// 1랭크 특정 장비 강화도 도달 횟수
        /// </summary>
        ITEM_RANK_4_LEVEL_COUNT = 80,

        /// <summary>
        /// 1랭크 특정 장비 강화도 도달 횟수
        /// </summary>
        ITEM_RANK_5_LEVEL_COUNT = 81,

        /// <summary>
        /// 장비 초월 특정 레벨 도달 횟수
        /// </summary>
        ITEM_TIER_UP_COUNT = 82,

        /// <summary>
        /// 특정 챕터 멀티 미로 도전 횟수
        /// <see cref="MultiMazeData.chapter"/>
        /// </summary>
        MULTI_MAZE_ENTER_COUNT = 83,

        /// <summary>
        /// 특정 난전 도전 횟수
        /// <see cref="FreeFightRewardDataManager.event_type"/>
        /// </summary>
        FREE_FIGHT_ENTER_COUNT = 84,

        /// <summary>
        /// 길드 습격 도전 횟수
        /// </summary>
        GUILD_ATTACK_ENTER_COUNT = 85,

        /// <summary>
        /// 길드 습격 기부 횟수
        /// </summary>
        GUILD_ATTACK_DONATION_COUNT = 86,

        /// <summary>
        /// 이벤트 모드 레벨 별 클리어 횟수
        /// <see cref="StageMode.Event"/>
        /// <see cref="StageMode.Challenge"/>
        /// </summary>
        EVENT_STAGE_LEVEL_CLEAR_COUNT = 87,

        /// <summary>
        /// 이벤트 모드 챕터 별 클리어 횟수
        /// <see cref="StageData.chapter"/>
        /// <see cref="StageMode.Event"/>
        /// <see cref="StageMode.Challenge"/>
        /// </summary>
        EVENT_STAGE_CHAPTER_CLEAR_COUNT = 88,

        /// <summary>
        /// 엔들리스 타워 특정 층 클리어 횟수
        /// </summary>
        ENDLESS_TOWER_FLOOR_CLEAR_COUNT = 89,

        /// <summary>
        /// 타임패트롤 구역 보스 처치 횟수
        /// </summary>
        TIME_PATROL_ZONE_BOOS_KILL_COUNT = 90,

        /// <summary>
        /// 멀티 미로 클리어 10회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_10 = 91,

        /// <summary>
        /// 멀티 미로 클리어 15회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_15 = 92,

        /// <summary>
        /// 멀티 미로 클리어 20회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_20 = 93,

        /// <summary>
        /// 멀티 미로 클리어 25회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_25 = 94,

        /// <summary>
        /// 멀티 미로 클리어 30회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_30 = 95,

        /// <summary>
        /// 멀티 미로 클리어 35회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_35 = 96,

        /// <summary>
        /// 멀티 미로 클리어 40회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_40 = 97,

        /// <summary>
        /// 멀티 미로 클리어 45회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_45 = 98,

        /// <summary>
        /// 멀티 미로 클리어 50회
        /// </summary>
        MULTI_MAZE_CLEAR_COUNT_50 = 99,

        /// <summary>
        /// 길드전 도전 횟수
        /// </summary>
        GUILD_BATTLE_ENTER_COUNT = 100,

        /// <summary>
        /// 특정 게이트 도전 횟수
        /// <see cref="GateData.id"/>
        /// </summary>
        GATE_ENTER_COUNT = 101,

        /// <summary>
        /// 듀얼:아레나 승리 횟수
        /// </summary>
        DUEL_ARENA_CLEAR_COUNT = 102,

        /// <summary>
        /// 냥다래 나무 모든 보상 획득 횟수
        /// </summary>
        CONNECT_TIME_ALL_REWARD = 103,

        /// <summary>
        /// 업데이트 예정 (메인퀘스트 막을때 사용)
        /// </summary>
        UPDATE = 999
    }

    public static class QuestTypeExtensions
    {
        public static string ToText(this QuestType type, int questValue, int conditionValue)
        {
            switch (type)
            {
                case QuestType.MONSTER_KILL:
                    return LocalizeKey._54000.ToText() // 몬스터 {COUNT}마리 사냥
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.MONSTER_KILL_TARGET:
                    return LocalizeKey._54001.ToText() // {MONSTER_NAME} {COUNT}마리 사냥
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{MONSTER_NAME}", MonsterDataManager.Instance.Get(conditionValue).name_id.ToText());

                case QuestType.MONSTER_KILL_SIZE:
                    return LocalizeKey._54002.ToText() // {UNIT_SIZE} 몬스터 {COUNT}마리 사냥
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{UNIT_SIZE}", conditionValue.ToEnum<UnitSizeType>().ToText());

                case QuestType.MONSTER_KILL_ELEMENT:
                    return LocalizeKey._54003.ToText() // {ELEMENT_TYPE} 몬스터 {COUNT}마리 사냥
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{ELEMENT_TYPE}", conditionValue.ToEnum<ElementType>().ToText());

                case QuestType.DAILY_QUEST_CLEAR:
                    return LocalizeKey._54008.ToText(); // 일일퀘스트 모두 클리어

                case QuestType.QUESTE_TYPE_CLEAR_COUNT:
                    return LocalizeKey._54009.ToText() // {QUEST_NAME} 퀘스트 {COUNT}회 클리어
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{QUEST_NAME}", conditionValue.ToEnum<QuestCategory>().ToText());

                case QuestType.ZENY_GAIN:
                    return LocalizeKey._54010.ToText() // 제니 {COUNT} 획득
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.ZENY_USE:
                    return LocalizeKey._54011.ToText() // 제니 {COUNT} 사용
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.ITEM_GAIN:
                    return LocalizeKey._54013.ToText() // {ITEM_NAME} {COUNT}개 획득
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{ITEM_NAME}", ItemDataManager.Instance.Get(conditionValue).name_id.ToText());

                case QuestType.ITEM_GAIN_TYPE:
                    return LocalizeKey._54014.ToText() // {ITEM_TYPE} {COUNT}개 획득
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{ITEM_TYPE}", conditionValue.ToEnum<ItemGroupType>().ToText());

                case QuestType.ITEM_USE:
                    return LocalizeKey._54016.ToText() // {ITEM_NAME} {COUNT}회 사용
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{ITEM_NAME}", ItemDataManager.Instance.Get(conditionValue).name_id.ToText());

                case QuestType.ITEM_USE_TYPE:
                    return LocalizeKey._54056.ToText() // {ITEM_TYPE} {COUNT}회 사용
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{ITEM_TYPE}", conditionValue.ToEnum<ItemGroupType>().ToText());

                case QuestType.ITEM_MAKING:
                    return LocalizeKey._54018.ToText() // 아이템 {COUNT}회 제작
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.ITEM_UPGRADE:
                    return LocalizeKey._54019.ToText() // 장비 아이템 {COUNT}회 제련
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.ITEM_UPGRADE_YES:
                    return LocalizeKey._54020.ToText() // 장비 아이템 제련 {COUNT}회 성공
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.ITEM_UPGRADE_NO:
                    return LocalizeKey._54021.ToText() // 장비 아이템 제련 {COUNT}회 실패
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.ITEM_UPGRADE_MAX:
                    return LocalizeKey._54022.ToText() // 장비 아이템 {ITEM_LEVEL}제련도 도달
                        .Replace("{ITEM_LEVEL}", questValue.ToString());

                case QuestType.ITEM_STAR_UPGRADE:
                    return LocalizeKey._54023.ToText() // {RATING}성 장비 아이템 {COUNT}회 제작
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{RATING}", conditionValue.ToString());

                case QuestType.ITEM_BREAK:
                    return LocalizeKey._54024.ToText() // 장비 아이템 {COUNT}회 분해
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.MAKE_ITEM_COUNT:
                    return LocalizeKey._54039.ToText() // {ITEM_NAME} {COUNT}회 제작
                        .Replace("{ITEM_NAME}", ItemDataManager.Instance.Get(conditionValue).name_id.ToText())
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.CUPET_FOUND:
                    return LocalizeKey._54026.ToText() // {CUPET_NAME} 큐펫 {COUNT}마리 획득
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{CUPET_NAME}", CupetDataManager.Instance.Get(conditionValue).name_id.ToText());

                case QuestType.CUPET_MAX:
                    return LocalizeKey._54027.ToText() // 큐펫 {COUNT}마리 수집
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.CUPET_MAX_STAR:
                    return LocalizeKey._54028.ToText() // {RATING}성 큐펫 {COUNT}마리 수집
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{RATING}", conditionValue.ToString());

                case QuestType.CUPET_STAR_UPGRADE:
                    return LocalizeKey._54029.ToText() // 큐펫 진화 {COUNT}회 완료
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.ITEM_MAKING_TYPE:
                    return LocalizeKey._54038.ToText() // {ITEM_TYPE} {COUNT}회 제작
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{ITEM_TYPE}", conditionValue.ToEnum<ItemGroupType>().ToText());

                case QuestType.CHARACTER_MAX_JOB_LEVEL:
                    return LocalizeKey._54033.ToText() // 직업 레벨 {LEVEL} 도달
                        .Replace("{LEVEL}", questValue.ToString());

                case QuestType.CHARACTER_MAX_BASIC_LEVEL:
                    return LocalizeKey._54034.ToText() // 일반 레벨 {LEVEL} 도달
                        .Replace("{LEVEL}", questValue.ToString());

                case QuestType.CHARACTER_BASIC_LEVEL_MAX:
                    return LocalizeKey._54035.ToText() // 일반 레벨업 {COUNT}회 완료
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.CHARACTER_RETURN:
                    return LocalizeKey._54036.ToText() // {COUNT}회 전승
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.DUNGEON_TYPE_CLEAR_COUNT:
                    return LocalizeKey._54040.ToText() // {DUNGEON_TYPE} {COUNT} 회 클리어
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{DUNGEON_TYPE}", conditionValue.ToEnum<DungeonType>().ToText());

                case QuestType.MONSTER_GROUP_KILL_COUNT:
                    return LocalizeKey._54043.ToText() // {MONSTER_GROUP_NAME} {COUNT}마리 사냥
                        .Replace(ReplaceKey.COUNT, questValue.ToString())
                        .Replace("{MONSTER_GROUP_NAME}", BasisType.MONSTER_TABLE_GROUP_ID_LANG_ID.GetInt(conditionValue).ToText());

                case QuestType.QUESTE_TYPE_CLEAR_COUNT_2:
                    return LocalizeKey._54046.ToText(); // 일일 퀘스트 모두 클리어

                case QuestType.ITEM_LEVEL_UPGRADE:
                    return LocalizeKey._54047.ToText() // 장비 아이템 {ITEM_LEVEL}제련도 도달 횟수
                        .Replace("{ITEM_LEVEL}", conditionValue.ToString());

                case QuestType.ITEM_EQUIP:
                    return LocalizeKey._54052.ToText() // 장비 장착 {COUNT}회 완료
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.JOB_DEGREE:
                    return LocalizeKey._54053.ToText() // {JOB_DEGREE}차 전직 완료
                        .Replace("{JOB_DEGREE}", conditionValue.ToString());

                case QuestType.SECRET_SHOP_BUY_COUNT:
                    return LocalizeKey._54054.ToText() // 비밀상점 {COUNT}회 구매
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.SKILL_LEVEL_UP_COUNT:
                    return LocalizeKey._54060.ToText() // 스킬 {COUNT}회 레벨업
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.CARD_ENCHANT_COUNT:
                    return LocalizeKey._54061.ToText() // 카드 {COUNT}회 인챈트
                        .Replace(ReplaceKey.COUNT, questValue.ToString());

                case QuestType.WORLD_BOSS_TYPE_COUNT:
                    {
                        int monsterId = WorldBossDataManager.Instance.Get(conditionValue).monster_id;
                        int nameId = MonsterDataManager.Instance.Get(monsterId).name_id;

                        return LocalizeKey._54062.ToText()
                            .Replace(ReplaceKey.NAME, nameId.ToText()); // 월드보스 {NAME} 처치
                    }
                case QuestType.PVE_COUNT:
                    return LocalizeKey._54063.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 대전 {COUNT}회 도전

                case QuestType.SCENARIO_MAZE_ID_CLEAR_COUNT:
                    return LocalizeKey._54064.ToText()
                        .Replace(ReplaceKey.NAME, ScenarioMazeDataManager.Instance.Get(conditionValue).name_id.ToText()); // 시나리오 {NAME} 클리어

                case QuestType.MULTI_MAZE_CHAPTER_CLEAR_COUNT:
                    {
                        string name = BasisType.STAGE_TBLAE_LANGUAGE_ID.GetInt(conditionValue).ToText();

                        return LocalizeKey._54065.ToText()
                            .Replace(ReplaceKey.NAME, name)
                            .Replace(ReplaceKey.COUNT, questValue); // 멀티미로 {NAME} {COUNT} 클리어
                    }

                case QuestType.MVP_CLEAR_COUNT:
                    return LocalizeKey._54066.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // MVP {COUNT}회 처치

                case QuestType.FIELD_ID_MVP_CLEAR_COUNT:
                    return LocalizeKey._54067.ToText()
                        .Replace(ReplaceKey.NAME, StageDataManager.Instance.Get(conditionValue).name_id.ToText())
                        .Replace(ReplaceKey.COUNT, questValue); // {NAME}에서 MVP {COUNT}회 처치

                case QuestType.FILED_ID_RARE_3_MVP_CLEAR_COUNT:
                    return LocalizeKey._54068.ToText()
                        .Replace(ReplaceKey.NAME, StageDataManager.Instance.Get(conditionValue).name_id.ToText())
                        .Replace(ReplaceKey.COUNT, questValue); // {NAME}에서 레어3 MVP {COUNT}회 처치

                case QuestType.SHARE_CHAR_USE_COUNT:
                    return LocalizeKey._54069.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 셰어 캐릭터 {COUNT}회 고용

                case QuestType.FIELD_ID_ASSEMBLE_COUNT:
                    return LocalizeKey._54070.ToText()
                        .Replace(ReplaceKey.NAME, StageDataManager.Instance.Get(conditionValue).name_id.ToText())
                        .Replace(ReplaceKey.COUNT, questValue); // {NAME} 동료 {COUNT}회 집결

                case QuestType.CARD_LEVEL_COUNT:
                    return LocalizeKey._54071.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 카드 {COUNT}회 레벨업

                case QuestType.CARD_LEVEL:
                    return LocalizeKey._54072.ToText()
                        .Replace(ReplaceKey.LEVEL, conditionValue); // 카드 {LEVEL}레벨 도달

                case QuestType.AGENT_COUNT:
                    return LocalizeKey._54073.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 동료 {COUNT}명 획득

                case QuestType.AGENT_COMPOSE_COUNT:
                    return LocalizeKey._54074.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 동료 {COUNT}회 합성

                case QuestType.AGENT_EXPLORE_COUNT:
                    return LocalizeKey._54075.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 동료 {COUNT}회 파견

                case QuestType.DUNGEON_TYPE_COUNT:
                    return LocalizeKey._54076.ToText()
                        .Replace(ReplaceKey.TYPE, conditionValue.ToEnum<DungeonType>().ToText())
                        .Replace(ReplaceKey.COUNT, questValue); // {TYPE} {COUNT}회 도전

                case QuestType.DUEL_COUNT:
                    return LocalizeKey._54077.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 듀얼 {COUNT}회 도전

                case QuestType.CHAPTER_DUEL_CLEAR_COUNT:
                    return LocalizeKey._54078.ToText()
                        .Replace(ReplaceKey.NAME, BasisType.STAGE_TBLAE_LANGUAGE_ID.GetInt(conditionValue).ToText())
                        .Replace(ReplaceKey.COUNT, questValue); // {NAME} 듀얼 {COUNT}회 받기

                case QuestType.ITEM_RANK_1_LEVEL_COUNT:
                    return LocalizeKey._54079.ToText()
                        .Replace(ReplaceKey.LEVEL, conditionValue); // 1랭크 장비 {LEVEL}강화 도달 

                case QuestType.ITEM_RANK_2_LEVEL_COUNT:
                    return LocalizeKey._54080.ToText()
                        .Replace(ReplaceKey.LEVEL, conditionValue); // 2랭크 장비 {LEVEL}강화 도달 

                case QuestType.ITEM_RANK_3_LEVEL_COUNT:
                    return LocalizeKey._54081.ToText()
                        .Replace(ReplaceKey.LEVEL, conditionValue); // 3랭크 장비 {LEVEL}강화 도달 

                case QuestType.ITEM_RANK_4_LEVEL_COUNT:
                    return LocalizeKey._54082.ToText()
                        .Replace(ReplaceKey.LEVEL, conditionValue); // 4랭크 장비 {LEVEL}강화 도달 

                case QuestType.ITEM_RANK_5_LEVEL_COUNT:
                    return LocalizeKey._54083.ToText()
                        .Replace(ReplaceKey.LEVEL, conditionValue); // 5랭크 장비 {LEVEL}강화 도달 

                case QuestType.ITEM_TIER_UP_COUNT:
                    return LocalizeKey._54084.ToText()
                        .Replace(ReplaceKey.LEVEL, conditionValue); // 장비 초월 {LEVEL} 도달

                case QuestType.MULTI_MAZE_ENTER_COUNT:
                    {
                        string name = BasisType.STAGE_TBLAE_LANGUAGE_ID.GetInt(conditionValue).ToText();

                        return LocalizeKey._54085.ToText() // {NAME} 멀티 미궁 {COUNT}회 도전
                            .Replace(ReplaceKey.NAME, name)
                            .Replace(ReplaceKey.COUNT, questValue);
                    }

                case QuestType.FREE_FIGHT_ENTER_COUNT:
                    {
                        FreeFightEventType freeFightType = conditionValue.ToEnum<FreeFightEventType>();
                        FreeFightConfig config = FreeFightConfig.GetByKey(freeFightType);
                        string typeName = config == null ? freeFightType.ToString() : config.NameId.ToText();

                        return LocalizeKey._54076.ToText()
                            .Replace(ReplaceKey.TYPE, typeName)
                            .Replace(ReplaceKey.COUNT, questValue); // {TYPE} {COUNT}회 도전
                    }

                case QuestType.GUILD_ATTACK_ENTER_COUNT:
                    return LocalizeKey._54086.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 길드 습격 {COUNT}회 도전

                case QuestType.GUILD_ATTACK_DONATION_COUNT:
                    return LocalizeKey._54087.ToText()
                        .Replace(ReplaceKey.COUNT, questValue); // 길드 습격 {COUNT}회 기부

                case QuestType.EVENT_STAGE_LEVEL_CLEAR_COUNT:
                    {
                        return LocalizeKey._54088.ToText() // 이벤트 모드 Lv.{LEVEL} {COUNT}회 클리어
                            .Replace(ReplaceKey.LEVEL, conditionValue)
                            .Replace(ReplaceKey.COUNT, questValue);
                    }

                case QuestType.EVENT_STAGE_CHAPTER_CLEAR_COUNT:
                    {
                        string name = BasisType.STAGE_TBLAE_LANGUAGE_ID.GetInt(conditionValue).ToText();

                        return LocalizeKey._54089.ToText() // 이벤트 모드 {NAME} {COUNT}회 클리어
                            .Replace(ReplaceKey.NAME, name)
                            .Replace(ReplaceKey.COUNT, questValue);
                    }
                case QuestType.ENDLESS_TOWER_FLOOR_CLEAR_COUNT:
                    {
                        return LocalizeKey._54090.ToText() // 엔들리스 타워 {VALUE}층 {COUNT}회 클리어
                            .Replace(ReplaceKey.VALUE, conditionValue)
                            .Replace(ReplaceKey.COUNT, questValue);
                    }
                case QuestType.TIME_PATROL_ZONE_BOOS_KILL_COUNT:
                    {
                        return LocalizeKey._54091.ToText() // 타임패트롤 {VALUE}구역 타임키퍼{COUNT}회 처치
                            .Replace(ReplaceKey.VALUE, conditionValue.ToEnum<TImePatrolZoneType>().GetName())
                            .Replace(ReplaceKey.COUNT, questValue);
                    }
                case QuestType.MULTI_MAZE_CLEAR_COUNT_10:
                case QuestType.MULTI_MAZE_CLEAR_COUNT_15:
                case QuestType.MULTI_MAZE_CLEAR_COUNT_20:
                case QuestType.MULTI_MAZE_CLEAR_COUNT_25:
                case QuestType.MULTI_MAZE_CLEAR_COUNT_30:
                case QuestType.MULTI_MAZE_CLEAR_COUNT_35:
                case QuestType.MULTI_MAZE_CLEAR_COUNT_40:
                case QuestType.MULTI_MAZE_CLEAR_COUNT_45:
                case QuestType.MULTI_MAZE_CLEAR_COUNT_50:
                    {
                        return LocalizeKey._54092.ToText() // 멀티 미궁 보스 {COUNT}회 처치
                            .Replace(ReplaceKey.COUNT, questValue);
                    }
                case QuestType.GUILD_BATTLE_ENTER_COUNT:
                        return LocalizeKey._54093.ToText()
                            .Replace(ReplaceKey.COUNT, questValue); // 길드전 {COUNT}회 도전

                case QuestType.GATE_ENTER_COUNT:
                    return LocalizeKey._54094.ToText()
                        .Replace(ReplaceKey.NAME, GateDataManager.Instance.Get(conditionValue).name_id.ToText())
                        .Replace(ReplaceKey.COUNT, questValue); // {NAME} 게이트 {COUNT}회 도전

                case QuestType.DUEL_ARENA_CLEAR_COUNT:
                    return LocalizeKey._54095.ToText() // 듀얼 아레나 {COUNT}회 승리
                        .Replace(ReplaceKey.COUNT, questValue);

                case QuestType.CONNECT_TIME_ALL_REWARD:
                    return LocalizeKey._54096.ToText() // 냥다래 나무 모든 보상 받기 {COUNT}회
                        .Replace(ReplaceKey.COUNT, questValue);

                case QuestType.UPDATE:
                    {
                        return LocalizeKey._90045.ToText(); // 업데이트 예정입니다.
                    }

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(QuestType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }
    }
}