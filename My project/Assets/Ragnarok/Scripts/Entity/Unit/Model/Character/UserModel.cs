using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 유저 기본 정보
    /// </summary>
    public class UserModel : CharacterEntityModel
    {
        private const string TAG = nameof(UserModel);

        public delegate void UserInfoEvent(int uid, int cid);

        private ObscuredInt uid;
        private ObscuredBool pushOn;
        private ObscuredInt connectTime;
        private ObscuredInt dayConnectTime;
        private ObscuredByte loginBonusCount;
        private ObscuredShort loginBonusGroupID;
        private ObscuredByte connectTimeRewardIndex;
        private ObscuredByte dailyQuestClear;
        private ObscuredInt maxClearStageId;
        private CumulativeTime dayConnnectionTime;
        private CumulativeTime zenyTreeTime;
        private CumulativeTime resourceTreeTime;
        private ObscuredBool isNewDaily; // 오늘 첫접속(출석체크 연출에 쓰면됨)        
        private ObscuredBool isOpenInfo;
        private bool isSharePush; // 셰어 푸쉬 정보
        private int channelIndex; // 채널 인덱스
        public bool IsShareBuff { get; private set; } // 셰어 버프
        public RemainTime KafraMemberShipReaminTime { get; private set; } // 카프라 회원권 남은시간
        private ObscuredBool isLikedFacebook; // 페이스북 페이지 방문 보상

        private EventLoginPacket[] eventLoginPackets = new EventLoginPacket[0];
        private LoginBonusDataManager LoginBonusRepo;
        private List<DailyCheckInfo> dailyCheckInfos;
        private List<CatCoinRewardInfo> catCoinRewardInfos;
        private readonly ConnectionManager connectionManager;
        private readonly IGamePotImpl gamePotImpl;
        private readonly LoginManager loginManager;

        public bool IsNewDaily => isNewDaily;
        public bool IsOpenInfo { get => isOpenInfo; set => isOpenInfo = value; }

        /// <summary>
        /// 이벤트 호출 주기
        /// </summary>
        private const float CALL_WAIT_TIME = 30f;

        /// <summary>
        /// 나무 보상 획득 가능 이벤트
        /// </summary>
        public Action OnTreeReward { get; set; }

        public int UID { get { return uid; } }

        /// <summary>
        /// 접속보상 Index
        /// </summary>
        public int ConnectTimeRewardIndex => connectTimeRewardIndex;

        /// <summary>
        /// 금일 누적 접속 시간(초)
        /// </summary>
        public CumulativeTime DayConnectionTime => dayConnnectionTime;

        /// <summary>
        /// 일일퀘 최종 보상 수령 여부
        /// </summary>
        public bool IsDailyQuestCleared
        {
            get => dailyQuestClear == 1;
            set => dailyQuestClear = value ? (byte)1 : (byte)0;
        }

        /// <summary>
        /// 클리어한 최대 스테이지 아이디
        /// </summary>
        public int MaxClearStageId => maxClearStageId;

        /// <summary>
        /// 제니 나무 시간
        /// </summary>
        public CumulativeTime ZenyTreeTime => zenyTreeTime;

        /// <summary>
        /// 재료 나무 시간
        /// </summary>
        public CumulativeTime MaterialTreeTime => resourceTreeTime;
        /// <summary>
        /// 나무 최대 누적 시간
        /// </summary>
        public float TreeMaxTime => BasisType.TREE_REWARD_MAX_SECOND.GetInt() * 1000f;
        /// <summary>
        /// 현재 제니 나무 시간
        /// </summary>
        public float CurZenyTreeTime => ZenyTreeTime.ToCumulativeTime() >= TreeMaxTime ? TreeMaxTime : ZenyTreeTime.ToCumulativeTime();
        /// <summary>
        /// 제니 나무 보상 있는지 여부
        /// </summary>
        public bool IsZenyTreeReward => (int)(CurZenyTreeTime / (BasisType.TREE_REWARD_ACCRUE_SECOND.GetInt() * 1000f)) > 0;
        /// <summary>
        /// 현재 재료 나무 시간
        /// </summary>
        public float CurMaterialTreeTime => MaterialTreeTime.ToCumulativeTime() >= TreeMaxTime ? TreeMaxTime : MaterialTreeTime.ToCumulativeTime();
        /// <summary>
        /// 재료 나무 보상 있는지 여부
        /// </summary>
        public bool IsMaterialTreeReward => (int)(CurMaterialTreeTime / (BasisType.TREE_REWARD_ACCRUE_SECOND.GetInt() * 1000f)) > 0;

        public IEnumerable<EventLoginPacket> EventLoginPackets => eventLoginPackets;

        /// <summary>
        /// 이벤트 로그인 보상 있는지 여부
        /// </summary>
        public bool IsRewardEventLogin { get; private set; }

        /// <summary>
        /// 온버프 Inno UID
        /// </summary>
        public string InnoUID { get; private set; }

        /// <summary>
        /// 온버프 로그인 여부
        /// </summary>
        public bool IsOnBuffLogin { get; private set; }

        /// <summary>
        /// 온버프 연동 이벤트
        /// </summary>
        public event Action OnBuffAccountLink;

        /// <summary>
        /// 온버프 연동 해제 이벤트
        /// </summary>
        public event Action OnBuffAccountUnLink;

        public event System.Action OnUpdateOptionSetting;

        public UserModel()
        {
            SetNewDaily(false);
            LoginBonusRepo = LoginBonusDataManager.Instance;
            dailyCheckInfos = new List<DailyCheckInfo>();
            catCoinRewardInfos = new List<CatCoinRewardInfo>();
            connectionManager = ConnectionManager.Instance;
            gamePotImpl = GamePotSystem.Instance;
            loginManager = LoginManager.Instance;
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_USER_DAILY_CALC.AddEvent(OnReceiveUserDailyCalc);
                BattleManager.OnStart += OnStartBattle;
                loginManager.OnLogout += OnLogOutSuccess;
                Protocol.RECEIVE_ONBUFF_COIN_ZERO.AddEvent(OnReceiveOnBuffPointZero);
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            Timing.KillCoroutines(TAG);

            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_USER_DAILY_CALC.RemoveEvent(OnReceiveUserDailyCalc);
                BattleManager.OnStart -= OnStartBattle;
                loginManager.OnLogout -= OnLogOutSuccess;
                Protocol.RECEIVE_ONBUFF_COIN_ZERO.RemoveEvent(OnReceiveOnBuffPointZero);
            }
        }

        public override void ResetData()
        {
            eventLoginPackets = new EventLoginPacket[0];
        }

        internal void Initialize(UserInfoData userPacket)
        {
            uid = userPacket.uid;
            pushOn = userPacket.pushOn;
            connectTime = userPacket.connectTime;
            dayConnectTime = userPacket.dayConnectTime;
            loginBonusCount = userPacket.loginBonusCount;
            loginBonusGroupID = userPacket.loginBonusGroupId;
            connectTimeRewardIndex = userPacket.connectTimeRewardIndex;
            dailyQuestClear = userPacket.daily_quest_clear;
            maxClearStageId = userPacket.max_clear_stage_id;
            IsShareBuff = userPacket.is_share_buff != 0;
            isLikedFacebook = userPacket.is_liked_facebook;

            Debug.Log($"loginBonusGroupID={loginBonusGroupID}");
            Debug.Log($"loginBonusCount={loginBonusCount}");

            SetDailyCheckInfo();
            SetCatCoinReward();

            Timing.KillCoroutines(TAG);
            Timing.RunCoroutine(UpdateTreeReward(), TAG);
        }

        internal void SetSharePushSetting(bool isSharePush)
        {
            this.isSharePush = isSharePush;
        }

        internal void SetChannelIndex(int channelIndex)
        {
            this.channelIndex = channelIndex;
        }

        internal void ConnnectInfo()
        {
            RequestGetConnnectInfo().WrapNetworkErrors();
        }

        /// <summary>
        /// 유저 정보 요청
        /// </summary>
        public async Task RequestUserInfo()
        {
            LanguageType language = Language.Current;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", language.ToByteValue());

            var response = await Protocol.USER_INFO.SendAsync(sfs);
            if (response.isSuccess)
            {
                UserInfoData userInfoData = response.GetBinaryPacket<UserInfoData>("1");
                Notify(userInfoData);

                IsRewardEventLogin = false;
                if (response.ContainsKey("2"))
                {
                    eventLoginPackets = response.GetPacketArray<EventLoginPacket>("2");
                    foreach (var item in eventLoginPackets)
                    {
                        if (item.IsReward)
                        {
                            IsRewardEventLogin = true;
                            if (item.Day == 1)
                            {
                                Analytics.TrackEvent(TrackType.Reward_Attendance_Day1);
                            }
                            break;
                        }
                    }
                }

                // 3.온버프 Inno UID
                if (response.ContainsKey("3"))
                {
                    InnoUID = response.GetUtfString("3");
                }
                else
                {
                    InnoUID = string.Empty;
                    IsOnBuffLogin = false;
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        private void SetDailyCheckInfo()
        {
            dailyCheckInfos.Clear();
            var datas = LoginBonusRepo.GetByGrupId(loginBonusGroupID);

            for (int i = 0; i < datas.Length; i++)
            {
                DailyCheckInfo info = new DailyCheckInfo();
                info.SetData(datas[i]);
                dailyCheckInfos.Add(info);
            }
        }

        /// <summary>
        /// 출석체크 보상 목록 반환
        /// </summary>
        /// <returns></returns>
        public DailyCheckInfo[] GetDailyCheckInfos()
        {
            return dailyCheckInfos.ToArray();
        }

        /// <summary>
        /// 출석 일수 반환
        /// </summary>
        /// <returns></returns>
        public byte DailyCount => loginBonusCount;

        private void OnReceiveUserDailyCalc(Response response)
        {
            if (response.isSuccess)
            {
                // 냥다래 나무 시간 초기화
                dayConnnectionTime = 0;
                connectTimeRewardIndex = 0;
                UpdateCumulativeTime();
                OnTreeReward?.Invoke();

                var data = LoginBonusRepo.Get(response.GetInt("1"));
                if (data != null)
                {
                    SetNewDaily(true);

                    // 출석체크 정보 세팅
                    loginBonusGroupID = (short)data.group_id;
                    loginBonusCount = (byte)data.day;
                }

                if (response.ContainsKey("2"))
                {
                    eventLoginPackets = response.GetPacketArray<EventLoginPacket>("2");
                    foreach (var item in eventLoginPackets)
                    {
                        if (item.IsReward)
                        {
                            IsRewardEventLogin = true;
                            break;
                        }
                    }
                }

                // 일일퀘 최종보상 수령 여부 초기화 (무조건 0으로 오겠지만, 혹시 모르니 추가 ..)
                dailyQuestClear = response.ContainsKey("3") ? response.GetByte("3") : (byte)0;

                if (response.ContainsKey("4"))
                {
                    Notify(response.GetPacket<EventQuizPacket>("4"));
                }
                else
                {
                    Notify(EventQuizPacket.EMPTY);
                }

                // 5. 고객감사 정보
                if (response.ContainsKey("5"))
                {
                    Notify(response.GetPacket<CustomerRewardPacket>("5"));
                }

                // 6. [스페셜] 냥다래 선물정보
                if (response.ContainsKey("6"))
                {
                    Notify(response.GetPacket<CatCoinGiftPacket>("6"));
                }

                int event_dice_complete_reward_step = response.GetInt("7"); // 주사위게임 보상수령한 회차
                int event_dice_complete_count = response.GetInt("8"); // 주사위게임 완주 횟수

                // 9. [출석 이벤트] 14일 출석이벤트 정보
                if (response.ContainsKey("9"))
                {
                    Notify(response.GetPacket<AttendEventPacket>("9"));
                }
                else
                {
                    Notify(AttendEventPacket.EMPTY);
                }

                SetDailyCheckInfo();

                // 상점 매일 보상 정보 조회
                Entity.ShopModel.RequestEveryDayGoodsInfo(isShowIndicator: false).WrapNetworkErrors();

                // 주사위이벤트 보상 정보 초기화
                Entity.Event.SetEventDiceCompleteInfo(event_dice_complete_count, event_dice_complete_reward_step);

                // 나비호 광고 보기 횟수 초기화
                Entity.Inventory.ResetNabihoInfo();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 냥다래 접속보상 세팅
        /// </summary>
        private void SetCatCoinReward()
        {
            catCoinRewardInfos.Clear();
            for (int i = 1; i < 6; i++)
            {
                CatCoinRewardInfo info = new CatCoinRewardInfo(BasisType.DAY_CONNECT_REWARD_TIME.GetInt(i));
                info.SetData(new RewardData((byte)RewardType.CatCoin, BasisType.DAY_CONNECT_REWARD.GetInt(i), 0));
                catCoinRewardInfos.Add(info);
            }
        }

        private void UpdateCumulativeTime()
        {
            for (int i = 0; i < catCoinRewardInfos.Count; i++)
            {
                catCoinRewardInfos[i].SetCumulativeTime(dayConnnectionTime);
                catCoinRewardInfos[i].SetReceived(i < connectTimeRewardIndex);
            }
        }

        public CatCoinRewardInfo[] GetCatCoinRewardInfos()
        {
            return catCoinRewardInfos.ToArray();
        }

        public bool IsCatCoinReward => catCoinRewardInfos.Count(x => x.CompleteType == CatCoinRewardInfo.CatCoinCompleteType.StandByReward) > 0;

        /// <summary>
        /// 접속 정보
        /// </summary>
        public async Task RequestGetConnnectInfo()
        {
            var response = await Protocol.REQUEST_GET_CONNECT_INFO.SendAsync();
            if (response.isSuccess)
            {
                dayConnnectionTime = response.GetInt("1");
                connectTimeRewardIndex = (byte)response.GetInt("2");
                zenyTreeTime = response.GetInt("3");
                resourceTreeTime = response.GetInt("4");

                UpdateCumulativeTime();
                OnTreeReward?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 누적 접속 보상 요청
        /// </summary>
        public async Task RequestGetConnectReward()
        {
            var response = await Protocol.REQUEST_GET_CONNECT_REWARD.SendAsync();
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                }

                // 1. 보상받은 인덱스
                connectTimeRewardIndex = (byte)response.GetInt("1");

                int maxRewardIndex = BasisType.DAY_CONNECT_REWARD.GetKeyList().Count;
                if (connectTimeRewardIndex == maxRewardIndex)
                {
                    Quest.QuestProgress(QuestType.CONNECT_TIME_ALL_REWARD);
                }

                UpdateCumulativeTime();
                OnTreeReward?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 온오프라인 접속시간 보상
        /// </summary>
        /// <param name="isZenyTree"></param>
        /// <returns></returns>
        public async Task RequestGetTreeReward(bool isZenyTree)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", isZenyTree ? (byte)1 : (byte)2); // 1:제니, 2:재료

            var response = await Protocol.REQUEST_GET_TREE_REWARD.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                }

                if (isZenyTree)
                {
                    zenyTreeTime = 0;
                }
                else
                {
                    resourceTreeTime = 0;
                }

                OnTreeReward?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 30초에 한번씩 나무보상 있는지 체크
        /// </summary>
        private IEnumerator<float> UpdateTreeReward()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(CALL_WAIT_TIME);
                OnTreeReward?.Invoke();
            }
        }

        public void SetNewDaily(bool value)
        {
            isNewDaily = value;
        }

        private void OnStartBattle(BattleMode mode)
        {
            if (!mode.IsDailyCheckMode())
                return;

            if (!Tutorial.isInProgress)
            {
                if (IsRewardEventLogin)
                {
                    IsRewardEventLogin = false;
                    UI.Show<UILoginBonus>();
                }
                else if (IsNewDaily)
                {
                    UIDailyCheck.tabType = UIDailyCheck.TabType.DailyCheck;
                    UI.Show<UIDailyCheck>();
                }
            }
        }

        /// <summary>
        /// 타 유저 정보 조회
        /// </summary>
        public async Task RequestOtherCharacterInfo(int uid, int cid)
        {
            var param = Protocol.NewInstance();
            param.PutInt("1", uid);
            param.PutInt("2", cid);
            var response = await Protocol.REQUEST_FRIEND_CHAR_INFO.SendAsync(param);
            if (!response.isSuccess)
            {
                UI.ShowToastPopup(LocalizeKey._4104.ToText()); // 캐릭터 정보가 비공개로 설정된 캐릭터 입니다.
                return;
            }

            UI.Close<UIChat>();
            UI.Show<UIOthersCharacterInfo>().Show(response.GetPacket<BattleCharacterPacket>("1"));
        }

        public bool IsSharePush()
        {
            return isSharePush;
        }

        /// <summary>
        /// 옵션 세팅 요청
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task RequestOptionSetting(OptionSettingType type, byte value)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", type.ToByteValue());
            sfs.PutByte("2", value);
            var response = await Protocol.REQUEST_OPTION_SETTING.SendAsync(sfs);
            if (!response.isSuccess)
                return;

            if (type == OptionSettingType.SharePush)
            {
                bool isSharePush = value == 1;
                SetSharePushSetting(isSharePush);
            }

            OnUpdateOptionSetting?.Invoke();
        }

        /// <summary>
        /// 채널 인덱스(결제, 쿠폰에 사용)
        /// </summary>
        /// <returns></returns>
        public int GetChannelIndex()
        {
            return channelIndex;
        }

        /// <summary>
        /// 페이스북 공식 페이지 이동 보상(좋아요 체크는 안하는걸로..)
        /// </summary>
        /// <returns></returns>
        public async Task LikedFacebook()
        {
            if (!isLikedFacebook)
            {
                isLikedFacebook = true;

                // 따로 좋아요 체크는 안하고 있어서.. 페이지 방문해서 좋아요 누르는 시간 5초정도로 딜레이 줬음..
                await Awaiters.Seconds(5f);

                var response = await Protocol.REQUEST_FB_LIKE_REWARD.SendAsync();
                if (response.isSuccess)
                {
                    // cud. 캐릭터 업데이트 데이터
                    if (response.ContainsKey("cud"))
                    {
                        CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                        Notify(charUpdateData);

                        UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                    }
                }
            }
        }

        /// <summary>
        /// 게임탈퇴 요청
        /// </summary>
        public async Task RequestDeleteMember()
        {
            var response = await Protocol.REQUEST_ACCOUNT_WITHDRAWAL.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }
            loginManager.AsyncLogOut().WrapNetworkErrors(); // 로그 아웃
        }

        /// <summary>
        /// 게임 로그아웃 성공
        /// </summary>
        private void OnLogOutSuccess()
        {
            connectionManager.DeleteServerGroupId();
            connectionManager.Disconnect();
            SceneLoader.LoadIntro(); // 타이틀 화면으로 이동
            UI.HideIndicator();
        }

        /// <summary>
        /// 게임 탈퇴 철회 요청
        /// </summary>
        public async Task RequestAccountWithdawalCancel()
        {
            var response = await Protocol.REQUEST_ACCOUNT_WITHDRAWAL_CANCEL.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            UI.ShowToastPopup(LocalizeKey._16.ToText()); // 탈퇴신청을 철회하였습니다.
        }

        /// <summary>
        /// 온버프 연동 여부
        /// </summary>
        public bool IsOnBuffAccountLink()
        {
            return !string.IsNullOrEmpty(InnoUID);
        }

        /// <summary>
        /// 온버프 로그인
        /// </summary>
        public async Task<bool> RequestOnBuffLogin()
        {
            // 온버프 연동 중일때만 호출
            if (!IsOnBuffAccountLink())
                return false;

            var response = await Protocol.REQUEST_ONBUFF_LOGIN.SendAsync();

            if (!response.isSuccess)
            {
                IsOnBuffLogin = false;
                response.ShowResultCode();
                return false;
            }

            IsOnBuffLogin = true;
            NotifyOnBuffPoint(response.GetInt("1"));
            return true;
        }

        /// <summary>
        /// 온버프 연동 요청
        /// </summary>
        public async Task RequestOnBuffAccountLink(string innoUID)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", innoUID);
            var response = await Protocol.REQUEST_ONBUFF_ACCOUNT_LINK.SendAsync(sfs);

            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.INNO_UID_EXPIRE_COOL)
                {
                    int remainSeconds = (int)response.GetLong("2"); // 남은 시간 (초)
                    string message = StringBuilderPool.Get()
                        .Append(response.resultCode.GetDescription())
                        .AppendLine()
                        .Append('(').Append(remainSeconds.ToTatalHourTime()).Append(')')
                        .Release();
                    UI.ConfirmPopup(message);
                    return;
                }

                response.ShowResultCode();
                return;
            }

            InnoUID = innoUID;
            IsOnBuffLogin = true;
            NotifyOnBuffPoint(response.GetInt("1"));

            OnBuffAccountLink?.Invoke();
        }

        /// <summary>
        /// 온버프 연동 해제
        /// </summary>
        /// <returns></returns>
        public async Task RequestOnBuffAccountUnLink()
        {
            var response = await Protocol.REQUEST_ONBUFF_ACCOUNT_UNLINK.SendAsync();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            InnoUID = string.Empty; // 온버프 연동 해제 시 InnoUID 초기화
            IsOnBuffLogin = false;
            NotifyOnBuffPoint(0);

            OnBuffAccountUnLink?.Invoke();
        }

        /// <summary>
        /// 지급 가능한 온버프 포인트 없음
        /// </summary>
        private void OnReceiveOnBuffPointZero(Response response)
        {
            UI.ShowToastPopup(LocalizeKey._90324.ToText()); // 지급 가능한 OnBuff Point가 모두 소진되었습니다.
        }
    }
}