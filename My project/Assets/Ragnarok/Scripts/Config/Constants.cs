using UnityEngine;

namespace Ragnarok
{
    public static class Constants
    {
        public static bool FACEBOOK_INVITE = true;

        /// 맵(필드)의 이동 가능 반경
        /// </summary>
        public static class Map
        {
            public static Vector2 MAP_LIMIT_X = new Vector2(-11.7f, 11.7f);
            public static Vector2 MAP_LIMIT_Z = new Vector2(-11.7f, 11.7f);
            public const float POSITION_Y = 2.47f;
            public const float BOSS_FALLING_POSITION_Y = 200f;


            public static class GuildMaze
            {
                // 맵 스캔 범위
                public const float NAVMESH_SAMPLE_POSITION_RANGE = 60f;

                // 블록인덱스 좌표 계산용
                private const int BLOCK_INDEX_OFFSET_X = -46;
                private const int BLOCK_INDEX_OFFSET_Z = -40;
                private const float BLOCK_OFFSET_X = -0.58045f;
                private const float BLOCK_OFFSET_Z = -0.30233f;
                private const float BLOCK_SIZE_X = 1.16953016666f; // 블록 하나 사이즈
                private const float BLOCK_SIZE_Z = 1.17027483333f; // 블록 하나 사이즈

                /// <summary>
                /// 블록인덱스의 좌표 반환
                /// </summary>
                public static Vector3 GetBlockPosition(short xIndex, short zIndex)
                {
                    float x = (xIndex + BLOCK_INDEX_OFFSET_X) * BLOCK_SIZE_X + BLOCK_OFFSET_X;
                    float z = (zIndex + BLOCK_INDEX_OFFSET_Z) * BLOCK_SIZE_Z + BLOCK_OFFSET_Z;
                    return new Vector3(x, 0f, z);
                }
            }
        }

        public static class Size
        {
            /// <summary>
            /// 스킬 슬롯 최대 개수 (4)
            /// </summary>
            public const int MAX_SKILL_SLOT_SIZE = 4;

            /// <summary>
            /// 큐펫 슬롯 개수 (4)
            /// </summary>
            public const int CUPET_SLOT_SIZE = 4;

            /// <summary>
            /// 몬스터 스킬 개수 (4)
            /// </summary>
            public const int MONSTER_SKILL_SIZE = 4;

            /// <summary>
            /// 챕터 개수 (6)
            /// </summary>
            public const int CHAPTER_SIZE = 6;

            /// <summary>
            /// 큐펫 페널 슬롯 개수 (캐릭터 제외) (8)
            /// </summary>
            public const int CUPET_PANEL_SLOT_SIZE = 8;

            /// <summary>
            /// 장비에 장착할 수 있는 카드 슬롯 최대 개수 (4)
            /// </summary>
            public const int MAX_EQUIPPED_CARD_COUNT = 4;

            /// <summary>
            /// 한 스킬 안에 존재할 수 있는 전투옵션타입 개수 (4)
            /// </summary>
            public const int BATTLE_OPTION_IN_SKILL_COUNT = 4;

            /// <summary>
            /// 일일 길드 퀘스트 완료 가능 횟수
            /// </summary> /// TODO: BasisType으로 받는게 좋을 듯. 아직은 테이블에 항목이 없음.
            public const int GUILD_QUEST_DAILY_REWARD_LIMIT = 3;

            /// <summary>
            /// 전투력 변동 연출에 쓰이는 능력치의 개수
            /// HP, ATK, DEF, MATK, MDEF, HIT, FLEE, CRI
            /// </summary>
            public const int ABILITY_STATUS_COUNT = 8;

            /// <summary>
            /// 셰어 슬롯 개수 (5)
            /// </summary>
            public const int SHARE_SLOT_SIZE = 5;

            /// <summary>
            /// 희망의 영웅 셰어 오픈 조건
            /// </summary>
            public const int HOPE_SHARE_NEED_SIZE = 4;

            /// <summary>
            /// 클론 포함 최대 셰어 슬롯 개수 (8)
            /// </summary>
            //public const int MAX_SHARE_SLOT_SIZE = 8;
        }

        public static class Battle
        {
            /// <summary>
            /// 기본 이동속도
            /// </summary>
            public const float DEFAULT_MOVE_SPEED = 5f;

            /// <summary>
            /// 미로 이동속도
            /// </summary>
            public const float MAZE_MOVE_SPEED = 10f;

            /// <summary>
            /// 마을 이동속도
            /// </summary>
            public const float LOBBY_MOVE_SPEED = 10f;

            /// <summary>
            /// 마을 몬스터 이동속도
            /// </summary>
            public const float LOBBY_MONSTER_MOVE_SPEED = 5f;

