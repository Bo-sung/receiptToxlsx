using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public sealed class ResultCode : EnumBaseType<ResultCode, int, int>
    {
        private string serverMessage;

        public ResultCode(int key, int localKey) : base(key, localKey)
        {
            serverMessage = string.Empty;
        }

        public bool Execute()
        {
            // 타이틀로 보내기
            if (IsGoToTitleError())
            {
                ConnectionManager.GoToTitle(GetDescription());
                return true;
            }

            // 인증서버 로그인 실패
            if (this == LOGIN_AUTH_FAIL)
            {
                ObscuredPrefs.DeleteKey(Config.SELECT_SERVER);
                ObscuredPrefs.Save();
            }

            // 게임 종료 에러
            if (IsQuitError())
            {
                ConnectionManager.Quit(GetDescription());
                return true;
            }

            // 비밀 번호가 잘못됨
            if (this == BAD_PASS_ACCOUNT)
            {
                ObscuredPrefs.DeleteKey(Config.ACCOUNT_KEY);
                ObscuredPrefs.DeleteKey(Config.PASSWORD);
                ObscuredPrefs.Save();

                ConnectionManager.Reconnect(); // 재접속 시도
                return true;
            }

            // 캐릭터 정보 저장 중
            if (this == WAIT_RETRY_LOGIN)
            {
                ConnectionManager.Reconnect(1f); // 재접속 시도 (1초 대기)
                return true;
            }

            // 온버프 내 포인트 조회(동기화)
            if (this == ONBUFF_QUANTITY_SYNC_ERROR)
            {
                Entity.player.Goods.RequestOnBuffMyPoint().WrapNetworkErrors();
                UI.ShowToastPopup(GetDescription());
                return true;
            }

            return false;
        }

        private bool IsQuitError()
        {
            return (this == APPVERSION_FAIL // 새로운 버전이 업데이트 되었습니다.
                || this == LOGIN_BAD_USERNAME // 유저 정보가 존재하지 않습니다.
                || this == LOGIN_AUTH_FAIL // 로그인 인증에 실패하였습니다.
                || this == SERVER_IS_BUSY // 서버 점검 중입니다.
                || this == USER_IS_BANNED // 차단 중인 유저입니다.
                || this == GUEST_LOGIN_FAIL // 게스트 로그인 실패\n잠시 후에 이용해 주십시오.
                || this == CREATE_ACCOUNT_IMPOSSIBLE // 계정을 생성할 수 없습니다.
                || this == DUPLICATION_LOGIN // 다른 디바이스에서 로그인 시도를 하였습니다.
                || this == USER_KICK // 유저 추방
            );
        }

        private bool IsGoToTitleError()
        {
            return this == DISCONNECT // 서버 연결이 끊어졌습니다.\n타이틀로 돌아갑니다. 
                || this == RETRY_AUTH // 변경된 내용이 존재합니다.\n인증 서버로 재접속 합니다.
                || this == BAD_REQUEST; // 잘못된 요청입니다. 재시작하시기 바랍니다.
        }

        /// <summary>
        /// 에러 코드에 해당하는 메시지 보여주기
        /// </summary>
        public void ShowResultCode()
        {
            UI.ConfirmPopup(GetDescription());
        }

        /// <summary>
        /// 서버에서 보내준 메세지 내용 세팅
        /// </summary>
        public void SetSeverMessage(string serverMessage)
        {
            this.serverMessage = serverMessage;
        }

        public string GetDescription()
        {
            // 서버에서 보내준 메시지가 있을 경우
            if (!string.IsNullOrEmpty(serverMessage))
                return serverMessage;

            return Value.ToText();
        }

        public static ResultCode GetByKey(int key)
        {
            return GetBaseByKey(key) ?? UNKNOWN;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is ResultCode value)
                return Equals(value);

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(ResultCode value)
        {
            if (value == null)
                return false;

            return Value == value.Value;
        }

        public static bool operator ==(ResultCode a, ResultCode b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ResultCode a, ResultCode b)
        {
            return !(a == b);
        }

#if UNITY_EDITOR
        private static System.Collections.Generic.Dictionary<int, string> fieldDic;
        public override string ToString()
        {
            if (fieldDic == null)
            {
                fieldDic = new System.Collections.Generic.Dictionary<int, string>(IntEqualityComparer.Default);

                foreach (var field in GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    ResultCode property = field.GetValue(null) as ResultCode;
                    fieldDic.Add(property.Key, field.Name);
                }
            }

            return $"{Key}:{fieldDic[Key]}";
        }
#endif
        /// <summary>
        /// -3. RETRY
        /// <para>요청에 실패하였습니다.\n해당 작업을 다시 요청합니다.</para>
        /// </summary>
        public static readonly ResultCode RETRY = new ResultCode(-3, LocalizeKey._523);

        /// <summary>
        /// -2. DISCONNECT
        /// <para>서버 연결이 끊어졌습니다.\n타이틀로 돌아갑니다.</para>
        /// </summary>
        public static readonly ResultCode DISCONNECT = new ResultCode(-2, LocalizeKey._10);

        /// <summary>
        /// -1. UNKNOWN
        /// <para>알 수 없는 에러입니다.</para>
        /// </summary>
        public static readonly ResultCode UNKNOWN = new ResultCode(-1, LocalizeKey._399);

        /// <summary>
        /// 0. FAIL
        /// <para>해당 작업을 요청할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode FAIL = new ResultCode(0, LocalizeKey._400);
        /// <summary>
        /// 1. SUCCESS
        /// <para>해당 작업을 완료하였습니다.</para>
        /// </summary>
        public static readonly ResultCode SUCCESS = new ResultCode(1, LocalizeKey._401);
        /// <summary>
        /// 2. duplication fail
        /// <para>이미 사용중인 이름입니다.</para>
        /// </summary>
        public static readonly ResultCode ALREADY_EXISTS = new ResultCode(2, LocalizeKey._402);
        /// <summary>
        /// 3. APP Version fail
        /// <para>새로운 버전이 업데이트 되었습니다.</para>
        /// </summary>
        public static readonly ResultCode APPVERSION_FAIL = new ResultCode(3, LocalizeKey._403);
        /// <summary>
        /// 4. Login Bad UserName
        /// <para>유저 정보가 존재하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode LOGIN_BAD_USERNAME = new ResultCode(4, LocalizeKey._404);
        /// <summary>
        /// 5. Login Auth fail
        /// <para>로그인 인증에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode LOGIN_AUTH_FAIL = new ResultCode(5, LocalizeKey._405);
        /// <summary>
        /// 6. 인증서버로 재접속
        /// <para>변경된 내용이 존재합니다.\n인증 서버로 재접속 합니다.</para>
        /// </summary>
        public static readonly ResultCode RETRY_AUTH = new ResultCode(6, LocalizeKey._406);
        /// <summary>
        /// 7. 서버가 준비중입니다????
        /// <para>서버 점검 중입니다.</para>
        /// </summary>
        public static readonly ResultCode SERVER_IS_BUSY = new ResultCode(7, LocalizeKey._407);
        /// <summary>
        /// 8. 중복로그인된 계정입니다.
        /// <para>다른 디바이스에서 로그인 시도를 하였습니다.</para>
        /// </summary>
        public static readonly ResultCode DUPLICATION_LOGIN = new ResultCode(8, LocalizeKey._408);
        /// <summary>
        /// 9. User is Banned
        /// <para>차단 중인 유저입니다.</para>
        /// </summary>
        public static readonly ResultCode USER_IS_BANNED = new ResultCode(9, LocalizeKey._409);
        /// <summary>
        /// 10. 게스트 로그인 실패
        /// <para>게스트 로그인 실패\n잠시 후에 이용해 주십시오.</para>
        /// </summary>
        public static readonly ResultCode GUEST_LOGIN_FAIL = new ResultCode(10, LocalizeKey._410);
        /// <summary>
        /// 11. 제한 횟수 도달
        /// <para>더 이상 진행할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode REACHED_LIMIT = new ResultCode(11, LocalizeKey._411);
        /// <summary>
        /// 12. 제니가 부족하다
        /// <para>제니가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_ZENY = new ResultCode(12, LocalizeKey._412);
        /// <summary>
        /// 13. 냥다래 열매가 부족하다.
        /// <para>냥다래 열매가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_CAT_COIN = new ResultCode(13, LocalizeKey._413);
        /// <summary>
        /// 14. 스킬 포인트가 부족하다
        /// <para>스킬 포인트가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_SKILL_POINT = new ResultCode(14, LocalizeKey._414);
        /// <summary>
        /// 15. 스텟 포인트가 부족하다
        /// <para>스텟 포인트가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_STAT_POINT = new ResultCode(15, LocalizeKey._415);
        /// <summary>
        /// 16. 필드 던전 티켓이 부족하다
        /// <para>필드 던전 티켓이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_FIELD_DUNGEON_TICKET = new ResultCode(16, LocalizeKey._416);
        /// <summary>
        /// 17. 특수 던전 티켓이 부족하다
        /// <para>특수 던전 티켓이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_SP_DUNGEON_TICKET = new ResultCode(17, LocalizeKey._417);
        /// <summary>
        /// 18. IOS_SAND_BOX_RECEIPT
        /// <para>유효하지 않은 iOS 영수증 입니다.</para>
        /// </summary>
        public static readonly ResultCode IOS_SAND_BOX_RECEIPT = new ResultCode(18, LocalizeKey._418);
        /// <summary>
        /// 19. IOS IAP 해킹 시도
        /// <para>유효하지 않은 iOS 결제 시도입니다.</para>
        /// </summary>
        public static readonly ResultCode IAP_IOS_URUS_CRACKER = new ResultCode(19, LocalizeKey._419);
        /// <summary>
        /// 20. IAP 영수증 재사용
        /// <para>이미 사용한 영수증입니다.</para>
        /// </summary>
        public static readonly ResultCode IAP_DUPLICATE_RECEIPT = new ResultCode(20, LocalizeKey._420);
        /// <summary>
        /// 21. 탈퇴전 데이터 처리필요
        /// <para>탈퇴 대기중인 데이터가 존재합니다.</para>
        /// </summary>
        public static readonly ResultCode SECESSION_DATA_CHOICE = new ResultCode(21, LocalizeKey._421);
        /// <summary>
        /// 22. IAP 상품코드 문제
        /// <para>잘못된 상품코드입니다.</para>
        /// </summary>
        public static readonly ResultCode IAP_PRODUCT_CODE_ERROR = new ResultCode(22, LocalizeKey._422);
        /// <summary>
        /// 23. IAP_DEVELOPER_PAYLOAD_ERROR
        /// <para>유효하지 않은 상품 결제입니다.</para>
        /// </summary>
        public static readonly ResultCode IAP_DEVELOPER_PAYLOAD_ERROR = new ResultCode(23, LocalizeKey._423);
        /// <summary>
        /// 24. IAP_ERROR
        /// <para>결제에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode IAP_ERROR = new ResultCode(24, LocalizeKey._424);
        /// <summary>
        /// 25. 비밀 번호가 잘못됨
        /// <para>패스워드가 일치하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode BAD_PASS_ACCOUNT = new ResultCode(25, LocalizeKey._425);
        /// <summary>
        /// 26. 계정생성불가
        /// <para>계정을 생성할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode CREATE_ACCOUNT_IMPOSSIBLE = new ResultCode(26, LocalizeKey._426);
        /// <summary>
        /// 27. 수량이 부족하다
        /// <para>최대 횟수를 초과하였습니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_COUNT = new ResultCode(27, LocalizeKey._427);
        /// <summary>
        /// 28. 스킬 쿨타임 시간이 지나지 않았다..
        /// <para>재사용 대기 시간이 남아있습니다.</para>
        /// </summary>
        public static readonly ResultCode LEFT_COOL_TIME = new ResultCode(28, LocalizeKey._428);
        /// <summary>
        /// 29. 쿠폰 일일시도횟수제한
        /// <para>쿠폰의 일일시도 횟수가 초과하였습니다.</para>
        /// </summary>
        public static readonly ResultCode DAILY_TRY_LIMIT = new ResultCode(29, LocalizeKey._429);
        /// <summary>
        /// 30. 쿠폰사용-계정이 없습니다.
        /// <para>유효하지 않는 쿠폰번호입니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_FOUND_ACCOUNTKEY_COUPON = new ResultCode(30, LocalizeKey._430);
        /// <summary>
        /// 31. 인벤토리가 부족 하다
        /// <para>가방의 무게가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_INVEN = new ResultCode(31, LocalizeKey._431);
        /// <summary>
        /// 32. 잘못된 요청. 재시작 바람
        /// <para>잘못된 요청입니다. 재시작하시기 바랍니다.</para>
        /// </summary>
        public static readonly ResultCode BAD_REQUEST = new ResultCode(32, LocalizeKey._432);
        /// <summary>
        /// 33. 인증 대기중
        /// <para>인증 대기중입니다.</para>
        /// </summary>
        public static readonly ResultCode AUTH_WAITING = new ResultCode(33, LocalizeKey._433);
        /// <summary>
        /// 34. 존재하지 않는다.
        /// <para>해당 정보가 존재하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_EXISTS = new ResultCode(34, LocalizeKey._434);
        /// <summary>
        /// 35. 진행중이 아니다.
        /// <para>현재 진행중이 아닙니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_IN_PROGRESS = new ResultCode(35, LocalizeKey._435);
        /// <summary>
        /// 36. 중복된 데이터다
        /// <para>중복된 요청입니다.</para>
        /// </summary>
        public static readonly ResultCode DUPLICATION_FAIL = new ResultCode(36, LocalizeKey._436);
        /// <summary>
        /// 37. 퀘스트 완료 카운트가 부족
        /// <para>퀘스트 완료 조건을 만족하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_QUEST_PROG = new ResultCode(37, LocalizeKey._437);
        /// <summary>
        /// 38. 사용할수 없다.
        /// <para>사용할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_USED_COMMAND = new ResultCode(38, LocalizeKey._438);
        /// <summary>
        /// 39. 신고 제한에 걸림.
        /// <para>더 이상 신고할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode REPORT_LIMIT = new ResultCode(39, LocalizeKey._439);
        /// <summary>
        /// 40. RCODE MSG
        /// <para>에러 코드 메시지를 확인하세요.</para>
        /// </summary>
        public static readonly ResultCode RCODE_MSG = new ResultCode(40, LocalizeKey._440);
        /// <summary>
        /// 41. 채널 채팅방에 들어가 있지 않다.
        /// <para>채팅 채널 연결이 끊겼습니다.</para>
        /// </summary>
        public static readonly ResultCode RESULT_NOT_JOIN_CHANNEL_CHAT = new ResultCode(41, LocalizeKey._441);
        /// <summary>
        /// 42. 시간이 부족하다
        /// <para>이용 가능한 시간이 아닙니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_TIME = new ResultCode(42, LocalizeKey._442);
        /// <summary>
        /// 43. 이미 연동된 계정
        /// <para>이미 연동된 계정입니다.</para>
        /// </summary>
        public static readonly ResultCode ALREADY_LINKED_ACCOUNT = new ResultCode(43, LocalizeKey._443);
        /// <summary>
        /// 44. 이미 연동된 게임센터,구글계정
        /// <para>이미 연동된 구글 계정입니다.</para>
        /// </summary>
        public static readonly ResultCode ALREADY_LINKED_PLATFORM_ACCOUNT = new ResultCode(44, LocalizeKey._444);
        /// <summary>
        /// 45. 잠시 후 다시 시도 해주세요
        /// <para>잠시 후 다시 시도 하십시오.</para>
        /// </summary>
        public static readonly ResultCode RETRY_LATTER = new ResultCode(45, LocalizeKey._445);
        /// <summary>
        /// 46. 레벨 제한에 걸림.
        /// <para>레벨이 충족되지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode LEVEL_CUT = new ResultCode(46, LocalizeKey._446);
        /// <summary>
        /// 47. 이미 길드에 속해있다.
        /// <para>이미 길드에 가입되어 있습니다.</para>
        /// </summary>
        public static readonly ResultCode ALREADY_GUILD_MEMBER = new ResultCode(47, LocalizeKey._447);
        /// <summary>
        /// 48. 길드에 속한 케릭터가 아닙니다.
        /// <para>길드원만 이용할 수 있습니다.</para>
        /// </summary>
        public static readonly ResultCode CHAR_GUILD_NOT_FOUND = new ResultCode(48, LocalizeKey._448);
        /// <summary>
        /// 49. 문자열이 너무 짧음
        /// <para>너무 짧은 이름은 사용할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode SHORT_STRING = new ResultCode(49, LocalizeKey._449);
        /// <summary>
        /// 50. 길드의 정원이 꽉찼습니다.
        /// <para>길드 정원을 초과하였습니다.</para>
        /// </summary>
        public static readonly ResultCode GUILD_MEMBER_IS_FULL = new ResultCode(50, LocalizeKey._450);
        /// <summary>
        /// 51. 권한이 없습니다.
        /// <para>요청 권한이 없습니다.</para>
        /// </summary>
        public static readonly ResultCode PERMISSION_FAIL = new ResultCode(51, LocalizeKey._451);
        /// <summary>
        /// 52. 길드 포인트가 부족하다
        /// <para>길드 포인트가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_GUILD_POINT = new ResultCode(52, LocalizeKey._452);
        /// <summary>
        /// 53. 쿨타임 남음
        /// <para>쿨타임 중에는 이용할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode LEFT_COOLTIME = new ResultCode(53, LocalizeKey._453);
        /// <summary>
        /// 54. 길드 가입요청이 존재하지 않는다.
        /// <para>길드 가입 요청을 철회한 유저입니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_EXISTS_GUILD_JOIN_REQUEST = new ResultCode(54, LocalizeKey._454);
        /// <summary>
        /// 55. 길드 재가입 쿨타임 남음
        /// <para>길드 재가입 대기시간이 남아있습니다.</para>
        /// </summary>
        public static readonly ResultCode GUILD_JOIN_COOL = new ResultCode(55, LocalizeKey._455);
        /// <summary>
        /// 56. 길드 코인이 부족하다
        /// <para>길드 코인이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_GUILD_COIN = new ResultCode(56, LocalizeKey._456);
        /// <summary>
        /// 57. 길드 생성 실패
        /// <para>길드를 생성할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode GUILD_CREATE_FAIL = new ResultCode(57, LocalizeKey._457);
        /// <summary>
        /// 58. 아이템이 잠금 상태이다.
        /// <para>아이템 잠금 해제 후 이용하십시오.</para>
        /// </summary>
        public static readonly ResultCode CHAR_ITEM_IS_LOCK = new ResultCode(58, LocalizeKey._458);
        /// <summary>
        /// 59. 직업 레벨이 부족하다.
        /// <para>직업 레벨이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_JOB_LEVEL = new ResultCode(59, LocalizeKey._459);
        /// <summary>
        /// 60. 채팅 보내기 실패다
        /// <para>귓속말을 보낼 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode WISPERCHAT_FAIL = new ResultCode(60, LocalizeKey._460);
        /// <summary>
        /// 61. PTMARKET_FULL
        /// <para>개인상점 정원을 초과하였습니다.</para>
        /// </summary>
        public static readonly ResultCode PTMARKET_FULL = new ResultCode(61, LocalizeKey._461);
        /// <summary>
        /// 62. PTMARKET_MAYBESELL
        /// <para>개인상점에 판매할 수 없는 아이템입니다.</para>
        /// </summary>
        public static readonly ResultCode PTMARKET_MAYBESELL = new ResultCode(62, LocalizeKey._462);
        /// <summary>
        /// 63. PTMARKET_NOREGITEM
        /// <para>개인상점에 등록할 수 없는 아이템입니다.</para>
        /// </summary>
        public static readonly ResultCode PTMARKET_NOREGITEM = new ResultCode(63, LocalizeKey._463);
        /// <summary>
        /// 64. 스테이지 진행도가 부족하다
        /// <para>스테이지 진행도가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_STAGE = new ResultCode(64, LocalizeKey._464);
        /// <summary>
        /// 65. 던전 진행도가 부족하다
        /// <para>던전 진행도가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_DUNGEON = new ResultCode(65, LocalizeKey._465);
        /// <summary>
        /// 66. 프로토콜은 성공이지만 무시됨
        /// <para>(요청 실패)</para>
        /// </summary>
        public static readonly ResultCode RESULT_PROTOCOL_FAIL_OK = new ResultCode(66, LocalizeKey._466);
        /// <summary>
        /// 67. 프로토콜 FAIL
        /// <para>요청 실패</para>
        /// </summary>
        public static readonly ResultCode RESULT_PROTOCOL_FAIL = new ResultCode(67, LocalizeKey._467);
        /// <summary>
        /// 68. 이미 소비(consume)한 아이템
        /// <para>이미 소비한 결제 정보입니다.</para>
        /// </summary>
        public static readonly ResultCode ALEARDY_CONSUME = new ResultCode(68, LocalizeKey._468);
        /// <summary>
        /// 69. 결제 취소
        /// <para>취소된 결제입니다.</para>
        /// </summary>
        public static readonly ResultCode CANCEL_PAY = new ResultCode(69, LocalizeKey._469);
        /// <summary>
        /// 70. 스킬 변경이 불가능함
        /// <para>변경할 수 없는 스킬입니다.</para>
        /// </summary>
        public static readonly ResultCode SKILL_CHANGE_FAIL = new ResultCode(70, LocalizeKey._470);
        /// <summary>
        /// 71. 개인거래 구매실패
        /// <para>개인거래 구매에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode TRADEPRIVATE_BUY_FAIL = new ResultCode(71, LocalizeKey._471);
        /// <summary>
        /// 72. 상점로비가 풀이다
        /// <para>개인거래 로비 정원을 초과하였습니다.</para>
        /// </summary>
        public static readonly ResultCode TRADEPRIVATE_ENTERROOM_FULL = new ResultCode(72, LocalizeKey._472);
        /// <summary>
        /// 73. 개인거래 로비 입장 실패
        /// <para>개인거래 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode TRADEPRIVATE_ENTERROOM_FAIL = new ResultCode(73, LocalizeKey._473);
        /// <summary>
        /// 74. 개인거래 로비의 거래 로비를 찾을수 없다
        /// <para>개인거래 로비 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode TRADEPRIVATE_TRADE_LOBBY_NOT_EXISTS = new ResultCode(74, LocalizeKey._474);
        /// <summary>
        /// 75. 개인거래 로비를 찾을수 없다
        /// <para>개인거래 룸 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode TRADEPRIVATE_TRADE_ROOM_NOT_EXISTS = new ResultCode(75, LocalizeKey._475);
        /// <summary>
        /// 76. 개인소 개설중이 아니다.
        /// <para>개인거래 개설중이 아닙니다.</para>
        /// </summary>
        public static readonly ResultCode TRADEPRIVATE_NOT_OPEN = new ResultCode(76, LocalizeKey._476);
        /// <summary>
        /// 77. 월드보스 인원이 풀이다.
        /// <para>월드보스 정원을 초과하였습니다.</para>
        /// </summary>
        public static readonly ResultCode WORLD_BOSS_ROOM_FULL = new ResultCode(77, LocalizeKey._477);
        /// <summary>
        /// 78. 월드보스가 hp가 0이다 닫혀있다
        /// <para>현재 진행중인 월드 보스가 아닙니다.</para>
        /// </summary>
        public static readonly ResultCode WORLD_BOSS_CLOSE = new ResultCode(78, LocalizeKey._478);
        /// <summary>
        /// 79. 디펜스 던전 티켓 부족
        /// <para>비공정 습격 입장권이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_DEF_DUNGEON_TICKET = new ResultCode(79, LocalizeKey._479);
        /// <summary>
        /// 80. 월드보스 티켓이 부족하다
        /// <para>무한의 공간 입장권이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_WORLD_BOSS_DUNGEON_TICKET = new ResultCode(80, LocalizeKey._480);
        /// <summary>
        /// 81. 1:1대전 티켓이 부족하다
        /// <para>대전 입장권이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_PVE_TICKET = new ResultCode(81, LocalizeKey._481);
        /// <summary>
        /// 82. 정산 중 입니다.
        /// <para>랭킹 정산 중입니다.</para>
        /// </summary>
        public static readonly ResultCode IN_CLAC_PVP_SEASON_RANK = new ResultCode(82, LocalizeKey._482);
        /// <summary>
        /// 83. 1:1대전 매칭에 실패 하였습니다.
        /// <para>대전 매칭에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode PVE_NOT_MATCH = new ResultCode(83, LocalizeKey._483);
        /// <summary>
        /// 84. 1:1대전 시즌 정보가 변경되었다
        /// <para>대전 시즌이 종료되었습니다.</para>
        /// </summary>
        public static readonly ResultCode PVE_SEASON_CHANGE = new ResultCode(84, LocalizeKey._484);
        /// <summary>
        /// 85. 1:1대전 중이다.
        /// <para>대전 중에는 이용할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode PVE_BATTLE_NOT_END = new ResultCode(85, LocalizeKey._485);
        /// <summary>
        /// 86. 필드 보스를 죽이지 않은 상태에서 클리어 요청이 들어왔다
        /// <para>클리어 완료 조건을 만족하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode FIELD_BOSS_NOT_CLEAR = new ResultCode(86, LocalizeKey._486);
        /// <summary>
        /// 87. 월드보스 입장실패
        /// <para>월드보스 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode WORLD_BOSS_ENTERROOM_FAIL = new ResultCode(87, LocalizeKey._487);
        /// <summary>
        /// 88. 데미지 체크 실패
        /// <para>대미지 검증에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode DAMAGE_CKECK_FAIL = new ResultCode(88, LocalizeKey._488);
        /// <summary>
        /// 89. GUEST_BAD_PASSWORD
        /// <para>기기 이동 인증에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode GUEST_BAD_PASSWORD = new ResultCode(89, LocalizeKey._489);
        /// <summary>
        /// 90. 스테이지 보스 티켓이 부족하다
        /// <para>MVP 보스 티켓이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_STAGE_BOSS_DUNGEON_TICKET = new ResultCode(90, LocalizeKey._490);
        /// <summary>
        /// 91. 스테이지 미로맵 입장 실패
        /// <para>스테이지 미로 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode STAGEMAZE_ENTERROOM_FAIL = new ResultCode(91, LocalizeKey._491);
        /// <summary>
        /// 92. 의뢰 퀘스트 코인 부족하다
        /// <para>의뢰 퀘스트 코인이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_NORMAL_QUEST_COIN = new ResultCode(92, LocalizeKey._492);
        /// <summary>
        /// 93. 튜토리얼이 존재하지 않는다.
        /// <para>튜토리얼이 존재하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_EXISTS_TUTORIAL_STEP = new ResultCode(93, LocalizeKey._493);
        /// <summary>
        /// 94. GVG룸 입장 실패 - 입장 할수 있는 room 이 없다
        /// <para>길드미로 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode GVG_ENTERROOM_FULL = new ResultCode(94, LocalizeKey._494);
        /// <summary>
        /// 95. GVG룸 입장 실패
        /// <para>길드미로 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode GVG_ENTERROOM_FAIL = new ResultCode(95, LocalizeKey._495);
        /// <summary>
        /// 96. 테이밍 미로 룸 입장 실패
        /// <para>테이밍 미로 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode TAMINIG_ENTERROOM_FAIL = new ResultCode(96, LocalizeKey._496);
        /// <summary>
        /// 97. 타운 룸 입장 실패
        /// <para>거래소 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode TOWN_ROOM_ENTERROOM_FAIL = new ResultCode(97, LocalizeKey._497);
        /// <summary>
        /// 98. 이미 테이밍 준상테
        /// <para>테이밍 중입니다.</para>
        /// </summary>
        public static readonly ResultCode ALEARDY_TAMING = new ResultCode(98, LocalizeKey._498);
        /// <summary>
        /// 99. 테이밍 방이 존제 하지 않는다.
        /// <para>테이밍 미로 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode TAMING_ROOM_NOT_EXITS = new ResultCode(99, LocalizeKey._499);
        /// <summary>
        /// 100. 테이중인(READY) 몬스터가 존재 하지 않는다.
        /// <para>테이밍 몬스터가 존재하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode TAMING_MONSTER_NOT_EXITS = new ResultCode(100, LocalizeKey._500);
        /// <summary>
        /// 101. 테이밍 룸에 윺저가 존제 하지 않는다.
        /// <para>테이밍 미로에 유저가 존재 하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode TAMING_ROOM_USER_NOT_EXITS = new ResultCode(101, LocalizeKey._501);
        /// <summary>
        /// 102. 타운 룸 입장 실패
        /// <para>길드로비 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode GUILD_LOBBY_ENTER_FAIL = new ResultCode(102, LocalizeKey._502);
        /// <summary>
        /// 103. 다른 플레이어가 이미 MVP주변으로 플레이어 소환을 했다
        /// <para>다른 유저가 이미 소환하였습니다.</para>
        /// </summary>
        public static readonly ResultCode ALEARDY_MVP_SUMMON = new ResultCode(103, LocalizeKey._503);
        /// <summary>
        /// 104. RO포인트가 부족하다.
        /// <para>RO포인트가 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_RO_POINT = new ResultCode(104, LocalizeKey._504);
        /// <summary>
        /// 105. 공유 캐릭터 사용시간 충전 아이템이 부족하다
        /// <para>충전 시간이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_CHAR_USE_TIME_TICKET = new ResultCode(105, LocalizeKey._505);
        /// <summary>
        /// 106. 캐릭터 정보 저장중 다시 로그인 요청이 들어왔다
        /// <para>캐릭터 정보 저장중입니다.</para>
        /// </summary>
        public static readonly ResultCode WAIT_RETRY_LOGIN = new ResultCode(106, LocalizeKey._506);
        /// <summary>
        /// 107. 멀티 던전 티켓이 부족하다
        /// <para>멀티 던전 티켓이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_MULTI_MAZE_TICKET = new ResultCode(107, LocalizeKey._507);
        /// <summary>
        /// 108. 티켓이 부족하다
        /// <para>티켓이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_TICKET = new ResultCode(108, LocalizeKey._508);
        /// <summary>
        /// 109. 접속할수 있는 멀티 서버가 없다
        /// <para>접속할수 있는 멀티 서버가 없습니다.</para>
        /// </summary>
        public static readonly ResultCode MULTI_SERVER_CONNECT_ERROR = new ResultCode(109, LocalizeKey._509);
        /// <summary>
        /// 110. 듀얼 조각을 가지고 있지 않다.
        /// <para>듀얼 조각이 존재하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_EXISTS_DUEL_PIECE = new ResultCode(110, LocalizeKey._510);
        /// <summary>
        /// 111. 듀얼 조각이 다 모이지 않았다.
        /// <para>듀얼 조각이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_DUEL_PIECE = new ResultCode(111, LocalizeKey._511);
        /// <summary>
        /// 112. 포인트가 맥스치에 도달되었다
        /// <para>이미 최대치에 도달하였습니다.</para>
        /// </summary>
        public static readonly ResultCode MAX_POINT = new ResultCode(112, LocalizeKey._512);
        /// <summary>
        /// 113. 릴리즈 할 캐릭터가 존제 하지 않는다
        /// <para>셰어링 캐릭터가 존재하지 않습니다.</para>
        /// </summary>
        public static readonly ResultCode RELEASE_SHARE_CHAR_IS_ZERO = new ResultCode(113, LocalizeKey._513);
        /// <summary>
        /// 114. 입장 할수 있는 멀티미로 방이 없다
        /// <para>입장 가능한 멀티미로 방이 없습니다.</para>
        /// </summary>
        public static readonly ResultCode MULTI_MAZE_ENTERROOM_FULL = new ResultCode(114, LocalizeKey._514);
        /// <summary>
        /// 115. 멀티 미로 입장 실패
        /// <para>멀티 미로 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode MULTI_MAZE_ENTERROOM_FAIL = new ResultCode(115, LocalizeKey._515);
        /// <summary>
        /// 116. 입장 할수 있는 멀티대기실이 없다
        /// <para>입장 가능한 멀티미로 대기실이 없습니다.</para>
        /// </summary>
        public static readonly ResultCode MULTI_MAZE_WAITING_ENTERROOM_FULL = new ResultCode(116, LocalizeKey._516);
        /// <summary>
        /// 117. 멀티 미로 대기실 입장 실패
        /// <para>멀티 미로 대기실 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode MULTI_MAZE_WAITING_ENTERROOM_FAIL = new ResultCode(117, LocalizeKey._517);
        /// <summary>
        /// 118. 월드보스 배틀 중이다.
        /// <para>월드보스 전투가 아직 진행 중입니다.</para>
        /// </summary>
        public static readonly ResultCode WORLDBOSS_BATTLE_NOT_END = new ResultCode(118, LocalizeKey._518);
        /// <summary>
        /// 119. 스테이지 데이터에 포함되지 않은 몬스터
        /// <para>일치하지 않은 몬스터 정보입니다.</para>
        /// </summary>
        public static readonly ResultCode STAGE_MON_NOT_EXISTS = new ResultCode(119, LocalizeKey._519);
        /// <summary>
        /// 120. 캐릭터 정보를 공개 하지 않음
        /// <para>정보를 공개 하지 않은 캐릭터입니다.</para>
        /// </summary>
        public static readonly ResultCode FRIEND_CHAR_INFO_CLOSED = new ResultCode(120, LocalizeKey._520);
        /// <summary>
        /// 121. 생성할수 없는 닉네임
        /// <para>해당 이름은 사용할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode NICK_FILTER = new ResultCode(121, LocalizeKey._521);
        /// <summary>
        /// 122. 입장 가능한 최대 스테이지에 도달되었다
        /// <para>입장 가능한 최대 스테이지에 도달하였습니다.</para>
        /// </summary>
        public static readonly ResultCode MAX_CLEAR_STAGE = new ResultCode(122, LocalizeKey._522);
        /// <summary>
        /// 123. 메시지가 빈 메세지이거나 길이가 초과 되었다
        /// <para>메시지가 빈 메시지이거나 길이가 초과되었습니다.</para>
        /// </summary>
        public static readonly ResultCode MESSAGE_TEXT_ERROR = new ResultCode(123, LocalizeKey._525);
        /// <summary>
        /// 124. 난전 입장 가능한 룸이 없다
        /// <para>난전 입장이 불가능합니다.</para>
        /// </summary>
        public static readonly ResultCode FREEFIGHT_FULL = new ResultCode(124, LocalizeKey._526);
        /// <summary>
        /// 125. 난전 입장 가능한 룸이 없다
        /// <para>난전 입장이 불가능합니다.</para>
        /// </summary>
        public static readonly ResultCode FF_ENTERROOM_FULL = new ResultCode(125, LocalizeKey._526);
        /// <summary>
        /// 126. 난전 룸 입장에 실패하였다.
        /// <para>난전 입장에 실패하였습니다.</para>
        /// </summary>
        public static readonly ResultCode FF_ENTERROOM_FAIL = new ResultCode(126, LocalizeKey._527);
        /// <summary>
        /// 127. 접속할수 있는 서버가 존제 하지 않는다.
        /// <para>현재 접속 가능한 서버가 없습니다.</para>
        /// </summary>
        public static readonly ResultCode SERVER_IS_FULL = new ResultCode(127, LocalizeKey._528);
        /// <summary>
        /// 128. 유저 추방
        /// <para>서버 연결이 끊어졌습니다.</para>
        /// </summary>
        public static readonly ResultCode USER_KICK = new ResultCode(128, LocalizeKey._529);
        /// <summary>
        /// 129. 상점 데이터가 존제 하지 않는다.
        /// <para>존재하지 않는 상품입니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_EXISTS_SHOP_DATA = new ResultCode(129, LocalizeKey._531);
        /// <summary>
        /// 130. 캐릭터 직업 조건에 부적합
        /// <para>조건에 맞지 않는 직업입니다.</para>
        /// </summary>
        public static readonly ResultCode ERROR_REPAYMENT_JOB = new ResultCode(130, LocalizeKey._532);
        /// <summary>
        /// 131. 캐릭터 직업 레벨 부적합
        /// <para>조건에 맞지 않는 직업 레벨입니다.</para>
        /// </summary>
        public static readonly ResultCode ERROR_REPAYMENT_LEVEL = new ResultCode(130, LocalizeKey._533);
        /// <summary>
        /// 132. 상점 데이터 재지급 체크 성공
        /// <para>상품이 지급되었습니다.</para>
        /// </summary>
        public static readonly ResultCode SUCCESS_REPAYMENT_CHECK = new ResultCode(132, LocalizeKey._534);
        /// <summary>
        /// 133. [현금 결제 상품검사] 매일 상품, 이미 있는 상품을 또 구매하려하였다
        /// <para>이미 구매한 상품입니다.</para>
        /// </summary>
        public static readonly ResultCode ERROR_REPAYMENT_28 = new ResultCode(133, LocalizeKey._535);
        /// <summary>
        /// 134. [현금 결제 상품검사] 매일 상품, 현재 갖고있는 상품을 끝까지 받았으나, 마지막 받은날이 지나지 않았다
        /// <para>상품 구매 가능 시간이 아닙니다.</para>
        /// </summary>
        public static readonly ResultCode ERROR_REPAYMENT_28_1 = new ResultCode(134, LocalizeKey._536);
        /// <summary>
        /// 135. [현금 결제 상품검사] 매일 상품 마지막날 db 삭제 실패.
        /// <para>해당 상품을 구매할 수 없습니다.</para>
        /// </summary>
        public static readonly ResultCode ERROR_REPAYMENT_28_2 = new ResultCode(135, LocalizeKey._537);
        /// <summary>
        /// 136. 채팅 블럭에 걸림
        /// <para>채팅을 할 수 없는 상태입니다.</para>
        /// </summary>
        public static readonly ResultCode CHAT_LIMIT = new ResultCode(136, LocalizeKey._538);
        /// <summary>
        /// 137. 스킬 체크 pass
        /// <para>스킬 정보를 동기화 합니다.</para>
        /// </summary>
        public static readonly ResultCode PASS_SKILL_CHECK = new ResultCode(137, LocalizeKey._539);
        /// <summary>
        /// 138. 멀티 던전 티켓이 부족하다.
        /// <para>멀티 던전 티켓이 부족합니다.</para>
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_EVENT_TICKET = new ResultCode(138, LocalizeKey._540);
        /// <summary>
        /// 139. FOREST_FULL
        /// <para>미궁숲 입장이 불가능합니다.</para>
        /// </summary>
        public static readonly ResultCode FOREST_FULL = new ResultCode(139, LocalizeKey._541);
        /// <summary>
        /// 140. 업데이트 예정
        /// <para>업데이트 예정입니다..</para>
        /// </summary>
        public static readonly ResultCode CONTENT_CLOSE = new ResultCode(140, LocalizeKey._542);
        /// <summary>
        /// 141. 길드전 중 길드 탈퇴 및 추방 불가
        /// </summary>
        public static readonly ResultCode ISIN_GUILD_BATTLE = new ResultCode(141, LocalizeKey._544);
        /// <summary>
        /// 142. 길드대전 상대의 엠펠리움 HP가 0
        /// </summary>
        public static readonly ResultCode EMPAL_HP_ZERO = new ResultCode(142, LocalizeKey._545);
        /// <summary>
        /// 143. 길드전 미신청 길드입니다.(길드전 진행중)
        /// </summary>
        public static readonly ResultCode NOT_REQUEST_GUILD_BATTLE = new ResultCode(143, LocalizeKey._546);
        /// <summary>
        /// 144. 길드전 신청 상태에서 길드 해산을 할 수 없습니다.
        /// </summary>
        public static readonly ResultCode CANNOT_GUILD_DISSOLVED = new ResultCode(144, LocalizeKey._547);
        /// <summary>
        /// 145. 해당 우편은 삭제 할 수 없습니다.
        /// </summary>
        public static readonly ResultCode CANNOT_REMOVE_MAIL     = new ResultCode(145, LocalizeKey._548);
        /// <summary>
        /// 146. 파티 정원을 초과하였습니다.
        /// </summary>
        public static readonly ResultCode PARTY_IS_FULL = new ResultCode(146, LocalizeKey._549);
        /// <summary>
        /// 150. INNO_UID 가 이미 사용중이다
        /// </summary>
        public static readonly ResultCode INNO_UID_ALREADY_IN_USE = new ResultCode(150, LocalizeKey._550);
        /// <summary>
        /// 151. INNO_UID 연동 에러
        /// </summary>
        public static readonly ResultCode INNO_UID_ACCOUNT_LOGIN_API_FAIL = new ResultCode(151, LocalizeKey._551);
        /// <summary>
        /// 152. INNO_UID 연동 해제 에러
        /// </summary>
        public static readonly ResultCode INNO_UID_ACCOUNT_UNLINK_FAIL = new ResultCode(152, LocalizeKey._552);
        /// <summary>
        /// 153. INNO_UID 연동이 되어 있지 않다
        /// </summary>
        public static readonly ResultCode INNO_UID_ACCOUNT_ERROR = new ResultCode(153, LocalizeKey._553);
        /// <summary>
        /// 154. 온버프 코인이 부족하다
        /// </summary>
        public static readonly ResultCode NOT_ENOUGHT_ONBUFF_COIN = new ResultCode(154, LocalizeKey._554);
        /// <summary>
        /// 155. INNO_UID 연동 해제후 쿨타임 상태
        /// </summary>
        public static readonly ResultCode INNO_UID_EXPIRE_COOL = new ResultCode(155, LocalizeKey._555);
        /// <summary>
        /// 156. 온버프 코인 싱크가 맞지 않는다. 게임서버&INNO
        /// </summary>
        public static readonly ResultCode ONBUFF_QUANTITY_SYNC_ERROR = new ResultCode(156, LocalizeKey._556);
        /// <summary>
        /// 157. INNO_상태이상(INNO 연결이 불안정합니다.\n잠시 후에 이용해 주십시오.)
        /// </summary>
        public static readonly ResultCode INNO_ABNORMAL = new ResultCode(157, LocalizeKey._557);
        /// <summary>
        /// 158. INNO_서비스 점검(점검 중입니다.\n점검이 종료된 후 다시 이용해 주십시오.)
        /// </summary>
        public static readonly ResultCode INNO_MAINTENANCE = new ResultCode(158, LocalizeKey._558);
        /// <summary>
        /// 159. INNO_서비스 지연(INNO 연결이 불안정합니다.\n잠시 후에 이용해 주십시오.)
        /// </summary>
        public static readonly ResultCode INNO_DELAY = new ResultCode(159, LocalizeKey._559);
    }
}