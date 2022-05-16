using System.Collections.Generic;

namespace Ragnarok
{
    public class BasisType : EnumBaseType<BasisType>
    {
        public BasisType(int key, string value) : base(key, value) { }

        public bool GetBool(int seq = -1)
        {
            if (seq == -1)
                return BasisDataManager.Instance.GetBool(Key);

            return BasisDetailDataManager.Instance.GetBool(Key, seq);
        }

        public int GetInt(int seq = -1)
        {
            if (seq == -1)
                return BasisDataManager.Instance.GetInt(Key);

            return BasisDetailDataManager.Instance.GetInt(Key, seq);
        }

        public string GetString(int seq = -1)
        {
            if (seq == -1)
                return BasisDataManager.Instance.GetString(Key);

            return BasisDetailDataManager.Instance.GetString(Key, seq);
        }

        public float GetFloat(int seq = -1)
        {
            if (seq == -1)
                return BasisDataManager.Instance.GetFloat(Key);

            return BasisDetailDataManager.Instance.GetFloat(Key, seq);
        }

        public List<int> GetKeyList()
        {
            return BasisDetailDataManager.Instance.GetKeyList(Key);
        }

        /// <summary>1. 스토리 필드 웨이브 쿨타임(millSec)</summary>
        public static BasisType STORY_FIELD_WAVE_COOL_TIME           = new BasisType(1, "스토리 필드 웨이브 쿨타임(millSec)");
        /// <summary>2. 마을 로비 씬 이름</summary>
        public static BasisType TOWN_LOBBY_SCENE_NAME                = new BasisType(2, "마을 로비 씬 이름");
        /// <summary>3. 스킬 id 참조</summary>
        public static BasisType REF_BASIS_ACTIVE_SKILL_ID            = new BasisType(3, "스킬 id 참조");
        /// <summary>4. 아이템 id 참조</summary>
        public static BasisType REF_ITEM_ID                          = new BasisType(4, "아이템 id 참조");
        /// <summary>5. 일반 경험치 지급 편차</summary>
        public static BasisType NORMAL_EXP_DEVIATION                 = new BasisType(5, "일반 경험치 지급 편차");
        /// <summary>6. 직업 경험치 지급 편차</summary>
        public static BasisType JOB_EXP_DEVIATION                    = new BasisType(6, "직업 경험치 지급 편차");
        /// <summary>7. 제니 지급 편차</summary>
        public static BasisType ZENY_DEVIATION                       = new BasisType(7, "제니 지급 편차");
        /// <summary>8. 필드 던전 무료 입장 횟수</summary>
        public static BasisType FIELD_DUNGEON_FREE_JOIN_CNT          = new BasisType(8, "필드 던전 무료 입장 횟수");
        /// <summary>9. 중앙실험실 던전 무료 입장 횟수</summary>
        public static BasisType CENTRAL_LAB_FREE_JOIN_CNT            = new BasisType(9, "중앙실험실 던전 무료 입장 횟수");
        /// <summary>10. 수면 던전 보상 감소율(필드 클리어 시 감소 누적)</summary>
        public static BasisType SLEEP_DUNGEON_FREE_JOIN_CNT          = new BasisType(10, "수면 던전 보상 감소율(필드 클리어 시 감소 누적)");
        /// <summary>11. 냥다래 맺히는 시간(밀리초)</summary>
        public static BasisType FREE_CAT_POINT_COOL_TIME             = new BasisType(11, "냥다래 맺히는 시간(밀리초)");
        /// <summary>12. 테이밍 던전 직업 제한</summary>
        public static BasisType TAMING_DUNGEON_LIMIT_JOB_LEVEL       = new BasisType(12, "테이밍 던전 직업 제한");
        /// <summary>13. 캐릭터 생성시 지급되는 제니</summary>
        public static BasisType CREATE_CHAR_DEFAULT_ZENY             = new BasisType(13, "캐릭터 생성시 지급되는 제니");
        /// <summary>14. 캐릭터 생성시 지급되는 냥다래 열매</summary>
        public static BasisType CREATE_CHAR_DEFAULT_CAT_POINT        = new BasisType(14, "캐릭터 생성시 지급되는 냥다래 열매");
        /// <summary>15. 길드 스퀘어 직업 제한</summary>
        public static BasisType GUILD_SQUARE_LIMIT_JOB_LEVEL         = new BasisType(15, "길드 스퀘어 직업 제한");
        /// <summary>16. 쉐어 레벨업 즉시 완료 냥다래 책정 단위 (밀리초)</summary>
        public static BasisType SHAREVICE_LEVELUP_INIT_TIME          = new BasisType(16, "쉐어 레벨업 즉시 완료 냥다래 책정 단위 (밀리초)");
        /// <summary>17. 스킬 포인트 기본값</summary>
        public static BasisType CREATE_CHAR_DEFAULT_SKILL_POINT      = new BasisType(17, "스킬 포인트 기본값");
        /// <summary>18. 스탯 포인트 기본값</summary>
        public static BasisType CREATE_CHAR_DEFAULT_STAT_POINT       = new BasisType(18, "스탯 포인트 기본값");
        /// <summary>19. 캐릭터 생성시 STR</summary>
        public static BasisType CREATE_CHAR_DEFAULT_STR              = new BasisType(19, "캐릭터 생성시 STR");
        /// <summary>20. 캐릭터 생성시 AGI</summary>
        public static BasisType CREATE_CHAR_DEFAULT_AGI              = new BasisType(20, "캐릭터 생성시 AGI");
        /// <summary>21. 캐릭터 생성시 VIT</summary>
        public static BasisType CREATE_CHAR_DEFAULT_VIT              = new BasisType(21, "캐릭터 생성시 VIT");
        /// <summary>22. 캐릭터 생성시 INT</summary>
        public static BasisType CREATE_CHAR_DEFAULT_INT              = new BasisType(22, "캐릭터 생성시 INT");
        /// <summary>23. 캐릭터 생성시 DEX</summary>
        public static BasisType CREATE_CHAR_DEFAULT_DEX              = new BasisType(23, "캐릭터 생성시 DEX");
        /// <summary>24. 캐릭터 생성시 LUX</summary>
        public static BasisType CREATE_CHAR_DEFAULT_LUX              = new BasisType(24, "캐릭터 생성시 LUX");
        /// <summary>25. 가방 슬롯 기본값</summary>
        public static BasisType CREATE_CHAR_DEFAULT_INVEN_CNT        = new BasisType(25, "가방 슬롯 기본값");
        /// <summary>26. 가방 슬롯 최대값</summary>
        public static BasisType MAX_INVEN_CNT                        = new BasisType(26, "가방 슬롯 최대값");
        /// <summary>27. 가방 슬롯 확장 단위</summary>
        public static BasisType BUY_INVEN_CNT                        = new BasisType(27, "가방 슬롯 확장 단위");
        /// <summary>28. 가방 슬롯 확장 가격(제니)</summary>
        public static BasisType PRICE_INVEN_CNT                      = new BasisType(28, "가방 슬롯 확장 가격(제니)");
        /// <summary>29. 가방 슬롯 확장 가격 가중치(제니)</summary>
        public static BasisType INC_PRICE_INCEN_CNT                  = new BasisType(29, "가방 슬롯 확장 가격 가중치(제니)");
        /// <summary>30. 캐릭터 슬롯 기본값</summary>
        public static BasisType CREATE_CHAR_DEFAULT_CHAR_SLOT        = new BasisType(30, "캐릭터 슬롯 기본값");
        /// <summary>31. 캐릭터 슬롯 최대값</summary>
        public static BasisType MAX_CHAR_SLOT                        = new BasisType(31, "캐릭터 슬롯 최대값");
        /// <summary>32. 파견 즉시 완료에 사용되는 냥다래. 기초데이터 33번 단위로 곱하여 사용</summary>
        public static BasisType EXPLORE_IMMEDIATLY_COMPLETE_CATCOIN  = new BasisType(32, "파견 즉시 완료에 사용되는 냥다래. 기초데이터 33번 단위로 곱하여 사용");
        /// <summary>33. 파견 즉시 완료 냥다래 책정 단위 (밀리초)</summary>
        public static BasisType EXPLORE_COMPLETE_INIT_TIME           = new BasisType(33, "파견 즉시 완료 냥다래 책정 단위 (밀리초)");
        /// <summary>34. 소형 몬스터 클래스 별 대미지 %</summary>
        public static BasisType SMALL_MONSTER_DAMAGE                 = new BasisType(34, "소형 몬스터 클래스 별 대미지 %");
        /// <summary>35. 중형 몬스터 클래스 별 대미지 %</summary>
        public static BasisType MEDIUM_MONSTER_DAMAGE                = new BasisType(35, "중형 몬스터 클래스 별 대미지 %");
        /// <summary>36. 대형 몬스터 클래스 별 대미지 %</summary>
        public static BasisType LARGE_MONSTER_DAMAGE                 = new BasisType(36, "대형 몬스터 클래스 별 대미지 %");
        /// <summary>37. 무속성 속성별 대미지 %</summary>
        public static BasisType NEUTRAL_ENEMENT_DAMAGE               = new BasisType(37, "무속성 속성별 대미지 %");
        /// <summary>38. 화속성 속성별 대미지 %</summary>
        public static BasisType FIRE_ENEMENT_DAMAGE                  = new BasisType(38, "화속성 속성별 대미지 %");
        /// <summary>39. 수속성 속성별 대미지 %</summary>
        public static BasisType WATER_ENEMENT_DAMAGE                 = new BasisType(39, "수속성 속성별 대미지 %");
        /// <summary>40. 풍속성 속성별 대미지 %</summary>
        public static BasisType WIND_ENEMENT_DAMAGE                  = new BasisType(40, "풍속성 속성별 대미지 %");
        /// <summary>41. 지속성 속성별 대미지 %</summary>
        public static BasisType EARTH_ENEMENT_DAMAGE                 = new BasisType(41, "지속성 속성별 대미지 %");
        /// <summary>42. 독속성 속성별 대미지 %"</summary>
        public static BasisType POISON_ENEMENT_DAMAGE                = new BasisType(42, "독속성 속성별 대미지 %");
        /// <summary>43. 성속성 속성별 대미지 %</summary>
        public static BasisType HOLY_ENEMENT_DAMAGE                  = new BasisType(43, "성속성 속성별 대미지 %");
        /// <summary>44. 암속성 속성별 대미지 %</summary>
        public static BasisType SHADOW_ENEMENT_DAMAGE                = new BasisType(44, "암속성 속성별 대미지 %");
        /// <summary>45. 염속성 속성별 대미지 %"</summary>
        public static BasisType GHOST_ENEMENT_DAMAGE                 = new BasisType(45, "염속성 속성별 대미지 %");
        /// <summary>46. 언데드속성 속성별 대미지 %</summary>
        public static BasisType UNDEAD_ENEMENT_DAMAGE                = new BasisType(46, "언데드속성 속성별 대미지 %");
        /// <summary>47. 챕터 별 스토리 제목 언어 아이디</summary>
        public static BasisType STORY_BOOK_TITLE_LANGUAGE_ID         = new BasisType(47, "챕터 별 스토리 제목 언어 아이디");
        /// <summary>48. 챕터 별 스토리 설명 언어 아이디</summary>
        public static BasisType STORY_BOOK_STORY_LANGUAGE_ID         = new BasisType(48, "챕터 별 스토리 설명 언어 아이디");
        /// <summary>49. 직업 차수에 따른 오픈 셰어 슬롯</summary>
        public static BasisType SHARE_OPEN_SLOT_BY_JOB_GRADE         = new BasisType(49, "직업 차수에 따른 오픈 셰어 슬롯");
        /// <summary>50. 길드 생성에 필요한 최소 직업 레벨</summary>
        public static BasisType GUILD_CREATE_JOB_LEVEL               = new BasisType(50, "길드 생성에 필요한 최소 직업 레벨");
        /// <summary>51. 길드 생성 비용(제니)</summary>
        public static BasisType GUILD_CREATE_ZENY                    = new BasisType(51, "길드 생성 비용(제니)");
        /// <summary>52. 길드 생성 시 최초 수용 인원</summary>
        public static BasisType GUILD_CREATE_MAX_USER                = new BasisType(52, "길드 생성 시 최초 수용 인원");
        /// <summary>53. 길드 레벨업 당 증가되는 수용 인원치</summary>
        public static BasisType GUILD_LEVEL_UP_INC_USER              = new BasisType(53, "길드 레벨업 당 증가되는 수용 인원치");
        /// <summary>54. 길드 탈퇴 시 재가입 대기시간(밀리초)</summary>
        public static BasisType GUILD_REJOIN_TIME                    = new BasisType(54, "길드 탈퇴 시 재가입 대기시간(밀리초)");
        /// <summary>55. 길드 출석 시 지급 되는 길드 포인트[길드에게])</summary>
        public static BasisType GUILD_ATTEND_GUILD_POINT             = new BasisType(55, "길드 출석 시 지급 되는 길드 포인트[길드에게])");
        /// <summary>56. 길드 출석 시 지급 되는 길드 코인[출석한 유저에게]</summary>
        public static BasisType GUILD_ATTEND_GUILD_COIN              = new BasisType(56, "길드 출석 시 지급 되는 길드 코인[출석한 유저에게]");
        /// <summary>57. 길드 스킬 경험치를 길드 코인으로 올리기 위한 코인량</summary>
        public static BasisType GUILD_SKILL_BUY_EXP_COIN             = new BasisType(57, "길드 스킬 경험치를 길드 코인으로 올리기 위한 코인량");
        /// <summary>58. 길드 스킬 경험치를 냥다래로 올리기 위한 냥다래량</summary>
        public static BasisType GUILD_SKILL_BUY_EXP_CAT_COIN         = new BasisType(58, "길드 스킬 경험치를 냥다래로 올리기 위한 냥다래량");
        /// <summary>59. 길드 스킬 경험치를 길드 코인으로 올렸을 때, 1회당 증가되는 경험치</summary>
        public static BasisType GUILD_SKILL_EXP_COIN                 = new BasisType(59, "길드 스킬 경험치를 길드 코인으로 올렸을 때, 1회당 증가되는 경험치");
        /// <summary>60. 길드 스킬 경험치를 냥다래로 올렸을 때, 1회당 증가되는 경험치</summary>
        public static BasisType GUILD_SKILL_EXP_CAT_COIN             = new BasisType(60, "길드 스킬 경험치를 냥다래로 올렸을 때, 1회당 증가되는 경험치");
        /// <summary>61. 길드 코인으로 길드 스킬 경험치를 올렸을 때, 재사용 대기시간(밀리초)</summary>
        public static BasisType GUILD_SKILL_BUY_EXP_COIN_COOLTIME    = new BasisType(61, "길드 코인으로 길드 스킬 경험치를 올렸을 때, 재사용 대기시간(밀리초)");
        /// <summary>62. 하루에 냥다래로 길드 스킬 경험치를 올릴 수 있는 횟수 제한[오전 0시초기화]</summary>
        public static BasisType GUILD_SKILL_BUY_EXP_CAT_COIN_MAX_CNT = new BasisType(62, "하루에 냥다래로 길드 스킬 경험치를 올릴 수 있는 횟수 제한[오전 0시초기화]");
        /// <summary>63. 길드 마크 변경에 드는 비용(냥다래)</summary>
        public static BasisType GUILD_MARK_CHANGE_CAT_COIN           = new BasisType(63, "길드 마크 변경에 드는 비용(냥다래)");
        /// <summary>64. 길드 레벨업에 필요한 길드 경험치</summary>
        public static BasisType GUILD_LEVEL_UP_EXP                   = new BasisType(64, "길드 레벨업에 필요한 길드 경험치");
        /// <summary>65. 길드 스킬의 레벨업에 필요한 경험치[모든 스킬 공통]</summary>
        public static BasisType GUILD_SKILL_LEVEL_UP_EXP             = new BasisType(65, "길드 스킬의 레벨업에 필요한 경험치[모든 스킬 공통]");
        /// <summary>66. 길드장 미접속 기간으로 인한 길드장 권환 해임(7일)</summary>
        public static BasisType GUILD_MASTER_NOCONNECTION_DAYS       = new BasisType(66, "길드장 미접속 기간으로 인한 길드장 권환 해임(7일)");
        /// <summary>67. 길드 가입 신청 제한(10회)</summary>
        public static BasisType GUILD_REQUEST_LIMIT                  = new BasisType(67, "길드 가입 신청 제한(10회)");
        /// <summary>68. 길드 스킬 경험치를 올리기 위해 필요한 길드 LV</summary>
        public static BasisType GUILD_SKILL_LEVEL_LIMIT_GUILD_LEVEL  = new BasisType(68, "길드 스킬 경험치를 올리기 위해 필요한 길드 LV");
        /// <summary>69. 전날 출석 인원에 비례하는, 길드 누적 출석 보상 (아이템_id 참조)</summary>
        public static BasisType GUILD_ATTEND_COUNT_REWARD            = new BasisType(69, "전날 출석 인원에 비례하는, 길드 누적 출석 보상 (아이템_id 참조)");
        /// <summary>70. 부 길드장 인원수 제한</summary>
        public static BasisType GUILD_PART_MASTER_LIMIT              = new BasisType(70, "부 길드장 인원수 제한");
        /// <summary>71. 길드 기여도를 개인에게 지급하는 방식</summary>
        public static BasisType GUILD_CONTRIBUTION_POINT             = new BasisType(71, "길드 기여도를 개인에게 지급하는 방식");
        /// <summary>72. 길드 퀘스트를 클리어 했을 때, 보상과 함께 지급되는 길드 경험치[길드에게]</summary>
        public static BasisType GUILD_QUEST_CLEAR_EXP                = new BasisType(72, "길드 퀘스트를 클리어 했을 때, 보상과 함께 지급되는 길드 경험치[길드에게]");
        /// <summary>73. 길드 퀘스트를 클리어 했을 때, 보상과 함께 지급되는 길드 코인[클리어한 유저에게]</summary>
        public static BasisType GUILD_QUEST_CLEAR_CHAR_COIN          = new BasisType(73, "길드 퀘스트를 클리어 했을 때, 보상과 함께 지급되는 길드 코인[클리어한 유저에게]");
        /// <summary>74. 길드 스킬 리스트</summary>
        public static BasisType GUILD_SKILL                          = new BasisType(74, "길드 스킬 리스트");
        /// <summary>75. 길드 가입에 필요한 최소 직업 레벨</summary>
        public static BasisType GUILD_JOIN_NEED_JOB_LEVEL            = new BasisType(75, "길드 가입에 필요한 최소 직업 레벨");
        /// <summary>76. 길드 스킬이 레벨업할 때, 필요한 대기 시간(밀리초) [모든 스킬 공통]</summary>
        public static BasisType GUILD_SKILL_BUILD_COOL_TIME          = new BasisType(76, "길드 스킬이 레벨업할 때, 필요한 대기 시간(밀리초) [모든 스킬 공통]");
        /// <summary>77. 제니 수식 상수</summary>
        public static BasisType CONST_ZENY                           = new BasisType(77, "제니 수식 상수");
        /// <summary>78. 오버 제련 재료 수식 상수</summary>
        public static BasisType CONST_OVER_SMELT_MATERIAL            = new BasisType(78, "오버 제련 재료 수식 상수");
        /// <summary>79. 일반 제련 재료 수식 상수</summary>
        public static BasisType CONST_SMELT_MATERIAL                 = new BasisType(79, "일반 제련 재료 수식 상수");
        /// <summary>80. Lv.1 동빙 마법 방어력 증가 %</summary>
        public static BasisType LV1_ICE_MAGIC_DEF_INCREASE           = new BasisType(80, "Lv.1 동빙 마법 방어력 증가 %");
        /// <summary>81. 레어 돌림판 소모 재화 수량</summary>
        public static BasisType RARE_ROULETTE_PIECE                  = new BasisType(81, "레어 돌림판 소모 재화 수량");
        /// <summary>82. 스페셜 룰렛 판 즉시 변경에 사용되는 냥다래</summary>
        public static BasisType SPECIAL_ROULETTE_CHANGE_CAT_COIN     = new BasisType(82, "스페셜 룰렛 판 즉시 변경에 사용되는 냥다래");
        /// <summary>83. 스페셜 룰렛 판 즉시 변경 냥다래 책정 단위(밀리초)</summary>
        public static BasisType SPECIAL_ROULETTE_CHANGE_TIME         = new BasisType(83, "스페셜 룰렛 판 즉시 변경 냥다래 책정 단위(밀리초)");
        /// <summary>84. 수면 마법 방어력 감소 %</summary>
        public static BasisType SLEEP_MAGIC_DEF_DECRASE              = new BasisType(84, "수면 마법 방어력 감소 %");
        /// <summary>85. 캐릭터 생성시 스킬 슬롯</summary>
        public static BasisType CREATE_CHAR_DEFAULT_SKILL_SLOT       = new BasisType(85, "캐릭터 생성시 스킬 슬롯");
        /// <summary>86. 맥스 캐릭터 스킬 슬롯</summary>
        public static BasisType MAX_CHAR_SKILL_SLOT                  = new BasisType(86, "맥스 캐릭터 스킬 슬롯");
        /// <summary>87. 첫번째 스킬 슬롯 구매 가격(냥다래 열매)</summary>
        public static BasisType PRICE_SKILL_SLOT_1ST                 = new BasisType(87, "첫번째 스킬 슬롯 구매 가격(냥다래 열매)");
        /// <summary>88. 두번째 스킬 슬롯 구매 가격(냥다래 열매)</summary>
        public static BasisType PRICE_SKILL_SLOT_2ST                 = new BasisType(88, "두번째 스킬 슬롯 구매 가격(냥다래 열매)");
        /// <summary>89. 기본 크리티컬 대미지 %</summary>
        public static BasisType DEFAULT_CRITICAL_DAMAGE_PER          = new BasisType(89, "기본 크리티컬 대미지 % ");
        /// <summary>90. 캐릭터 맥스 레벨</summary>
        public static BasisType USER_MAX_LEVEL                       = new BasisType(90, "캐릭터 맥스 레벨");
        /// <summary>91. 차수별 직업 레벨 최대값</summary>
        public static BasisType JOB_MAX_LEVEL                        = new BasisType(91, "차수별 직업 레벨 최대값");
        /// <summary>92. 스탯 최대치</summary>
        public static BasisType MAX_STAT                             = new BasisType(92, "스탯 최대치");
        /// <summary>93. 기본 전승 누적 스텟 최대값</summary>
        public static BasisType MAX_BASE_REBIRTH_ACCRUE_STAT         = new BasisType(93, "기본 전승 누적 스텟 최대값");
        /// <summary>94. 추가 전승 누적 스텟 최대값</summary>
        public static BasisType MAX_EXTEND_REBIRTH_ACCRUE_STAT       = new BasisType(94, "추가 전승 누적 스텟 최대값");
        /// <summary>95. 추가 전승 비용(냥다래)/기본 전승 최대값 초가부터 필요</summary>
        public static BasisType EXTEND_REBIRTH_PRICE                 = new BasisType(95, "추가 전승 비용(냥다래)/기본 전승 최대값 초가부터 필요");
        /// <summary>96. 클리커 던전 몬스터 속도 (백분율)</summary>
        public static BasisType CLICKER_DUNGEON_MONSTER_SPEED_RATE   = new BasisType(96, "클리커 던전 몬스터 속도 (백분율)");
        /// <summary>97. 대형 몬스터 카메라 각도 (백분율)</summary>
        public static BasisType MVP_VIEW_CAMERA_ANGLE                = new BasisType(97, "대형 몬스터 카메라 각도 (백분율)");
        /// <summary>98. 전직 보상</summary>
        public static BasisType JOB_REWARD                           = new BasisType(98, "전직 보상");
        /// <summary>99. 초월 아이템 제한 직업 레벨 (장착과 초월 제작 제한)</summary>
        public static BasisType ITEM_TRANSCEND_JOB_LEVEL             = new BasisType(99, "초월 아이템 제한 직업 레벨 (장착과 초월 제작 제한)");
        /// <summary>100. 헤어컬러</summary>
        public static BasisType HEAR_COLOR                           = new BasisType(100, "헤어컬러");
        /// <summary>101. 스킬 레벨업에 필요한 스킬 포인트</summary>
        public static BasisType SKILL_LEVEL_NEED_POINT               = new BasisType(101, "스킬 레벨업에 필요한 스킬 포인트");
        /// <summary>102. 스킬 초기화 비용(냥다래 열매)</summary>
        public static BasisType PRICE_SKILL_INIT                     = new BasisType(102, "스킬 초기화 비용(냥다래 열매)");
        /// <summary>103. 광고가 표시되는 전직 차수</summary>
        public static BasisType AD_NEED_JOB_GRADE                    = new BasisType(103, "광고가 표시되는 전직 차수");
        /// <summary>104. 난전 오픈 시간</summary>
        public static BasisType FREE_FIGHT_OPEN_HOUR                 = new BasisType(104, "난전 오픈 시간");
        /// <summary>105. 티어 등급 확률</summary>
        public static BasisType TIAR_RANK_RATE                       = new BasisType(105, "티어 등급 확률");
        /// <summary>106. 타격 확률 x이상은 100%로 타격</summary>
        public static BasisType TARGET_RATE_OVER_TEN_PER             = new BasisType(106, "타격 확률 x이상은 100%로 타격");
        /// <summary>107. 방어 감댐율 최대 퍼센트값</summary>
        public static BasisType MAX_PER_DEF_DECREASE                 = new BasisType(107, "방어 감댐율 최대 퍼센트값");
        /// <summary>108. 공격 속도 최소값</summary>
        public static BasisType MIN_ATTACK_SPEED                     = new BasisType(108, "공격 속도 최소값");
        /// <summary>109. 공격 속도 최대값</summary>
        public static BasisType MAX_ATTACK_SPEED                     = new BasisType(109, "공격 속도 최대값");
        /// <summary>110. 공격 사정 거리 최대값</summary>
        public static BasisType MAX_ATTACK_RANGE                     = new BasisType(110, "공격 사정 거리 최대값");
        /// <summary>111. 이동 속도 최대값</summary>
        public static BasisType MAX_MOVE_SPEED                       = new BasisType(111, "이동 속도 최대값");
        /// <summary>112. 재사용 대기시간 최대 퍼센트값</summary>
        public static BasisType MAX_PER_REUSE_WAIT_TIME              = new BasisType(112, "재사용 대기시간 최대 퍼센트값");
        /// <summary>113. 장비 제련 성공 확률</summary>
        public static BasisType PARTS_ITEM_LEVEL_UP_RATE             = new BasisType(113, "장비 제련 성공 확률");
        /// <summary>114. 큐펫 등급 별 필요 몬스터 조각</summary>
        public static BasisType CUPET_NEED_MON_PIECE                 = new BasisType(114, "큐펫 등급 별 필요 몬스터 조각");
        /// <summary>115. 거래소 물품 최대 등록 수</summary>
        public static BasisType MARKET_MAX_REGIST_COUNT              = new BasisType(115, "거래소 물품 최대 등록 수");
        /// <summary>116. 카드 제련 퍼센트 최대값</summary>
        public static BasisType CARD_SMELT_MAX_PER                   = new BasisType(116, "카드 제련 퍼센트 최대값");
        /// <summary>117. 카드 제련 100퍼센트 시 추가값</summary>
        public static BasisType CARD_SMELT_INC_VALUE                 = new BasisType(117, "카드 제련 100퍼센트 시 추가값");
        /// <summary>118. 냥다래 선물 이벤트 설정값</summary>
        public static BasisType CAT_COIN_GIFT_EVENT_FLAG             = new BasisType(118, "냥다래 선물 이벤트 설정값");
        /// <summary>119. Lv.1 스턴 시간(밀리초)</summary>
        public static BasisType DEFENSE_COEFFICIENT                  = new BasisType(119, "방어력 적용 계수");
        /// <summary>120. 듀얼 대미지 감소 퍼센트가 적용되는 직업 레벨</summary>
        public static BasisType DUEL_DAMAGE_LEVEL                    = new BasisType(120, "듀얼 대미지 감소 퍼센트가 적용되는 직업 레벨");
        /// <summary>121. 듀얼 대미지 감소 퍼센트(백분율)</summary>
        public static BasisType DUEL_DAMAGE_PER                      = new BasisType(121, "듀얼 대미지 감소 퍼센트(백분율)");
        /// <summary>122. 이벤트 멀티미로 무료 입장 횟수 (하루 기준 초기화)</summary>
        public static BasisType EVENT_MULTI_MAZE_FREE_JOIN_COUNT     = new BasisType(122, "이벤트 멀티미로 무료 입장 횟수 (하루 기준 초기화)");
        /// <summary>123. 이벤트 멀티미로 유료 입장 비용(냥다래)</summary>
        public static BasisType EVENT_MULTI_MAZE_CAT_COIN_JOIN       = new BasisType(123, "이벤트 멀티미로 유료 입장 비용(냥다래)");
        /// <summary>124. 이벤트 멀티미로 유료 입장 비용 가중치(냥다래)</summary>
        public static BasisType EVENT_MULTI_MAZE_CAT_COIN_INC        = new BasisType(124, "이벤트 멀티미로 유료 입장 비용 가중치(냥다래)");
        /// <summary>125. 카드 제련 성공 확률</summary>
        public static BasisType CARD_ENCHANT_RATE                    = new BasisType(125, "카드 제련 성공 확률");
        /// <summary>126. 큐펫 등급 최대치</summary>
        public static BasisType CUPET_MAX_RANK                       = new BasisType(126, "큐펫 등급 최대치");
        /// <summary>127. 장비 레벨 최대값</summary>
        public static BasisType PARTS_ITEM_MAX_SMELT                 = new BasisType(127, "장비 레벨 최대값");
        /// <summary>128. 장비 등급 별 오버 제련 시작 제련도</summary>
        public static BasisType PARTS_ITEM_OVER_SMELT                = new BasisType(128, "장비 등급 별 오버 제련 시작 제련도");
        /// <summary>129. 사망 대기 시간</summary>
        public static BasisType UNIT_DEATH_COOL_TIME                 = new BasisType(129, "사망 대기 시간");
        /// <summary>130. 클릭 시 사망 대기 감소 시간</summary>
        public static BasisType UNITY_DEATH_COOL_TIME_DECREASE_CLICK = new BasisType(130, "클릭 시 사망 대기 감소 시간");
        /// <summary>131. 의뢰 퀘스트 무료 지급 시간(밀리초)</summary>
        public static BasisType NORMAL_QUEST_COOL_TIME               = new BasisType(131, "의뢰 퀘스트 무료 지급 시간(밀리초)");
        /// <summary>132. 의뢰 퀘스트 직접 의뢰 받기 비용 (냥다래 열매)</summary>
        public static BasisType NORMAL_QUEST_GET_CAT_COIN            = new BasisType(132, "의뢰 퀘스트 직접 의뢰 받기 비용 (냥다래 열매)");
        /// <summary>133. 의뢰 퀘스트 즉시 완료 비용(냥다래 열매)</summary>
        public static BasisType NORMAL_QUEST_CLEAR_CAT_COIN          = new BasisType(133, "의뢰 퀘스트 즉시 완료 비용(냥다래 열매)");
        /// <summary>134. 주사위 던지기 필요 비용</summary>
        public static BasisType DICE_EVENT_NEED_POINT                = new BasisType(134, "필드 주사위 던지기 필요 비용");
        /// <summary>135. 던전 유료 입장 비용(냥다래 열매) [수면 던전 제외]</summary>
        public static BasisType BUY_DUNGEON_TICKET_CAT_COIN          = new BasisType(135, "던전 유료 입장 비용(냥다래 열매) [수면 던전 제외]");
        /// <summary>136. 던전 유료 입장 비용 가중치(냥다래 열매) [수면 던전 제외]</summary>
        public static BasisType BUY_DUNGEON_TICKET_INC_CAT_COIN      = new BasisType(136, "던전 유료 입장 비용 가중치(냥다래 열매) [수면 던전 제외]");
        /// <summary>137. 의뢰 퀘스트 목록 최대치</summary>
        public static BasisType NORMAL_QUEST_MAX_COUNT               = new BasisType(137, "의뢰 퀘스트 목록 최대치");
        /// <summary>138. 스탯 초기화 비용(냥다래 열매)</summary>
        public static BasisType PRICE_STAT_INIT                      = new BasisType(138, "스탯 초기화 비용(냥다래 열매)");
        /// <summary>139. 비밀 상점 초기화 시간</summary>
        public static BasisType SECRET_SHOP_COOL_TIME                = new BasisType(139, "비밀 상점 초기화 시간");
        /// <summary>140. 비밀 상점 초기화 비용(냥다래)</summary>
        public static BasisType SECRET_SHOP_COOL_INIT_CAT_COIN       = new BasisType(140, "비밀 상점 초기화 비용(냥다래)");
        /// <summary>141. 비밀 상점 목록 수</summary>
        public static BasisType SECRET_SHOP_LIST_MAX                 = new BasisType(141, "비밀 상점 목록 수");
        /// <summary>142. 최초 닉네임 무료 변경 횟수</summary>
        public static BasisType FREE_NAME_CHANGE_CNT                 = new BasisType(142, "최초 닉네임 무료 변경 횟수");
        /// <summary>143. 닉네임 변경 비용(냥다래)</summary>
        public static BasisType NAME_CHANE_CAT_COIN                  = new BasisType(143, "닉네임 변경 비용(냥다래)");
        /// <summary>144. 웨이브 재생성 시 회복력 배율</summary>
        public static BasisType WAVE_HP_INC_RATE                     = new BasisType(144, "웨이브 재생성 시 회복력 배율");
        /// <summary>145. 직업 레벨 최대 Level</summary>
        [System.Obsolete]
        public static BasisType MAX_JOB_LEVEL                        = new BasisType(145, "직업 레벨 최대 Level");
        /// <summary>146. 특별 룰렛판 교체 쿨타임</summary>
        public static BasisType SPECIAL_ROULETTE_CHANGE_COOL_TIME    = new BasisType(146, "특별 룰렛판 교체 쿨타임");
        /// <summary>147. 툭별 룰렛판 뽑기 횟수별 필요 수량(상세)</summary>
        public static BasisType SPECIAL_ROULETTE_NEED_COUNT          = new BasisType(147, "툭별 룰렛판 뽑기 횟수별 필요 수량(상세)");
        /// <summary>148. 냥다래 나무 온라인 보상 시간</summary>
        public static BasisType DAY_CONNECT_REWARD_TIME              = new BasisType(148, "냥다래 나무 온라인 보상 시간");
        /// <summary>149. 냥다래 나무 온라인 보상 수량</summary>
        public static BasisType DAY_CONNECT_REWARD                   = new BasisType(149, "냥다래 나무 온라인 보상 수량");
        /// <summary>150. 오프라인 보상 누적 최대 시간(밀리초X, 초O)</summary>
        public static BasisType TREE_REWARD_MAX_SECOND               = new BasisType(150, "오프라인 보상 누적 최대 시간(밀리초X, 초O)");
        /// <summary>151. 오프라인 보상 누적 단위 시간(초)</summary>
        public static BasisType TREE_REWARD_ACCRUE_SECOND            = new BasisType(151, "오프라인 보상 누적 단위 시간(초)");
        /// <summary>152. 제니 나무 계수(거듭 제곱 밑) //1.04</summary>
        public static BasisType TREE_REWARD_ZENY_COEFFICIENT         = new BasisType(152, "제니 나무 계수(거듭 제곱 밑) //1.04");
        /// <summary>153. 제니 나무 기본값(거듭 제곱 지수)</summary>
        public static BasisType TREE_REWARD_ZENY_BASE                = new BasisType(153, "제니 나무 기본값(거듭 제곱 지수)");
        /// <summary>154. 재료 나무 그룹 참조용(직업 레벨 제수)</summary>
        public static BasisType TREE_REWARD_RESOURCE_REF             = new BasisType(154, "재료 나무 그룹 참조용(직업 레벨 제수)");
        /// <summary>155. 보스 연출(제니 드랍 나누기값,클릭해 횟수)</summary>
        public static BasisType BOSS_BONUS_ZENY_COUNT                = new BasisType(155, "보스 연출(제니 드랍 나누기값,클릭해 횟수)");
        /// <summary>156. 이벤트 난전 지속 시간(밀리초)</summary>
        public static BasisType EVENT_FF_PLAY_TIEM                   = new BasisType(156, "이벤트 난전 지속 시간(밀리초)");
        /// <summary>157. 이벤트 난전 라운드 수</summary>
        public static BasisType EVENT_FF_ROUNT_COUNT                 = new BasisType(157, "이벤트 난전 라운드 수");
        /// <summary>158. 큐펫 패널 효과 적용 칸 별 확률</summary>
        public static BasisType CUPET_PANEL_RATE_POS                 = new BasisType(158, "큐펫 패널 효과 적용 칸 별 확률");
        /// <summary>159. 길드습격 엠펠리움 최대 레벨</summary>
        public static BasisType GUILD_ATTACK_EMPERIUM_MAX_LEVEL      = new BasisType(159, "길드습격 엠펠리움 최대 레벨");
        /// <summary>160. 길드습격 클리어 시간(밀리초)</summary>
        public static BasisType GUILD_ATTACK_CLEAR_TIME              = new BasisType(160, "길드습격 클리어 시간(밀리초)");
        /// <summary>161. 길드습격 시간 변경 비용(엠펠리움 결정)</summary>
        public static BasisType GUILD_ATTACK_CHANGE_TIME_COIN        = new BasisType(161, "길드습격 시간 변경 비용(엠펠리움 결정)");
        /// <summary>162. 길드습격 기부 보상 수량(길드코인)</summary>
        public static BasisType GUILD_ATTACK_DONATION_REWARD_COIN    = new BasisType(162, "길드습격 기부 보상 수량(길드코인)");
        /// <summary>163. 직업 레벨별 마나포션 비용(제니)</summary>
        public static BasisType MP_POTION_ZENY_BY_JOB_LEVEL          = new BasisType(163, "직업 레벨별 마나포션 비용(제니)");
        /// <summary>164. 직업 레벨별 부활 비용(제니)</summary>
        public static BasisType REBIRTH_ZENY_BY_JOB_LEVEL            = new BasisType(164, "직업 레벨별 부활 비용(제니)");
        /// <summary>165. 마나 포션 쿨타임(밀리초)</summary>
        public static BasisType MP_POTION_COOL_TIME                  = new BasisType(165, "마나 포션 쿨타임(밀리초)");
        /// <summary>166. 부활 대기 시간(밀리초)</summary>
        public static BasisType GUILD_ATTACK_REBIRTH_COOL_TIME       = new BasisType(166, "부활 대기 시간(밀리초)");
        /// <summary>167. 노점 판매 제니 판매 수수료(만분율)</summary>
        public static BasisType STALL_ZENY_SALES_CHARGE_RATE         = new BasisType(167, "노점 판매 제니 판매 수수료(만분율)");
        /// <summary>168. 노점 하한가 할인율 (만분율)</summary>
        public static BasisType STALL_LOW_DISCOUNT_RATE              = new BasisType(168, "노점 하한가 할인율 (만분율)");
        /// <summary>169. 고객 보상 감사 리워드 쿨타임(밀리초)</summary>
        public static BasisType CUSTOMER_REWARD_NORMAL_COOLTIME      = new BasisType(169, "고객 보상 감사 리워드 쿨타임(밀리초)");
        /// <summary>170. 고객 보상 프리미엄 쿨타임(밀리초)</summary>
        public static BasisType CUSTOMER_REWARD_PREMIUM_COOLTIME     = new BasisType(170, "고객 보상 프리미엄 쿨타임(밀리초)");
        /// <summary>171. 거래소 판매 제니 수수료 (만분율)</summary>
        public static BasisType MARKET_ZENY_SALES_CHARGE_RATE        = new BasisType(171, "거래소 판매 제니 수수료 (만분율)");
        /// <summary>172. 오버 스탯 성공 확률(만분률)</summary>
        public static BasisType OVER_STATUS_SUCCESS_RATE             = new BasisType(172, "오버 스탯 성공 확률(만분률)");
        /// <summary>173. 고객 보상 감사 리워드 광고 스킵 조건(마일리지)</summary>
        public static BasisType CUSTOMER_REWARD_NORMAL_MILEAGE       = new BasisType(173, "고객 보상 감사 리워드 광고 스킵 조건(마일리지)");
        /// <summary>174. 고객 보상 프리미엄 광고 스킵 조건(마일리지)</summary>
        public static BasisType CUSTOMER_REWARD_PREMIUM_MILEAGE      = new BasisType(174, "고객 보상 프리미엄 광고 스킵 조건(마일리지)");
        /// <summary>175. 교역 및 생산 광고 스킵 조건(마일리지)</summary>
        public static BasisType TRADE_PRODUCTION_MILEAGE             = new BasisType(175, "교역 및 생산 광고 스킵 조건(마일리지)");
        /// <summary>176. 스테이지테이블 언어 아이디</summary>
        public static BasisType STAGE_TBLAE_LANGUAGE_ID              = new BasisType(176, "스테이지테이블 언어 아이디");
        /// <summary>177. 서버 별 입장 가능한 최대 스테이지</summary>
        public static BasisType ENTERABLE_MAXIMUM_STAGE_BY_SERVER    = new BasisType(177, "서버 별 입장 가능한 최대 스테이지");
        /// <summary>178. 몬스터테이블 몬스터 그룹 언어 아이디</summary>
        public static BasisType MONSTER_TABLE_GROUP_ID_LANG_ID       = new BasisType(178, "몬스터테이블 몬스터 그룹 언어 아이디");
        /// <summary>179. 큐펫 레벨 최대값</summary>
        public static BasisType MAX_CUPET_LEVEL                      = new BasisType(179, "큐펫 레벨 최대값");
        /// <summary>180. 교역 및 생산 광고 ON OFF 기능(1=ON 0=OFF)</summary>
        public static BasisType TRADE_PRODUCTION_AD_FLAG             = new BasisType(180, "교역 및 생산 광고 ON OFF 기능(1=ON 0=OFF)");
        /// <summary>181. 큐펫 장착칸 해금 조건</summary>
        public static BasisType OPEN_CUPET_SLOT                      = new BasisType(181, "큐펫 장착칸 해금 조건");
        /// <summary>182. 랜덤 타격 계수</summary>
        public static BasisType RANDOM_DAMAGE_RANGE                  = new BasisType(182, "랜덤 타격 계수");
        /// <summary>183. 큐펫 사망 대기시간 & 큐펫 장착 후 소환 대기시간</summary>
        public static BasisType CUPET_COOL_TIME                      = new BasisType(183, "큐펫 사망 대기시간 & 큐펫 장착 후 소환 대기시간");
        /// <summary>184. Job Level 상승당 지급되는 스킬 포인트</summary>
        public static BasisType JOB_LEVEL_UP_SKILL_POINT             = new BasisType(184, "Job Level 상승당 지급되는 스킬 포인트");
        /// <summary>185. 큐펫 사망 대기시간 & 큐펫 장착 후 소환 대기시간</summary>
        public static BasisType SUMMON_BALL_SKILL_ID                 = new BasisType(185, "서몬 볼로 선택할 수 있는 스킬 4종류(skill id)");
        /// <summary>186. 디펜스 던전(비공정 습격) 무료 입장 횟수</summary>
        public static BasisType DEF_DUNGEON_FREE_JOIN_CNT            = new BasisType(186, "디펜스 던전(비공정 습격) 무료 입장 횟수");
        /// <summary>187. 월드 보스전(무한의 공간) 무료 입장 보유 최대치</summary>
        public static BasisType WORLD_BOSS_FREE_JOIN_CNT             = new BasisType(187, "월드 보스전(무한의 공간) 무료 입장 보유 최대치");
        /// <summary>188. 월드 보스전(무한의 공간) 무료 입장 충전 시간(밀리초</summary>
        public static BasisType WORLD_BOSS_TICKET_TIME_COOL          = new BasisType(188, "월드 보스전(무한의 공간) 무료 입장 충전 시간(밀리초");
        /// <summary>189. 월드 보스전(무한의 공간) 유료 입장 비용(냥다래)</summary>
        public static BasisType WORLD_BOSS_TICKET_CAT_COIN           = new BasisType(189, "월드 보스전(무한의 공간) 유료 입장 비용(냥다래)");
        /// <summary>190. PVE 승리나 패배 시 변동되는 대전 포인트 기본값</summary>
        public static BasisType PVE_SCORE                            = new BasisType(190, "PVE 승리나 패배 시 변동되는 대전 포인트 기본값");
        /// <summary>191. PVE 승리 시 대전 포인트 보정값</summary>
        public static BasisType PVE_WIN_SCORE_REVISION               = new BasisType(191, "PVE 승리 시 대전 포인트 보정값");
        /// <summary>192. PVE 패배 시 대전 포인트 보정값</summary>
        public static BasisType PVE_LOSE_SCORE_REVISION              = new BasisType(192, "PVE 패배 시 대전 포인트 보정값");
        /// <summary>193. PVE 승리나 패배 시 대전 포인트 보정결과 제한값</summary>
        public static BasisType PVE_LIMIT_SCORE_REVISION             = new BasisType(193, "PVE 승리나 패배 시 대전 포인트 보정결과 제한값");
        /// <summary>194. PVE 무료 입장 횟수 [하루 기준 초기화]</summary>
        public static BasisType PVE_FREE_JOIN_CNT                    = new BasisType(194, "PVE 무료 입장 횟수 [하루 기준 초기화]");
        /// <summary>195. PVE 매칭 기본 점수차</summary>
        public static BasisType PVE_MATCH_RANGE_SCORE                = new BasisType(195, "PVE 매칭 기본 점수차");
        /// <summary>196. 3x3 무료 뽑기 직업 레벨 제한</summary>
        public static BasisType SPECIAL_ROULETTE_JOB_LEVEL_LIMIT     = new BasisType(196, "3x3 무료 뽑기 직업 레벨 제한");
        /// <summary>197. 하드 & 챌린지 모드 최대 레벨</summary>
        public static BasisType HARD_CHELLENGE_MAX_LEVEL             = new BasisType(197, "하드 & 챌린지 모드 최대 레벨");
        /// <summary>198. 하드 & 챌린지 모드 Lv당 Stage몬스터 레벨 증가량</summary>
        public static BasisType HARD_CHELLENGE_MONSTER_LEVEL         = new BasisType(198, "하드 & 챌린지 모드 Lv당 Stage몬스터 레벨 증가량");
        /// <summary>199. PVE 시간 초과 패배(밀리초)</summary>
        public static BasisType PVE_BATTLE_TIME                      = new BasisType(199, "PVE 시간 초과 패배(밀리초)");
        /// <summary>200. 전투력 측정 계수</summary>
        public static BasisType ATTACK_POWER_COEFFICIENT             = new BasisType(200, "전투력 측정 계수");
        /// <summary>201. 길드습격 엠펠리움 생성 비용</summary>
        public static BasisType GUILD_ATTACK_EMPERIUM_CREATE_ZENY    = new BasisType(201, "길드습격 엠펠리움 생성 비용");
        /// <summary>202. 길드 퀘스트 완료 가능 최대치</summary>
        public static BasisType GUILD_QUEST_MAX_REWARD_COUNT         = new BasisType(202, "길드 퀘스트 완료 가능 최대치");
        /// <summary>203. 길드습격 엠펠리움 몬스터 ID</summary>
        public static BasisType GUILD_ATTACK_EMPERIUM_MONSTER_ID     = new BasisType(203, "길드습격 엠펠리움 몬스터 ID");
        /// <summary>204. 길드습격 몬스터 타일별 동시 소환 제한</summary>
        public static BasisType GUILD_ATTACK_ONCE_SUMMON_LIMIT       = new BasisType(204, "길드습격 몬스터 타일별 동시 소환 제한");
        /// <summary>205. 길드습격 성공 보상 수량(엠펠리움 결정)</summary>
        public static BasisType GUILD_ATTACK_SUCCESS_REWARD_COUNT    = new BasisType(205, "길드습격 성공 보상 수량(엠펠리움 결정)");
        /// <summary>206. 오버 스탯 성공 확률(천분률)</summary>
        public static BasisType TEMP_OVER_STATUS_SUCCESS_RATE        = new BasisType(206, "오버 스탯 성공 확률(천분률)");
        /// <summary>207. 오버 스탯 max 값</summary>
        public static BasisType OVER_STATUS_MAX                      = new BasisType(207, "오버 스탯 max 값");
        /// <summary>208. 오버 스탯 제니 비용</summary>
        public static BasisType OVER_STATUS_ZENY                     = new BasisType(208, "오버 스탯 제니 비용");
        /// <summary>209. 오버 강화 수치에 따라 증가되는 제니 비용 값(1 * Value)</summary>
        public static BasisType OVER_STATUS_INC_ZENY                 = new BasisType(209, "오버 강화 수치에 따라 증가되는 제니 비용 값(1 * Value)");
        /// <summary>210. 큐브 조각 드랍 확률(만분율)</summary>
        public static BasisType CUBE_PICE_DROP_RATE                  = new BasisType(210, "큐브 조각 드랍 확률(만분율)");
        /// <summary>211. 하드모드, 챌린지 보스 도전 유지시간 (밀리초)</summary>
        public static BasisType HARD_CHELLENGE_BOSS_BATTLE_TIME      = new BasisType(211, "하드모드, 챌린지 보스 도전 유지시간 (밀리초)");
        /// <summary>212. 챌린지 모드 무료 입장 가능 횟수(일일)</summary>
        public static BasisType CHELLENGE_FREE_ENTER_COUNT           = new BasisType(212, "챌린지 모드 무료 입장 가능 횟수(일일)");
        /// <summary>213. 듀얼 시 소모되는 입장권 수</summary>
        public static BasisType ENTER_DUEL_POINT                     = new BasisType(213, "듀얼 시 소모되는 입장권 수");
        /// <summary>214. 챌린지 일일 클리어 가능 횟수</summary>
        public static BasisType CHELLENGE_CLEAR_COUNT_LIMIT          = new BasisType(214, "챌린지 일일 클리어 가능 횟수");
        /// <summary>215. 시야 차단 폭탄의 시야 차단 시간(밀리초)</summary>
        public static BasisType BOMB_MAZE_TIME                       = new BasisType(215, "시야 차단 폭탄의 시야 차단 시간(밀리초)");
        /// <summary>216. 체력 소모 폭탄의 최대 체력 비례 피해 최소값(만분율)</summary>
        public static BasisType BOMB_HP_MAX                          = new BasisType(216, "체력 소모 폭탄의 최대 체력 비례 피해 최소값(만분율)");
        /// <summary>217. 체력 소모 폭탄의 최대 체력 비례 피해 최대값(만분율)</summary>
        public static BasisType BOMB_HP_MIN                          = new BasisType(217, "체력 소모 폭탄의 최대 체력 비례 피해 최대값(만분율)");
        /// <summary>218. 길드 습격 엠펠리움 버프 스킬 ID</summary>
        public static BasisType GUILD_ATTACK_EMPERIUM_BUFF_SKILL_ID  = new BasisType(218, "길드 습격 엠펠리움 버프 스킬 ID");
        /// <summary>221. 길드 습격 시작 시간 목록</summary>
        public static BasisType GUILD_ATTACK_START_TIME_LIST         = new BasisType(221, "길드 습격 시작 시간 목록");
        /// <summary>222. 카드 제련 성공률 (레벨별)</summary>
        public static BasisType CARD_SMELT_SUCCESS_RATE              = new BasisType(222, "카드 제련 성공률 (레벨별)");
        /// <summary>223. 동료 합성에 소모되는 비용(제니)</summary>
        public static BasisType AGENT_COMPOSE_ZENY                   = new BasisType(223, "동료 합성에 소모되는 비용(제니)");
        /// <summary>224. 각 자사별 교역 가능 횟수 [하루 기준 초기화]</summary>
        public static BasisType AGENT_TRADE_MAX_COUNT                = new BasisType(224, "각 자사별 교역 가능 횟수 [하루 기준 초기화]");
        /// <summary>225. 교역, 채집, 발굴 즉시 완료 비용(낭다래)</summary>
        public static BasisType EXPLORE_FAST_CLEAR_PRICE             = new BasisType(225, "교역, 채집, 발굴 즉시 완료 비용(낭다래)");
        /// <summary>226. 교역 횟수 초기화 가격(냥다래)</summary>
        public static BasisType AGENT_TRADE_RESET_PRICE              = new BasisType(226, "교역 횟수 초기화 가격(냥다래)");
        /// <summary>227. 교역 최대 인원</summary>
        public static BasisType AGENT_FIELD_TRADE_MAX_COUNT          = new BasisType(227, "교역 최대 인원");
        /// <summary>228. 채집, 발굴 최대 인원</summary>
        public static BasisType AGENT_FIELD_OTHER_EXPLORE_MAZ_COUNT  = new BasisType(228, "채집, 발굴 최대 인원");
        /// <summary>229. 캐릭터 쉐어 일일 무료 충전 시간(밀리초) [하루 기준 초기화]</summary>
        public static BasisType FREE_USE_CHAR_SHARE_TIME             = new BasisType(229, "캐릭터 쉐어 일일 무료 충전 시간(밀리초) [하루 기준 초기화]");
        /// <summary>230. 캐릭터 쉐어 리스트 노출 제한 누적 전투 시간(밀리초) [보상 받을 </summary>
        public static BasisType CHAR_SHARE_TIME                      = new BasisType(230, "캐릭터 쉐어 리스트 노출 제한 누적 전투 시간(밀리초) [보상 받을 시 초기화]");
        /// <summary>231. 캐릭터 쉐어 이용가능시간 최대치</summary>
        public static BasisType MAX_USE_CHAR_SHARE_TIME              = new BasisType(231, "캐릭터 쉐어 이용가능시간 최대치");
        /// <summary>232. 멀티미로 무료 입장 횟수 [하루 기준 초기화]</summary>
        public static BasisType MULTI_MAZE_FREE_JOIN_COUNT           = new BasisType(232, "멀티미로 무료 입장 횟수 [하루 기준 초기화]");
        /// <summary>233. 멀티미로 유료 입장 비용(냥다래 열매)</summary>
        public static BasisType MULTI_MAZE_CAT_COIN_JOIN             = new BasisType(233, "멀티미로 유료 입장 비용(냥다래 열매)");
        /// <summary>234. 멀티미로 유료 입장 비용 가중치(냥다래 열매)</summary>
        public static BasisType MULTI_MAZE_CAT_COIN_INC              = new BasisType(234, "멀티미로 유료 입장 비용 가중치(냥다래 열매)");
        /// <summary>235. 제니 던전 무료 입장 횟수 [하루 기준 초기화]</summary>
        public static BasisType ZENY_DUNGEON_FREE_JOIN_COUNT         = new BasisType(235, "제니 던전 무료 입장 횟수 [하루 기준 초기화]");
        /// <summary>236. 경험치 던전 무료 입장 횟수 [하루 기준 초기화]</summary>
        public static BasisType EXP_DUNGEON_FREE_JOIN_COUNT          = new BasisType(236, "경험치 던전 무료 입장 횟수 [하루 기준 초기화]");
        /// <summary>237. 드랍으로 획득 가능한 듀얼 입장권 최대치</summary>
        public static BasisType DUEL_POINT_DROP_MAX                  = new BasisType(237, "드랍으로 획득 가능한 듀얼 입장권 최대치");
        /// <summary>238. 듀얼 입장권 충전값(냥다래 열매)</summary>
        public static BasisType DUEL_CAT_COIN_JOIN                   = new BasisType(238, "듀얼 입장권 충전값(냥다래 열매)");
        /// <summary>239. 듀얼 입장권 충전값 가중치(냥다래 열매) [하루 기준 초기화]</summary>
        public static BasisType DUEL_CAT_COIN_INC                    = new BasisType(239, "듀얼 입장권 충전값 가중치(냥다래 열매) [하루 기준 초기화]");
        /// <summary>240. 충전 시 충전되는 입장권 수</summary>
        public static BasisType DUEL_POINT_CHARGE                    = new BasisType(240, "충전 시 충전되는 입장권 수");
        /// <summary>241. 1chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER1          = new BasisType(241, "1chapter 듀얼 조각 name_id");
        /// <summary>242. 2chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER2          = new BasisType(242, "2chapter 듀얼 조각 name_id");
        /// <summary>243. 3chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER3          = new BasisType(243, "3chapter 듀얼 조각 name_id");
        /// <summary>244. 4chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER4          = new BasisType(244, "4chapter 듀얼 조각 name_id");
        /// <summary>245. 5chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER5          = new BasisType(245, "5chapter 듀얼 조각 name_id");
        /// <summary>246. 6chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER6          = new BasisType(246, "6chapter 듀얼 조각 name_id");
        /// <summary>247. 7chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER7          = new BasisType(247, "7chapter 듀얼 조각 name_id");
        /// <summary>248. 8chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER8          = new BasisType(248, "8chapter 듀얼 조각 name_id");
        /// <summary>249. 9chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER9          = new BasisType(249, "9chapter 듀얼 조각 name_id");
        /// <summary>250. 10chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER10         = new BasisType(250, "10chapter 듀얼 조각 name_id");
        /// <summary>251. 11chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER11         = new BasisType(251, "11chapter 듀얼 조각 name_id");
        /// <summary>252. 12chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER12         = new BasisType(252, "12chapter 듀얼 조각 name_id");
        /// <summary>253. 듀얼 입장권 종합 최대치</summary>
        public static BasisType DUEL_POINT_TOTAL_MAX                 = new BasisType(253, "듀얼 입장권 종합 최대치");
        /// <summary>254. 듀얼 목록 변경 대기시간(밀리초)</summary>
        public static BasisType DUEL_LIST_CHANGE_TIME                = new BasisType(254, "듀얼 목록 변경 대기시간(밀리초)");
        /// <summary>255. 월드 보스 전투 제한 시간(밀리초)</summary>
        public static BasisType WORLD_BOSS_BATTLE_TIME               = new BasisType(255, "월드 보스 전투 제한 시간(밀리초)");
        /// <summary>256. PVP 최대 체력 보정 계수(퍼밀리)</summary>
        public static BasisType PVP_HP_RATE                          = new BasisType(256, "PVP 최대 체력 보정 계수(퍼밀리)");
        /// <summary>258. 장비 아이템 카드 슬롯이 해금되는 강화 레벨</summary>
        public static BasisType EQUIPMENT_CARD_SLOT_UNLOCK_LEVEL     = new BasisType(258, "장비 아이템 카드 슬롯이 해금되는 강화 레벨");
        /// <summary>259. 보스 실패 쿨타임</summary>
        public static BasisType BOSS_FAILURE_COOLTIME                = new BasisType(259, "보스 실패 쿨타임");
        /// <summary>260. 캐릭터셰어 충전 아이템 1</summary>
        public static BasisType CHAR_SHARE_CHARGE_ITEM_1             = new BasisType(260, "캐릭터셰어 충전 아이템 1");
        /// <summary>261. 캐릭터셰어 충전 아이템 2</summary>
        public static BasisType CHAR_SHARE_CHARGE_ITEM_2             = new BasisType(261, "캐릭터셰어 충전 아이템 2");
        /// <summary>262. 캐릭터셰어 충전 아이템 3</summary>
        public static BasisType CHAR_SHARE_CHARGE_ITEM_3             = new BasisType(262, "캐릭터셰어 충전 아이템 3");
        /// <summary>263. 일반 돌림판 이벤트 소모 재화 개수</summary>
        public static BasisType NORMAL_ROULETTE_PIECE                = new BasisType(263, "돌림판 이벤트 소모 재화 개수");
        /// <summary>264. 필드 보스 전투 성공시 제한 시간(밀리초)</summary>
        public static BasisType BOSS_CLEAR_COOLTIME                  = new BasisType(264, "보스 처치 성공 후 보스 도전 쿨타임(밀리초)");
        /// <summary>265. 입장 가능한 최대 스테이지</summary>
        public static BasisType ENTERABLE_MAXIMUM_STAGE              = new BasisType(265, "입장 가능한 최대 스테이지");
        /// <summary>267. 월드 보스전(무한의 공간) 입장 비용 가중치(냥다래 열매)</summary>
        public static BasisType WORLD_BOSS_INC_CAT_COIN              = new BasisType(267, "월드 보스전(무한의 공간) 입장 비용 가중치(냥다래 열매)");
        /// <summary>268. 셰어바이스 콘텐츠 해금 시나리오 미로 id</summary>
        public static BasisType SHAREVICE_CONTENTS_OPEN_SCENARIO     = new BasisType(268, "셰어바이스 콘텐츠 해금 시나리오 미로 id");
        /// <summary>269. 로딩 팁 메시지</summary>
        public static BasisType LOADING_TIP_MESSAGE                  = new BasisType(269, "로딩 팁 메시지");
        /// <summary>270. 로딩 팁 이미지 이름 인덱스</summary>
        public static BasisType LOADING_TIP_IMAGE_NAME_INDEX         = new BasisType(270, "로딩 팁 이미지 이름 인덱스");
        /// <summary>271. 최초 전승 보너스 Status point</summary>
        public static BasisType FIRST_REBIRTH_BONUS                  = new BasisType(271, "최초 전승 보너스 Status point");
        /// <summary>272. 서버 이름 ID</summary>
        public static BasisType SERVER_NAME_ID                       = new BasisType(272, "서버 이름 ID");
        /// <summary>273. 카드 강화 복원 비용</summary>
        public static BasisType CARD_RESTORING_COST                  = new BasisType(273, "카드 복원 비용");
        /// <summary>274. 카드 재구성 비용</summary>
        public static BasisType CARD_RESET_COST                      = new BasisType(274, "카드 재구성 비용");
        /// <summary>275. 셰어 고용 기준 전투력</summary>
        public static BasisType SHARE_HIRING_QUALIFICATION           = new BasisType(275, "셰어 고용 기준 전투력");
        /// <summary>276. 상점 상품 한 번에 구매 가능한 수량</summary>
        public static BasisType SHOP_BULK_PURCHASE_MAX_COUNT         = new BasisType(276, "상점 상품 한 번에 구매 가능한 수량");
        /// <summary>277. 던전 추가 입장 제한 횟수</summary>
        public static BasisType DUNGEON_ADD_ENTER_LIMIT              = new BasisType(277, "던전 추가 입장 제한 횟수");
        /// <summary>278. 최초 1회 일일 퀘스트 전체 완료 보상(냥다래)</summary>
        public static BasisType COMPLETE_FIRST_DAILY_QUEST_REWARD    = new BasisType(278, "최초 1회 일일 퀘스트 전체 완료 보상(냥다래)");
        /// <summary>279. 캐릭터 슬롯 해금 조건. 해당 챕터 클리어 시</summary>
        public static BasisType UNROCK_CHARACTER_SLOT                = new BasisType(279, "캐릭터 슬롯 해금 조건. 해당 챕터 진입시 오픈");
        /// <summary>280. 상점 ID 참조</summary>
        public static BasisType REF_SHOP_ID                          = new BasisType(280, "상점 ID 참조");
        /// <summary>281. 첫 결제 보상 가방 무게 증가량</summary>
        public static BasisType FIRST_PURCHASE_REWARD_INVEN_WEIGHT   = new BasisType(281, "첫 결제 보상 가방 무게 증가량");
        /// <summary>282. 테이밍 미로 오픈 시간</summary>
        public static BasisType TAMING_DUNGEON_OPEN_START_TIME       = new BasisType(282, "테이밍 미로 오픈 시간");
        /// <summary>283. 테이밍 미로 지속 시간(밀리초)</summary>
        public static BasisType TAMING_DUNGEON_OPEN_DURATION         = new BasisType(283, "테이밍 미로 지속 시간(밀리초)");
        /// <summary>284. 서버 이름 대표 알파벳(이벤트 듀얼)</summary>
        public static BasisType EVENT_DUEL_ALPHABET                  = new BasisType(284, "서버 이름 대표 알파벳(이벤트 듀얼)");
        /// <summary>285. 셰어 보상 UP 팩 영구 버프 재화 획득량 증가율(만분율)</summary>
        public static BasisType SHARE_UP_POINT_RATE                  = new BasisType(285, "셰어 보상 UP 팩 영구 버프 재화 획득량 증가율(만분율)");
        /// <summary>286. 카프라 회원권 28일 버프 재화 획득량 증가율(만분율)</summary>
        public static BasisType KAFRA_POINT_RATE                     = new BasisType(286, "카프라 회원권 28일 버프 재화 획득량 증가율(만분율)");
        /// <summary>287. 난전 오픈 시각</summary>
        public static BasisType FF_OPEN_HOUR                         = new BasisType(287, "난전 오픈 시각");
        /// <summary>288. 난전 지속 시간(밀리초)</summary>
        public static BasisType FF_PLAY_TIME                         = new BasisType(288, "난전 지속 시간(밀리초)");
        /// <summary>289. 난전 라운드 수</summary>
        public static BasisType FF_ROUNT_COUNT                       = new BasisType(289, "난전 라운드 수");
        /// <summary>290. 초월 아이템 거래 시 추가 RoPoint</summary>
        public static BasisType TRANSCEND_ITEM_EXTRA_ROPOINT         = new BasisType(290, "초월 아이템 거래 시 추가 RoPoint");
        /// <summary>291. 중앙실험실 콘텐츠 해금 직업 레벨</summary>
        public static BasisType CLAB_UNLOCK_LEVEL                    = new BasisType(291, "중앙실험실 콘텐츠 해금 직업 레벨");
        /// <summary>292. 상점 상품 그룹 ID 참조값</summary>
        public static BasisType SHOP_REWARD_GROUP_ID                 = new BasisType(292, "상점 상품 그룹 ID 참조값");
        /// <summary>293. 쉐어 레벨업 시간 단축에 사용되는 냥다래 (16번 단위로 곱하여 사용)</summary>
        public static BasisType SHAREVICE_IMMEDIATLY_LEVELUP_CATCOIN = new BasisType(293, "쉐어 레벨업 시간 단축에 사용되는 냥다래 (16번 단위로 곱하여 사용)");
        /// <summary>294. 페이스북 링크(이벤트 듀얼 결과 확인)</summary>
        [System.Obsolete]
        public static BasisType FACEBOOK_LINK_EVENT_DUEL_RESULT      = new BasisType(294, "페이스북 링크(이벤트 듀얼 결과 확인)");
        /// <summary>295. 스테이지에 해당하는 쿠폰 데이터</summary>
        public static BasisType COUPON_ID_BY_STAGE                   = new BasisType(295, "스테이지에 해당하는 쿠폰 데이터");
        /// <summary>296. 스피드핵 체크용 데이터(서버)</summary>
        public static BasisType SPEED_HACK_CHECK                     = new BasisType(296, "스피드핵 체크용 데이터(서버)");
        /// <summary>297. 이벤트 난전 오픈 시간</summary>
        public static BasisType EVENT_FREE_FIGHT_OPEN_HOUR           = new BasisType(297, "이벤트 난전 오픈 시간");
        /// <summary>298. 냥다래 선물 보상 리스트</summary>
        public static BasisType CAT_COIN_GIFT_REWARD                 = new BasisType(298, "냥다래 선물 보상 리스트");
        /// <summary>299. 빈 기초데이터 상세 데이터(서버)</summary>
        public static BasisType EMPTY_REF_DATA_3                     = new BasisType(299, "빈 기초데이터 상세 데이터(서버)");
        /// <summary>300. 하드, 챌린지 모드 쿨타임(밀리초)</summary>
        public static BasisType STAGE_HARD_CHELLENGE_COOL_TIME       = new BasisType(300, "하드, 챌린지 모드 쿨타임(밀리초)");
        /// <summary>301. 쉐도우 장비 최대 강화레벨(등급별)</summary>
        public static BasisType SHADOW_MAX_LEVEL                     = new BasisType(301, "쉐도우 장비 최대 강화레벨(등급별)");
        /// <summary>302. 엔들리스 타워 어둠의 재 사용 수 별 시작 층</summary>
        public static BasisType ENDLESS_TOWER_SKIP_INFO              = new BasisType(302, "엔들리스 타워 어둠의 재 사용 수 별 시작 층");
        /// <summary>303. 냥다래 나무 Ro포인트 보상 수량(이벤트)</summary>
        public static BasisType CAT_COIN_TREE_RO_POINT_COUNT         = new BasisType(303, "냥다래 나무 Ro포인트 보상 수량(이벤트)");
        /// <summary>304. 신규 복귀 레벨 달성 이벤트</summary>
        public static BasisType COMEBACK_REWARD_EVENT_ITEM           = new BasisType(304, "신규 복귀 레벨 달성 이벤트");
        /// <summary>305. 룬 마스터리로 선택할 수 있는 스킬 4종류(skill id)</summary>
        public static BasisType RUNE_MASTERY_SKILL_IDS               = new BasisType(305, "룬 마스터리로 선택할 수 있는 스킬 4종류(skill id)");
        /// <summary>306. 타임슈트 지급 퀘스트 (daily_group 기준)</summary>
        public static BasisType TIME_SUIT_QUEST_ID                   = new BasisType(306, "타임슈트 지급 퀘스트 (daily_group 기준)");
        /// <summary>307. 타임 패트롤 일반 몬스터 보상</summary>
        public static BasisType TIME_PATROL_NORMAL_MONSTER_REWARD    = new BasisType(307, "타임 패트롤 일반 몬스터 보상");
        /// <summary>308. 미궁숲 정보</summary>
        public static BasisType REF_FOREST_MAZE_INFO                 = new BasisType(308, "미궁숲 정보 참조");
        /// <summary>309. 쉐어포스 받는 대미지 계산</summary>
        public static BasisType SHARE_FORCE_ATTACKED_DAMAGE          = new BasisType(309, "쉐어포스 받는 대미지 계산");
        /// <summary>310. 쉐어포스 주는 대미지 계산</summary>
        public static BasisType SHARE_FORCE_ATTACK_DAMAGE            = new BasisType(310, "쉐어포스 주는 대미지 계산");
        /// <summary>311. 타임 슈트 강화 수치에 따른 쉐어 포스 증가량</summary>
        public static BasisType TIME_SUIT_LEVEL_INC                  = new BasisType(311, "타임 슈트 강화 수치에 따른 쉐어 포스 증가량");
        /// <summary>312. 타임 패트롤 타임 키퍼 보상</summary>
        public static BasisType TIME_PATROL_BOSS_MONSTER_REWARD      = new BasisType(312, "타임 패트롤 타임 키퍼 보상");
        /// <summary>313. url 참조</summary>
        public static BasisType REF_URL                              = new BasisType(313, "url 참조");
        /// <summary>314. 콘텐츠 업데이트 예정 처리(1 = 업데이트 예정 처리, 0 = FALSE)</summary>
        public static BasisType CONTENT_OPEN                         = new BasisType(314, "콘텐츠 업데이트 예정 처리");
        /// <summary>315. 특정 아이템 설명의 URL 참조 데이터</summary>
        public static BasisType REF_ITEM_DESC_URL                    = new BasisType(315, "특정 아이템 설명의 URL 참조 데이터");
        /// <summary>316. 쉐도우 카드 최대 레벨</summary>
        public static BasisType SHADOW_CARD_MAX_LEVEL                = new BasisType(316, "쉐도우 카드 최대 레벨");
        /// <summary>317. 쉐도우 카드 레벨업 제니 기초치</summary>
        public static BasisType SHADOW_CARD_LEVEL_UP_ZENY            = new BasisType(317, "쉐도우 카드 레벨업 제니 기초치");
        /// <summary>318. 엔들리스 타워 플레이 타임(밀리초)</summary>
        public static BasisType ENDLESS_TOWER_LIMIT_TIME             = new BasisType(318, "엔들리스 타워 플레이 타임(밀리초)");
        /// <summary>319. 엔들리스 타워 입장 가능 최대 층</summary>
        public static BasisType ENDLESS_TOWER_MAX_FLOOR              = new BasisType(319, "엔들리스 타워 입장 가능 최대 층");
        /// <summary>320. 엔들리스 타워 무료 입장 횟수(주간 초기화)</summary>
        public static BasisType ENDLESS_TOWER_FREE_JOIN_COUNT        = new BasisType(320, "엔들리스 타워 무료 입장 횟수(주간 초기화)");
        /// <summary>321. 엔들리스 타워 오픈 직업 레벨</summary>
        public static BasisType ENDLESS_TOWER_LIMIT_JOB_LEVEL        = new BasisType(321, "엔들리스 타워 오픈 직업 레벨");
        /// <summary>322. 타임 패트롤 스테이지 최대 레벨</summary>
        public static BasisType TP_MAX_LEVEL                         = new BasisType(322, "타임 패트롤 스테이지 최대 레벨");
        /// <summary>323. 타임 패트롤 입장 조건 스테이지(도달 스테이지)</summary>
        public static BasisType TP_OPEN_STAGE_ID                     = new BasisType(323, "타임 패트롤 입장 조건 스테이지");
        /// <summary>324. 쉐어포스 최대 강화 레벨</summary>
        public static BasisType SHARE_FORCE_MAX_LEVEL                = new BasisType(324, "쉐어포스 최대 강화 레벨");
        /// <summary>325. 돌림판에 사용되는 이벤트 주화 획득 가능 최대치</summary>
        public static BasisType EVENT_COIN_MAX_COUNT                 = new BasisType(325, "돌림판에 사용되는 이벤트 주화 획득 가능 최대치");
        /// <summary>326. 서버 필터(0이면 모든 서버 보임, 1이면 접속한 국가에 해당하는 서버만 보임)</summary>
        public static BasisType SERVER_FILTER_FLAG                   = new BasisType(326, "서버 필터(0이면 모든 서버 보임, 1이면 접속한 국가에 해당하는 서버만 보임)");
        /// <summary>327. 쉐어 슬롯 1개 추가되는 시나리오 미로 인덱스</summary>
        public static BasisType HOPE_SLOT_OPEN_MAGE                  = new BasisType(327, "쉐어 슬롯 1개 추가되는 시나리오 미로 인덱스");
        /// <summary>328. 내 캐릭터 클론 쉐어 열리는 직업 차수</summary>
        public static BasisType CLONE_SHARE_JOB_GRADE                = new BasisType(328, "내 캐릭터 클론 쉐어 열리는 직업 차수");
        /// <summary>329. 스페셜 룰렛 스킨 인덱스</summary>
        public static BasisType SPECIAL_ROULETTE_SKIN                = new BasisType(329, "스페셜 룰렛 스킨 인덱스");
        /// <summary>330. 길드명 변경에 필요한 냥다래 비용</summary>
        public static BasisType GUILD_NAME_CHANGE_CATCOIN            = new BasisType(330, "길드명 변경에 필요한 냥다래 비용");
        /// <summary>331. 한 번에 연계되는 스킬의 제한 수</summary>
        public static BasisType SKILL_CHAIN_MAX_COUNT                = new BasisType(331, "한 번에 연계되는 스킬의 제한 수");
        /// <summary>332. 쉐어스텟강화 초기화 비용</summary>
        public static BasisType REQUEST_SHARE_STAT_RESET_CATCOIN     = new BasisType(332, "쉐어스텟강화 초기화 비용");
        /// <summary>333. 길드 쉐어 사용 기준 길드 기여도 값</summary>
        public static BasisType GUILD_SHARE_NEED_DONATION            = new BasisType(333, "길드 쉐어 사용 기준 길드 기여도 값");
        /// <summary>334. 라비린스 패스 유료 레벨업 비용(냥다래)</summary>
        public static BasisType PASS_LEVEL_UP_CAT_COIN               = new BasisType(334, "라비린스 패스 유료 레벨업 비용(냥다래)");
        /// <summary>335. 빈 기초데이터 상세 데이터(서버)</summary>
        public static BasisType EMPTY_REF_DATA_39                    = new BasisType(335, "빈 기초데이터 상세 데이터(서버)");
        /// <summary>336. 챕터 별 보스 처치 실패 시 보스 도전 쿨타임(밀리초)</summary>
        public static BasisType CHAPTER_BOSS_FAILURE_COOLTIME        = new BasisType(336, "챕터 별 보스 처치 실패 시 보스 도전 쿨타임(밀리초)");
        /// <summary>337. 챕터 별 보스 처치 성공 후 보스 도전 쿨타임(밀리초)</summary>
        public static BasisType CHAPTER_BOSS_CLEAR_COOLTIME          = new BasisType(337, "챕터 별 보스 처치 성공 후 보스 도전 쿨타임(밀리초)");
        /// <summary>338. 길드전 정보 참조</summary>
        public static BasisType REF_GUILD_WAR_INFO                   = new BasisType(338, "길드전 정보 참조");
        /// <summary>339. 큐펫 정보 참조</summary>
        public static BasisType REF_CUPET_INFO                       = new BasisType(339, "큐펫 정보 참조");
        /// <summary>340. 13chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER13         = new BasisType(340, "13chapter 듀얼 조각 name_id");
        /// <summary>341. 14chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER14         = new BasisType(341, "14chapter 듀얼 조각 name_id");
        /// <summary>342. 15chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER15         = new BasisType(342, "15chapter 듀얼 조각 name_id");
        /// <summary>343. 16chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER16         = new BasisType(343, "16chapter 듀얼 조각 name_id");
        /// <summary>344. 17chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER17         = new BasisType(344, "17chapter 듀얼 조각 name_id");
        /// <summary>345. 18chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER18         = new BasisType(345, "18chapter 듀얼 조각 name_id");
        /// <summary>346. 19chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER19         = new BasisType(346, "19chapter 듀얼 조각 name_id");
        /// <summary>347. 20chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER20         = new BasisType(347, "20chapter 듀얼 조각 name_id");
        /// <summary>348. 21chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER21         = new BasisType(348, "21chapter 듀얼 조각 name_id");
        /// <summary>349. 22chapter 듀얼 조각 name_id</summary>
        public static BasisType DUEL_PIECE_NAME_ID_CHAPTER22         = new BasisType(349, "22chapter 듀얼 조각 name_id");
        /// <summary>350. 이벤트미궁:암흑 정보 참조</summary>
        public static BasisType REF_DARK_MAZE_INFO                   = new BasisType(350, "이벤트미궁:암흑 정보 참조");
        /// <summary>351. 듀얼 아레나 정보</summary>
        public static BasisType REF_DUEL_ARENA_INFO                  = new BasisType(351, "듀얼 아레나 정보");
        /// <summary>352. 나비호 정보</summary>
        public static BasisType REF_NABIHO_INFO                      = new BasisType(352, "나비호 정보");
        /// <summary>353. 서버 그룹별 직업레벨 최대값</summary>
        public static BasisType MAX_JOB_LEVEL_STAGE_BY_SERVER        = new BasisType(353, "서버 그룹별 직업레벨 최대값");
        /// <summary>354. 서버 별 나비호 콘텐츠 활성화 (1 = 콘텐츠 비활성화, 0 = 콘텐츠 활성화)</summary>
        public static BasisType NABIHO_OPEN_BY_SERVER                = new BasisType(354, "서버 별 나비호 콘텐츠 활성화 (1 = 콘텐츠 비활성화, 0 = 콘텐츠 활성화)");
        /// <summary>355. 라비린스 단어 찾기 이벤트 아이템 ID 목록</summary>
        public static BasisType WORD_COLLECTION_ITEMS                = new BasisType(355, "라비린스 단어 찾기 이벤트 아이템 ID 목록");
        /// <summary>356. </summary>
        public static BasisType REF_ONBUFF_INFO                      = new BasisType(356, "온버프 정보");
        /// <summary>357. 프로젝트 타입별 언어 키값</summary>
        public static BasisType PROJECT_TYPE_LOCALIZE_KEY            = new BasisType(357, "프로젝트 타입별 언어 키값");
        /// <summary>358. </summary>
        public static BasisType EMPTY_REF_DATA_043                   = new BasisType(358, "");
        /// <summary>359. </summary>
        public static BasisType EMPTY_REF_DATA_044                   = new BasisType(359, "");
        /// <summary>360. </summary>
        public static BasisType EMPTY_REF_DATA_045                   = new BasisType(360, "");
        /// <summary>361. </summary>
        public static BasisType EMPTY_REF_DATA_046                   = new BasisType(361, "");
        /// <summary>362. </summary>
        public static BasisType EMPTY_REF_DATA_047                   = new BasisType(362, "");
        /// <summary>363. </summary>
        public static BasisType EMPTY_REF_DATA_048                   = new BasisType(363, "");
        /// <summary>364. </summary>
        public static BasisType EMPTY_REF_DATA_049                   = new BasisType(364, "");
        /// <summary>365. </summary>
        public static BasisType EMPTY_REF_DATA_050                   = new BasisType(365, "");
        /// <summary>366. 나비호 광고 쿨타임</summary>
        public static BasisType NABIHO_AD_COOL_TIME                  = new BasisType(366, "나비호 광고 쿨타임");
        /// <summary>367. 마나 재생 딜레이</summary>
        public static BasisType REGEN_MP_DELAY                       = new BasisType(367, "마나 재생 딜레이");
        /// <summary>368. 라비린스 단어 수집 재료 필요 수</summary>
        public static BasisType WORD_COLLECTION_MATERIAL_NEED_COUNT  = new BasisType(368, "라비린스 단어 수집 재료 필요 수");
        /// <summary>369. 일일퀘스트 이벤트 냥다래 소모 스킵권</summary>
        public static BasisType DAILY_QUEST_EVENT_SKIP_CAT_COIN      = new BasisType(369, "일일퀘스트 이벤트 냥다래 소모 스킵권");
        /// <summary>370. 냥다래 나무 이벤트 리워드 타입</summary>
        public static BasisType CAT_COIN_TREE_EVENT_REWARD_TYPE      = new BasisType(370, "냥다래 나무 이벤트 리워드 타입");
        /// <summary>371. 냥다래 나무 하단 텍스트 스크립트</summary>
        public static BasisType CAT_COIN_TREE_BOTTOM_TEXT_SCRIPT     = new BasisType(371, "냥다래 나무 하단 텍스트 스크립트");
        /// <summary>372. </summary>
        public static BasisType EMPTY_007                            = new BasisType(372, "");
        /// <summary>373. </summary>
        public static BasisType CONSUMABLE_ITEM_USE_LIMIT            = new BasisType(373, "소비 아이템 1회당 최대 사용 횟수");
        /// <summary>374. </summary>
        public static BasisType EMPTY_009                            = new BasisType(374, "");
        /// <summary>375. </summary>
        public static BasisType EMPTY_010                            = new BasisType(375, "");
        /// <summary>376. </summary>
        public static BasisType EMPTY_011                            = new BasisType(376, "");
        /// <summary>377. </summary>
        public static BasisType EMPTY_012                            = new BasisType(377, "");
        /// <summary>378. </summary>
        public static BasisType EMPTY_013                            = new BasisType(378, "");
        /// <summary>379. </summary>
        public static BasisType EMPTY_014                            = new BasisType(379, "");
        /// <summary>380. </summary>
        public static BasisType EMPTY_015                            = new BasisType(380, "");
        /// <summary>381. </summary>
        public static BasisType EMPTY_016                            = new BasisType(381, "");
        /// <summary>382. </summary>
        public static BasisType EMPTY_017                            = new BasisType(382, "");
        /// <summary>383. </summary>
        public static BasisType EMPTY_018                            = new BasisType(383, "");
        /// <summary>384. </summary>
        public static BasisType EMPTY_019                            = new BasisType(384, "");
        /// <summary>385. </summary>
        public static BasisType EMPTY_020                            = new BasisType(385, "");
    }
}