            /// <summary>
            /// 공성전 이동속도
            /// </summary>
            public const float SIEGE_MOVE_SPEED = 10f;

            /// <summary>
            /// 도달 거리 제한
            /// </summary>
            public const float SMALL_REMAINING_DISTANCE_LIMIT = 0.1f;

            /// <summary>
            /// 도달 거리 제한 (미로)
            /// </summary>
            public const float MAZE_SMALL_REMAINING_DISTANCE_LIMIT = 2f;

            /// <summary>
            /// 넉백 (보스 연출) 파라메터
            /// </summary>
            public const float KNOCKBACK_POWER_SCALE = 30f;
            public const float BossKnockBackPower = 2.25f;
            public const float KnockBackDrag = 0.3f;
            public const float DragWeightAdd = 0.03f;

            public const float RushKnockBackPower = 1.2f;

            /// <summary>
            /// 돌진 파라메터
            /// </summary>
            public const int RushLerpFunctionOrder = 2;
            public const float RushTime = 0.8f;
            public const float RushEndDeg = 0.45f;

            public static Vector3 RushEffect_Front_StartPosition = new Vector3(0f, 1.5f, 2f);
            public static Vector3 RushEffect_Front_Scale = new Vector3(1.25f, 1.25f, 1f);
            public const float RushEffect_Front_Duration = 0.25f;
            public const float RushEffect_Back_Duration = 0.25f;

            /// <summary>
            /// 낙하 시간
            /// </summary>
            public const float FALL_TIME = 1.9f;
            public const float FALL_SHADOW_MAX_SIZE = 5f;
            public const string BOSS_SHADOW_NAME = "Shadow150";
            public const string FALL_DUSTWAVE_NAME = "CrashDust";

            public const int JOB_CHANGE_EFFECT_DURATION = 250;

            /// <summary>
            /// 최대 난이도 (8 = 1~8난이도)
            /// </summary>
            public const int MAX_DIFFICULTY = 8;

            /// <summary>
            /// 미로 최대 마나
            /// </summary>
            public const int MAX_MAZE_MP = 100;

            /// <summary>
            /// 보스 전투 가능한 퀘스트코인 수 (3개)
            /// </summary>
            public const int MATCH_MULTI_NEED_TO_BOSS_BATTLE = 3;

            public const int MAZE_DIFFICULTY_EASY = 1;
            public const int MAZE_DIFFICULTY_HARD = 2;

            /// <summary>
            /// 빈 스킬 ID
            /// </summary>
            public const int EMPTY_SKILL_ID = 9999999;

            /// <summary>
            /// mp 리젠 딜레이
            /// </summary>
            public static float REGEN_MP_DELAY = 1f;
        }

        public static class URL
        {
            /// <summary>
            /// 문의하기 사이트
            /// </summary>
            public const string CENTER = "https://latalew.zendesk.com/hc/ko/requests/new";

            /// <summary>
            /// 쿠폰 등록 사이트
            /// </summary>
            public const string COUPON = "http://latale.coupon.funipoll.co.kr/funiweb/coupon/";
        }

        /// <summary>
        /// 몬스터 관련 클래스
        /// </summary>
        public static class Monster
        {
            public static class GuildMaze
            {
                public const int NEXUS_MONSTER_ID = 59923;
            }
        }

        /// <summary>
        /// 거래소/개인상점 관련 클래스
        /// </summary>
        public static class Trade
        {
            public const int PRIVATE_STORE_PRODUCT_COUNT_PER_LINE = 3;
            public const int PRIVATE_STORE_MAX_UPLOAD_COUNT = 12;
            public const int PRIVATE_STORE_NAME_MAX_COUNT = 16;

            /// <summary>
            /// 거래소 충돌/무시 관련 값.
            /// 숫자가 낮을 수록 우선된다.
            /// </summary>
            public const int TRADESHOP_DEFAULT_AVOIDANCE_PRIORITY = 50;
            public const int TRADESHOP_OBSTACLE_AVOIDANCE_PRIORITY = 40;

            public const float CHARACTER_NICKNAME_POS_OFFSET = -45F;
            public const int CHARACTER_NICKNAME_FONT_SIZE = 18;

            /// <summary>
            /// 개인상점 개설 시 플레이어가 변신할 모습의 prefab 이름
            /// </summary>
            public static readonly string PRIVATE_STORE_FORM_PREFAB_NAME_1 = "NPC_Poring";
            public static readonly string PRIVATE_STORE_FORM_PREFAB_NAME_2 = "NPC_Poporing";
            public static readonly string PRIVATE_STORE_FORM_PREFAB_NAME_3 = "NPC_Marin";
            public static readonly string PRIVATE_STORE_FORM_PREFAB_NAME_4 = "NPC_Drops";

