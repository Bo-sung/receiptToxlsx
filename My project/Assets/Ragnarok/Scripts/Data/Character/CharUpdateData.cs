using UnityEngine;

namespace Ragnarok
{
    public class CharUpdateData : IPacket<Response>, IUIData
    {
        public long? zeny;
        public int? catCoin;
        public short? level;
        public int? levelExp;
        public long? jobExp;
        public short? jobLevel;
        public short? statPoint;
        public short? skillPoint;
        public short? dungeonTicket;
        public int? dungeonCount;
        public short? centralLabFreeTicket;
        public int? centralLabTryCount;
        public UpdateItemData[] items;
        public CharacterStatData statData;
        public UpdateBuffPacket[] buffs;
        public short? accrueStatPoint; // 누적 스탯 포인트
        public int? guildCoin;
        public int? changedZeny; // 제니 변화량
        public UpdateCharAgentPacket[] charAgents;
        public RewardPacket[] rewards; // 보상으로 받은 아이템 정보 (획득량 정보 UI 표시용으로 사용)
        public short? defenceDungeonTicket;
        public short? worldBossTicket;
        public int? defenceDungeonCount;
        public int? worldBossCount;
        public short? pveTicket;
        public int? pveCount;
        public int? worldBossRemainTime;
        public int? battleScore; // 전투력
        public int? stageBossTicket;
        public int? stageBossTicketRemainTime;
        public int? normalQuestCoin; // 의뢰 퀘스트 코인
        public int? roPoint;
        public int? share_char_use_ticket_10m; // 공유캐릭터 사용가능시간 티켓 10분
        public int? share_char_use_ticket_30m; // 공유캐릭터 사용가능시간 티켓 30분
        public int? share_char_use_ticket_60m; // 공유캐릭터 사용가능시간 티켓 60분
        public int? dayMultiMazeTicket; // 멀티미로 클리어시 소모되는 티켓
        public int? dayMultiMazeCount; // 멀티미로 클리어 횟수
        public int? summonMvpTicket; // MVP 몬스터 소환 티켓
        public int? dayZenyDungeonTicket; // 제니 던전 입장시 소모되는 티켓
        public int? dayZenyDungeonCount; // 제니 던전 입장 횟수
        public int? dayExpDungeonTicket; // 경험치 던전 입장시 소모되는 티켓
        public int? dayExpDungeonCount; // 경험치 던전 입장 횟수
        public int? shareExp; // 셰어바이스 경험치
        public int? shareLevel; // 셰어바이스 레벨
        public int? jobLevelPackageShopId; // 직업레벨 달성 패키지 상점 ID
        public long? jobLevelPackageReaminTime; // 직업레벨 달성 패키지 구입가능 시간(남은시간)
        public int? eventMultiMazeFreeTicket; // 이벤트멀티미로 클리어시 소모되는 티켓
        public int? eventMultiMazeEntryCount; // 이벤트멀티미로 입장 횟수
        public int? endlessTowerFreeTicket; // 엔들리스타워 무료티켓
        public long? endlessTowerCooldownTime; // 엔들리스타워 무료입장까지 남은시간
        public int? forestMazeFreeTicket; // 미궁숲 무료티켓
        public int? forestMazeEntryCount; // 미궁숲 입장횟수
        public int? passExp; // 패스 경험치
        public int? onBuffPassExp; // 온버프패스 경험치
        public int? onBuffPoint; // 온버프 포인트
        public int? onBuffMvpPoint; // 온버프 mvp 포인트

