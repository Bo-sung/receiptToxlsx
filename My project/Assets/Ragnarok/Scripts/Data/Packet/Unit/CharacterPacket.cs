using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 플레이중인 캐릭터 정보
    /// </summary>
    public class CharacterPacket : IPacket<Response>, CharacterModel.IInputValue, StatusModel.IInputValue, GuildModel.IInputValue
    {
        public int cid;
        public string name;
        public long tutorialFinish;
        public long zeny;
        public byte job;
        public byte gender;
        public short level;
        public int expPoint;
        public short jobLevel;
        public long jobExp;
        public short str;
        public short agi;
        public short vit;
        public short inte;
        public short dex;
        public short lux;
        public short rebirthCount;
        public short remainStatPoint;
        public short remainSkillPoint;
        public short finalStageId; // 도달한 스테이지 ID
        public short autoStageId;
        public int invenWeight;
        public short dungeonFreeTicket;
        public int dungeonCount;
        public short centralLabFreeTicket;
        public int centralLabTryCount;
        public short invenWeightBuyCount;
        public short rebirthStatAccruePoint; // 전승 누적 스탯 포인트       
        public int nameChangeCnt; // 이름 변경한 횟수      
        public short scenario_step; // 챕터 달설 스탭
        public short job_level_step; // 레벨 달성 스탭
        public int dungeonFreeReward; // 던전3종 일일 무료보상 수령여부
        public int shareTempExp; // 셰어바이스 레벨업에 사용된 경험치
        public long shareLevelupRemainTime; // 셰어바이스 레벨업까지 남음 경험치
        public string cidHex; // cid (Hex)
        public int guildRejoinTime;
        private int guild_id;
        private string guild_name;
        private int guild_emblem;
        private byte guild_position;
        public int guild_coin;
        private byte guild_quest_reward_cnt; // 길드 퀘스트 보상 받은 횟수
        private long guild_skill_buy_dt; // 길드 스킬 구입 쿨타임
        private byte guild_skill_buy_count; // 오늘 냥다래로 구입한 길드스킬 구입 카운드

        public int roPoint;
        public string clearDungeonGroupIdText;
        public int guildCoin;
        public short dayWorldBossTicket;
        public int dayWorldBossCount;
        public short dayDefDungeonTicket;
        public int dayDefDungeonCount;
        public short dayPveTicket;
        public int dayPveCount;
        public int worldBossTicketRemainTime;
        public string worldBossAlarm;
        public byte gameStartMapType;
        public int stageBossTicket; // 미로맵 스테이지 입장시 필요한 티켓
        public int stageBossTicketRemainTime; // 미로맵 스테이지 입장시 필요한 티켓 다음 충전까지 남은 시간        
        public int normalQuestCoin; // 의뢰 퀘스트 코인
        public byte autoStat; // 능력치 자동분배 여부 (0:수동, 1:자동)
        public int shareCharUseTimeSec; // 공유캐릭터 사용가능시간
        public byte shareCharUseDailyTicket; // 공유캐릭터 사용가능시간 무료티켓
        public int shareCharUse10mTicket; // 공유캐릭터 사용가능시간 티켓 10분
        public int shareCharUse30mTicket; // 공유캐릭터 사용가능시간 티켓 30분
        public int shareCharUse60mTicket; // 공유캐릭터 사용가능시간 티켓 60분

        public int dayMultiMazeTicket; // 멀티미로 클리어시 소모되는 티켓
        public int dayMultiMazeCount; // 멀티미로 클리어 횟수
        public string clearScenarioMazeIds; // 클리어한 시나리오 미로 ID 목록
        public int summonMvpTicket; // MVP 몬스터 소환 티켓
        public int dayZenyDungeonTicket; // 제니 던전 티켓
        public int dayZenyDungeonCount; // 제니 던전 클리어 횟수
        public int dayExpDungeonTicket; // 경험치 던전 티켓
        public int dayExpDungeonCount; // 경험치 던전 클리어 횟수
        public int duelPoint; // 듀얼 포인트
        public int duelPointBuyCount; // 듀얼 포인트 구매 횟수

        public int? jobLevelPackageShopId; // 직업 레벨 달성 패키지 상점 ID
        public long? joblevelPackageReaminTime; // 직업 레벨 달성 패키지 구입 가능 시간(남은시간)

        public int shareviceLevel; // 셰어바이스 레벨
        public int shareviceExp; // 셰어바이스 경험치

        public short free_job_level_step; // 무료 레벨 달성 스탭
        public short free_scenario_step; // 무료 시나리오 스탭

        public byte isFreeSkillReset; // 스킬초기화 무료 여부 0: 무료, 1: 유료

        public int specialRouletteUsedCount; // 스페셜 룰렛 돌린 횟수
        public long specialRouletteChangeRemainTime; // 스페셜 룰렛 초기화까지 남은 시간

        public int eventMultiMazeFreeTicket; // 이벤트멀티미로 티켓
        public int eventMultiMazeEntryCount; // 이벤트멀티미로 입장 횟수
        public int endlessTowerFreeTicket; // 엔들리스타워 티켓
        public long endlessTowerCooldownTime; // 엔들리스타워 초기화까지 남은시간
        public int forestMazeFreeTicket; // 미궁숲 티켓
        public int forestMazeEntryCount; // 미궁숲 입장횟수
        public int endlessTowerClearedFloor; // 엔들리스타워 클리어한 층
        private int profileId; // 프로필 아이디
        public bool isEnteredEventDarkMaze; // 이벤트미궁:암흑 입장 여부
        public int onBuffMvpPoint; // 온버프 mvp 처치 포인트
        public bool isReceivedOnBuffPoint; // 온버프 포인트 획득 여부

        int CharacterModel.IInputValue.Cid => cid;
        string CharacterModel.IInputValue.Name => name;
        byte CharacterModel.IInputValue.Job => job;
        byte CharacterModel.IInputValue.Gender => gender;
        int CharacterModel.IInputValue.Level => level;
        int CharacterModel.IInputValue.LevelExp => expPoint;
        int CharacterModel.IInputValue.JobLevel => jobLevel;
        long CharacterModel.IInputValue.JobLevelExp => jobExp;
        int CharacterModel.IInputValue.RebirthCount => rebirthCount;
        int CharacterModel.IInputValue.RebirthAccrueCount => rebirthStatAccruePoint;
        int CharacterModel.IInputValue.NameChangeCount => nameChangeCnt;
        string CharacterModel.IInputValue.CidHex => cidHex;
        int CharacterModel.IInputValue.ProfileId => profileId;

        int StatusModel.IInputValue.Str => str;
        int StatusModel.IInputValue.Agi => agi;
        int StatusModel.IInputValue.Vit => vit;
        int StatusModel.IInputValue.Int => inte;
        int StatusModel.IInputValue.Dex => dex;
        int StatusModel.IInputValue.Luk => lux;
        int StatusModel.IInputValue.StatPoint => remainStatPoint;

        int GuildModel.IInputValue.GuildId => guild_id;
        string GuildModel.IInputValue.GuildName => guild_name;
        int GuildModel.IInputValue.GuildEmblem => guild_emblem;
        byte GuildModel.IInputValue.GuildPosition => guild_position;
        int GuildModel.IInputValue.GuildCoin => guild_coin;
        int GuildModel.IInputValue.GuildQuestRewardCount => guild_quest_reward_cnt;
        long GuildModel.IInputValue.GuildSkillBuyDateTime => guild_skill_buy_dt;
        byte GuildModel.IInputValue.GuildSkillBuyCount => guild_skill_buy_count;
        long GuildModel.IInputValue.GuildRejoinTime => guildRejoinTime;

        void IInitializable<Response>.Initialize(Response response)
        {
            cid = response.GetInt("1");
            name = response.GetUtfString("2");
            tutorialFinish = response.GetLong("3");
            zeny = response.GetLong("4");
            job = response.GetByte("5");
            gender = response.GetByte("6");
            level = response.GetShort("7");
            expPoint = response.GetInt("8");
            jobLevel = response.GetShort("9");
            jobExp = response.GetLong("10");
            str = response.GetShort("13");
            agi = response.GetShort("14");
            vit = response.GetShort("15");
            inte = response.GetShort("16");
            dex = response.GetShort("17");
            lux = response.GetShort("18");
            rebirthCount = response.GetShort("19");
            remainStatPoint = response.GetShort("20");
            remainSkillPoint = response.GetShort("21");
            finalStageId = response.GetShort("22");
            autoStageId = response.GetShort("24");
            invenWeight = response.GetInt("26");
            dungeonFreeTicket = response.GetShort("27");
            dungeonCount = response.GetInt("28");
            centralLabFreeTicket = response.GetShort("29");
            centralLabTryCount = response.GetInt("30");
            invenWeightBuyCount = response.GetShort("32");
            rebirthStatAccruePoint = response.GetShort("33");
            nameChangeCnt = response.GetShort("35");
            scenario_step = response.GetShort("36");
            job_level_step = response.GetShort("37");
            cidHex = response.GetUtfString("38");
            roPoint = response.GetInt("43");
            clearDungeonGroupIdText = response.GetUtfString("44");

            if (response.ContainsKey("g"))
            {
                GuildPacket guildPacket = response.GetPacket<GuildPacket>("g");
                guild_id = guildPacket.guild_id;
                guild_name = guildPacket.guild_name;
                guild_emblem = guildPacket.guild_emblem;
                guild_position = guildPacket.guild_position;
                guild_coin = guildPacket.guild_coin;
                guild_quest_reward_cnt = guildPacket.guild_quest_reward_cnt;
                guild_skill_buy_dt = guildPacket.guild_skill_buy_dt;
                guild_skill_buy_count = guildPacket.guild_skill_buy_count;
            }

            if (response.ContainsKey("gt"))
                guildRejoinTime = response.GetInt("gt");

            guildCoin = response.GetInt("45");
            dayWorldBossTicket = response.GetShort("47");
            dayWorldBossCount = response.GetInt("48");
            dayDefDungeonTicket = response.GetShort("49");
            dayDefDungeonCount = response.GetInt("50");
            dayPveTicket = response.GetShort("51");
            dayPveCount = response.GetInt("52");

            if (response.ContainsKey("53"))
                worldBossTicketRemainTime = response.GetInt("53");

            if (response.ContainsKey("54"))
                worldBossAlarm = response.GetUtfString("54");

            gameStartMapType = response.GetByte("61");
            stageBossTicket = response.GetInt("62");
            stageBossTicketRemainTime = response.GetInt("63");

            normalQuestCoin = response.GetInt("66");

            if (response.ContainsKey("67"))
            {
                autoStat = response.GetByte("67");
            }

            shareCharUseTimeSec = response.GetInt("68");
            shareCharUseDailyTicket = response.GetByte("69");
            shareCharUse10mTicket = response.GetInt("70");
            shareCharUse30mTicket = response.GetInt("71");
            shareCharUse60mTicket = response.GetInt("72");
            dayMultiMazeTicket = response.GetInt("73");
            dayMultiMazeCount = response.GetInt("74");
            clearScenarioMazeIds = response.GetUtfString("75");
            summonMvpTicket = response.GetInt("76");
            dayZenyDungeonTicket = response.GetInt("77");
            dayZenyDungeonCount = response.GetInt("78");
            dayExpDungeonTicket = response.GetInt("79");
            dayExpDungeonCount = response.GetInt("80");
            duelPoint = response.GetInt("81");
            duelPointBuyCount = response.GetInt("82");

            if (response.ContainsKey("83"))
                jobLevelPackageShopId = response.GetInt("83");

            if (response.ContainsKey("84"))
                joblevelPackageReaminTime = response.GetLong("84");

            shareviceLevel = response.GetInt("85");
            shareviceExp = response.GetInt("86");

            free_job_level_step = response.GetShort("87");
            free_scenario_step = response.GetShort("88");
            dungeonFreeReward = response.GetInt("89");
            shareTempExp = response.GetInt("90");
            shareLevelupRemainTime = response.GetLong("91");
            isFreeSkillReset = response.GetByte("92");
            specialRouletteUsedCount = response.GetInt("93");
            specialRouletteChangeRemainTime = response.GetLong("94");
            eventMultiMazeFreeTicket = response.GetByte("95");
            eventMultiMazeEntryCount = response.GetShort("96");
            endlessTowerFreeTicket = response.GetByte("97");
            endlessTowerCooldownTime = response.GetLong("98");
            forestMazeFreeTicket = response.GetByte("99");
            forestMazeEntryCount = response.GetShort("100");
            endlessTowerClearedFloor = response.GetShort("101");
            profileId = response.GetInt("102");

            const byte EVENT_DARK_MAZE_ENTER_FLAG = 1;
            isEnteredEventDarkMaze = response.GetByte("103") == EVENT_DARK_MAZE_ENTER_FLAG;
            onBuffMvpPoint = response.GetInt("104");
            isReceivedOnBuffPoint = response.GetBool("105");

            Debug.Log($"{nameof(dayZenyDungeonTicket)} = {dayZenyDungeonTicket}");
            Debug.Log($"{nameof(dayZenyDungeonCount)} = {dayZenyDungeonCount}");
            Debug.Log($"{nameof(dayExpDungeonTicket)} = {dayExpDungeonTicket}");
            Debug.Log($"{nameof(dayExpDungeonCount)} = {dayExpDungeonCount}");
            Debug.Log($"{nameof(duelPoint)} = {duelPoint}");
            Debug.Log($"{nameof(duelPointBuyCount)} = {duelPointBuyCount}");
            Debug.Log($"{nameof(scenario_step)} = {scenario_step}");
            Debug.Log($"{nameof(job_level_step)} = {job_level_step}");
            Debug.Log($"{nameof(free_scenario_step)} = {free_scenario_step}");
            Debug.Log($"{nameof(free_job_level_step)} = {free_job_level_step}");
            Debug.Log($"{nameof(finalStageId)} = {finalStageId}");
            Debug.Log($"{nameof(isFreeSkillReset)} = {isFreeSkillReset}");
            Debug.Log($"{nameof(specialRouletteUsedCount)} = {specialRouletteUsedCount}");
            Debug.Log($"{nameof(specialRouletteChangeRemainTime)} = {specialRouletteChangeRemainTime}");
            Debug.Log($"{nameof(dayDefDungeonTicket)} = {dayDefDungeonTicket}");
            Debug.Log($"{nameof(dayDefDungeonCount)} = {dayDefDungeonCount}");
            Debug.Log($"{nameof(centralLabFreeTicket)} = {centralLabFreeTicket}");
            Debug.Log($"{nameof(centralLabTryCount)} = {centralLabTryCount}");
            Debug.Log($"{nameof(jobExp)} = {jobExp:N0}");
        }
    }
}