            public static string GetPrivateStoreFormPrefabName(int index)
            {
                switch (index)
                {
                    case 0: return PRIVATE_STORE_FORM_PREFAB_NAME_1;
                    case 1: return PRIVATE_STORE_FORM_PREFAB_NAME_2;
                    case 2: return PRIVATE_STORE_FORM_PREFAB_NAME_3;
                    case 3: return PRIVATE_STORE_FORM_PREFAB_NAME_4;
                }
                return string.Empty;
            }
        }


        public static class AutoEquip
        {
            /// <summary>
            /// 자동장착 뷰 제한 시간 (단위: 초)
            /// </summary>
            public static float VIEW_TIMER_DURATION = 20f;
        }

        public static class Screen
        {
            public static int SCREEN_WIDTH = 720;
            public static int SCREEN_HEIGHT = 1280;
            public static int WORLDMAP_ZOOM_PADDING = 100;
            public static float UI_CAMERA_SCALE_INVERSE = 640f;
            public static float UI_CAMERA_SCALE = 1f / UI_CAMERA_SCALE_INVERSE;
        }

        public static class MiniGame
        {
            /// <summary>
            /// 바위 생성 간격
            /// </summary>
            public static float ROCK_SPAWN_COOLTIME = 2f;

            /// <summary>
            /// 바위 내려오는 속도
            /// </summary>
            public static float ROCK_SPEED = 300f;

            /// <summary>
            /// 바위 내려오는 속도가 두배속이 되기까지 필요한 시간 (단위 : 초)
            /// </summary>
            public static float NEED_TIME_TO_ROCK_X2_SPEED = 25f;
        }

        public static class NPC
        {
            /// <summary>
            /// NPC 대사 구분자
            /// </summary>
            public static string[] NPC_DIALOG_DELIMITER = new string[] { "$$" };
        }

        public static class SFX
        {
            public static class Battle
            {
                public static string ZENY = "Drop_Coin";
                public static string EXP = "Exp";
                public static string QUEST_COIN = "Quest_Coin";
                public static string SPEED_POTION = "Speed_Potion";
                public static string SNOWBALL = "[SYSTEM] Gacha_Result_Window";
                public static string POWER_UP_POTION = "[SYSTEM] Field_Boss_Appear";
                public static string HP_POTION = "[SYSTEM] Field_Boss_Appear";
                public static string EMPERIUM = "Quest_Coin";
            }

            public static class UI
            {
                public static string OPEN_QUEST_REWARD = "UI_QuestReward";
                public static string OPEN_CONTENTS_UNLOCK = "UI_OpenContent";
                public static string CARD_LEVEL_UP = "UI_QuestReward";
                public static string EQUIPMENT_LEVEL_UP = "UI_QuestReward";
            }
        }

        public static class OpenCondition
        {
            public const int NEED_SKILL_JOB_GRADE = 1; // 스킬 이용 가능 조건: 1차 전직 후 이용
            public const int NEED_SHARE_REGISTER_JOB_GRADE = 1; // 셰어 등록 가능 조건 : 1차 전직 후 이용
            public const int NEED_CENTRAL_LAB_JOB_GRADE = 2; // 중앙실험실 가능 조건 : 2차 전직 후 이용
            public const int NEED_SHARE_FILTER_JOB_GRADE = 3; // 셰어바이스 필터 가능 조곤 : 3차 전직 후 이용
        }

        public static class LocalizeTexture
        {
            public const string PROLOGUE_TIP_NAME = "PrologueTip{0:D2}";
        }

        /// <summary>
        /// 아바타 장착 기본위치
        /// </summary>
        public static class AvatarNode
        {
            public const string HAND_RIGHT = "Root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand";
            public const string HAND_LEFT = "Root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm/Bip001 L Forearm/Bip001 L Hand";
            public const string HEAD = "Root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head/Bip001 HeadNub";
            public const string CAPE = "Root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1";
        }

        public static class Appearance
        {
            public const string EMPTY_WEAPON_PREFAB_NAME = "1_G_SwordO";
        }

        public static class Quest
        {
            public const int MAIN_QUEST_JUMP_START_GROUP_ID = 225; // 메인 퀘스트 점프 시작 그룹ID
            public const int MAIN_QUEST_JUMP_DESTINATION_GROUP_ID = 298; // 메인 퀘스트 점프 도착 그룹ID
        }