        void IInitializable<Response>.Initialize(Response response)
        {
            if (response.ContainsKey("1"))
                zeny = response.GetLong("1");

            if (response.ContainsKey("2"))
                catCoin = response.GetInt("2");

            if (response.ContainsKey("3"))
                level = response.GetShort("3");

            if (response.ContainsKey("4"))
                levelExp = response.GetInt("4");

            if (response.ContainsKey("5"))
                jobExp = response.GetLong("5");

            if (response.ContainsKey("6"))
                jobLevel = response.GetShort("6");

            if (response.ContainsKey("7"))
                statPoint = response.GetShort("7");

            if (response.ContainsKey("8"))
                skillPoint = response.GetShort("8");

            if (response.ContainsKey("9"))
                dungeonTicket = response.GetShort("9");

            if (response.ContainsKey("10"))
                dungeonCount = response.GetInt("10");

            if (response.ContainsKey("11"))
            {
                centralLabFreeTicket = response.GetShort("11");
                Debug.Log($"CUD {nameof(centralLabFreeTicket)} = {centralLabFreeTicket}");
            }

            if (response.ContainsKey("12"))
            {
                centralLabTryCount = response.GetInt("12");
                Debug.Log($"CUD {nameof(centralLabTryCount)} = {centralLabTryCount}");
            }

            if (response.ContainsKey("13"))
                items = response.GetPacketArray<UpdateItemData>("13");          

            if (response.ContainsKey("15"))
                statData = response.GetPacket<CharacterStatData>("15");

            if (response.ContainsKey("16"))
                buffs = response.GetPacketArray<UpdateBuffPacket>("16");

            if (response.ContainsKey("19"))
                accrueStatPoint = response.GetShort("19");

            if (response.ContainsKey("20"))
                guildCoin = response.GetInt("20");

            if (response.ContainsKey("21"))
                changedZeny = response.GetInt("21");

            if (response.ContainsKey("22"))
            {
                defenceDungeonTicket = response.GetShort("22");
                Debug.Log($"CUD {nameof(defenceDungeonTicket)} = {defenceDungeonTicket}");
            }

            if (response.ContainsKey("23"))
                worldBossTicket = response.GetShort("23");

            if (response.ContainsKey("24"))
            {
                defenceDungeonCount = response.GetInt("24");
                Debug.Log($"CUD {nameof(defenceDungeonCount)} = {defenceDungeonCount}");
            }

            if (response.ContainsKey("25"))
                worldBossCount = response.GetInt("25");

            if (response.ContainsKey("26"))
                pveTicket = response.GetShort("26");

            if (response.ContainsKey("27"))
                pveCount = response.GetInt("27");

            if (response.ContainsKey("28"))
                worldBossRemainTime = response.GetInt("28");

            if (response.ContainsKey("29"))
            {
                battleScore = response.GetInt("29");
                if (DebugUtils.IsLogBattleLog)
                    Debug.Log($"전투력 = {battleScore}");
            }

            if (response.ContainsKey("30"))
            {
                stageBossTicket = response.GetInt("30");
                Debug.Log($"<color=magenta>스테이지 보스 티켓 수량 변동 = {stageBossTicket}</color>");
            }

            if (response.ContainsKey("31"))
            {
                stageBossTicketRemainTime = response.GetInt("31");
                Debug.Log($"<color=magenta>스테이지 보스 티켓 충전 시간 변동 = {stageBossTicketRemainTime}</color>");
            }

            if (response.ContainsKey("34"))
            {
                normalQuestCoin = response.GetInt("34");
                Debug.Log($"<color=magenta>의뢰 퀘스트 코인 변동 = {normalQuestCoin}</color>");
            }

            if (response.ContainsKey("35"))
                charAgents = response.GetPacketArray<UpdateCharAgentPacket>("35");

            if (response.ContainsKey("36"))
            {
                roPoint = response.GetInt("36");
            }

            if (response.ContainsKey("37"))
            {
                share_char_use_ticket_10m = response.GetInt("37");
            }

            if (response.ContainsKey("38"))
            {
                share_char_use_ticket_30m = response.GetInt("38");
            }

            if (response.ContainsKey("39"))
            {
                share_char_use_ticket_60m = response.GetInt("39");
            }

            if (response.ContainsKey("40"))
            {
                dayMultiMazeTicket = response.GetInt("40");
                Debug.Log($"CUD {nameof(dayMultiMazeTicket)} = {dayMultiMazeTicket}");
            }

            if (response.ContainsKey("41"))
            {
                dayMultiMazeCount = response.GetInt("41");
                Debug.Log($"CUD {nameof(dayMultiMazeCount)} = {dayMultiMazeCount}");
            }

            if (response.ContainsKey("42"))
            {
                summonMvpTicket = response.GetInt("42");
            }

            if (response.ContainsKey("43"))
            {
                dayZenyDungeonTicket = response.GetInt("43");
                Debug.Log($"CUD {nameof(dayZenyDungeonTicket)} = {dayZenyDungeonTicket}");
            }

            if (response.ContainsKey("44"))
            {
                dayZenyDungeonCount = response.GetInt("44");
                Debug.Log($"CUD {nameof(dayZenyDungeonCount)} = {dayZenyDungeonCount}");
            }

            if (response.ContainsKey("45"))
            {
                dayExpDungeonTicket = response.GetInt("45");
                Debug.Log($"CUD {nameof(dayExpDungeonTicket)} = {dayExpDungeonTicket}");
            }

            if (response.ContainsKey("46"))
            {
                dayExpDungeonCount = response.GetInt("46");
                Debug.Log($"CUD {nameof(dayExpDungeonCount)} = {dayExpDungeonCount}");
            }

            if (response.ContainsKey("47"))
                shareExp = response.GetInt("47");

            if (response.ContainsKey("48"))
                shareLevel = response.GetInt("48");

            if (response.ContainsKey("49"))
                jobLevelPackageShopId = response.GetInt("49");

            if (response.ContainsKey("50"))
                jobLevelPackageReaminTime = response.GetLong("50");

            if (response.ContainsKey("51"))
                eventMultiMazeFreeTicket = response.GetByte("51");

            if (response.ContainsKey("52"))
                eventMultiMazeEntryCount = response.GetShort("52");

            if (response.ContainsKey("53"))
                endlessTowerFreeTicket = response.GetByte("53");

            if (response.ContainsKey("54"))
                endlessTowerCooldownTime = response.GetLong("54");

            if (response.ContainsKey("55"))
                forestMazeFreeTicket = response.GetByte("55");

            if (response.ContainsKey("56"))
                forestMazeEntryCount = response.GetShort("56");

            if (response.ContainsKey("57"))
                passExp = response.GetInt("57");

            if (response.ContainsKey("58"))
                onBuffPassExp = response.GetInt("58");

            if (response.ContainsKey("59"))
                onBuffPoint = response.GetInt("59");

            if (response.ContainsKey("60"))
                onBuffMvpPoint = response.GetInt("60");

            if (response.ContainsKey("99"))
                rewards = response.GetPacketArray<RewardPacket>("99");
        }
    }
}