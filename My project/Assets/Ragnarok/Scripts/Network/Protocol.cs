using Sfs2X.Entities.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class Protocol : EnumBaseType<Protocol, string, int>
    {
        public delegate void ResponseEvent(Response response); // 프로토콜 응답 이벤트

        public enum ProtocolType
        {
            /// <summary>
            /// 기본적인 요청 응답
            /// </summary>
            Request,
            /// <summary>
            /// 응답없이 보내기만 
            /// </summary>
            SendOnly,
            /// <summary>
            /// 요청없이 응답만
            /// </summary>
            ResponseOnly,
        }

        public readonly List<ResponseEvent> fixedEventList; // 고정 이벤트 (AddEvent 및 RemoveEvent 로 제어)
        public readonly ProtocolType protocolType;

        public readonly bool isIndicator; // true일 경우 서버로 패킷을 보낼 때 (중복 보내기를 방지하기 위한) 인디케이터 창이 뜬다
        private readonly bool isLog;

        /// <summary>
        /// 그룹 아이디가 같은 Protocol들 중
        /// 서버로 보냈지만 응답이 오지 않은 패킷이 존재하는지 체크한 후
        /// 해당 패킷이 서버로부터 응답이 온 후에 다음 패킷을 보낸다.
        /// </summary>
        public readonly int? sendingQueueGroupId;

        /// <summary>
        /// 프로토콜 정의
        /// </summary>
        /// <param name="cmd">cmd</param>
        /// <param name="index">언어 index</param>
        /// <param name="protocolType">프로토콜 타입</param>
        /// <param name="isIndicator">중복 패킷 보내기 방지를 위한 인디케이터 창 Active 여부</param>
        /// <param name="isLog">로그 온오프</param>
        /// <param name="isExecuteDefaultResult">ResultCode에 해당하는 기본 행동 처리 유무</param>
        /// <param name="sendingQueueGroupId">Queue에 담을 그룹 아이디</param>
        protected Protocol(string cmd, int index, ProtocolType protocolType, bool isIndicator = true, bool isLog = true, int? sendingQueueGroupId = null) : base(cmd, index)
        {
            fixedEventList = new List<ResponseEvent>();

            this.protocolType = protocolType;
            this.isIndicator = protocolType == ProtocolType.ResponseOnly ? false : isIndicator;
            this.isLog = isLog;
            this.sendingQueueGroupId = sendingQueueGroupId;
        }

        public static Protocol GetByKey(string key)
        {
            return GetBaseByKey(key);
        }

        public static ISFSObject NewInstance()
        {
            return SFSObject.NewInstance();
        }

        public static ISFSArray NewArrayInstance()
        {
            return SFSArray.NewInstance();
        }

        /// <summary>
        /// 프로토콜 보내기
        /// </summary>
        public async Task<Response> SendAsync()
        {
            return await SendAsync(NewInstance());
        }

        /// <summary>
        /// 프로토콜 보내기
        /// </summary>
        public virtual async Task<Response> SendAsync(ISFSObject param)
        {
            return await ConnectionManager.Instance.AsyncSend(this, param);
        }

        /// <summary>
        /// 이벤트 등록
        /// </summary>
        public void AddEvent(ResponseEvent fixedResponse)
        {
            for (int i = 0, imax = fixedEventList.Count; i < imax; i++)
            {
                ResponseEvent current = fixedEventList[i];

                // 이미 등록된 이벤트
                if (current != null && current.Equals(fixedResponse))
                    return;
            }

            fixedEventList.Add(fixedResponse); // Add
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        public void RemoveEvent(ResponseEvent fixedResponse)
        {
            for (int i = 0, imax = fixedEventList.Count; i < imax; i++)
            {
                ResponseEvent current = fixedEventList[i];

                // 이미 등록된 이벤트
                if (current != null && current.Equals(fixedResponse))
                {
                    fixedEventList.RemoveAt(i); // Remove
                    return;
                }
            }
        }

        public bool IsShowLog()
        {
            // 디버그 빌드 상태가 아닐 경우 로그를 보지 않습니다.
            if (!Debug.isDebugBuild)
                return false;

            if (DebugUtils.IsLogAutoStageDrop && this == REQUEST_AUTO_STAGE_MON_DROP)
                return true;

            if (DebugUtils.IsLogUseSkill && this == SKILL_COOLTIME_CHECK)
                return true;

            return isLog;
        }

#if UNITY_EDITOR
        private static Dictionary<string, string> fieldDic; // 로그 보여주기 위한 작업
        public override string ToString()
        {
            if (fieldDic == null)
            {
                fieldDic = new Dictionary<string, string>(System.StringComparer.Ordinal);

                foreach (var field in GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    Protocol property = field.GetValue(null) as Protocol;
                    fieldDic.Add(property.Key, field.Name);
                }
            }

            return $"{Key}:{fieldDic[Key]}";
        }
#endif

        /// <summary>1. 게임서버 정보(인증서버 로그인 성공시 자동으로 들어옴)</summary>
        public static readonly Protocol GAME_SERVER_INFO                            = new Protocol("1", 0, ProtocolType.ResponseOnly);
        /// <summary>2. 리소스 버전 정보</summary>
        public static readonly Protocol RESOURCE_VERSION                            = new Protocol("2", 0, ProtocolType.Request);
        /// <summary>3. 유저 정보</summary>
        public static readonly Protocol USER_INFO                                   = new Protocol("3", 0, ProtocolType.Request);
        /// <summary>4. 캐릭터 리스트</summary>
        public static readonly Protocol CHARACTER_LIST                              = new Protocol("4", 0, ProtocolType.Request);
        /// <summary>5. 캐릭터 생성</summary>
        public static readonly Protocol CREATE_CHARACTER                            = new Protocol("5", 0, ProtocolType.Request);
        /// <summary>6. 캐릭터 슬롯 구매</summary>
        public static readonly Protocol BUY_CHARACTER_SLOT                          = new Protocol("6", 0, ProtocolType.Request);
        /// <summary>7. 캐릭터 삭제대기 요청</summary>
        public static readonly Protocol DELETE_CHARACTER_PRE                        = new Protocol("7", 0, ProtocolType.Request);
        /// <summary>8. 캐릭터 삭제완료 요청</summary>
        public static readonly Protocol DELETE_CHARACTER_COMPLETE                   = new Protocol("8", 0, ProtocolType.Request);
        /// <summary>9. 캐릭터 삭제취소 요청</summary>
        public static readonly Protocol DELETE_CHARACTER_CANCEL                     = new Protocol("9", 0, ProtocolType.Request);
        /// <summary>10. 게임에 입장</summary>
        public static readonly Protocol JOIN_GAME_MAP                               = new Protocol("10", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>11. 핑퐁</summary>
        public static readonly Protocol REQUEST_PING                                = new Protocol("11", 0, ProtocolType.SendOnly, isIndicator: false, isLog: true);

        /// <summary>12. 가방(인벤) 슬롯 구매</summary>
        public static readonly Protocol INVEN_EXPAND                                = new Protocol("12", 0, ProtocolType.Request);
        /// <summary>13. 스킬 슬롯 구매</summary>
        public static readonly Protocol BUY_SKILL_SLOT                              = new Protocol("13", 0, ProtocolType.Request);
        /// <summary>14. 캐릭터 전직</summary>
        public static readonly Protocol CHAR_CHANGE_JOB                             = new Protocol("14", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>15. 캐릭터 전승</summary>
        public static readonly Protocol CHAR_REBIRTH                                = new Protocol("15", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>16. 스탯포인트 초기화</summary>
        public static readonly Protocol CHAR_STAT_POINT_INIT                        = new Protocol("16", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>17. 스탯포인트 업데이트</summary>
        public static readonly Protocol CHAR_STAT_POINT_UPDATE                      = new Protocol("17", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>18. 스킬 레벨업</summary>
        public static readonly Protocol SKILL_LEVEL_UP                              = new Protocol("18", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>19. 스킬 초기화</summary>
        public static readonly Protocol SKILL_INIT                                  = new Protocol("19", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>20. 도감 버프 정보</summary>
        public static readonly Protocol GET_DICTIONARY_BUF                          = new Protocol("20", 0, ProtocolType.Request);
        /// <summary>21. 스킬 슬롯에 스킬 등록,수정</summary>
        public static readonly Protocol UPDATE_SKILL_SLOT                           = new Protocol("21", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>22. 우편 리스트</summary>
        public static readonly Protocol MAIL_LIST                                   = new Protocol("22", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>23. 우편 받기</summary>
        public static readonly Protocol MAIL_GET                                    = new Protocol("23", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>24. 우편 전체 받기</summary>
        public static readonly Protocol MAIL_GET_ALL                                = new Protocol("24", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>25. 장비 장착</summary>
        public static readonly Protocol ITEM_EQUIP                                  = new Protocol("25", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>26. 소모성 아이템 사용</summary>
        public static readonly Protocol USE_CONSUMABLE_ITEM                         = new Protocol("26", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>30. 장비에 카드 인챈트</summary>
        public static readonly Protocol ENCHANT_PARTS_ITEM                          = new Protocol("30", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>32. 몬스터 조각으로 큐펫 소환</summary>
        public static readonly Protocol SUMMON_CUPET                                = new Protocol("32", 0, ProtocolType.Request);
        /// <summary>33. 몬스터 조각으로 큐펫 진화</summary>
        public static readonly Protocol CUPET_EVOLUTION                             = new Protocol("33", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>34. 카드 제련</summary>
        public static readonly Protocol SMELT_CARD                                  = new Protocol("34", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>37. 장비 제련</summary>
        [System.Obsolete]
        public static readonly Protocol PARTS_ITEM_LEVEL_UP                         = new Protocol("37", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>38. 큐펫 장착</summary>
        public static readonly Protocol CUPET_EQUIP                                 = new Protocol("38", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>39. 스킬 훔쳐 배우기</summary>
        public static readonly Protocol SKILL_ID_CHANGE                             = new Protocol("39", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>41. 슬롯 에 등록된 스킬중 사용 스킬 설정/해제</summary>
        public static readonly Protocol REGIST_USE_SKILL                            = new Protocol("41", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>42. 스킬 쿨타임 체크</summary>
        public static readonly Protocol SKILL_COOLTIME_CHECK                        = new Protocol("42", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        /// <summary>43. 장비 제작</summary>
        public static readonly Protocol MAKE_ITEM                                   = new Protocol("43", 0, ProtocolType.Request);
        /// <summary>44. [미로] 보스 스테이지 시작</summary>
        public static readonly Protocol BOSS_STAGE_START                            = new Protocol("44", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>45. [미로] 보스 스테이지 클리어</summary>
        public static readonly Protocol BOSS_STAGE_CLEAR                            = new Protocol("45", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>47. 퀘스트 보상 수령</summary>
        public static readonly Protocol QUEST_REWARD                                = new Protocol("47", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>49. 캐릭터 이름 변경</summary>
        public static readonly Protocol REQUEST_CHANGE_NAME                         = new Protocol("49", 0, ProtocolType.Request);
        /// <summary>50. 의뢰 퀘스트 수령</summary>
        public static readonly Protocol GET_NORMAL_QUEST                            = new Protocol("50", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>51. 의뢰 퀘스트 포기</summary>
        public static readonly Protocol DROP_NORMAL_QUEST                           = new Protocol("51", 0, ProtocolType.Request);
        /// <summary>52. 게임 알람</summary>
        public static readonly Protocol GAME_ALARM                                  = new Protocol("52", 0, ProtocolType.ResponseOnly);
        /// <summary>53. 장비 분해</summary>
        public static readonly Protocol ITEM_DISASSEMBLE                            = new Protocol("53", 0, ProtocolType.Request);
        /// <summary>54. 비밀상점 목록 초기화</summary>
        public static readonly Protocol SECRET_SHOP_INIT_LIST                       = new Protocol("54", 0, ProtocolType.Request);
        /// <summary>55. 상점 구매</summary>
        public static readonly Protocol SHOP_PURCHASE                               = new Protocol("55", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>56. 결제 페이로드</summary>
        public static readonly Protocol REQUEST_GET_PAYLOAD                         = new Protocol("56", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>57. 필드 던전 소탕</summary>
        public static readonly Protocol DUNGEON_FAST_CLEAR                          = new Protocol("57", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>58. 접속 보상 정보</summary>
        public static readonly Protocol REQUEST_GET_CONNECT_INFO                    = new Protocol("58", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>59. 접속 보상</summary>
        public static readonly Protocol REQUEST_GET_CONNECT_REWARD                  = new Protocol("59", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>60. 핫타임 사용</summary>
        [System.Obsolete]
        public static readonly Protocol REQUEST_USE_HOT_TIME                        = new Protocol("60", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>61. 튜토리얼 진행상황 기록</summary>
		public static readonly Protocol TUTORIAL_STEP                               = new Protocol("61", 0, ProtocolType.Request, isIndicator: false, isLog: true);
        /// <summary>62. 채팅 채널 변경</summary>
        public static readonly Protocol REQUEST_JOIN_CHANNEL_CHAT                   = new Protocol("62", 0, ProtocolType.Request/*, isIndicator: false*/);
        /// <summary>63. 채팅 메시지</summary>
        public static readonly Protocol REQUEST_CHANNEL_CHAT_MSG                    = new Protocol("63", 0, ProtocolType.SendOnly, isIndicator: false, isLog: true);
        /// <summary>64. 온오프라인 접속시간 보상</summary>
        public static readonly Protocol REQUEST_GET_TREE_REWARD                     = new Protocol("64", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>65. 미로맵 아이템 획득</summary>
        [System.Obsolete]
        public static readonly Protocol REQUEST_MAZE_MAP_STAGR_REWARD               = new Protocol("65", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>66. 코스튬 장착/해제</summary>
		public static readonly Protocol REQUEST_COSTUME_EQUIP                       = new Protocol("66", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>67. 랭킹 정보 목록 요청</summary>
		public static readonly Protocol REQUEST_RANK_LIST                           = new Protocol("67", 0, ProtocolType.Request);
        /// <summary>68. 계정 연동</summary>
		public static readonly Protocol REQUEST_PLATFORM_LINK_ACCOUNT               = new Protocol("68", 0, ProtocolType.Request);

        /// <summary>73. 캐릭터 변경 전 호출</summary>
        public static readonly Protocol REQUEST_GOTO_CHAR_LIST                      = new Protocol("73", 0, ProtocolType.Request);
        /// <summary>74. 친구가 속한 마을로 이동</summary>
		public static readonly Protocol REQUEST_TOWN_ROOM_FOLLOW_USER               = new Protocol("74", 0, ProtocolType.Request);
        /// <summary>75. 패널 옵션 변경</summary>
		public static readonly Protocol REQUEST_PANEL_OPTION_CHANGE                 = new Protocol("75", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>76. 패널 구입</summary>
		public static readonly Protocol REQUEST_PANEL_BUY                           = new Protocol("76", 0, ProtocolType.Request);
        /// <summary>77. 패널 장착</summary>
		public static readonly Protocol REQUEST_PANEL_CUPET_EQUIP                   = new Protocol("77", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>78. 선택 패널 적용</summary>
        public static readonly Protocol REQUEST_PANEL_USE                           = new Protocol("78", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>79. 이벤트 퀘스트 목록 요청</summary>
        public static readonly Protocol REQUEST_EVENT_QUEST                         = new Protocol("79", 0, ProtocolType.Request);
        /// <summary>80. 길드 생성</summary>
        public static readonly Protocol GUILD_CREATE                                = new Protocol("80", 0, ProtocolType.Request);
        /// <summary>81. 길드 정보</summary>        
        public static readonly Protocol GUILD_INFO                                  = new Protocol("81", 0, ProtocolType.Request);
        /// <summary>82. 가입한 길드 정보</summary>
        public static readonly Protocol GUILD_MY                                    = new Protocol("82", 0, ProtocolType.Request);
        /// <summary>83. 길드 가입 신청</summary>
        public static readonly Protocol REQUEST_GUILD_JOIN                          = new Protocol("83", 0, ProtocolType.Request);
        /// <summary>84. 내가 길드 가입 신청한 목록</summary>
        public static readonly Protocol REQUEST_GUILD_JOIN_LIST                     = new Protocol("84", 0, ProtocolType.Request);
        /// <summary>85. 길드 가입 신청 취소</summary>
        public static readonly Protocol REQUEST_CANCEL_GUILD_JOIN                   = new Protocol("85", 0, ProtocolType.Request);
        /// <summary>86. 길드 가입 신청한 유저 목록</summary>
        public static readonly Protocol REQUEST_GUILD_LIST                          = new Protocol("86", 0, ProtocolType.Request);
        /// <summary>87. 길드 가입 신청 처리</summary>
        public static readonly Protocol REQUEST_GUILD_PROC                          = new Protocol("87", 0, ProtocolType.Request);
        /// <summary>88. 추천 길드 목록</summary>
        public static readonly Protocol REQUEST_GUILD_RANDOM                        = new Protocol("88", 0, ProtocolType.Request);
        /// <summary>89. 길드 검색</summary>
        public static readonly Protocol REQUEST_GUILD_SEARCH                        = new Protocol("89", 0, ProtocolType.Request);
        /// <summary>90. 길드원 목록</summary>
        public static readonly Protocol REQUEST_GUILD_MEMBER                        = new Protocol("90", 0, ProtocolType.Request);
        /// <summary>91. 길드원 추방</summary>
        public static readonly Protocol REQUEST_GUILD_MEMBER_KICK                   = new Protocol("91", 0, ProtocolType.Request);
        /// <summary>92. 길드 탈퇴</summary>
        public static readonly Protocol REQUEST_GUILD_OUT                           = new Protocol("92", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>93. 길드 채팅</summary>
        public static readonly Protocol REQUEST_GUILD_ROOM_CHAT                     = new Protocol("93", 0, ProtocolType.SendOnly);
        /// <summary>94. 길드 상태값 변동관련</summary>
        public static readonly Protocol RESPONSE_GUILD_MESSAGE                      = new Protocol("94", 0, ProtocolType.ResponseOnly);
        /// <summary>95. 길드 소개글 변경</summary>
        public static readonly Protocol REQUEST_GUILD_CAHNGE_INTRO                  = new Protocol("95", 0, ProtocolType.Request);
        /// <summary>96. 길드 엠블렘 변경</summary>
        public static readonly Protocol REQUEST_GUILD_CHANGE_EMBLEM                 = new Protocol("96", 0, ProtocolType.Request);
        /// <summary>97. 부길마 임명/해임</summary>
        public static readonly Protocol REQUEST_GUILD_GRANT_PART_MASTER             = new Protocol("97", 0, ProtocolType.Request);
        /// <summary>98. 길드 마스터 위임</summary>
        public static readonly Protocol REQUEST_GUILD_MASTER_CHANGE                 = new Protocol("98", 0, ProtocolType.Request);
        /// <summary>99. 길드 마스터 권한 가져오기</summary>
        public static readonly Protocol REQUEST_GUILD_MASTER_GET                    = new Protocol("99", 0, ProtocolType.Request);
        /// <summary>100. 길드 공지 변경</summary>
        public static readonly Protocol REQUEST_GUILD_CHANGE_NOTICE                 = new Protocol("100", 0, ProtocolType.Request);
        /// <summary>101. 길드 게시판 조회</summary>
        public static readonly Protocol REQUEST_GUILD_BOARD_LIST                    = new Protocol("101", 0, ProtocolType.Request);
        /// <summary>102. 길드 게시판 작성</summary>
        public static readonly Protocol REQUEST_GUILD_BOARD_WRITE                   = new Protocol("102", 0, ProtocolType.Request);
        /// <summary>103. 길드 출석 현황</summary>
        public static readonly Protocol REQUEST_GUILD_ATTEND_INFO                   = new Protocol("103", 0, ProtocolType.Request);
        /// <summary>104. 길드 출석</summary>
        public static readonly Protocol REQUEST_GUILD_USER_ATTEND                   = new Protocol("104", 0, ProtocolType.Request);
        /// <summary>105. 길드 출석 보상</summary>
        public static readonly Protocol REQUEST_GUILD_ATTEND_REWARD                 = new Protocol("105", 0, ProtocolType.Request);
        /// <summary>106. 길드 게시판 삭제</summary>
        public static readonly Protocol REQUEST_GUILD_BOARD_DELETE                  = new Protocol("106", 0, ProtocolType.Request);
        /// <summary>107. 길드 스킬 레벨업</summary>
        public static readonly Protocol REQUEST_GUILD_LEVELUP                       = new Protocol("107", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>108. 길드 스킬 경험치 구입</summary>
        public static readonly Protocol REQUEST_GUILD_BUY_EXP                       = new Protocol("108", 0, ProtocolType.Request);
        /// <summary>109. 길드 스킬 목록</summary>
        public static readonly Protocol REQUEST_GUILD_SKILL_LIST                    = new Protocol("109", 0, ProtocolType.Request);
        /// <summary>110. 길드 스킬 레벨 변경</summary>
        public static readonly Protocol REQUEST_GUILD_SKILL_LEVEL_CHANGE            = new Protocol("110", 0, ProtocolType.ResponseOnly);
        /// <summary>111. 디버깅 전투 옵션 목록</summary>
        public static readonly Protocol DEBUG_BATTLE_OPTION_LIST                    = new Protocol("111", 0, ProtocolType.Request);
        /// <summary>112. 아이템 잠금</summary>
        public static readonly Protocol CHAR_ITEM_LOCK                              = new Protocol("112", 0, ProtocolType.Request);
        /// <summary>113. 귓속말 메시지 송신</summary>
        public static readonly Protocol REQUEST_CHAT_WISPER_MSG                     = new Protocol("113", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>114. 귓속말 메시지 수신</summary>
        public static readonly Protocol RESULT_CHAT_WISPER_MSG                      = new Protocol("114", 0, ProtocolType.ResponseOnly);
        /// <summary>115. 구매 횟수 제한이 있는 아이템 구매 이력</summary>
        public static readonly Protocol REQUEST_ITEMSHOP_LIMIT_LIST                 = new Protocol("115", 0, ProtocolType.Request);
        /// <summary>116. 길드 관리(즉시가입/신청가입)</summary>
        public static readonly Protocol REQUEST_GUILD_AUTO_JOIN_UPDATE              = new Protocol("116", 0, ProtocolType.Request);

        /// <summary>123. 서버 변경</summary>
        public static readonly Protocol REQUEST_TRADE_PIRVATE_MOVE_SELLER_ROOM      = new Protocol("123", 0, ProtocolType.Request);
        /// <summary>124. 서버 변경 신호</summary>
        public static readonly Protocol REQUEST_TRADE_PIRVATE_SERVER_CHANGE         = new Protocol("124", 0, ProtocolType.ResponseOnly);
        /// <summary>125. 월드보스 목록</summary>
        public static readonly Protocol REQUEST_WORLD_BOSS_LIST                     = new Protocol("125", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>126. 월드보스 방 입장</summary>
        public static readonly Protocol REQUEST_WORLD_BOSS_ROOM_JOIN                = new Protocol("126", 0, ProtocolType.Request);
        /// <summary>127. 월드보스 방 퇴장</summary>
        public static readonly Protocol REQUEST_WORLD_BOSS_EXIT                     = new Protocol("127", 0, ProtocolType.Request);
        /// <summary>128. 월드보스 공격</summary>
        public static readonly Protocol REQUEST_WORLD_BOSS_ATTACK                   = new Protocol("128", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>129. 월드보스 데미지 랭킹정보</summary>
        public static readonly Protocol RECEIVE_WORLD_BOSS_RANK_INFO                = new Protocol("129", 0, ProtocolType.ResponseOnly);
        /// <summary>130. 월드보스가 죽어서 종료 되었다</summary>
        public static readonly Protocol RECEIVE_WORLD_BOSS_CLOSE                    = new Protocol("130", 0, ProtocolType.ResponseOnly);
        /// <summary>131. 디펜스 던전 시작</summary>
        public static readonly Protocol REQUEST_DEF_DUNGEON_START                   = new Protocol("131", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>132. 디펜스 던전 종료</summary>
        public static readonly Protocol REQUEST_DEF_DUNGEON_END                   = new Protocol("132", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>133. 디펜스 던전 몬스터 아이템 드랍</summary>
        public static readonly Protocol REQUEST_DEF_DUNGEON_ITEM_DROP               = new Protocol("133", 0, ProtocolType.Request, isIndicator: false, isLog: false, sendingQueueGroupId: 1);
        /// <summary>134. PVE 시작</summary>
        public static readonly Protocol REQUEST_PVE_START                           = new Protocol("134", 0, ProtocolType.Request);
        /// <summary>135. PVE 종료</summary>
        public static readonly Protocol REQUEST_PVE_END                             = new Protocol("135", 0, ProtocolType.Request);
        /// <summary>137. PVE 입장 정보 확인</summary>
        public static readonly Protocol REQUEST_PVE_INFO                            = new Protocol("137", 0, ProtocolType.Request);

        /// <summary>140. PVE 대미지 체크</summary>
        public static readonly Protocol REQUEST_PVE_DAMAGE_CHECK                    = new Protocol("140", 0, ProtocolType.Request);
        /// <summary>141. 월드보스 알람 체크설정</summary>
        public static readonly Protocol REQUEST_WORLD_BOSS_ALARM                    = new Protocol("141", 0, ProtocolType.Request);
        /// <summary>142. 알람 설정된 월드보스 쿨 정보 전달</summary>
        public static readonly Protocol RECEIVE_WORLD_BOSS_ALARM                    = new Protocol("142", 0, ProtocolType.ResponseOnly);
        /// <summary>143. 월드보스 다른 플레이어 입장</summary>
        public static readonly Protocol RECEIVE_WORLD_BOSS_ROOM_JOIN                = new Protocol("143", 0, ProtocolType.ResponseOnly);
        /// <summary>144. 월드보스 다른 플레이어 퇴장</summary>
        public static readonly Protocol RECEIVE_WORLD_BOSS_ROOM_EXIT                = new Protocol("144", 0, ProtocolType.ResponseOnly);
        /// <summary>145. 스테이지 미로 맵 입장</summary>
        [System.Obsolete]
        public static readonly Protocol REQUEST_STAGE_MAZE_ROOM_JOIN                = new Protocol("145", 0, ProtocolType.Request);
        /// <summary>146. 스테이지 미로 맵 퇴장</summary>
        [System.Obsolete]
        public static readonly Protocol REQUEST_STAGE_MAZE_ROOM_EXIT                = new Protocol("146", 0, ProtocolType.Request);
        /// <summary>147. 스테이지 미로 맵 캐릭터 이동</summary>
        [System.Obsolete]
        public static readonly Protocol REQUEST_STAGE_MAZE_ROOM_TRANSFORM           = new Protocol("147", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        /// <summary>148. 스테이지 미로 맵 다른 유저 입장</summary>
        [System.Obsolete]
        public static readonly Protocol RECEIVE_STAGE_MAZE_ROOM_JOIN                = new Protocol("148", 0, ProtocolType.ResponseOnly);
        /// <summary>149. 스테이지 미로 맵 다른 유저 퇴장</summary>
        [System.Obsolete]
        public static readonly Protocol RECEIVE_STAGE_MAZE_ROOM_EXIT                = new Protocol("149", 0, ProtocolType.ResponseOnly);
        /// <summary>150. 장비 초월 </summary>
        public static readonly Protocol REQUEST_ITEM_TIER_UP                        = new Protocol("150", 0, ProtocolType.Request);

        /// <summary>152. 미로맵 입장 요청</summary>
        [System.Obsolete]
        public static readonly Protocol REQUEST_JOIN_MAP                            = new Protocol("152", 0, ProtocolType.Request);
        /// <summary>153. 스테이지 정보 요청</summary>
        [System.Obsolete]
        public static readonly Protocol REQUEST_STAGE_REJOIN_MAP_EXIT               = new Protocol("153", 0, ProtocolType.Request);

        /// <summary>156. 자동사냥 스테이지 입장 요청</summary>
        public static readonly Protocol REQUEST_AUTO_STAGE_ENTER                    = new Protocol("156", 0, ProtocolType.Request);
        /// <summary>157. 아이템 메시지 채팅</summary>
        public static readonly Protocol REQUEST_CHAT_ITEM_MSG                       = new Protocol("157", 0, ProtocolType.Request);
        /// <summary>158. 시스템 메시지 수신</summary>
        public static readonly Protocol REQUEST_SYSTEM_MSG                          = new Protocol("158", 0, ProtocolType.ResponseOnly);
        /// <summary>160. </summary>
        [System.Obsolete]
        public static readonly Protocol RESPONSE_APPEARANCE_UPDATE                  = new Protocol("160", 0, ProtocolType.ResponseOnly);
        /// <summary>161. </summary>
        public static readonly Protocol REQUEST_EXP_DUNGEON_START                   = new Protocol("161", 0, ProtocolType.Request);
        /// <summary>162. </summary>
        public static readonly Protocol REQUEST_EXP_DUNGEON_ITEM_DROP               = new Protocol("162", 0, ProtocolType.Request);
        /// <summary>163. 미로맵에서 다른유저가 보낸 방해 아이템</summary>
        public static readonly Protocol RESPONSE_MAZE_BOMB                          = new Protocol("163", 0, ProtocolType.ResponseOnly);
        /// <summary>164. 미로맵 클리어 시간 랭킹 요청</summary>
        public static readonly Protocol REQUEST_MAZE_RANK_LIST                      = new Protocol("164", 0, ProtocolType.Request);
        /// <summary>165. 더미 개인상점 아이템 구매 요청</summary>
        public static readonly Protocol REQUEST_TEST_ITEM                           = new Protocol("165", 0, ProtocolType.Request);
        /// <summary>166. [길드 미로] 입장</summary>
        public static readonly Protocol REQUEST_GVG_ROOM_JOIN                       = new Protocol("166", 0, ProtocolType.Request);
        /// <summary>167. [길드 미로] 퇴장</summary>
        public static readonly Protocol REQUEST_GVG_ROOM_EXIT                       = new Protocol("167", 0, ProtocolType.Request);
        /// <summary>168. [길드 미로] 캐릭터 이동</summary>
        public static readonly Protocol REQUEST_GVG_ROOM_TRANSFORM                  = new Protocol("168", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>169. [길드 미로] 다른 유저 캐릭터 입장</summary>
        public static readonly Protocol RECEIVE_GVG_ROOM_JOIN                       = new Protocol("169", 0, ProtocolType.ResponseOnly);
        /// <summary>170. [길드 미로] 다른 유저 캐릭터 퇴장</summary>
        public static readonly Protocol RECEIVE_GVG_ROOM_EXIT                       = new Protocol("170", 0, ProtocolType.ResponseOnly);
        /// <summary>171. 능력치 자동 분배 옵션</summary>
        public static readonly Protocol REQUEST_AUTO_STAT                           = new Protocol("171", 0, ProtocolType.Request);

        /// <summary>173. [테이밍 미로] 입장</summary>
        public static readonly Protocol REQUEST_TAMING_MAZE_ROOM_JOIN               = new Protocol("173", 0, ProtocolType.Request);
        /// <summary>174. [테이밍 미로] 퇴장</summary>
        public static readonly Protocol REQUEST_TAMING_MAZE_ROOM_EXIT               = new Protocol("174", 0, ProtocolType.Request);
        /// <summary>175. [테이밍 미로] 캐릭터 이동</summary>
        public static readonly Protocol REQUEST_TAMING_MAZE_ROOM_TRANSFORM          = new Protocol("175", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>176. [테이밍 미로] 다른 유저 캐릭터 입장</summary>
        public static readonly Protocol RECEIVE_TAMING_MAZE_ROOM_JOIN               = new Protocol("176", 0, ProtocolType.ResponseOnly);
        /// <summary>177. [테이밍 미로] 다른 유저 캐릭터 퇴장</summary>
        public static readonly Protocol RECEIVE_TAMING_MAZE_ROOM_EXIT               = new Protocol("177", 0, ProtocolType.ResponseOnly);
        /// <summary>183. [테이밍 미로] 준비</summary>
        public static readonly Protocol REQUEST_TAMING_MONSTER_READY                = new Protocol("183", 0, ProtocolType.Request);
        /// <summary>184. [테이밍 미로] 시작</summary>
        public static readonly Protocol REQUEST_TAMING_MONSTER                      = new Protocol("184", 0, ProtocolType.Request);
        /// <summary>185. [테이밍 미로] 몬스터 상태 업데이트</summary>
        public static readonly Protocol RECEIVE_TAMING_MONSTER_UPDATE               = new Protocol("185", 0, ProtocolType.ResponseOnly);
        [System.Obsolete] /// <summary>186. [길드 로비] 입장</summary>
        public static readonly Protocol REQUEST_GUILD_LOBBY_JOIN                    = new Protocol("186", 0, ProtocolType.Request);
        [System.Obsolete] /// <summary>187. [길드 로비] 퇴장</summary>
        public static readonly Protocol REQUEST_GUILD_LOBBY_EXIT                    = new Protocol("187", 0, ProtocolType.Request);
        [System.Obsolete] /// <summary>188. [길드 로비] 캐릭터 이동</summary>
        public static readonly Protocol REQUEST_GUILD_LOBBY_TRANSFORM               = new Protocol("188", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        [System.Obsolete] /// <summary>189. [길드 로비] 유저 입장</summary>
        public static readonly Protocol RECEIVE_GUILD_LOBBY_JOIN                    = new Protocol("189", 0, ProtocolType.ResponseOnly);
        [System.Obsolete] /// <summary>190. [길드 로비] 유저 퇴장</summary>
        public static readonly Protocol RECEIVE_GUILD_LOBBY_EXIT                    = new Protocol("190", 0, ProtocolType.ResponseOnly);
        [System.Obsolete] /// <summary>191. [길드 로비] 서버 변경</summary>
        public static readonly Protocol RECEIVE_GUILD_LOBBY_SERVER_CHANGE           = new Protocol("191", 0, ProtocolType.ResponseOnly);
        [System.Obsolete] /// <summary>192. [테이밍] 몬스터 이동(방장 보냄)</summary>
        public static readonly Protocol REQUEST_TAMING_MAZE_MONSTER_TRANSFORM       = new Protocol("192", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        [System.Obsolete] /// <summary>193. [테이밍] 방장 변경</summary>
        public static readonly Protocol RECEIVE_TAMING_MAZE_ROOM_CHANGE_OWNER       = new Protocol("193", 0, ProtocolType.ResponseOnly);
        /// <summary>194. [스테이지] 입장 </summary>
        public static readonly Protocol REQUEST_FIELD_ROOM_JOIN                     = new Protocol("194", 0, ProtocolType.Request);
        /// <summary>195. [스테이지] 퇴장</summary>
        public static readonly Protocol REQUEST_FIELD_ROOM_EXIT                     = new Protocol("195", 0, ProtocolType.Request);
        /// <summary>196. [스테이지] 캐릭터 이동</summary>
        public static readonly Protocol REQUEST_FIELD_ROOM_TRANSFORM                = new Protocol("196", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        /// <summary>197. [스테이지] 유저 입장</summary>
        public static readonly Protocol RECEIVE_FIELD_ROOM_JOIN                     = new Protocol("197", 0, ProtocolType.ResponseOnly);
        /// <summary>198. [스테이지] 유저 퇴장</summary>
        public static readonly Protocol RECEIVE_FIELD_ROOM_EXIT                     = new Protocol("198", 0, ProtocolType.ResponseOnly);
        /// <summary>199. [시나리오] 시나리오 미로 입장</summary>
        public static readonly Protocol REQUEST_SCENARIO_MAZE_START                 = new Protocol("199", 0, ProtocolType.Request);
        /// <summary>200. [시나리오] 코인/제니 획득</summary>
        public static readonly Protocol REQUEST_SCENARIO_MAZE_GET_COIN              = new Protocol("200", 0, ProtocolType.Request);
        /// <summary>201. [시나리오] 입장 전투 시작</summary>
        public static readonly Protocol REQUEST_MAZE_BOSS_BATTLE_START              = new Protocol("201", 0, ProtocolType.Request);
        /// <summary>202. [시나리오] 입장 전투 클리어</summary>
        public static readonly Protocol REQUEST_MAZE_BOSS_CLEAR                     = new Protocol("202", 0, ProtocolType.Request);
        /// <summary>203. [시나리오] 몬스터와 충돌</summary>
        public static readonly Protocol REQUEST_SCENARIO_MAZE_NORMAL_MONSTER_DAMAGE = new Protocol("203", 0, ProtocolType.Request);
        /// <summary>204. 동료 장착, 변경, 해제</summary>
        public static readonly Protocol REQUEST_AGENT_SLOT_UPDATE                   = new Protocol("204", 0, ProtocolType.Request);
        /// <summary>205. 동료 합성</summary>
        public static readonly Protocol REQUEST_AGENT_COMPOSE                       = new Protocol("205", 0, ProtocolType.Request);
        /// <summary>206. 동료 탐험 시작 요청</summary>
        public static readonly Protocol REQUEST_AGENT_EXPLORE_START                 = new Protocol("206", 0, ProtocolType.Request);
        /// <summary>207. 동료 탐험 보상 요청</summary>
        public static readonly Protocol REQUEST_AGENT_EXPLORE_REWARD                = new Protocol("207", 0, ProtocolType.Request);
        /// <summary>208. 동료 탐험 교역 횟수 초기화</summary>
        public static readonly Protocol REQUEST_AGENT_INIT_TRADE_COUNT              = new Protocol("208", 0, ProtocolType.Request);
        /// <summary>209. 동료 탐험 취소</summary>
        public static readonly Protocol REQUEST_AGENT_EXPLORE_CANCEL                = new Protocol("209", 0, ProtocolType.Request);
        /// <summary>210. 동료 도감 보상 요청</summary>
        public static readonly Protocol REQUEST_AGENT_BOOK_ENABLE                   = new Protocol("210", 0, ProtocolType.Request);
        /// <summary>211. 내 캐릭터 공유 설정</summary>
        public static readonly Protocol REQUEST_SHARE_CHAR_SETTING                  = new Protocol("211", 0, ProtocolType.Request);
        /// <summary>212. 공유설정된 캐릭터 리스트 목록 요청</summary>
        public static readonly Protocol REQUEST_SHARE_CHAR_LIST                     = new Protocol("212", 0, ProtocolType.Request);
        /// <summary>213. 공유설정된 사용 및 취소 요청</summary>
        public static readonly Protocol REQUEST_SHARE_CHAR_USE_SETTING              = new Protocol("213", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>214. 공유 캐릭터 주인의 중도 공유 철회 시 호출</summary>
        public static readonly Protocol RECEIVE_SHARE_CHAR_SETTING_CANCEL           = new Protocol("214", 0, ProtocolType.ResponseOnly);
        /// <summary>215. 내 공유 캐릭터의 지금까지의 획득 보상 정보</summary>
        public static readonly Protocol REQUEST_SHARE_CHAR_REWARD_INFO              = new Protocol("215", 0, ProtocolType.Request);
        /// <summary>216. 내 공유 캐릭터 사용 보상 획득</summary>
        public static readonly Protocol REQUEST_SHARE_CHAR_REWARD_GET               = new Protocol("216", 0, ProtocolType.Request);
        /// <summary>217. 내 공유 캐릭터가 버림받았을 때 호출</summary>
        public static readonly Protocol RECEIVE_SHARE_CHAR_USE_SETTING_STOP         = new Protocol("217", 0, ProtocolType.ResponseOnly);
        /// <summary>218. 내 공유 캐릭터가 선택받았을 때 호출</summary>
        public static readonly Protocol RECEIVE_SHARE_CHAR_USE_SETTING_START        = new Protocol("218", 0, ProtocolType.ResponseOnly);
        /// <summary>219. 중도 철회된 공유 캐릭터 보상 정산</summary>
        public static readonly Protocol REQUEST_SHARE_CHAR_REWARD_CALC              = new Protocol("219", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>220. 내 캐릭터 공유 정산 완료 시 호출</summary>
        public static readonly Protocol RECEIVE_SHARE_CHAR_SETTING_CANCEL_OK        = new Protocol("220", 0, ProtocolType.ResponseOnly);
        /// <summary>221. 공유 캐릭터 사용 시간 충전</summary>
        public static readonly Protocol REQUEST_SHARE_CHAR_TIME_TICKET_USE          = new Protocol("221", 0, ProtocolType.Request);
        /// <summary>222. [스테이지] 일반 몬스터 드랍</summary>
        public static readonly Protocol REQUEST_AUTO_STAGE_MON_DROP                 = new Protocol("222", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        /// <summary>223. [시나리오] MVP 몬스터 드랍</summary>
        public static readonly Protocol REQUEST_AUTO_STAGE_MVP_DROP                 = new Protocol("223", 0, ProtocolType.Request, isIndicator: false, isLog: true);
        /// <summary>224. [시나리오] 플레이어 소환</summary>
        public static readonly Protocol REQUEST_AUTO_STAGE_SUMMON_PLAYER            = new Protocol("224", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>225. [시나리오] 내 캐릭터 공유 정산 실패 시 호출</summary>
        public static readonly Protocol RECEIVE_SHARE_CHAR_SETTING_CANCEL_FAIL      = new Protocol("225", 0, ProtocolType.ResponseOnly);
        /// <summary>226. [스테이지] MVP 몬스터 소환</summary>
        public static readonly Protocol REQUEST_AUTO_STAGE_SUMMON_MVP               = new Protocol("226", 0, ProtocolType.Request);
        /// <summary>227. [클리커 던전] 클리커 던전 입장</summary>
        public static readonly Protocol REQUEST_CLICKER_DUNGEON_START               = new Protocol("227", 0, ProtocolType.Request);
        /// <summary>228. [클리커 던전] 클리커 던전 보상 획득</summary>
        public static readonly Protocol REQUEST_CLICKER_DUNGEON_GET_REWARD          = new Protocol("228", 0, ProtocolType.Request);
        /// <summary>229. [듀얼] 듀얼 정보 요청</summary>
        public static readonly Protocol REQUEST_DUEL_INFO                           = new Protocol("229", 0, ProtocolType.Request);
        /// <summary>230. [듀얼] 듀얼 캐릭터 목록 요청</summary>
        public static readonly Protocol REQUEST_DUEL_CHAR_LIST                      = new Protocol("230", 0, ProtocolType.Request);
        /// <summary>231. [듀얼] 듀얼 시작</summary>
        public static readonly Protocol REQUEST_DUEL_START                          = new Protocol("231", 0, ProtocolType.Request);
        /// <summary>232. [듀얼] 듀얼 종료</summary>
        public static readonly Protocol REQUEST_DUEL_END                            = new Protocol("232", 0, ProtocolType.Request);
        /// <summary>233. [듀얼] 듀얼 보상 요청</summary>
        public static readonly Protocol REQUEST_DUEL_GET_REWARD                     = new Protocol("233", 0, ProtocolType.Request);
        /// <summary>234. [듀얼] 듀얼 포인트 구매 요청</summary>
        public static readonly Protocol REQUEST_DUEL_POINT_BUY                      = new Protocol("234", 0, ProtocolType.Request);
        /// <summary>235. 아이템 획득 알람</summary>
        public static readonly Protocol RECEIVE_ITEM_GET_NOTICE                     = new Protocol("235", 0, ProtocolType.ResponseOnly);
        /// <summary>236. [자동사냥] 보스 소환</summary>
        public static readonly Protocol REQUEST_SUMMON_STAGE_BOSS                   = new Protocol("236", 0, ProtocolType.Request, sendingQueueGroupId: 1);
        /// <summary>237. [자동사냥] 보스 클리어</summary>
        public static readonly Protocol REQUEST_STAGE_BOSS_CLEAR                    = new Protocol("237", 0, ProtocolType.Request);
        /// <summary>238. 장비 강화 요청</summary>
        public static readonly Protocol REQUEST_EQUIP_ITEM_LEVELUP                  = new Protocol("238", 0, ProtocolType.Request);
        /// <summary>239. 캐릭터 세어중 어플 백그라운드로 갔을때 누적된 보상 db저장처리</summary>
        public static readonly Protocol REQUEST_SHARE_CHAR_REWARD_CALC_ALL          = new Protocol("239", 0, ProtocolType.Request);
        /// <summary>240. 캐릭터 세어중 어플 백그라운드에서 포그라운드로 왔을때 쉐어사용중인 캐릭터 리스</summary>
        public static readonly Protocol REQUEST_SHARE_USE_CHAR_LIST                 = new Protocol("240", 0, ProtocolType.Request);
        /// <summary>241. [채팅] 공지사항 갱신 요청</summary>
        public static readonly Protocol REQUEST_CHAT_NOTICE_DATA                    = new Protocol("241", 0, ProtocolType.Request);
        /// <summary>242. 서버 변경시 셰어 캐릭터 다시 등록</summary>
        public static readonly Protocol REQUEST_SERVER_CHANGE_SHARE_CHAR_REUSE      = new Protocol("242", 0, ProtocolType.Request);
        /// <summary>243. 타 유저 정보 보기</summary>
        public static readonly Protocol REQUEST_FRIEND_CHAR_INFO                    = new Protocol("243", 0, ProtocolType.Request);
        /// <summary>244. 정보 공개 여부 설정</summary>
        public static readonly Protocol REQUEST_OPEN_INFO_SETTING                   = new Protocol("244", 0, ProtocolType.Request);
        /// <summary>245. 종료 시 셰어 자동 등록 설정</summary>
        public static readonly Protocol REQUEST_AUTO_SHARE_SETTING                  = new Protocol("245", 0, ProtocolType.Request);
        /// <summary>246. 이벤트 룰렛 실행</summary>
        public static readonly Protocol REQUEST_ROULETTE_REWARD                     = new Protocol("246", 0, ProtocolType.Request);
        /// <summary>247. 옵션 셋팅</summary>
        public static readonly Protocol REQUEST_OPTION_SETTING                      = new Protocol("247", 0, ProtocolType.Request);
        /// <summary>248. 셰어 강제 중지</summary>
        public static readonly Protocol REQUEST_SHARE_FORCE_QUIT                    = new Protocol("248", 0, ProtocolType.Request);
        /// <summary>249. 확성기 수신</summary>
        public static readonly Protocol RECEIVE_LOUD_SPEAKER_MESSAGE                = new Protocol("249", 0, ProtocolType.ResponseOnly);
        /// <summary>250. 장비 속성 변경</summary>
        public static readonly Protocol REQUEST_ITEM_ELEMENT_CHANGE                 = new Protocol("250", 0, ProtocolType.Request);
        /// <summary>251. 셰어 캐릭터 목록 호출 (직업 필터링)</summary>
        public static readonly Protocol REQUEST_SHARE_JOB_FILTER                    = new Protocol("251", 0, ProtocolType.Request);
        /// <summary>252. 카드 복원</summary>
        public static readonly Protocol REQUEST_RESTORE_MON_CARD                    = new Protocol("252", 0, ProtocolType.Request);
        /// <summary>253. 카드 초기화</summary>
        public static readonly Protocol REQUEST_RESET_MON_CARD                      = new Protocol("253", 0, ProtocolType.Request);
        /// <summary>254. 셰어 바이스 업그레이드</summary>
        public static readonly Protocol REQUEST_SHARE_VICE_LEVELUP                  = new Protocol("254", 0, ProtocolType.Request);
        /// <summary>255. 코스튬 장착,해제,교체</summary>
        public static readonly Protocol COSTUME_EQUIP                               = new Protocol("255", 0, ProtocolType.Request);
        /// <summary>256. 다른 유저 무기,코스튬 변경 이벤트</summary>
        public static readonly Protocol RESPONSE_CHAR_UPDATE                        = new Protocol("256", 0, ProtocolType.ResponseOnly);
        /// <summary>257. 테이밍 시즌 정보</summary>
        public static readonly Protocol REQUEST_TAMING_SEASON_INFO                  = new Protocol("257", 0, ProtocolType.Request);
        /// <summary>258. 길드 큐펫 목록</summary>
        public static readonly Protocol GUILD_CUPET_LIST                            = new Protocol("258", 0, ProtocolType.Request);
        /// <summary>259. 길드 큐펫 등급 업</summary>
        public static readonly Protocol GUILD_CUPET_RANK_UP                         = new Protocol("259", 0, ProtocolType.Request);
        /// <summary>260. 길드 큐펫 등급 변경</summary>
        public static readonly Protocol GUILD_CUPET_RANK_CHANGE                     = new Protocol("260", 0, ProtocolType.Request);
        /// <summary>261. 매일매일 보상 패키지 받기</summary>
        public static readonly Protocol EVERYDAY_GOODS_GET                          = new Protocol("261", 0, ProtocolType.Request);
        /// <summary>262. 매일매일 보상 패키지 내 정보</summary>
        public static readonly Protocol EVERYDAY_GOODS_MY_INFO                      = new Protocol("262", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>263. 레벨업(직업) 패키지 보상 받기</summary>
        public static readonly Protocol REQUEST_PAY_JOB_LEVEL_REWARD                = new Protocol("263", 0, ProtocolType.Request);
        /// <summary>264. 시나리오 패키지 보상 받기</summary>
        public static readonly Protocol REQUEST_PAY_SCENARIO_REWARD                 = new Protocol("264", 0, ProtocolType.Request);
        /// <summary>265. 결제 성공 이벤트</summary>
        public static readonly Protocol RECEIVE_PURCHASE_SUCCESS                    = new Protocol("265", 0, ProtocolType.ResponseOnly);
        /// <summary>266. 우편함 상점 탭 받기</summary>
        public static readonly Protocol REQUEST_SHOP_MAIL_GET                       = new Protocol("266", 0, ProtocolType.Request);
        /// <summary>267. 마일리지 보상 받기</summary>
        public static readonly Protocol REQUEST_MILEAGE_REWARD_GET                  = new Protocol("267", 0, ProtocolType.Request);
        /// <summary>268. 던전 소탕 요청</summary>
        public static readonly Protocol REQUEST_DUNGEON_FAST_CLEAR                  = new Protocol("268", 0, ProtocolType.Request);
        /// <summary>269. 빙고 퀘스트 클리어</summary>
        public static readonly Protocol REQUEST_BINGO_QUESTCLEAR                    = new Protocol("269", 0, ProtocolType.Request);
        /// <summary>270. 빙고 아이템 사용</summary>
        public static readonly Protocol REQUEST_BINGO_INPUTITEM                     = new Protocol("270", 0, ProtocolType.Request);
        /// <summary>271. 빙고 줄 완성</summary>
        public static readonly Protocol REQUEST_BINGO_CLEARLINE                     = new Protocol("271", 0, ProtocolType.Request);
        /// <summary>272. 중앙실험실 시작</summary>
        public static readonly Protocol REQUEST_CLAB_START                          = new Protocol("272", 0, ProtocolType.Request);
        /// <summary>273. 중앙실험실 몬스터 처치</summary>
        public static readonly Protocol REQUEST_CLAB_MONKILL                        = new Protocol("273", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>274. 중앙실험실 아이템 사용</summary>
        public static readonly Protocol REQUEST_CLAB_USEITEM                        = new Protocol("274", 0, ProtocolType.Request);
        /// <summary>275. 중앙실험실 나가기</summary>
        public static readonly Protocol REQUEST_CLAB_EXIT                           = new Protocol("275", 0, ProtocolType.Request);
        /// <summary>276. 빙고 정보 요청</summary>
        public static readonly Protocol REQUEST_BINGO_SEASONINFO                    = new Protocol("276", 0, ProtocolType.Request);
        /// <summary>277. 중앙실험실 스킬 선택</summary>
        public static readonly Protocol REQUEST_CLAB_SELECT_SKILL                   = new Protocol("277", 0, ProtocolType.Request);
        /// <summary>278. 중앙실험실 스킬 쿨타임 체크</summary>
        public static readonly Protocol REQUEST_CLAB_SKILL_COOLTIME_CHECK           = new Protocol("278", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>279. 특정 던전 무료 보상</summary>
        public static readonly Protocol REQUEST_DUNGEON_DAILY_FREE_REWARD           = new Protocol("279", 0, ProtocolType.Request);
        /// <summary>280. 셰어 바이스 레벨업</summary>
        public static readonly Protocol REQUEST_SHARE_VICE_LEVELUP_COMP             = new Protocol("280", 0, ProtocolType.Request);
        /// <summary>281. 스페셜 룰렛 뽑기 요청</summary>
        public static readonly Protocol REQUEST_SPECIAL_ROULETTE_REWARD             = new Protocol("281", 0, ProtocolType.Request);
        /// <summary>282. 스페셜 룰렛판 변경 요청</summary>
        public static readonly Protocol REQUEST_SPECIAL_ROULETTE_INIT_BOARD         = new Protocol("282", 0, ProtocolType.Request);
        /// <summary>283. 유저 신고하기</summary>
        public static readonly Protocol REQUEST_REPORT_CHAR                         = new Protocol("283", 0, ProtocolType.Request);
        /// <summary>284. 채금 남은시간</summary>
        public static readonly Protocol RESULT_CHAT_LIMIT                           = new Protocol("284", 0, ProtocolType.ResponseOnly);
        /// <summary>285. 클론 캐릭터 셰어</summary>
        public static readonly Protocol REQUEST_CLONE_SHARE                         = new Protocol("285", 0, ProtocolType.Request);
        /// <summary>286. 클론 캐릭터 셰어 해제</summary>
        public static readonly Protocol REQUEST_CLONE_SHARE_RELEASE                 = new Protocol("286", 0, ProtocolType.Request);
        /// <summary>287. 퀴즈 이벤트 보상 받기</summary>
        public static readonly Protocol REQUEST_EVENT_QUIZ_REWARD                   = new Protocol("287", 0, ProtocolType.Request);
        /// <summary>288. 이모션 연출</summary>
        public static readonly Protocol REQUEST_EMOTION                             = new Protocol("288", 0, ProtocolType.SendOnly);
        /// <summary>289. 오버 스탯 요청</summary>
        public static readonly Protocol REQUEST_OVER_STAT                           = new Protocol("289", 0, ProtocolType.Request);
        /// <summary>290. 광고 보상 요청(고객보상)</summary>
        public static readonly Protocol REQUEST_AD_REWARD                           = new Protocol("290", 0, ProtocolType.Request);
        /// <summary>291. 가위바위보 게임 시작</summary>
        public static readonly Protocol REQUEST_RPS_GAME                            = new Protocol("291", 0, ProtocolType.Request);
        /// <summary>292. 가위바위보 게임 초기화</summary>
        public static readonly Protocol REQUEST_RPS_INIT                            = new Protocol("292", 0, ProtocolType.Request);
        /// <summary>293. 유저 이벤트 퀘스트 보상</summary>
        public static readonly Protocol REQUEST_USER_EVNET_QUEST_REWARD             = new Protocol("293", 0, ProtocolType.Request);
        /// <summary>294. [주사위이벤트] 주사위 굴리기</summary>
        public static readonly Protocol REQUEST_DICE_ROLL                           = new Protocol("294", 0, ProtocolType.Request);
        /// <summary>295. [주사위이벤트] 완주 보상 받기</summary>
        public static readonly Protocol REQUEST_DICE_REWARD                         = new Protocol("295", 0, ProtocolType.Request);
        /// <summary>296. 메일 삭제</summary>
        public static readonly Protocol REQUEST_MAIL_DELETE                         = new Protocol("296", 0, ProtocolType.Request);
        /// <summary>297. 페이스북 친구 초대</summary>
        public static readonly Protocol REQUEST_INVITE_FB_IDS                       = new Protocol("297", 0, ProtocolType.Request);
        /// <summary>298. 페이스북 완료한 친구</summary>
        public static readonly Protocol REQUEST_ACCEPT_FB_IDS                       = new Protocol("298", 0, ProtocolType.Request);
        /// <summary>299. 이벤트 스테이지 정보</summary>
        public static readonly Protocol REQUEST_EVENT_STAGE_INFO                    = new Protocol("299", 0, ProtocolType.Request);
        /// <summary>300. 쉐도우 장비 카드슬롯 오픈</summary>
        public static readonly Protocol REQUEST_SHADOW_ITEM_OPEN_CARD_SLOT          = new Protocol("300", 0, ProtocolType.Request);
        /// <summary>301. 페이스북 좋아요 완료보상</summary>
        public static readonly Protocol REQUEST_FB_LIKE_REWARD                      = new Protocol("301", 0, ProtocolType.Request);
        /// <summary>302. 어둠의 나무 보상 선택</summary>
        public static readonly Protocol REQUEST_DARK_TREE_SELECT_REWARD             = new Protocol("302", 0, ProtocolType.Request);
        /// <summary>303. 어둠의 나무 재료 넣기</summary>
        public static readonly Protocol REQUEST_DARK_TREE_REG_POINT                 = new Protocol("303", 0, ProtocolType.Request);
        /// <summary>304. 어둠의 나무 시작</summary>
        public static readonly Protocol REQUEST_DARK_TREE_START                     = new Protocol("304", 0, ProtocolType.Request);
        /// <summary>305. 어둠의 나무 보상 받기</summary>
        public static readonly Protocol REQUEST_DARK_TREE_GET_REWARD                = new Protocol("305", 0, ProtocolType.Request);
        /// <summary>306. 타임패트롤 입장</summary>
        public static readonly Protocol REQUEST_TP_STAGE_ENTER                      = new Protocol("306", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>307. 타임패트롤 보스 소환</summary>
        public static readonly Protocol REQUEST_TP_SUMMON_BOSS                      = new Protocol("307", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>308. 타임패트롤 몬스터 처치</summary>
        public static readonly Protocol REQUEST_TP_MON_KILL                         = new Protocol("308", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        /// <summary>309. 타임패트롤 보스 처치</summary>
        public static readonly Protocol REQUEST_TP_BOSS_KILL                        = new Protocol("309", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>310. 타임패트롤 코스튬 레벨업</summary>
        public static readonly Protocol REQUEST_TP_COSTUME_LEVELUP                  = new Protocol("310", 0, ProtocolType.Request);
        /// <summary>311. 타임패트롤 코스튬 On/Off</summary>
        public static readonly Protocol REQUEST_TP_COSTUME_ONOFF                    = new Protocol("311", 0, ProtocolType.Request);
        /// <summary>312. 아이템 교환</summary>
        public static readonly Protocol REQUEST_KAF_EXCHANGE                        = new Protocol("312", 0, ProtocolType.Request);
        /// <summary>313. 프로필 변경</summary>
        public static readonly Protocol REQUEST_PROFILE_CHANGE                      = new Protocol("313", 0, ProtocolType.Request);
        /// <summary>314. [카프라 운송] 퀘스트 수락</summary>
        public static readonly Protocol REQUEST_KAF_DELIVERY                        = new Protocol("314", 0, ProtocolType.Request);
        /// <summary>315. [카프라 운송] 퀘스트 완료</summary>
        public static readonly Protocol REQUEST_KAF_DELIVERY_ACCEPT                 = new Protocol("315", 0, ProtocolType.Request);
        /// <summary>316. [카프라 운송] 퀘스트 보상 획득 요청</summary>
        public static readonly Protocol REQUEST_KAF_DELIVERY_REWARD                 = new Protocol("316", 0, ProtocolType.Request);
        /// <summary>317. [클리커 던전] 종료</summary>
        public static readonly Protocol REQUEST_CLICKER_DUNGEON_END                 = new Protocol("317", 0, ProtocolType.Request);
        /// <summary>318. [길드] 길드명 변경 요청</summary>
        public static readonly Protocol REQUEST_GUILD_NAME_CHANGE                   = new Protocol("318", 0, ProtocolType.Request);
        /// <summary>319. [길드전] 길드전 시즌 정보</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_SEASON_INFO            = new Protocol("319", 0, ProtocolType.Request);
        /// <summary>320. [길드전] 길드전 참가 신청 - 큐펫 세팅(방어)</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_ENTRY                  = new Protocol("320", 0, ProtocolType.Request);
        /// <summary>321. [길드전] 길드전 대전상대 목록중 선택한 길드의 상세 정보</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_TARGET_DETAIL_INFO     = new Protocol("321", 0, ProtocolType.Request);
        /// <summary>322. [길드전] 길드전 시작</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_START_CHECK            = new Protocol("322", 0, ProtocolType.Request);
        /// <summary>323. [길드전] 길드전 시작 로딩 완료</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_START_LOADING_COMP     = new Protocol("323", 0, ProtocolType.Request);
        /// <summary>324. [길드전] 길드전 큐펫 세텡(공격)</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_SUPPORT_CUPET_SETTING  = new Protocol("324", 0, ProtocolType.Request);
        /// <summary>325. [길드전] 길드전 전투정보 탭 정보 표시용</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_DEF_INFO               = new Protocol("325", 0, ProtocolType.Request);
        /// <summary>326. [길드전] 길드전 큐펫 경혐치 업</summary>
        public static readonly Protocol REQUEST_GUILD_CUPET_EXP_UP                  = new Protocol("326", 0, ProtocolType.Request);
        /// <summary>327. [길드전] 길드전 버프 목록</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_BUFF_INFO              = new Protocol("327", 0, ProtocolType.Request);
        /// <summary>328. [길드전] 길드전 버프 경험치 업</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_BUFF_EXP_UP            = new Protocol("328", 0, ProtocolType.Request);
        /// <summary>329. [길드전] 길드전 엠펠리움 공격</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_ATTACK_DAMAGE          = new Protocol("329", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        /// <summary>330. [길드전] 길드전 랭킹 목록</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_RANK                   = new Protocol("330", 0, ProtocolType.Request);
        /// <summary>331. [길드전] 길드전 전투 종료</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_END                    = new Protocol("331", 0, ProtocolType.Request);
        /// <summary>332. [길드전] 길드전 전투 길드 목록 정보</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_LIST                   = new Protocol("332", 0, ProtocolType.Request);
        /// <summary>333. [길드전] 길드전 전투 진형 탭 정보</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_ATTACK_POSITION        = new Protocol("333", 0, ProtocolType.Request);
        /// <summary>334. 직업 변경</summary>
        public static readonly Protocol REQUEST_JOB_CHANGE_TICKET                   = new Protocol("334", 0, ProtocolType.Request);
        /// <summary>335. [길드전] 이벤트 길드전 랭킹 목록</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_EVENT_RANK             = new Protocol("335", 0, ProtocolType.Request);
        /// <summary>336. [길드전] 길드전 내 길드원 데미지 랭킹</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_GUILD_CHAR_RANK        = new Protocol("336", 0, ProtocolType.Request);
        /// <summary>337. [길드전] 길드전 수비 큐펫 정보</summary>
        public static readonly Protocol REQUEST_GUILD_BATTLE_DEF_CUPET_INFO         = new Protocol("337", 0, ProtocolType.Request);
        /// <summary>338. [쉐어] 쉐어스텟 강화</summary>
        public static readonly Protocol REQUEST_SHARE_STAT_BUILD_UP                 = new Protocol("338", 0, ProtocolType.Request);
        /// <summary>339. [쉐어] 쉐어스텟 초기화</summary>
        public static readonly Protocol REQUEST_SHARE_STAT_RESET                    = new Protocol("339", 0, ProtocolType.Request);
        /// <summary>340. [쉐어] 길드 쉐어 캐릭터 목록 요청</summary>
        public static readonly Protocol REQUEST_GUILD_SHARE_CHAR_LIST               = new Protocol("340", 0, ProtocolType.Request);
        /// <summary>341. 네이버 라운지 프로모션 이벤트 로그인 보너스 14일차</summary>
        public static readonly Protocol REQUEST_PROMOTION_LOGIN_BONUS_REWARD        = new Protocol("341", 0, ProtocolType.Request);
        /// <summary>342. 패스 보상 수령 요청</summary>
        public static readonly Protocol REQUEST_PASS_REWARD                         = new Protocol("342", 0, ProtocolType.Request);
        /// <summary>343. 패스 경험치 구매 요청</summary>
        public static readonly Protocol REQUEST_PASS_BUY_POINT                      = new Protocol("343", 0, ProtocolType.Request);
        /// <summary>344. [듀얼:아레나] 정보</summary>
        public static readonly Protocol REQUEST_ARENA_INFO                          = new Protocol("344", 0, ProtocolType.Request, isIndicator: true);
        /// <summary>345. [듀얼:아레나] 매칭 정보</summary>
        public static readonly Protocol REQUEST_ARENA_MATCHING_LIST                 = new Protocol("345", 0, ProtocolType.Request);
        /// <summary>346. [듀얼:아레나] 전투 시작</summary>
        public static readonly Protocol REQUEST_ARENA_BATTLE_START                  = new Protocol("346", 0, ProtocolType.Request);
        /// <summary>347. [듀얼:아레나] 전투 종료</summary>
        public static readonly Protocol REQUEST_ARENA_BATTLE_END                    = new Protocol("347", 0, ProtocolType.Request);
        /// <summary>348. [듀얼:아레나] 보상 받기</summary>
        public static readonly Protocol REQUEST_ARENA_POINT_REWARD_GET              = new Protocol("348", 0, ProtocolType.Request);
        /// <summary>349. [듀얼:아레나] 랭킹</summary>
        [System.Obsolete]
        public static readonly Protocol REQUEST_ARENA_RANK                          = new Protocol("349", 0, ProtocolType.Request);
        /// <summary>350. [듀얼:아레나] 아레나 깃발 구매</summary>
        public static readonly Protocol REQUEST_ARENA_POINT_BUY                     = new Protocol("350", 0, ProtocolType.Request);
        /// <summary>351. [나비호] 정보</summary>
        public static readonly Protocol REQUEST_NABIHO_INFO                         = new Protocol("351", 0, ProtocolType.Request);
        /// <summary>352. [나비호] 의뢰 아이템 선택</summary>
        public static readonly Protocol REQUEST_NABIHO_ITEM_SELECT                  = new Protocol("352", 0, ProtocolType.Request);
        /// <summary>353. [나비호] 의뢰 아이템 선택 취소</summary>
        public static readonly Protocol REQUEST_NABIHO_ITEM_SELECT_CANCEL           = new Protocol("353", 0, ProtocolType.Request);
        /// <summary>354. [나비호] 의뢰 아이템 광고 시청후 시간 단축</summary>
        public static readonly Protocol REQUEST_NABIHO_ITEM_AD_TIME_REDUCTION       = new Protocol("354", 0, ProtocolType.Request);
        /// <summary>355. [나비호] 의뢰 아이템 수령</summary>
        public static readonly Protocol REQUEST_NABIHO_ITEM_SELECT_GET              = new Protocol("355", 0, ProtocolType.Request);
        /// <summary>356. [나비호] 선물 - 친밀도 증가</summary>
        public static readonly Protocol REQUEST_NABIHO_SEND_PRESENT                 = new Protocol("356", 0, ProtocolType.Request);
        /// <summary>357. [글자수집] 보상 요청</summary>
        public static readonly Protocol REQUEST_FIND_ALPHABET_REWARD                = new Protocol("357", 0, ProtocolType.Request);
        /// <summary>358. [이벤트] 냥다래 나무 모든보상 획득 퀘스트(103) 스킵</summary>
        public static readonly Protocol REQUEST_EVENT_CONNECT_QUEST_SKIP            = new Protocol("358", 0, ProtocolType.Request);
        /// <summary>359. [온버프] 온버프 로그인</summary>
        public static readonly Protocol REQUEST_ONBUFF_LOGIN                        = new Protocol("359", 0, ProtocolType.Request);
        /// <summary>360. [온버프] 온버프 계정 연동</summary>
        public static readonly Protocol REQUEST_ONBUFF_ACCOUNT_LINK                 = new Protocol("360", 0, ProtocolType.Request);
        /// <summary>361. [온버프] 온버프 계정 연동 해제</summary>
        public static readonly Protocol REQUEST_ONBUFF_ACCOUNT_UNLINK               = new Protocol("361", 0, ProtocolType.Request);
        /// <summary>362. [온버프] 지급 가능한 온버프 포인트 없음</summary>
        public static readonly Protocol RECEIVE_ONBUFF_COIN_ZERO                    = new Protocol("362", 0, ProtocolType.ResponseOnly);
        /// <summary>363. [온버프] 온버프 포인트 메일함 수령</summary>
        public static readonly Protocol REQUEST_COIN_MAIL_GET                       = new Protocol("363", 0, ProtocolType.Request);
        /// <summary>364. [온버프] 온버프 내 포인트 동기화</summary>
        public static readonly Protocol REQUEST_ONBUFF_MY_POINT_INFO                = new Protocol("364", 0, ProtocolType.Request);
        /// <summary>365. [온버프] 온버프 패스 보상받기</summary>
        public static readonly Protocol REQUEST_ONBUFF_PASS_REWARD                  = new Protocol("365", 0, ProtocolType.Request);
        /// <summary>366. [온버프] 온버프 MVP 처치포인트 변환</summary>
        public static readonly Protocol REQUEST_ONBUFF_MVP_POINT_GET                = new Protocol("366", 0, ProtocolType.Request);
        /// <summary>367. [온버프] 전체 이벤트 온버프 포인트 남은 잔여량 조회</summary>
        public static readonly Protocol REQUEST_ONBUFF_TOTAL_REMAIN_POINT           = new Protocol("367", 0, ProtocolType.Request);

        /// <summary>501. 듀얼 서버대전 정보</summary>
        public static readonly Protocol REQUEST_DUELWORLD_INFO                      = new Protocol("501", 0, ProtocolType.Request);
        /// <summary>502. 듀얼 서버대전 캐릭터 목록</summary>
        public static readonly Protocol REQUEST_DUELWORLD_CHAR_LIST                 = new Protocol("502", 0, ProtocolType.Request);
        /// <summary>503. 듀얼 서버대전 대전 시작</summary>
        public static readonly Protocol REQUEST_DUELWORLD_BATTLESTART               = new Protocol("503", 0, ProtocolType.Request);
        /// <summary>504. 듀얼 서버대전 대전 종료</summary>
        public static readonly Protocol REQUEST_DUELWORLD_BATTLEEND                 = new Protocol("504", 0, ProtocolType.Request);
        /// <summary>505. 듀얼 서버대전 보상 받기</summary>
        public static readonly Protocol REQUEST_DUELWORLD_GET_REWARD                = new Protocol("505", 0, ProtocolType.Request);
        /// <summary>506. 듀얼 서버대전 랭킹 정보</summary>
        public static readonly Protocol REQUEST_DUELWORLD_RANKING                   = new Protocol("506", 0, ProtocolType.Request);

        /// <summary>572. 거래소 아이템 등록</summary>
        public static readonly Protocol REQUEST_TRADE_REGISTER                      = new Protocol("572", 0, ProtocolType.Request);
        /// <summary>573. 거래소 아이템 등록 취소</summary>
        public static readonly Protocol REQUEST_TRADE_REGISTER_CANCEL               = new Protocol("573", 0, ProtocolType.Request);
        /// <summary>574. 거래소 아이템 검색</summary>
        public static readonly Protocol REQUEST_TRADE_SEARCH                        = new Protocol("574", 0, ProtocolType.Request);
        /// <summary>575. 거래소 아이템 구매 요청</summary>
        public static readonly Protocol REQUEST_TRADE_BUY                           = new Protocol("575", 0, ProtocolType.Request);
        /// <summary>576. 구매/판매 리스트 요청</summary>
        public static readonly Protocol REQUEST_TRADEDONE_LIST                      = new Protocol("576", 0, ProtocolType.Request);
        /// <summary>577. 아이템/골드수령(단일)</summary>
        public static readonly Protocol REQUEST_TRADE_GET_SELLITEM                  = new Protocol("577", 0, ProtocolType.Request);
        /// <summary>578. 아이템/골드수령(모두)</summary>
        public static readonly Protocol REQUEST_TRADE_GET_SELLITEMALL               = new Protocol("578", 0, ProtocolType.Request);
        /// <summary>590. 개인 상점 로비 입장</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_ENTERROOM              = new Protocol("590", 0, ProtocolType.Request);
        /// <summary>591. 개인 상점 로비 퇴장</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_EXITROOM               = new Protocol("591", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>592. 개인 상점 로비 내 캐릭터 이동</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_TRANSFORM              = new Protocol("592", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>593. 개인 상점에 아이템 등록</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_REGISTER               = new Protocol("593", 0, ProtocolType.Request);
        /// <summary>594. 다른 유저의 개인 상점 아이템 리스트 요청</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_REGLIST                = new Protocol("594", 0, ProtocolType.Request);
        /// <summary>595. 다른 유저의 개인 상점 아이템 품목 구매 요청</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_BUY                    = new Protocol("595", 0, ProtocolType.Request);
        /// <summary>596. 개인 상점 로비 다른 유저 입장</summary>
        public static readonly Protocol RESPONSE_TRADEPRIVATE_ENTERROOM             = new Protocol("596", 0, ProtocolType.ResponseOnly);
        /// <summary>597. 다른 유저가 내 개인 상점 품목을 구매함</summary>
        public static readonly Protocol RESPONSE_TRADEPRIVATE_SELL                  = new Protocol("597", 0, ProtocolType.ResponseOnly);
        /// <summary>598. 다른 유저의 거래소 퇴장 이벤트 수신</summary>
        public static readonly Protocol RESPONSE_TRADEPRIVATE_EXITROOM              = new Protocol("598", 0, ProtocolType.ResponseOnly);
        /// <summary>599. 다른 유저의 개인상점 아이템 등록 이벤트 수신</summary>
        public static readonly Protocol RESPONSE_TRADEPRIVATE_REGISTER              = new Protocol("599", 0, ProtocolType.ResponseOnly);
        /// <summary>600. 노점 아이템 회수 요청</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_REGCANCELONE           = new Protocol("600", 0, ProtocolType.Request);
        /// <summary>601. 노점 판매 종료 요청</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_END                    = new Protocol("601", 0, ProtocolType.Request);
        /// <summary>602. 다른 유저의 노점 판매 종료</summary>
        public static readonly Protocol RESPONSE_TRADEPRIVATE_END                   = new Protocol("602", 0, ProtocolType.ResponseOnly);
        /// <summary>603. 거래소 전체 채팅</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_ALLUSERCHAT            = new Protocol("603", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>604. 거래소 1:1 채팅</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_WHISPER                = new Protocol("604", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>605. 다른 유저의 개인상점(채팅채널) 퇴장/summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_EXITUSERSHOP           = new Protocol("605", 0, ProtocolType.Request);
        /// <summary>606. 개인상점 내 채팅</summary>
        public static readonly Protocol REQUEST_TRADEPRIVATE_USERSHOPCHAT           = new Protocol("606", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>621. [길드 로비] 입장</summary>
        public static readonly Protocol REQUEST_GUILDUSER_ENTERROOM                 = new Protocol("621", 0, ProtocolType.Request);
        /// <summary>622. [길드 로비] 퇴장</summary>
        public static readonly Protocol REQUEST_GUILDUSER_EXITROOM                  = new Protocol("622", 0, ProtocolType.Request);
        /// <summary>623. [길드 로비] 캐릭터 이동</summary>
        public static readonly Protocol REQUEST_GUILDUSER_TRANSFORM                 = new Protocol("623", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>624. [길드 로비] 다른 유저 입장</summary>
        public static readonly Protocol RESPONSE_GUILDUSER_ENTERROOM                = new Protocol("624", 0, ProtocolType.ResponseOnly);
        /// <summary>625. [길드 로비] 다른 유저 퇴장</summary>
        public static readonly Protocol RESPONSE_GUILDUSER_EXITROOM                 = new Protocol("625", 0, ProtocolType.ResponseOnly);
        /// <summary>651. [길드 습격] 시작</summary>
        public static readonly Protocol RECEIVE_GA_START                            = new Protocol("651", 0, ProtocolType.ResponseOnly);
        /// <summary>652. [길드 습격] 종료</summary>
        public static readonly Protocol RECEIVE_GA_END                              = new Protocol("652", 0, ProtocolType.ResponseOnly);
        /// <summary>653. [길드 습격] 공격</summary>
        public static readonly Protocol REQUEST_GA_ATTACK                           = new Protocol("653", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>654. [길드 습격] 공격 모션 (공격자를 제외한 모두가 수신)</summary>
        public static readonly Protocol REQUEST_GA_ATTACK_MOT                       = new Protocol("654", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>655. [길드 습격] 몬스터 상태 변경</summary>
        public static readonly Protocol RECEIVE_GA_MONSTATUS                        = new Protocol("655", 0, ProtocolType.ResponseOnly);
        /// <summary>656. [길드 습격] 특정 유저 힐</summary>
        public static readonly Protocol RECEIVE_GA_PLAYER_PLUSHP                    = new Protocol("656", 0, ProtocolType.ResponseOnly);
        /// <summary>657. [길드 습격] 몬스터 상태이상</summary>
        public static readonly Protocol RECEIVE_GA_MON_GETCROWDCONTROL              = new Protocol("657", 0, ProtocolType.ResponseOnly);
        /// <summary>658. [길드 습격] 몬스터 대미지</summary>
        public static readonly Protocol RECEIVE_GA_MON_DAMAGE                       = new Protocol("658", 0, ProtocolType.ResponseOnly);
        /// <summary>659. [길드 습격] 몬스터 죽음</summary>
        public static readonly Protocol RECEIVE_GA_MON_DIE                          = new Protocol("659", 0, ProtocolType.ResponseOnly);
        /// <summary>660. [길드 습격] 엠펠리온 대미지</summary>
        public static readonly Protocol RECEIVE_GA_EMPAL_DAMAGE                     = new Protocol("660", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>661. [길드 습격] 엠펠리온 죽음</summary>
        public static readonly Protocol RECEIVE_GA_EMPAL_DIE                        = new Protocol("661", 0, ProtocolType.ResponseOnly);
        /// <summary>662. [길드 습격] 특정 유저 대미지</summary>
        public static readonly Protocol RECEIVE_GA_PLAYER_DAMAGE                    = new Protocol("662", 0, ProtocolType.ResponseOnly);
        /// <summary>663. [길드 습격] 특정 유저 죽음</summary>
        public static readonly Protocol RECEIVE_GA_PLAYER_DIE                       = new Protocol("663", 0, ProtocolType.ResponseOnly);
        /// <summary>664. [길드 습격] 특정 유저 부활</summary>
        public static readonly Protocol RECEIVE_GA_PLAYER_APPEAR                    = new Protocol("664", 0, ProtocolType.ResponseOnly);
        /// <summary>665. [길드 습격] 거석 떨어지기 전 알림</summary>
        public static readonly Protocol RECEIVE_GA_ROCK_APPEAR_READY                = new Protocol("665", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>666. [길드 습격] 거석 낙하</summary>
        public static readonly Protocol RECEIVE_GA_ROCK_APPEAR                      = new Protocol("666", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>667. [길드 습격] 거석 사라짐</summary>
        public static readonly Protocol RECEIVE_GA_ROCK_DISAPPEAR                   = new Protocol("667", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>668. [길드 습격] 플레이어 부활</summary>
        public static readonly Protocol REQUEST_GA_USER_REVIVE                      = new Protocol("668", 0, ProtocolType.Request);
        /// <summary>669. [길드 습격] 플레이어 포션 사용</summary>
        public static readonly Protocol REQUEST_GA_USE_PORTION                      = new Protocol("669", 0, ProtocolType.Request);
        /// <summary>670. [길드 습격] 엠펠리움 기부</summary>
        public static readonly Protocol REQUEST_GA_EMPERIUM_CONTRIBUTION            = new Protocol("670", 0, ProtocolType.Request);
        /// <summary>671. [길드 습격] 엠펠리움 결정 수량 업데이트</summary>
        public static readonly Protocol RECEIVE_GA_EMPERIUM                         = new Protocol("671", 0, ProtocolType.ResponseOnly);
        /// <summary>672. [길드 습격] 시작 시간 변경</summary>
        public static readonly Protocol REQUEST_GA_CHANGE_TIME                      = new Protocol("672", 0, ProtocolType.Request);
        /// <summary>673. [길드 습격] 시작 시간 변경 업데이트</summary>
        public static readonly Protocol RECEIVE_GA_EMPERIUM_AND_START_TIME          = new Protocol("673", 0, ProtocolType.ResponseOnly);
        /// <summary>674. [길드 습격] 엠펠리움 생성</summary>
        public static readonly Protocol REQUEST_GA_MAKEEMPAL                        = new Protocol("674", 0, ProtocolType.Request);
        /// <summary>675. [길드 습격] 엠펠리움 생성 응답</summary>
        public static readonly Protocol RECEIVE_GA_MAKEEMPAL                        = new Protocol("675", 0, ProtocolType.ResponseOnly);
        /// <summary>676. [길드 습격] 특정 유저 버프 스킬 시전</summary>
        public static readonly Protocol REQUEST_GA_ACTIVEBUFSKILL                   = new Protocol("676", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>677. [길드 습격] 상태이상 대미지</summary>
        public static readonly Protocol RECEIVE_GA_MONDOTDAMAGE                     = new Protocol("677", 0, ProtocolType.ResponseOnly);
        /// <summary>705. [길드 미로] 공격</summary>
        public static readonly Protocol REQUEST_GVG_ATTACK                          = new Protocol("705", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>706. [길드 미로] 공격 모션 (공격자를 제외한 모두가 수신)</summary>
        public static readonly Protocol REQUEST_GVG_ATTACK_MOT                      = new Protocol("706", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>707. [길드 미로] 특정 유저 대미지 받음 (모든 유저가 수신)</summary>
        public static readonly Protocol RECEIVE_GVG_DAMAGE                          = new Protocol("707", 0, ProtocolType.ResponseOnly);
        /// <summary>708. [길드 미로] 특정 유저 사망 (모든 유저가 수신)</summary>
        public static readonly Protocol RECEIVE_GVG_DIE                             = new Protocol("708", 0, ProtocolType.ResponseOnly);
        /// <summary>709. [길드 미로] 특정 유저 리스폰 (모든 유저가 수신)</summary>
        public static readonly Protocol RECEIVE_GVG_RESPAWN                         = new Protocol("709", 0, ProtocolType.ResponseOnly);
        /// <summary>710. [길드 미로] 점령 시작</summary>
        public static readonly Protocol RECEIVE_GVG_ATTACKTOWER_START               = new Protocol("710", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>711. [길드 미로] 점령 중 타워 대미지 입음</summary>
        public static readonly Protocol RECEIVE_GVG_ATTACKTOWER_ING                 = new Protocol("711", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>712. [길드 미로] 점령 종료됨</summary>
        public static readonly Protocol RECEIVE_GVG_ATTACKTOWER_END                 = new Protocol("712", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>713. [길드 미로] 타워 파괴</summary>
        public static readonly Protocol RECEIVE_GVG_ATTACKTOWER_DESTROY             = new Protocol("713", 0, ProtocolType.ResponseOnly);
        /// <summary>714. [길드 미로] 플레이어 수면 시작</summary>
        public static readonly Protocol RECEIVE_GVG_SLEEP_START                     = new Protocol("714", 0, ProtocolType.ResponseOnly);
        /// <summary>715. [길드 미로] 플레이어 수면 종료</summary>
        public static readonly Protocol RECEIVE_GVG_SLEEP_END                       = new Protocol("715", 0, ProtocolType.ResponseOnly);
        /// <summary>716. [길드 미로] 플레이어 은신 시작</summary>
        public static readonly Protocol RECEIVE_GVG_INVISIBLE_START                 = new Protocol("716", 0, ProtocolType.ResponseOnly);
        /// <summary>717. [길드 미로] 플레이어 은신 종료</summary>
        public static readonly Protocol RECEIVE_GVG_INVISIBLE_END                   = new Protocol("717", 0, ProtocolType.ResponseOnly);
        /// <summary>718. [길드 미로] 거석 떨어지기 전 알림</summary>
        public static readonly Protocol RECEIVE_GVG_ROCK_APPEAR_READY               = new Protocol("718", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>719. [길드 미로] 거석 낙하</summary>
        public static readonly Protocol RECEIVE_GVG_ROCK_APPEAR                     = new Protocol("719", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>720. [길드 미로] 거석 사라짐/파괴</summary>
        public static readonly Protocol RECEIVE_GVG_ROCK_DISAPPEAR                  = new Protocol("720", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>721. [길드 미로] 아이템 생성</summary>
        public static readonly Protocol RECEIVE_GVG_ITEM_APPEAR                     = new Protocol("721", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>722. [길드 미로] 아이템 사라짐</summary>
        public static readonly Protocol RECEIVE_GVG_ITEM_DISAPPEAR                  = new Protocol("722", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>723. [길드 미로] 돌진, 넉백 이후 포지션 동기화</summary>
        public static readonly Protocol REQUEST_GVG_ROOM_TRANSFORMEX                = new Protocol("723", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>724. [길드 미로] 특정 유저 버프 스킬 시전</summary>
        public static readonly Protocol REQUEST_GVG_ACTIVEBUFSKILL                  = new Protocol("724", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>725. [길드 미로] 팀 전체 대미지 알림</summary>
        public static readonly Protocol RECEIVE_GVG_ITEM_ATTACK                     = new Protocol("725", 0, ProtocolType.ResponseOnly);
        /// <summary>726. [길드 미로] 특정 유저 상태이상</summary>
        public static readonly Protocol RECEIVE_GVG_GETCROWDCONTROL                 = new Protocol("726", 0, ProtocolType.ResponseOnly);
        /// <summary>727. [길드 미로] 특정 유저 대미지(도트딜)</summary>
        public static readonly Protocol RECEIVE_GVG_DOTDAMAGE                       = new Protocol("727", 0, ProtocolType.ResponseOnly);
        /// <summary>728. [길드 미로] 특정 유저 힐</summary>
        public static readonly Protocol RECEIVE_GVG_PLUSHP                          = new Protocol("728", 0, ProtocolType.ResponseOnly);

        /// <summary>751. [난전] 입장</summary>
        public static readonly Protocol REQUEST_FF_ROOM_JOIN                        = new Protocol("751", 0, ProtocolType.Request);
        /// <summary>752. [난전] 퇴장</summary>
        public static readonly Protocol REQUEST_FF_ROOM_EXIT                        = new Protocol("752", 0, ProtocolType.Request);
        /// <summary>753. [난전] 캐릭터 이동</summary>
        public static readonly Protocol REQUEST_FF_ROOM_TRANSFORM                   = new Protocol("753", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>754. [난전] 다른 유저 캐릭터 입장</summary>
        public static readonly Protocol RECEIVE_FF_ROOM_JOIN                        = new Protocol("754", 0, ProtocolType.ResponseOnly);
        /// <summary>755. [난전] 다른 유저 캐릭터 퇴장</summary>
        public static readonly Protocol RECEIVE_FF_ROOM_EXIT                        = new Protocol("755", 0, ProtocolType.ResponseOnly);
        /// <summary>756. [난전] 공격</summary>
        public static readonly Protocol REQUEST_FF_ATTACK                           = new Protocol("756", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>757. [난전] 공격 모션 (공격자를 제외한 모두가 수신)</summary>
        public static readonly Protocol REQUEST_FF_ATTACK_MOT                       = new Protocol("757", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>758. [난전] 특정 유저 대미지 받음 (모든 유저가 수신)</summary>
        public static readonly Protocol RECEIVE_FF_DAMAGE                           = new Protocol("758", 0, ProtocolType.ResponseOnly);
        /// <summary>759. [난전] 특정 유저 사망 (모든 유저가 수신)</summary>
        public static readonly Protocol RECEIVE_FF_DIE                              = new Protocol("759", 0, ProtocolType.ResponseOnly);
        /// <summary>760. [난전] 특정 유저 등장 (모든 유저가 수신)</summary>
        public static readonly Protocol RECEIVE_FF_USER_APPEAR                      = new Protocol("760", 0, ProtocolType.ResponseOnly);
        /// <summary>761. [난전] 등장</summary>
        public static readonly Protocol REQUEST_FF_USER_APPEAR                      = new Protocol("761", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>762. [난전] 대기 시작</summary>
        public static readonly Protocol RECEIVE_FF_NODAMAGE_START                   = new Protocol("762", 0, ProtocolType.ResponseOnly);
        /// <summary>763. [난전] 대기 종료</summary>
        public static readonly Protocol RECEIVE_FF_NODAMAGE_END                     = new Protocol("763", 0, ProtocolType.ResponseOnly);
        /// <summary>764. [난전] 특정 유저 무적 시작</summary>
        public static readonly Protocol RECEIVE_FF_POWERUP_START                    = new Protocol("764", 0, ProtocolType.ResponseOnly);
        /// <summary>765. [난전] 특정 유저 무적 종료</summary>
        public static readonly Protocol RECEIVE_FF_POWERUP_END                      = new Protocol("765", 0, ProtocolType.ResponseOnly);
        /// <summary>766. [난전] 거석 떨어지기 전 알림</summary>
        public static readonly Protocol RECEIVE_FF_ROCK_APPEAR_READY                = new Protocol("766", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>767. [난전] 거석 낙하</summary>
        public static readonly Protocol RECEIVE_FF_ROCK_APPEAR                      = new Protocol("767", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>768. [난전] 거석 사라짐</summary>
        public static readonly Protocol RECEIVE_FF_ROCK_DISAPPEAR                   = new Protocol("768", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>769. [난전] 아이템 등장 전 알림</summary>
        public static readonly Protocol RECEIVE_FF_ITEM_APPEAR_READY                = new Protocol("769", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>770. [난전] 아이템 등장</summary>
        public static readonly Protocol RECEIVE_FF_ITEM_APPEAR                      = new Protocol("770", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>771. [난전] 아이템 사라짐</summary>
        public static readonly Protocol RECEIVE_FF_ITEM_DISAPPEAR                   = new Protocol("771", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>772. [난전] 돌진, 넉백 이후 포지션 동기화</summary>
        public static readonly Protocol REQUEST_FF_ROOM_TRANSFORMEX                 = new Protocol("772", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>773. [난전] 특정 유저 버프 스킬 시전</summary>
        public static readonly Protocol REQUEST_FF_ACTIVEBUFSKILL                   = new Protocol("773", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>774. [난전] 특정 유저 상태이상</summary>
        public static readonly Protocol RECEIVE_FF_GETCROWDCONTROL                  = new Protocol("774", 0, ProtocolType.ResponseOnly);
        /// <summary>775. [난전] 특정 유저 대미지(도트딜)</summary>
        public static readonly Protocol RECEIVE_FF_DOTDAMAGE                        = new Protocol("775", 0, ProtocolType.ResponseOnly);
        /// <summary>776. [난전] 특정 유저 힐</summary>
        public static readonly Protocol RECEIVE_FF_PLUSHP                           = new Protocol("776", 0, ProtocolType.ResponseOnly);
        /// <summary>777. [난전] 라운드 알림</summary>
        public static readonly Protocol RECEIVE_FF_NOTICEROUNDINFO                  = new Protocol("777", 0, ProtocolType.ResponseOnly);
        /// <summary>778. [난전] 채팅</summary>
        public static readonly Protocol REQUEST_FF_FREEFIGHTCHAT                    = new Protocol("778", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>779. [난전] 정보</summary>
        public static readonly Protocol REQUEST_FF_NOTICETIME                       = new Protocol("779", 0, ProtocolType.Request);

        /// <summary>780. [이벤트 난전] 입장</summary>
        public static readonly Protocol REQUEST_FF_EVENTROOM_JOIN                   = new Protocol("780", 0, ProtocolType.Request);
        /// <summary>781. [이벤트 난전] 정보</summary>
        public static readonly Protocol REQUEST_FF_EVENTNOTICETIME                  = new Protocol("781", 0, ProtocolType.Request);
        /// <summary>782. [이벤트 난전] 다른 유저 캐릭터 입장</summary>
        public static readonly Protocol RECEIVE_FF_EVENTROOM_JOIN                   = new Protocol("782", 0, ProtocolType.ResponseOnly);
        /// <summary>783. [이벤트 난전] 빙결 시작</summary>
        public static readonly Protocol RECEIVE_FF_FREEZE                           = new Protocol("783", 0, ProtocolType.ResponseOnly);
        /// <summary>784. [이벤트 난전] 빙결 종료</summary>
        public static readonly Protocol RECEIVE_FF_FREEZE_END                       = new Protocol("784", 0, ProtocolType.ResponseOnly);
        /// <summary>784. [이벤트 난전] 실드 시작</summary>
        public static readonly Protocol RECEIVE_FF_SHIELD                           = new Protocol("785", 0, ProtocolType.ResponseOnly);
        /// <summary>784. [이벤트 난전] 실드 종료</summary>
        public static readonly Protocol RECEIVE_FF_SHIELD_END                       = new Protocol("786", 0, ProtocolType.ResponseOnly);
        /// <summary>784. [이벤트 난전] 퇴장</summary>
        public static readonly Protocol REQUEST_FF_EVENTROOM_EXIT                   = new Protocol("787", 0, ProtocolType.Request);

        /// <summary>801. [멀티 미로] 멀티 미로 입장</summary>
        public static readonly Protocol REQUEST_MULMAZE_ROOM_JOIN                   = new Protocol("801", 0, ProtocolType.Request);
        /// <summary>802. [멀티 미로] 멀티 미로 퇴장</summary>
        public static readonly Protocol REQUEST_MULMAZE_ROOM_EXIT                   = new Protocol("802", 0, ProtocolType.Request);
        /// <summary>803. [멀티 미로] 멀티 미로 움직임</summary>
        public static readonly Protocol REQUEST_MULMAZE_ROOM_TRANSFORM              = new Protocol("803", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>804. [멀티 미로] 멀티 미로 입장 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_ROOM_JOIN                   = new Protocol("804", 0, ProtocolType.ResponseOnly);
        /// <summary>805. [멀티 미로] 멀티 미로 퇴장 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_ROOM_EXIT                   = new Protocol("805", 0, ProtocolType.ResponseOnly);
        /// <summary>806. [멀티 미로] 멀티 미로 몬스터 움직임 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_ROOM_MONMOVE                = new Protocol("806", 0, ProtocolType.ResponseOnly, isLog: false);
        /// <summary>807. [멀티 미로] 멀티 미로 일반 몬스터 충돌 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_NOMALMON_CRASH              = new Protocol("807", 0, ProtocolType.ResponseOnly);
        /// <summary>808. [멀티 미로] 멀티 미로 몬스터 리스폰 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_MON_REGEN                   = new Protocol("808", 0, ProtocolType.ResponseOnly);
        /// <summary>809. [멀티 미로] 멀티 미로 보스 전투 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_BOSSBATTLE_START            = new Protocol("809", 0, ProtocolType.ResponseOnly);
        /// <summary>810. [멀티 미로] 멀티 미로 보스 전투 결과</summary>
        public static readonly Protocol REQUEST_MULMAZE_BOSSBATTLE_END              = new Protocol("810", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>811. [멀티 미로] 멀티 미로 퀘스트코인 획득</summary>
        public static readonly Protocol REQUEST_MULMAZE_GET_QUESTCOIN               = new Protocol("811", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>812. [멀티 미로] 멀티 미로 미로조각 획득 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_GET_QUESTCOIN               = new Protocol("812", 0, ProtocolType.ResponseOnly);
        /// <summary>813. [멀티 미로] 멀티 미로 미로조각 리스폰 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_QUESTCOIN_REGEN             = new Protocol("813", 0, ProtocolType.ResponseOnly);
        /// <summary>814. [멀티 미로] 멀티 미로 제니 획득</summary>
        public static readonly Protocol REQUEST_MULMAZE_GET_ZENY                    = new Protocol("814", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>815. [멀티 미로] 멀티 미로 아이템 획득</summary>
        public static readonly Protocol REQUEST_MULMAZE_GET_ITEM                    = new Protocol("815", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>816. [멀티 미로] 멀티 미로 아이템 획득 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_GET_ITEM                    = new Protocol("816", 0, ProtocolType.ResponseOnly);
        /// <summary>817. [멀티 미로] 멀티 미로 대기방 입장</summary>
        public static readonly Protocol REQUEST_MULMAZE_WAITINGROOM_JOIN            = new Protocol("817", 0, ProtocolType.Request);
        /// <summary>818. [멀티 미로] 멀티 미로 대기방 퇴장</summary>
        public static readonly Protocol REQUEST_MULMAZE_WAITINGROOM_EXIT            = new Protocol("818", 0, ProtocolType.Request);
        /// <summary>819. [멀티 미로] 멀티 미로 대기방 움직임</summary>
        public static readonly Protocol REQUEST_MULMAZE_WAITINGROOM_TRANSFORM       = new Protocol("819", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>820. [멀티 미로] 멀티 미로 대기방 입장 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_WAITINGROOM_JOIN            = new Protocol("820", 0, ProtocolType.ResponseOnly);
        /// <summary>821. [멀티 미로] 멀티 미로 대기방 퇴장 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_WAITINGROOM_EXIT            = new Protocol("821", 0, ProtocolType.ResponseOnly);
        /// <summary>822. [멀티 미로] 플레이어 죽음 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_USERDIE                     = new Protocol("822", 0, ProtocolType.ResponseOnly);
        /// <summary>823. [멀티 미로] 행동 불능의 플레이어 터치</summary>
        public static readonly Protocol REQUEST_MULMAZE_FREEZETAG                   = new Protocol("823", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>824. [멀티 미로] 행동 불능 상태 빨리 벗어나기 위한 탭핑</summary>
        public static readonly Protocol REQUEST_MULMAZE_TABFREEZE                   = new Protocol("824", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>825. [멀티 미로] 행동 불능 종료</summary>
        public static readonly Protocol RECEIVE_MULMAZE_FREEZEEND                   = new Protocol("825", 0, ProtocolType.ResponseOnly);
        /// <summary>826. [멀티 미로] 행동 불능 시작</summary>
        public static readonly Protocol RECEIVE_MULMAZE_FREEZESTART                 = new Protocol("826", 0, ProtocolType.ResponseOnly);
        /// <summary>827. [멀티 미로] 멀티 미로 대기방 입장시 서버변경</summary>
        public static readonly Protocol RECEIVE_MULMAZE_WAITINGROOM_SERVERCHANGE    = new Protocol("827", 0, ProtocolType.ResponseOnly);
        /// <summary>828. [멀티 미로] 멀티 미로 대기방 퇴장시 서버변경</summary>
        public static readonly Protocol RECEIVE_BATTLEFIELD_SERVERCHANGE            = new Protocol("828", 0, ProtocolType.ResponseOnly);
        /// <summary>829. [매칭-멀티미로] 아이템 획득</summary>
        public static readonly Protocol REQUEST_MULMAZE_TOUCH_ITEM                  = new Protocol("829", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>830. [매칭-멀티미로] 아이템 획득 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_TOUCH_ITEM                  = new Protocol("830", 0, ProtocolType.ResponseOnly);
        /// <summary>831. [매칭-멀티미로] 아이템 리젠 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MULMAZE_ITEM_REGEN                  = new Protocol("831", 0, ProtocolType.ResponseOnly);
        /// <summary>832. [매칭-멀티미로] 매칭-멀티미로 보스 전투</summary>
        public static readonly Protocol REQUEST_MATCHMULMAZE_BOSSBATTLE_START       = new Protocol("832", 0, ProtocolType.SendOnly);
        /// <summary>833. [매칭-멀티미로] 매칭-멀티미로 보스 전투 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_MATCHMULMAZE_BOSSBATTLE_START       = new Protocol("833", 0, ProtocolType.ResponseOnly);
        /// <summary>834. [매칭-멀티미로] 매칭-멀티미로 매칭 시작</summary>
        public static readonly Protocol REQUEST_MULMAZE_MATCH_START                 = new Protocol("834", 0, ProtocolType.Request);
        /// <summary>835. [매칭-멀티미로] 매칭-멀티미로 매칭 취소</summary>
        public static readonly Protocol REQUEST_MULMAZE_MATCH_CANCEL                = new Protocol("835", 0, ProtocolType.Request);
        /// <summary>836. [매칭-멀티미로] 매칭-멀티미로 입장</summary>
        public static readonly Protocol RECEIVE_MATCHMULMAZE_ROOM_JOIN              = new Protocol("836", 0, ProtocolType.ResponseOnly);
        /// <summary>837. [멀티미로] 참여보상 처리</summary>
        public static readonly Protocol RECEIVE_MULMAZE_BASEREWARD                  = new Protocol("837", 0, ProtocolType.ResponseOnly);
        /// <summary>838. [멀티미로] 매칭된 유저 인원</summary>
        public static readonly Protocol RECEIVE_MATMULMAZE_WAITINGUSERNUM           = new Protocol("838", 0, ProtocolType.ResponseOnly);
        /// <summary>839. [멀티미로] 모든 유저 채팅</summary>
        public static readonly Protocol REQUEST_MATMULMAZE_ALLUSERCHAT              = new Protocol("839", 0, ProtocolType.Request);
        /// <summary>840. [멀티미로] 멀티미로 매칭 시 확성기</summary>
        public static readonly Protocol RECEIVE_MATMULMAZE_WAITINGMESSAGE           = new Protocol("840", 0, ProtocolType.ResponseOnly);

        /// <summary>841. [이벤트 멀티미로] 시작</summary>
        public static readonly Protocol REQUEST_EVENTMULMAZE_MATCH_START            = new Protocol("841", 0, ProtocolType.Request);
        /// <summary>842. [이벤트 멀티미로] 미니루돌프 리젠</summary>
        public static readonly Protocol RECEIVE_MULMAZE_EVENT1ITEM_REGEN            = new Protocol("842", 0, ProtocolType.ResponseOnly);
        /// <summary>843. [이벤트 멀티미로] 강탈물약 리젠</summary>
        public static readonly Protocol RECEIVE_MULMAZE_EVENT2ITEM_REGEN            = new Protocol("843", 0, ProtocolType.ResponseOnly);
        /// <summary>844. [이벤트 멀티미로] 미니루돌프 획득</summary>
        public static readonly Protocol REQUEST_MULMAZE_GET_EVENT1ITEM              = new Protocol("844", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>845. [이벤트 멀티미로] 강탈물약 획득</summary>
        public static readonly Protocol REQUEST_MULMAZE_GET_EVENT2ITEM              = new Protocol("845", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>846. [이벤트 멀티미로] 미니루돌프 획득 응답</summary>
        public static readonly Protocol RECEIVE_MULMAZE_GET_EVENT1ITEM              = new Protocol("846", 0, ProtocolType.ResponseOnly);
        /// <summary>847. [이벤트 멀티미로] 강탈자 획득 응답</summary>
        public static readonly Protocol RECEIVE_MULMAZE_GET_EVENT2ITEM              = new Protocol("847", 0, ProtocolType.ResponseOnly);
        /// <summary>848. [이벤트 멀티미로] 몬스터 죽음</summary>
        public static readonly Protocol RECEIVE_MULMAZE_MONKILL                     = new Protocol("848", 0, ProtocolType.ResponseOnly);
        /// <summary>849. [이벤트 멀티미로] 강탈모드 종료</summary>
        public static readonly Protocol RECEIVE_POWERUP_END                         = new Protocol("849", 0, ProtocolType.ResponseOnly);
        /// <summary>850. [이벤트 멀티미로] 강탈</summary>
        public static readonly Protocol REQUEST_MULMAZE_TAKE_EVENTITEM              = new Protocol("850", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>851. [이벤트 멀티미로] 강탈 응답</summary>
        public static readonly Protocol RECEIVE_MULMAZE_TAKE_EVENTITEM              = new Protocol("851", 0, ProtocolType.ResponseOnly);
        /// <summary>852. [이벤트 멀티미로] 종료 보상</summary>
        public static readonly Protocol RECEIVE_MULMAZE_REWARD                      = new Protocol("852", 0, ProtocolType.ResponseOnly);

        /// <summary>853. [엔들리스타워] 엔들리스타워 시작</summary>
        public static readonly Protocol ENDLESS_DUNGEON_START                       = new Protocol("853", 0, ProtocolType.Request);
        /// <summary>854. [엔들리스타워] 엔들리스타워 클리어</summary>
        public static readonly Protocol ENDLESS_DUNGEON_STORYCLEAR                  = new Protocol("854", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>855. [엔들리스타워] 엔들리스타워 종료</summary>
        public static readonly Protocol ENDLESS_DUNGEON_END                         = new Protocol("855", 0, ProtocolType.SendOnly);

        /// <summary>856. [미궁숲] 입장</summary>
        public static readonly Protocol REQUEST_FOREST_ROOM_JOIN                    = new Protocol("856", 0, ProtocolType.Request);
        /// <summary>857. [미궁숲] 입장 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_FOREST_ROOM_JOIN                    = new Protocol("857", 0, ProtocolType.ResponseOnly);
        /// <summary>858. [미궁숲] 퇴장</summary>
        public static readonly Protocol REQUEST_FOREST_ROOM_EXIT                    = new Protocol("858", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>859. [미궁숲] 퇴장 (응답 전용)</summary>
        public static readonly Protocol RECIEVE_FOREST_ROOM_EXIT                    = new Protocol("859", 0, ProtocolType.ResponseOnly);
        /// <summary>860. [미궁숲] 움직임</summary>
        public static readonly Protocol REQUEST_FOREST_ROOM_TRANSFORM               = new Protocol("860", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>861. [미궁숲] 엠펠리움 획득</summary>
        public static readonly Protocol REQUEST_FOREST_GET_EMPAL                    = new Protocol("861", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>862. [미궁숲] 엠펠리움 획득 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_FOREST_GET_EMPAL                    = new Protocol("862", 0, ProtocolType.ResponseOnly);
        /// <summary>863. [미궁숲] 아이템 획득</summary>
        public static readonly Protocol REQUEST_FOREST_GET_ITEM                     = new Protocol("863", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>864. [미궁숲] 아이템 획득 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_FOREST_GET_ITEM                     = new Protocol("864", 0, ProtocolType.ResponseOnly);
        /// <summary>865. [미궁숲] 중간보스 전투</summary>
        public static readonly Protocol RECEIVE_FOREST_MINIBOSSBATTLE_START         = new Protocol("865", 0, ProtocolType.ResponseOnly);
        /// <summary>866. [미궁숲] 최종보스 전투</summary>
        public static readonly Protocol RECEIVE_FOREST_BOSSBATTLE_START             = new Protocol("866", 0, ProtocolType.ResponseOnly);
        /// <summary>867. [미궁숲] 중간보스 전투 종료</summary>
        public static readonly Protocol REQUEST_FOREST_MINIBOSSBATTLE_END           = new Protocol("867", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>868. [미궁숲] 최종보스 전투 종료</summary>
        public static readonly Protocol REQUEST_FOREST_BOSSBATTLE_END               = new Protocol("868", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>869. [미궁숲] 최종보스 전투 패배처리 퇴장 (응답 전용)</summary>
        [System.Obsolete]
        public static readonly Protocol RECEIVE_FOREST_BOSSBATTLE_LOSE              = new Protocol("869", 0, ProtocolType.ResponseOnly);
        /// <summary>870. [미궁숲] 유저 죽음 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_FOREST_USERDIE                      = new Protocol("870", 0, ProtocolType.ResponseOnly);
        /// <summary>871. [미궁숲] 일반 몬스터 충돌</summary>
        public static readonly Protocol RECEIVE_FOREST_NOMALMON_CRASH               = new Protocol("871", 0, ProtocolType.ResponseOnly);
        /// <summary>872. [미궁숲] 층 올라가기</summary>
        public static readonly Protocol REQUEST_FOREST_UPSTORY                      = new Protocol("872", 0, ProtocolType.Request);
        /// <summary>873. [미궁숲] 층 내려가기</summary>
        public static readonly Protocol REQUEST_FOREST_DOWNSTORY                    = new Protocol("873", 0, ProtocolType.Request);
        /// <summary>874. [미궁숲] 아이템 획득</summary>
        public static readonly Protocol REQUEST_FOREST_ITEMSELECT                   = new Protocol("874", 0, ProtocolType.Request);
        /// <summary>875. [미궁숲] 엠펠리움 리젠</summary>
        public static readonly Protocol RECEIVE_FOREST_EMPAL_REGEN                  = new Protocol("875", 0, ProtocolType.ResponseOnly);
        /// <summary>876. [미궁숲] 포션 리젠</summary>
        public static readonly Protocol RECEIVE_FOREST_ITEM_REGEN                   = new Protocol("876", 0, ProtocolType.ResponseOnly);
        /// <summary>877. [미궁숲] 중간보스 전투 승리</summary>
        public static readonly Protocol RECEIVE_FOREST_MINIBOSSBATTLE_WIN           = new Protocol("877", 0, ProtocolType.ResponseOnly);

        /// <summary>878. [이벤트미궁:암흑] 시작</summary>
        public static readonly Protocol REQUEST_DARKMAZE_MATCH_START                = new Protocol("878", 0, ProtocolType.Request);
        /// <summary>879. [이벤트미궁:암흑] 보상</summary>
        public static readonly Protocol RECEIVE_DARKMAZE_REWARD                     = new Protocol("879", 0, ProtocolType.ResponseOnly);

        /// <summary>880. [게이트] 파티생성</summary>
        public static readonly Protocol REQUEST_GATE_MAKEROOM                       = new Protocol("880", 0, ProtocolType.Request);
        /// <summary>881. [게이트] 파티목록 호출</summary>
        public static readonly Protocol REQUEST_GATE_GETALLROOMLIST                 = new Protocol("881", 0, ProtocolType.Request);
        /// <summary>882. [게이트] 내 파티 정보</summary>
        public static readonly Protocol RECEIVE_GATE_GETMYROOMLIST                  = new Protocol("882", 0, ProtocolType.ResponseOnly);
        /// <summary>883. [게이트] 파티 가입</summary>
        public static readonly Protocol REQUEST_GATE_JOINROOM                       = new Protocol("883", 0, ProtocolType.Request);
        /// <summary>884. [게이트] 파티원 강퇴</summary>
        public static readonly Protocol REQUEST_GATE_BANUSER                        = new Protocol("884", 0, ProtocolType.Request);
        /// <summary>885. [게이트] 파티 나가기</summary>
        public static readonly Protocol REQUEST_GATE_USEROUT                        = new Protocol("885", 0, ProtocolType.SendOnly);
        /// <summary>886. [게이트] 게임 시작</summary>
        public static readonly Protocol REQUEST_GATE_GAMESTART                      = new Protocol("886", 0, ProtocolType.SendOnly);
        /// <summary>887. [게이트] 이동</summary>
        public static readonly Protocol REQUEST_GATE_ROOM_TRANSFORM                 = new Protocol("887", 0, ProtocolType.SendOnly, isIndicator: false, isLog: false);
        /// <summary>888. [게이트] 중간보스 전투 시작</summary>
        public static readonly Protocol RECEIVE_GATE_MINIBOSSBATTLE_START           = new Protocol("888", 0, ProtocolType.ResponseOnly);
        /// <summary>889. [게이트] 중간보스 전투 종료</summary>
        public static readonly Protocol REQUEST_GATE_MINIBOSSBATTLE_END             = new Protocol("889", 0, ProtocolType.Request);
        /// <summary>890. [게이트] 중간보스 전투 승리</summary>
        public static readonly Protocol RECEIVE_GATE_MINIBOSSBATTLE_WIN             = new Protocol("890", 0, ProtocolType.ResponseOnly);
        /// <summary>891. [게이트] 월드보스 전투 시작</summary>
        public static readonly Protocol RECEIVE_GATE_BOSSBATTLE_START               = new Protocol("891", 0, ProtocolType.ResponseOnly);
        /// <summary>892. [게이트] 월드보스 공격</summary>
        public static readonly Protocol REQUEST_GATE_BOSSBATTLE_ATTACK              = new Protocol("892", 0, ProtocolType.Request, isIndicator: false, isLog: false);
        /// <summary>893. [게이트] 월드보스 공격 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_GATE_BOSSBATTLE_ATTACK              = new Protocol("893", 0, ProtocolType.ResponseOnly);
        /// <summary>894. [게이트] 월드보스 도중 죽음</summary>
        public static readonly Protocol RECEIVE_GATE_USERDIE                        = new Protocol("894", 0, ProtocolType.SendOnly);
        /// <summary>895. [게이트] 게임 나가기</summary>
        public static readonly Protocol REQUEST_GATE_ROOM_EXIT                      = new Protocol("895", 0, ProtocolType.Request);
        /// <summary>896. [게이트] 게임 나가기 (응답 전용)</summary>
        public static readonly Protocol RECIEVE_GATE_ROOM_EXIT                      = new Protocol("896", 0, ProtocolType.ResponseOnly);
        /// <summary>897. [게이트] 중간보스 전투 패배 (응답 전용)</summary>
        public static readonly Protocol RECEIVE_GATE_MINIBOSSBATTLE_LOSE            = new Protocol("897", 0, ProtocolType.ResponseOnly);

        /// <summary>1000. </summary>
        public static readonly Protocol PUBLIC_MESSAGE                              = new Protocol("1000", 0, ProtocolType.ResponseOnly);
        /// <summary>1001. 유저 일일 정산 데이터</summary>
        public static readonly Protocol RESULT_USER_DAILY_CALC                      = new Protocol("1001", 0, ProtocolType.ResponseOnly);
        /// <summary>1002. 캐릭터 일일 정산 데이터</summary>
        public static readonly Protocol RESULT_CHAR_DAILY_CALC                      = new Protocol("1002", 0, ProtocolType.ResponseOnly);

        /// <summary>2000. [스테이지] 몬스터 정보 브로드 캐스팅</summary>
        public static readonly Protocol RECEIVE_MONSTER_SENDDATA                    = new Protocol("2000", 0, ProtocolType.ResponseOnly);
        /// <summary>2001. [스테이지] 몬스터에 대한 공격 </summary>
        public static readonly Protocol REQUEST_ATTACK_FIELD_MONSTER                = new Protocol("2001", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>2003. [스테이지] 몬스터에 대한 공격 </summary>
        public static readonly Protocol REQUEST_ATTACK_CASTING_FIELD_MONSTER        = new Protocol("2003", 0, ProtocolType.Request, isIndicator: false);
        /// <summary>2004. [스테이지] 플레이어 소환</summary>
        public static readonly Protocol REQUEST_SUMMON_FIELD_PLAYER                 = new Protocol("2004", 0, ProtocolType.Request);
        /// <summary>2005. [스테이지] CUD 새로고침</summary>
        public static readonly Protocol RECEIVE_REFRESH_CUD                         = new Protocol("2005", 0, ProtocolType.ResponseOnly);
        /// <summary>2006. 모든 서버 정보 목록</summary>
        public static readonly Protocol REQUEST_ALL_SERVER_INFO                     = new Protocol("2006", 0, ProtocolType.Request);

        /// <summary>2007. CBT쿠폰 보상</summary>
        public static readonly Protocol REQUEST_GETCBTCOUPONITEM                    = new Protocol("2007", 0, ProtocolType.Request);

        /// <summary>9999. 일시정지 시 서버로 날리는 빈 패킷 (단순 끊어짐 방지 용)</summary>
        public static readonly Protocol REQUEST_PAUSE                               = new Protocol("9999", 0, ProtocolType.SendOnly, isIndicator: false);
        /// <summary>1000. 게임탈퇴 요청</summary>
        public static readonly Protocol REQUEST_ACCOUNT_WITHDRAWAL                  = new Protocol("10000", 0, ProtocolType.Request);
        /// <summary>1000. 게임탈퇴 철회 요청</summary>
        public static readonly Protocol REQUEST_ACCOUNT_WITHDRAWAL_CANCEL           = new Protocol("10001", 0, ProtocolType.Request);
    }
}