        public static class Movement
        {
            public const int NPC_AVOIDANCE_PRIORITY = 0;
            public const int PLAYER_AVOIDANCE_PRIORITY = 1;
            public const int ETC_AVOIDANCE_PRIORITY = 50;
        }

        public static class CommonAtlas
        {
            public const string UI_COMMON_BG_MENU_UPDATE = "Ui_Common_BG_MenuUpdate";
            public const string UI_COMMON_BG_MENU_LOCK = "Ui_Common_BG_MenuLock";
            public const string UI_COMMON_ICON_MVP = "Ui_Common_Icon_MVP";
            public const string UI_COMMON_ICON_MVP_2 = "Ui_Common_Icon_MVP_2";
            public const string UI_COMMON_ICON_MVP_3 = "Ui_Common_Icon_MVP_3";
            public const string UI_COMMON_ICON_BOSS4 = "Ui_Common_Icon_Boss4";
            public const string UI_COMMON_ICON_SMALL = "Ui_Common_Icon_Small";
            public const string UI_COMMON_ICON_MEDIUM = "Ui_Common_Icon_Medium";
            public const string UI_COMMON_ICON_LARGE = "Ui_Common_Icon_Large";
            public const string Ui_Common_BG_Item_Default = "Ui_Common_BG_Item_Default";
            public const string UI_COMMON_BG_ITEM_01 = "Ui_Common_BG_Item_01";
            public const string UI_COMMON_BG_ITEM_02 = "Ui_Common_BG_Item_02";
            public const string UI_COMMON_BG_ITEM_03 = "Ui_Common_BG_Item_03";
            public const string UI_COMMON_BG_ITEM_04 = "Ui_Common_BG_Item_04";
            public const string UI_COMMON_BG_ITEM_05 = "Ui_Common_BG_Item_05";
            public const string UI_COMMON_BG_ITEM_06 = "Ui_Common_BG_Item_06";
            public const string UI_COMMON_BG_ITEM_SHADOW = "Ui_Common_BG_Item_Shadow";
            public const string Ui_Common_BG_Item_Type2_Default = "Ui_Common_BG_Item_Type2_Default";
            public const string UI_COMMON_BG_ITEM_TYPE2_01 = "Ui_Common_BG_Item_Type2_01";
            public const string UI_COMMON_BG_ITEM_TYPE2_02 = "Ui_Common_BG_Item_Type2_02";
            public const string UI_COMMON_BG_ITEM_TYPE2_03 = "Ui_Common_BG_Item_Type2_03";
            public const string UI_COMMON_BG_ITEM_TYPE2_04 = "Ui_Common_BG_Item_Type2_04";
            public const string UI_COMMON_BG_ITEM_TYPE2_05 = "Ui_Common_BG_Item_Type2_05";
            public const string UI_COMMON_BG_ITEM_TYPE2_06 = "Ui_Common_BG_Item_Type2_06";
            public const string UI_COMMON_BG_ITEM_TYPE2_SHADOW = "Ui_Common_BG_Item_Type2_Shadow";
            public const string UI_COMMON_ICON_CARD_NONE = "Ui_Common_Icon_Card_None";
            public const string UI_COMMON_ICON_CARD_1 = "Ui_Common_Icon_Card_1";
            public const string UI_COMMON_ICON_CARD_2 = "Ui_Common_Icon_Card_2";
            public const string UI_COMMON_ICON_CARD_3 = "Ui_Common_Icon_Card_3";
            public const string UI_COMMON_ICON_EQUIP_NORMAL = "Ui_Common_Icon_Equip_Normal";
            public const string UI_COMMON_ICON_EQUIP_SHADOW = "Ui_Common_Icon_Equip_Shadow";
            public const string UI_COMMON_INFO = "Ui_Common_Info";
            public const string UI_COMMON_AGENT = "Ui_Common_Agent";
        }

        public static class MapAtlas
        {
            public const string UI_ICON_REWARD_ZENY = "UI_icon_reward_zeny";
            public const string UI_ICON_REWARD_EXP = "UI_icon_reward_exp";
            public const string UI_ICON_REWARD_AIRSHIP = "UI_icon_reward_airship";
        }

        public static class UITexute
        {
            public const string UI_MAP_INFO_MEMORIAL_01 = "Ui_Map_Info_Memorial_01";
            public const string UI_MAP_INFO_MEMORIAL_02 = "Ui_Map_Info_Memorial_02";
            public const string UI_MAP_INFO_MEMORIAL_03 = "Ui_Map_Info_Memorial_03";

            public const string LABYRINTY_PASS = "Labyrinth_pass_banner";
            public const string LABYRINTY_PASS_ONBUFF = "Labyrinth_pass_banner_OnBuff";
        }
    }
}