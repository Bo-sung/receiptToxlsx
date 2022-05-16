namespace Ragnarok
{
    [System.Flags]
    public enum ItemSourceCategoryType : long
    {
        None = 0,

        /// <summary> [획득처] 스테이지 드랍 </summary>
        StageDrop = 1L << 0,

        /// <summary> [획득처] 던전 드랍 </summary>
        DungeonDrop = 1L << 1,

        /// <summary> [획득처] 월드 보스 </summary>
        WorldBoss = 1L << 2,

        /// <summary> [획득처] 상자 </summary>
        Box = 1L << 3,

        /// <summary> [획득처] 제작 </summary>
        Make = 1L << 4,

        /// <summary> [획득처] 비밀 상점 </summary>
        SecretShop = 1L << 5,

        /// <summary> [획득처] 소모품 상점 </summary>
        ConsumableShop = 1L << 6,

        /// <summary> [획득처] 상자 상점 </summary>
        BoxShop = 1L << 7,

        /// <summary> [획득처] 길드 상점 </summary>
        [System.Obsolete("길드 상점 탭 사라짐")]
        GuildShop = 1L << 8,

        /// <summary> [획득처] 매일매일 상점 </summary>
        EveryDayShop = 1L << 9,

        /// <summary> [획득처] 일일 퀘스트 </summary>
        DailyQuest = 1L << 10,

        /// <summary> [획득처] 길드 퀘스트 </summary>
        GuildQuest = 1L << 11,

        /// <summary> [획득처] 이벤트 </summary>
        Event = 1L << 12,

        /// <summary> [사용처] 제작 </summary>
        UseMake = 1L << 13,

        /// <summary> [사용처] 강화 재료 </summary>
        UseEnchantMaterial = 1L << 14,

        /// <summary> [사용처] 등급 변경 </summary>
        UseTierChange = 1L << 15,

        /// <summary> [사용처] 강화 보호 </summary>
        UseProtectEnchantBreak = 1L << 16,

        /// <summary> [사용처] 던전 소탕 </summary>
        UseDungeonSweep = 1L << 17,

        /// <summary> [획득처] 대전 </summary>
        League = 1L << 18,

        /// <summary> [사용처] 대전 </summary>
        UseLeague = 1L << 19,

        /// <summary> [획득처] 장비 분해 </summary>
        DisassembleEquipment = 1L << 20,

        /// <summary> [획득처] 카드 분해 </summary>
        DisassembleCard = 1L << 21,

        /// <summary> [획득처] 미로</summary>
        Maze = 1L << 22,

        /// <summary> [획득처] MVP</summary>
        MVP = 1L << 23,

        /// <summary> [획득처] 파견</summary>
        AgentExplore = 1L << 24,

        /// <summary> [사용처] 이벤트 </summary>
        UseEvent = 1L << 25,

        /// <summary> [획득처] 난전 </summary>
        FreeFight = 1L << 26,

        /// <summary> [획득처] 엔들리스 타워 </summary>
        EndlessTower = 1L << 27,

        /// <summary> [획득처] 미궁숲 </summary>
        ForestMaze = 1L << 28,

        /// <summary> [사용처] 카프라 교환소 </summary>
        UseExchange = 1L << 29,

        /// <summary> [사용처] 카프라 운송 </summary>
        UseKafraDelivery = 1L << 30,

        /// <summary> [획득처] 코스튬 분해 </summary>
        DisassembleCostume = 1L << 31,

        /// <summary> [획득처] 타임패트롤 </summary>
        TimePatrol = 1L << 32,

        /// <summary> [획득처] 길드전 </summary>
        GuildBattle = 1L << 33,

        /// <summary> [획득처] 게이트 </summary>
        Gate = 1L << 34,

        /// <summary> [획득처] 나비호 </summary>
        Nabiho = 1L << 35,

        /// <summary> [사용처] 나비호 </summary>
        UseNabiho = 1L << 36,
    }

    /// <summary>
    /// 1차 카테고리. (버튼없음|이동|보기)
    /// </summary>
    public enum ItemSourceButtonType
    {
        /// <summary> 버튼없음 </summary>
        None = 0,

        /// <summary> 
        /// <see cref="UIItemSourceDetail"/>
        /// 보기 (리스트팝업 띄우기) 
        /// </summary>
        ListPopup,

        /// <summary> 이동 (해당 팝업/던전 등으로 이동) </summary>
        Move,
    }

    /// <summary>
    /// <see cref="UIItemSourceDetail"/>
    /// 2차 상세 카테고리. (1차에서 ListPopup을 통해 들어옴)
    /// </summary>
    public enum ItemSourceDetailButtonType
    {
        /// <summary> 버튼 없음 </summary>
        None = 0,

        /// <summary> 이동 (특정 스테이지, 던전 등) </summary>
        Move,

        /// <summary> 정보. 해당 항목의 정보 팝업 (상자 -> 해당 상자의 ItemInfo 등) </summary>
        Info,
    }


    public static class ItemSourceCategoryTypeExtension
    {
        /// <summary>
        /// 1차 카테고리 슬롯 아이콘이름
        /// </summary>
        public static string GetIconName(this ItemSourceCategoryType type, ItemInfo itemInfo = null)
        {
            switch (type)
            {
                case ItemSourceCategoryType.None:
                    return default;
                case ItemSourceCategoryType.StageDrop:
                    return "Ui_Common_Icon_GetSourceBitType_01";
                case ItemSourceCategoryType.DungeonDrop:
                    return "Ui_Common_Icon_GetSourceBitType_02";
                case ItemSourceCategoryType.WorldBoss:
                    return "Ui_Common_Icon_GetSourceBitType_03";
                case ItemSourceCategoryType.Box:
                    return "Ui_Common_Icon_GetSourceBitType_04";
                case ItemSourceCategoryType.Make:
                    return "Ui_Common_Icon_GetSourceBitType_05";
                case ItemSourceCategoryType.SecretShop:
                    return "Ui_Common_Icon_GetSourceBitType_06";
                case ItemSourceCategoryType.ConsumableShop:
                    return "Ui_Common_Icon_GetSourceBitType_07";
                case ItemSourceCategoryType.BoxShop:
                    return "Ui_Common_Icon_GetSourceBitType_08";
                case ItemSourceCategoryType.GuildShop:
                    return "Ui_Common_Icon_GetSourceBitType_09";
                case ItemSourceCategoryType.EveryDayShop:
                    return "Ui_Common_Icon_GetSourceBitType_10";
                case ItemSourceCategoryType.DailyQuest:
                    return "Ui_Common_Icon_GetSourceBitType_11";
                case ItemSourceCategoryType.GuildQuest:
                    return "Ui_Common_Icon_GetSourceBitType_12";
                case ItemSourceCategoryType.Event:
                case ItemSourceCategoryType.UseEvent:
                    return "Ui_Common_Icon_GetSourceBitType_13";
                case ItemSourceCategoryType.League:
                    return "Ui_Common_Icon_GetSourceBitType_19";
                case ItemSourceCategoryType.DisassembleEquipment:
                    return "Ui_Common_Icon_GetSourceBitType_21";
                case ItemSourceCategoryType.DisassembleCard:
                    return "Ui_Common_Icon_GetSourceBitType_22";
                case ItemSourceCategoryType.Maze:
                    return "Ui_Common_Icon_GetSourceBitType_23";
                case ItemSourceCategoryType.MVP:
                    return "Ui_Common_Icon_GetSourceBitType_24";
                case ItemSourceCategoryType.AgentExplore:
                    return "Ui_Common_Icon_GetSourceBitType_25";
                case ItemSourceCategoryType.UseMake:
                    return "Ui_Common_Icon_GetSourceBitType_05";
                case ItemSourceCategoryType.UseEnchantMaterial:
                    return itemInfo.IconName;
                case ItemSourceCategoryType.UseTierChange:
                    return itemInfo.IconName;
                case ItemSourceCategoryType.UseProtectEnchantBreak:
                    return itemInfo.IconName;
                case ItemSourceCategoryType.UseDungeonSweep:
                    return itemInfo.IconName;
                case ItemSourceCategoryType.UseLeague:
                    return "Ui_Common_Icon_GetSourceBitType_19";
                case ItemSourceCategoryType.FreeFight:
                    return "Ui_Common_Icon_GetSourceBitType_27";
                case ItemSourceCategoryType.EndlessTower:
                    return "Ui_Common_Icon_GetSourceBitType_28";
                case ItemSourceCategoryType.ForestMaze:
                    return "Ui_Common_Icon_GetSourceBitType_29";
                case ItemSourceCategoryType.UseExchange:
                    return "Ui_Common_Icon_GetSourceBitType_36";
                case ItemSourceCategoryType.UseKafraDelivery:
                    return "Ui_Common_Icon_GetSourceBitType_30";
                case ItemSourceCategoryType.DisassembleCostume:
                    return "Ui_Common_Icon_GetSourceBitType_31";
                case ItemSourceCategoryType.TimePatrol:
                    return "Ui_Common_Icon_GetSourceBitType_32";
                case ItemSourceCategoryType.GuildBattle:
                    return "Ui_Common_Icon_GetSourceBitType_33";
                case ItemSourceCategoryType.Gate:
                    return "Ui_Common_Icon_GetSourceBitType_34";
                case ItemSourceCategoryType.Nabiho:
                    return "Ui_Common_Icon_GetSourceBitType_35";
                case ItemSourceCategoryType.UseNabiho:
                    return "Ui_Common_Icon_GetSourceBitType_35";
                default:
                    return default;
            }
        }

        /// <summary>
        /// 1차 카테고리 슬롯 이름
        /// </summary>
        public static string GetName(this ItemSourceCategoryType type)
        {
            switch (type)
            {
                case ItemSourceCategoryType.None:
                    return default;
                case ItemSourceCategoryType.StageDrop:
                    return LocalizeKey._46003.ToText(); // 스테이지
                case ItemSourceCategoryType.DungeonDrop:
                    return LocalizeKey._46004.ToText(); // 던전
                case ItemSourceCategoryType.WorldBoss:
                    return LocalizeKey._46005.ToText(); // 월드 보스
                case ItemSourceCategoryType.Box:
                    return LocalizeKey._46006.ToText(); // 상자
                case ItemSourceCategoryType.Make:
                    return LocalizeKey._46007.ToText(); // 제작
                case ItemSourceCategoryType.SecretShop:
                    return LocalizeKey._46008.ToText(); // 비밀 상점
                case ItemSourceCategoryType.ConsumableShop:
                    return LocalizeKey._46068.ToText(); // 소모품 상점
                case ItemSourceCategoryType.BoxShop:
                    return LocalizeKey._46010.ToText(); // 상자 상점
                case ItemSourceCategoryType.GuildShop:
                    return LocalizeKey._46011.ToText(); // 길드 상점
                case ItemSourceCategoryType.EveryDayShop:
                    return LocalizeKey._46066.ToText(); // 매일매일 상점
                case ItemSourceCategoryType.DailyQuest:
                    return LocalizeKey._46013.ToText(); // 일일 퀘스트
                case ItemSourceCategoryType.GuildQuest:
                    return LocalizeKey._46014.ToText(); // 길드 퀘스트
                case ItemSourceCategoryType.Event:
                case ItemSourceCategoryType.UseEvent:
                    return LocalizeKey._46015.ToText(); // 이벤트
                case ItemSourceCategoryType.League:
                    return LocalizeKey._46041.ToText(); // 대전 
                case ItemSourceCategoryType.DisassembleEquipment:
                    return LocalizeKey._46045.ToText(); // 장비 분해
                case ItemSourceCategoryType.DisassembleCard:
                    return LocalizeKey._46046.ToText(); // 카드 분해
                case ItemSourceCategoryType.Maze:
                    return LocalizeKey._46049.ToText(); // 카드 미로
                case ItemSourceCategoryType.MVP:
                    return LocalizeKey._46050.ToText(); // MVP
                case ItemSourceCategoryType.AgentExplore:
                    return LocalizeKey._46051.ToText(); // 파견

                case ItemSourceCategoryType.UseMake:
                    return LocalizeKey._46016.ToText(); // 제작 재료
                case ItemSourceCategoryType.UseEnchantMaterial:
                    return LocalizeKey._46017.ToText(); // 강화 재료
                case ItemSourceCategoryType.UseTierChange:
                    return LocalizeKey._46018.ToText(); // 등급 변경
                case ItemSourceCategoryType.UseProtectEnchantBreak:
                    return LocalizeKey._46019.ToText(); // 강화 보호
                case ItemSourceCategoryType.UseDungeonSweep:
                    return LocalizeKey._46020.ToText(); // 던전 소탕
                case ItemSourceCategoryType.UseLeague:
                    return LocalizeKey._46042.ToText(); // 대전
                case ItemSourceCategoryType.FreeFight:
                    return LocalizeKey._2515.ToText(); // 난전
                case ItemSourceCategoryType.EndlessTower:
                    return LocalizeKey._46058.ToText(); // 엔들리스 타워
                case ItemSourceCategoryType.ForestMaze:
                    return LocalizeKey._46060.ToText(); // 미궁숲
                case ItemSourceCategoryType.UseExchange:
                    return LocalizeKey._46062.ToText(); // 카프라 교환소
                case ItemSourceCategoryType.UseKafraDelivery:
                    return LocalizeKey._46064.ToText(); // 카프라 운송
                case ItemSourceCategoryType.DisassembleCostume:
                    return LocalizeKey._46070.ToText(); // 코스튬 분해
                case ItemSourceCategoryType.TimePatrol:
                    return LocalizeKey._46072.ToText(); // 타임패트롤
                case ItemSourceCategoryType.GuildBattle:
                    return LocalizeKey._46074.ToText(); // 길드전
                case ItemSourceCategoryType.Gate:
                    return LocalizeKey._46076.ToText(); // 게이트
                case ItemSourceCategoryType.Nabiho:
                case ItemSourceCategoryType.UseNabiho:
                    return LocalizeKey._46078.ToText(); // 나비호

                default:
                    return default;
            }
        }

        /// <summary>
        /// 1차 카테고리 슬롯 설명
        /// </summary>
        public static string GetDescription(this ItemSourceCategoryType type)
        {
            switch (type)
            {
                case ItemSourceCategoryType.None:
                    return default;
                case ItemSourceCategoryType.StageDrop:
                    return LocalizeKey._46023.ToText(); // 스테이지 설명
                case ItemSourceCategoryType.DungeonDrop:
                    return LocalizeKey._46024.ToText(); // 던전 설명
                case ItemSourceCategoryType.WorldBoss:
                    return LocalizeKey._46025.ToText(); // 월드 보스 설명
                case ItemSourceCategoryType.Box:
                    return LocalizeKey._46026.ToText(); // 상자 설명
                case ItemSourceCategoryType.Make:
                    return LocalizeKey._46027.ToText(); // 제작 설명
                case ItemSourceCategoryType.SecretShop:
                    return LocalizeKey._46028.ToText(); // 비밀 상점 설명
                case ItemSourceCategoryType.ConsumableShop:
                    return LocalizeKey._46069.ToText(); // 소모품 상점에서 획득할 수 있습니다.
                case ItemSourceCategoryType.BoxShop:
                    return LocalizeKey._46030.ToText(); // 상자 상점 설명
                case ItemSourceCategoryType.GuildShop:
                    return LocalizeKey._46031.ToText(); // 길드 상점 설명
                case ItemSourceCategoryType.EveryDayShop:
                    return LocalizeKey._46067.ToText(); // 매일매일 상점에서 획득할 수 있습니다.
                case ItemSourceCategoryType.DailyQuest:
                    return LocalizeKey._46033.ToText(); // 일일 퀘스트 설명
                case ItemSourceCategoryType.GuildQuest:
                    return LocalizeKey._46034.ToText(); // 길드 퀘스트 설명
                case ItemSourceCategoryType.Event:
                    return LocalizeKey._46035.ToText(); // 이벤트 설명
                case ItemSourceCategoryType.League:
                    return LocalizeKey._46043.ToText(); // ** 대전 카테고리 설명 **
                case ItemSourceCategoryType.DisassembleEquipment:
                    return LocalizeKey._46047.ToText(); // ** 장비 분해 설명 **
                case ItemSourceCategoryType.DisassembleCard:
                    return LocalizeKey._46048.ToText(); // ** 카드 분해 설명 **
                case ItemSourceCategoryType.Maze:
                    return LocalizeKey._46052.ToText(); // 카드 미로 보상으로 획득할 수 있습니다.
                case ItemSourceCategoryType.MVP:
                    return LocalizeKey._46053.ToText(); // MVP 보상으로 획득할 수 있습니다.
                case ItemSourceCategoryType.AgentExplore:
                    return LocalizeKey._46054.ToText(); // 파견 보상으로 획득할 수 있습니다.

                case ItemSourceCategoryType.UseMake:
                    return LocalizeKey._46036.ToText(); // 사용처 제작 설명
                case ItemSourceCategoryType.UseEnchantMaterial:
                    return LocalizeKey._46037.ToText(); // 사용처 강화 재료 설명
                case ItemSourceCategoryType.UseTierChange:
                    return LocalizeKey._46038.ToText(); // 사용처 등급 변경 설명
                case ItemSourceCategoryType.UseProtectEnchantBreak:
                    return LocalizeKey._46039.ToText(); // 사용처 강화 보호 설명
                case ItemSourceCategoryType.UseDungeonSweep:
                    return LocalizeKey._46040.ToText(); // 사용처 던전 소탕 설명
                case ItemSourceCategoryType.UseLeague:
                    return LocalizeKey._46044.ToText(); // ** 사용처 대전 카테고리 설명 **
                case ItemSourceCategoryType.UseEvent:
                    return LocalizeKey._46056.ToText(); // 사용처 이벤트 설명 **
                case ItemSourceCategoryType.FreeFight:
                    return LocalizeKey._46057.ToText(); // 난전 보상 설명
                case ItemSourceCategoryType.EndlessTower:
                    return LocalizeKey._46059.ToText(); // 엔들리스 타워 섧명
                case ItemSourceCategoryType.ForestMaze:
                    return LocalizeKey._46061.ToText(); // 미궁숲 섧명
                case ItemSourceCategoryType.UseExchange:
                    return LocalizeKey._46063.ToText(); // 거래소에 있는 테일링에게 교환할 수 있습니다.
                case ItemSourceCategoryType.UseKafraDelivery:
                    return LocalizeKey._46065.ToText(); // 거래소에 있는 소린을 통해 사용할 수 있습니다.
                case ItemSourceCategoryType.DisassembleCostume:
                    return LocalizeKey._46071.ToText(); // 코스튬 분해를 통해 획득할 수 있습니다.
                case ItemSourceCategoryType.TimePatrol:
                    return LocalizeKey._46073.ToText(); // 타임패트롤에서 획득할 수 있습니다.
                case ItemSourceCategoryType.GuildBattle:
                    return LocalizeKey._46075.ToText(); // 길드전에서 획득할 수 있습니다.
                case ItemSourceCategoryType.Gate:
                    return LocalizeKey._46077.ToText(); // 게이트에서 획득할 수 있습니다.
                case ItemSourceCategoryType.Nabiho:
                    return LocalizeKey._46079.ToText(); // 나비호에서 획득할 수 있습니다.
                case ItemSourceCategoryType.UseNabiho:
                    return LocalizeKey._46080.ToText(); // 도람족의 친밀도를 올릴 수 있습니다.

                default:
                    return default;
            }
        }

        /// <summary>
        /// 1차 카테고리 버튼 타입
        /// </summary>
        public static ItemSourceButtonType GetButtonType(this ItemSourceCategoryType type)
        {
            switch (type)
            {
                case ItemSourceCategoryType.StageDrop:
                case ItemSourceCategoryType.Box:
                case ItemSourceCategoryType.UseMake:
                    return ItemSourceButtonType.ListPopup;

                case ItemSourceCategoryType.Make:
                case ItemSourceCategoryType.SecretShop:
                case ItemSourceCategoryType.ConsumableShop:
                case ItemSourceCategoryType.BoxShop:
                case ItemSourceCategoryType.GuildShop:
                case ItemSourceCategoryType.EveryDayShop:
                case ItemSourceCategoryType.DailyQuest:
                case ItemSourceCategoryType.GuildQuest:
                case ItemSourceCategoryType.Event:
                case ItemSourceCategoryType.UseEvent:
                case ItemSourceCategoryType.League:
                case ItemSourceCategoryType.UseLeague:
                case ItemSourceCategoryType.WorldBoss:
                case ItemSourceCategoryType.DungeonDrop:
                case ItemSourceCategoryType.Maze:
                case ItemSourceCategoryType.AgentExplore:
                case ItemSourceCategoryType.FreeFight:
                case ItemSourceCategoryType.EndlessTower:
                case ItemSourceCategoryType.ForestMaze:
                case ItemSourceCategoryType.UseExchange:
                case ItemSourceCategoryType.UseKafraDelivery:
                case ItemSourceCategoryType.DisassembleEquipment:
                case ItemSourceCategoryType.DisassembleCard:
                case ItemSourceCategoryType.DisassembleCostume:
                case ItemSourceCategoryType.TimePatrol:
                case ItemSourceCategoryType.GuildBattle:
                case ItemSourceCategoryType.Gate:
                case ItemSourceCategoryType.Nabiho:
                case ItemSourceCategoryType.UseNabiho:
                    return ItemSourceButtonType.Move;

                case ItemSourceCategoryType.None:
                case ItemSourceCategoryType.MVP:
                default:
                    return ItemSourceButtonType.None;
            }
        }

        /// <summary>
        /// 2차 카테고리 버튼 타입
        /// </summary>
        public static ItemSourceDetailButtonType GetDetailButtonType(this ItemSourceCategoryType type)
        {
            switch (type)
            {
                case ItemSourceCategoryType.StageDrop:
                case ItemSourceCategoryType.UseMake:
                    return ItemSourceDetailButtonType.Move;

                case ItemSourceCategoryType.Box:
                    return ItemSourceDetailButtonType.Info;

                case ItemSourceCategoryType.None:
                default:
                    return ItemSourceDetailButtonType.None;
            }
        }

        /// <summary>
        /// 타입에 사용처가 있는지 여부
        /// </summary>
        public static bool HasUseItemSourceCategoryType(this ItemSourceCategoryType type)
        {
            if (type.HasFlag(ItemSourceCategoryType.UseMake))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseEnchantMaterial))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseTierChange))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseProtectEnchantBreak))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseDungeonSweep))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseLeague))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseEvent))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseExchange))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseKafraDelivery))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.UseNabiho))
                return true;

            return false;
        }

        /// <summary>
        /// 타입에 획득처 있는지 여부
        /// </summary>
        public static bool HasGetItemSourceCategoryType(this ItemSourceCategoryType type)
        {
            if (type.HasFlag(ItemSourceCategoryType.StageDrop))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.DungeonDrop))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.WorldBoss))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.Box))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.Make))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.SecretShop))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.ConsumableShop))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.BoxShop))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.GuildShop))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.EveryDayShop))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.DailyQuest))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.GuildQuest))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.Event))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.League))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.DisassembleEquipment))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.DisassembleCard))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.Maze))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.MVP))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.AgentExplore))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.FreeFight))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.EndlessTower))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.ForestMaze))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.DisassembleCostume))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.TimePatrol))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.GuildBattle))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.Gate))
                return true;
            if (type.HasFlag(ItemSourceCategoryType.Nabiho))
                return true;

            return false;
        }

        /// <summary>
        /// 사용 하지 않는 타입 있는지 여부
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasObsoleteItemSourceCategoryType(this ItemSourceCategoryType type)
        {
            if (type.HasFlag(ItemSourceCategoryType.GuildShop))
                return true;

            return false;
        }

        public static int Sort(this ItemSourceCategoryType type)
        {
            switch (type)
            {
                // 획득처

                case ItemSourceCategoryType.StageDrop:
                    return 1;
                case ItemSourceCategoryType.DungeonDrop:
                    return 2;
                case ItemSourceCategoryType.WorldBoss:
                    return 3;
                case ItemSourceCategoryType.Box:
                    return 4;
                case ItemSourceCategoryType.Make:
                    return 5;
                case ItemSourceCategoryType.SecretShop:
                    return 6;
                case ItemSourceCategoryType.ConsumableShop:
                    return 7;
                case ItemSourceCategoryType.BoxShop:
                    return 8;
                case ItemSourceCategoryType.GuildShop:
                    return 9;
                case ItemSourceCategoryType.EveryDayShop:
                    return 10;
                case ItemSourceCategoryType.DailyQuest:
                    return 11;
                case ItemSourceCategoryType.GuildQuest:
                    return 12;
                case ItemSourceCategoryType.Event:
                    return 13;
                case ItemSourceCategoryType.League:
                    return 14;
                case ItemSourceCategoryType.DisassembleEquipment:
                    return 15;
                case ItemSourceCategoryType.DisassembleCard:
                    return 16;
                case ItemSourceCategoryType.DisassembleCostume:
                    return 17;
                case ItemSourceCategoryType.Maze:
                    return 18;
                case ItemSourceCategoryType.MVP:
                    return 19;
                case ItemSourceCategoryType.AgentExplore:
                    return 20;
                case ItemSourceCategoryType.FreeFight:
                    return 21;
                case ItemSourceCategoryType.EndlessTower:
                    return 22;
                case ItemSourceCategoryType.ForestMaze:
                    return 23;
                case ItemSourceCategoryType.TimePatrol:
                    return 24;
                case ItemSourceCategoryType.GuildBattle:
                    return 25;
                case ItemSourceCategoryType.Gate:
                    return 26;
                case ItemSourceCategoryType.Nabiho:
                    return 27;

                // 사용처

                case ItemSourceCategoryType.UseMake: // 13
                    return 1;
                case ItemSourceCategoryType.UseEnchantMaterial: // 14
                    return 2;
                case ItemSourceCategoryType.UseTierChange: // 15
                    return 3;
                case ItemSourceCategoryType.UseProtectEnchantBreak: // 16
                    return 4;
                case ItemSourceCategoryType.UseDungeonSweep: // 17
                    return 5;
                case ItemSourceCategoryType.UseLeague: //  19
                    return 6;
                case ItemSourceCategoryType.UseEvent: // 25
                    return 7;
                case ItemSourceCategoryType.UseExchange: // 29
                    return 9;
                case ItemSourceCategoryType.UseKafraDelivery: // 30 
                    return 8;
                case ItemSourceCategoryType.UseNabiho:
                    return 10;
            }
            return default;
        }
    }

    public static class ItemSourceButtonTypeExtension
    {
        public static string GetText(this ItemSourceButtonType type)
        {
            switch (type)
            {
                case ItemSourceButtonType.Move:
                    return LocalizeKey._46021.ToText(); // 이동
                case ItemSourceButtonType.ListPopup:
                    return LocalizeKey._46022.ToText(); // 목록
            }
            return "-";
        }
    }

    public static class ItemSourceDetailButtonTypeExtension
    {
        public static string GetText(this ItemSourceDetailButtonType type)
        {
            switch (type)
            {
                case ItemSourceDetailButtonType.Move:
                    return LocalizeKey._46101.ToText(); // 이동
                case ItemSourceDetailButtonType.Info:
                    return LocalizeKey._46102.ToText(); // 정보
            }
            return "-";
        }
    }
}