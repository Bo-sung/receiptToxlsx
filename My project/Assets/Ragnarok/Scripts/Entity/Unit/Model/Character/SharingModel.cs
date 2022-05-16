using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using Ragnarok.View.CharacterShare;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class SharingModel : CharacterEntityModel, IEqualityComparer<SharingModel.CloneCharacterType>
    {
        private const string TAG = nameof(SharingModel);

        /// <summary>
        /// 셰어 중지 응답 대기 시간
        /// </summary>
        private const float WAIT_TIME_TO_SHARE_FORCE_QUIT = 10f;

        public enum SharingState : byte
        {
            /// <summary>
            /// 내 캐릭터 공유상태 아님
            /// </summary>
            None = 0,
            /// <summary>
            /// 내 캐릭터 공유중
            /// </summary>
            Sharing = 1,
            /// <summary>
            /// 내 캐릭터 공유보상 대기상태
            /// </summary>
            StandByReward = 2,
        }

        public enum CharacterShareFlag : byte
        {
            /// <summary>
            /// 캐릭터 셰어 사용중지
            /// </summary>
            Cancel = 0,
            /// <summary>
            /// 캐릭터 셰어 사용
            /// </summary>
            Use = 1,
            /// <summary>
            /// 캐릭터 셰어 전체종료
            /// </summary>
            Release = 2,
            /// <summary>
            /// 더미로 자동장착
            /// </summary>
            AutoUseOnlyDummy = 3,
            /// <summary>
            /// 자동장착
            /// </summary>
            AutoUse = 4,
        }

        public enum SharingCharacterType : byte
        {
            /// <summary>
            /// 일반
            /// </summary>
            Normal = 1,
            /// <summary>
            /// 더미 (AngetData 이용)
            /// </summary>
            Dummy = 2,
        }

        public enum CloneCharacterType : byte
        {
            /// <summary>
            /// 내 캐릭터
            /// </summary>
            MyCharacter = 1,
            /// <summary>
            /// 길드원
            /// </summary>
            GuildCharacter = 2,
            /// <summary>
            /// 친구
            /// </summary>
            Friend = 3,
        }

        private struct RequestShareCharacterListKey
        {
            public bool isFirstSend;
            public int? chapter, offset;

            public void ResetData()
            {
                isFirstSend = true;
                chapter = null;
                offset = null;
            }

            public void SetData(int? chapter, int? offset)
            {
                isFirstSend = false;
                this.chapter = chapter;
                this.offset = offset;
            }

            public bool CanSendProtocol()
            {
                return isFirstSend || HasNextPage();
            }

            public bool HasNextPage()
            {
                return chapter.HasValue && offset.HasValue;
            }
        }

        private const int COUNT_OF_SHARING_CHARACTER_LIST_PER_PAGE = 5; // 한 페이지 당 목록 수 (5)

        private readonly SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl;
        private readonly ItemDataManager.IItemDataRepoImpl itemDataRepoImpl;
        private readonly StageDataManager.IStageDataRepoImpl stageDataRepoImpl;
        private readonly ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl;
        private readonly AgentDataManager agentDataRepo;
        private readonly ShareDataManager shareDataRepo;
        private readonly Dictionary<int, BattleSharingCharacterPacket> sharingCharacterDic; // 사용중인 셰어캐릭터 dic
        private readonly Dictionary<int, DummySharingCharacter> dummySharingCharacterDic; // 사용중인 더미셰어캐릭터 dic
        private readonly Dictionary<CloneCharacterType, BattleSharingCharacterPacket> cloneCharacterDic; // 사용중인 클론캐릭터 dic
        private readonly BetterList<IBattleSharingCharacter> sharingCharacterList; // 실제 사용중인 셰어캐릭터 목록
        private readonly List<UISimpleCharacterShareBar.IInput> shareCharacterList; // 셰어캐릭터 대기 목록
        private readonly Buffer<IBattleSharingCharacter> sharingCharacterBuffer;
        private readonly RemainTimeStopwatch remainTimeForShare; // 공유 가능 남은시간
        private readonly List<(int uid, int cid)> savedSharingCharacterTupleList;
        private readonly List<int> savedAgentSharingCharacterIdList;
        private readonly Dictionary<CloneCharacterType, (int uid, int cid)> savedCloneCharacterTupleDic;

        /// <summary>
        /// 내 캐릭터 셰어상태 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateSharingState;
        /// <summary>
        /// 내 캐릭터 셰어보상 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateSharingRewards;
        /// <summary>
        /// 고용주 정보 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateEmployer;
        /// <summary>
        /// 사용중인 셰어캐릭터 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateSharingCharacters;
        /// <summary>
        /// 사용중인 셰어캐릭터 추가 시 호출
        /// </summary>
        public event System.Action<IBattleSharingCharacter[]> OnAddSharingCharacters;
        /// <summary>
        /// 사용중인 셰어캐릭터 제거 시 호출
        /// </summary>
        public event System.Action<int> OnRemoveSharingCharacter;
        /// <summary>
        /// 사용중인 셰어캐릭터 초기화 시 호출
        /// </summary>
        public event System.Action OnClearSharingCharacters;
        /// <summary>
        /// 셰어캐릭터 사용가능 남은시간 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateRemainTimeForShare;
        /// <summary>
        /// 셰어캐릭터 무료충전티켓 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateShareFreeTicket;
        /// <summary>
        /// 셰어캐릭터 충전티켓 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateShareTicketCount;
        /// <summary>
        /// 셰어캐릭터 목록 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateShareCharacterList;
        /// <summary>
        /// 종료 시 셰어 자동 등록 설정 변경 시 호출
        /// </summary>
        public event System.Action OnUpdateShareExitAutoSetting;
        /// <summary>
        /// 셰어 필터 업데이트 후 셰어필터UI 닫을때 호출
        /// </summary>
        public event System.Action OnHideShareFilterUI;
        /// <summary>
        /// 셰어바이스 경험치 증가 및 레벨업
        /// </summary>
        public event System.Action OnUpdateShareviceExperience;
        /// <summary>
        /// 셰어 정산시 버프로 추가보상 받을때 호출
        /// </summary>
        public event System.Action<bool, bool, bool> OnUpdateShareAddReward;
        /// <summary>
        /// 사용중인 클론캐릭터 추가 시 호출
        /// </summary>
        public event System.Action<IBattleSharingCharacter> OnAddCloneCharacter;
        /// <summary>
        /// 사용중인 클론캐릭터 제거 시 호출
        /// </summary>
        public event System.Action<CloneCharacterType> OnRemoveCloneCharacter;

        private ObscuredInt dailyFreeShareTicket;
        private ObscuredInt minutes10ShareTicket;
        private ObscuredInt minutes30ShareTicket;
        private ObscuredInt minutes60ShareTicket;
        private ObscuredInt shareviceLevel;
        private ObscuredInt shareviceExp;
        private ObscuredInt shareviceTempExp; // 셰어바이스 레벨업에 사용된 경험치
        private RemainTime shareviceLevelUpRemainTime; // 셰어바이스 레벨업까지 남은시간
        private SharingState sharingState; // 내 캐릭터 셰어상태
        private SharingRewardPacket sharingRewardPacket; // 셰어로 인한 보상
        private SharingEmployerPacket sharingEmployerPacket; // 내 캐릭터를 셰어하고 있는 고용주
        private JobFilter[] shareJobFilterAry; // 셰어 자동장착 필터
        private RequestShareCharacterListKey requestShareCharacterListKey;
        private bool isShareExitAutoSetting;

        public SharingModel()
        {
            skillDataRepoImpl = SkillDataManager.Instance;
            itemDataRepoImpl = ItemDataManager.Instance;
            stageDataRepoImpl = StageDataManager.Instance;
            profileDataRepoImpl = ProfileDataManager.Instance;
            agentDataRepo = AgentDataManager.Instance;
            shareDataRepo = ShareDataManager.Instance;
            sharingCharacterDic = new Dictionary<int, BattleSharingCharacterPacket>(IntEqualityComparer.Default);
            dummySharingCharacterDic = new Dictionary<int, DummySharingCharacter>(IntEqualityComparer.Default);
            cloneCharacterDic = new Dictionary<CloneCharacterType, BattleSharingCharacterPacket>(this);
            sharingCharacterList = new BetterList<IBattleSharingCharacter>();
            shareCharacterList = new List<UISimpleCharacterShareBar.IInput>();
            sharingCharacterBuffer = new Buffer<IBattleSharingCharacter>();
            savedSharingCharacterTupleList = new List<(int uid, int cid)>();
            savedAgentSharingCharacterIdList = new List<int>();
            savedCloneCharacterTupleDic = new Dictionary<CloneCharacterType, (int uid, int cid)>(this);

            shareJobFilterAry = new JobFilter[Constants.Size.SHARE_SLOT_SIZE];

            remainTimeForShare = new RemainTimeStopwatch();
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.RECEIVE_SHARE_CHAR_SETTING_CANCEL.AddEvent(OnReceiveShareCharacterSettingCancel);
                Protocol.RECEIVE_SHARE_CHAR_USE_SETTING_STOP.AddEvent(OnReceiveShareCharacterUseSettingStop);
                Protocol.RECEIVE_SHARE_CHAR_USE_SETTING_START.AddEvent(OnReceiveShareCharacterUseSettingStart);
                Protocol.RECEIVE_SHARE_CHAR_SETTING_CANCEL_OK.AddEvent(OnReceiveShareCharacterSettingCancelOk);
                Protocol.RECEIVE_SHARE_CHAR_SETTING_CANCEL_FAIL.AddEvent(OnReceiveShareCharacterSettingCancelFail);

                ConnectionManager.OnServerChange += OnServerChange;
                ConnectionManager.OnSuccessServerChange += OnSuccessServerChange;
                AppPauseManager.OnAppPause += OnAppPause;
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.RECEIVE_SHARE_CHAR_SETTING_CANCEL.RemoveEvent(OnReceiveShareCharacterSettingCancel);
                Protocol.RECEIVE_SHARE_CHAR_USE_SETTING_STOP.RemoveEvent(OnReceiveShareCharacterUseSettingStop);
                Protocol.RECEIVE_SHARE_CHAR_USE_SETTING_START.RemoveEvent(OnReceiveShareCharacterUseSettingStart);
                Protocol.RECEIVE_SHARE_CHAR_SETTING_CANCEL_OK.RemoveEvent(OnReceiveShareCharacterSettingCancelOk);
                Protocol.RECEIVE_SHARE_CHAR_SETTING_CANCEL_FAIL.RemoveEvent(OnReceiveShareCharacterSettingCancelFail);

                ConnectionManager.OnServerChange -= OnServerChange;
                ConnectionManager.OnSuccessServerChange -= OnSuccessServerChange;
                AppPauseManager.OnAppPause -= OnAppPause;
            }
        }

        public override void ResetData()
        {
            base.ResetData();

            ClearData();
            ClearSavedSharingCharacter();
            ResetCharacterList(); // 캐릭터 리스트 초기화 (사용중인 셰어캐릭터 변경되면 캐릭터 리스트를 한 번씩 초기화 시킴)
            dailyFreeShareTicket = 0;
            minutes10ShareTicket = 0;
            minutes30ShareTicket = 0;
            minutes60ShareTicket = 0;
            shareviceLevel = 0;
            shareviceExp = 0;
            sharingRewardPacket = null;
            sharingEmployerPacket = null;

            StopShareForceQuit();
        }

        /// <summary>
        /// 사용중인 셰어캐릭터 초기화
        /// </summary>
        public void ClearData()
        {
            sharingCharacterList.Clear();
            sharingCharacterDic.Clear();
            dummySharingCharacterDic.Clear();
            cloneCharacterDic.Clear();
            OnClearSharingCharacters?.Invoke();
            InvokeUpdateSharingCharacters();
        }

        private void ClearSavedSharingCharacter()
        {
            savedSharingCharacterTupleList.Clear(); // 저장된 셰어링캐릭터 초기화
            savedAgentSharingCharacterIdList.Clear(); // 저장된 더미셰어링캐릭터 초기화
            savedCloneCharacterTupleDic.Clear(); // 저장된 클론캐릭터 초기화
        }

        public void ResetCharacterList()
        {
            requestShareCharacterListKey.ResetData();
            shareCharacterList.Clear();
            InvokeUpdateShareCharacterList();
        }

        internal void Initialize(CharacterPacket characterPacket)
        {
            SetRemainTimeForShare(characterPacket.shareCharUseTimeSec);
            SetDailyFreeShareTicket(characterPacket.shareCharUseDailyTicket);
            Update(characterPacket.shareCharUse10mTicket, characterPacket.shareCharUse30mTicket, characterPacket.shareCharUse60mTicket);
            SetShareviceExperience(characterPacket.shareviceLevel, characterPacket.shareviceExp);

            shareviceTempExp = characterPacket.shareTempExp;
            shareviceLevelUpRemainTime = characterPacket.shareLevelupRemainTime;
        }

        internal void SetSharingState(SharingState sharingState)
        {
            ClearData(); // 데이터 초기화

            if (this.sharingState == sharingState)
                return;

            this.sharingState = sharingState;
            OnUpdateSharingState?.Invoke();
        }

        internal void SetSharingReward(SharingRewardPacket sharingRewardPacket)
        {
            this.sharingRewardPacket = sharingRewardPacket;

            if (this.sharingRewardPacket != null)
                this.sharingRewardPacket.Initialize(itemDataRepoImpl);

            OnUpdateSharingRewards?.Invoke();
        }

        internal void SetSharingEmployer(SharingEmployerPacket sharingEmployerPacket)
        {
            this.sharingEmployerPacket = sharingEmployerPacket;

            if (this.sharingEmployerPacket != null)
            {
                this.sharingEmployerPacket.Initialize(stageDataRepoImpl);
            }

            OnUpdateEmployer?.Invoke();
        }

        internal void SetSharingFilter(int[] filterAry)
        {
            for (int i = 0; i < shareJobFilterAry.Length; i++)
            {
                if (i < filterAry.Length)
                {
                    shareJobFilterAry[i] = filterAry[i].ToEnum<JobFilter>();
                }
                else
                {
                    shareJobFilterAry[i] = JobFilter.None;
                }
            }
        }

        internal void SetShareviceExperience(int? level, int? exp)
        {
            bool isDirtyLevel = shareviceLevel.Replace(level);
            bool isDirtyExp = shareviceExp.Replace(exp);

            if (isDirtyLevel || isDirtyExp)
                OnUpdateShareviceExperience?.Invoke();
        }

        internal void SetDailyFreeShareTicket(int dailyFreeShareTicket)
        {
            bool isDirtyFree = this.dailyFreeShareTicket.Replace(dailyFreeShareTicket);

            if (isDirtyFree)
                OnUpdateShareFreeTicket?.Invoke();
        }

        internal void SetShareExitAutoSetting(bool isShareExitAutoSetting)
        {
            this.isShareExitAutoSetting = isShareExitAutoSetting;
            OnUpdateShareExitAutoSetting?.Invoke();
        }

        internal void Update(int? minutes10ShareTicket, int? minutes30ShareTicket, int? minutes60ShareTicket)
        {
            bool isDirtyMinutes10 = this.minutes10ShareTicket.Replace(minutes10ShareTicket);
            bool isDirtyMinutes30 = this.minutes30ShareTicket.Replace(minutes30ShareTicket);
            bool isDirtyMinutes60 = this.minutes60ShareTicket.Replace(minutes60ShareTicket);

            if (isDirtyMinutes10 || isDirtyMinutes30 || isDirtyMinutes60)
                OnUpdateShareTicketCount?.Invoke();
        }

        void OnAppPause(bool isPause)
        {
            if (isPause)
            {
                RequestShareCharRewardCalcAll().WrapNetworkErrors();
            }
            else
            {
                RequestShareUseCharList().WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 내 캐릭터 셰어상태 반환
        /// </summary>
        public SharingState GetSharingState()
        {
            return sharingState;
        }

        /// <summary>
        /// 내 캐릭터 셰어보상 반환
        /// </summary>
        public SharingRewardPacket GetSharingRewardPacket()
        {
            return sharingRewardPacket;
        }

        /// <summary>
        /// 고용주 정보 반환
        /// </summary>
        public SharingEmployerPacket GetEmployerInfo()
        {
            return sharingEmployerPacket;
        }

        /// <summary>
        /// 셰어 자동장착 필터정보
        /// </summary>
        public JobFilter[] GetShareJobFilterAry()
        {
            return shareJobFilterAry;
        }

        /// <summary>
        /// 쉐어바이스 레벨
        /// </summary>
        public int GetShareviceLevel()
        {
            return shareviceLevel;
        }

        /// <summary>
        /// 선택중인 아이템을 포함한 레벨
        /// </summary>
        /// <param name="selectedExp"> 선택한 아이템 경험치</param>
        /// <param name="jobLevel">캐릭터 잡 레벨</param>
        /// <returns></returns>
        public int GetShareviceLevel(int selectedExp, int jobLevel)
        {
            for (int level = GetShareviceLevel(); level <= jobLevel; level++)
            {
                if (shareviceExp + selectedExp < shareDataRepo.Get(level).need_exp)
                    return level;
            }

            Debug.LogError("________이거 타면 문제가 있음..");
            return jobLevel;
        }

        public long GetLevelUpTotalRemainTime(int targetLevel)
        {
            return shareDataRepo.Get(targetLevel).need_time;
        }
        /// <summary>
        /// 쉐어바이스 현제 경험치
        /// </summary>
        /// <returns></returns>
        public int GetShareviceExp()
        {
            return shareviceExp - GetShareviceCumulateExp();
        }
        /// <summary>
        /// 쉐어바이스 현제 레벨에서 다음 레벨까지 남은 총 경험치 반환
        /// </summary>
        /// <returns></returns>
        public int GetShareviceNeedExp()
        {
            return shareDataRepo.Get(GetShareviceLevel()).need_exp - GetShareviceCumulateExp();
        }
        /// <summary>
        /// 쉐어바이스 누적 경험치 반환
        /// </summary>
        public int GetShareviceCumulateExp()
        {
            if (GetShareviceLevel() > 1)
            {
                return shareDataRepo.Get(GetShareviceLevel() - 1).need_exp;
            }
            else
            {
                return 0;
            }
        }

        // 셰어바이스 최대 레벨(캐릭터 잡레벨)까지 필요한 경험치(누적 제외)
        /// <summary>
        /// 쉐어바이스 현제 레벨에서 최대 도달가능 레벨(캐릭터 잡 레벨)까지 힐요한 경험치
        /// </summary>
        /// <param name="jobLevel"></param>
        /// <returns></returns>
        public int GetMaxShareviceExp(int jobLevel)
        {
            return shareDataRepo.Get(jobLevel).need_exp - GetShareviceCumulateExp();
        }

        /// <summary>
        /// 쉐어바이스 최대 출력 전투력
        /// </summary>
        public int GetShareviceMaxBP(int addLevel = 0)
        {
            return shareDataRepo.Get(GetShareviceLevel() + addLevel).max_battle_score;
        }

        /// <summary>
        /// 현재 사용중인 셰어링 캐릭터 존재 유무
        /// </summary>
        public bool HasSharingCharacters()
        {
            return GetSharingCharacterSize() > 0;
        }

        /// <summary>
        /// 현재 사용중인 셰어링 캐릭터 수 (클론 캐릭터 포함)
        /// </summary>
        public int GetSharingCharacterSize()
        {
            return sharingCharacterList.size + cloneCharacterDic.Count;
        }

        /// <summary>
        /// 인덱스에 해당하는 클론 타입 반환
        /// </summary>
        public CloneCharacterType GetCloneCharacterType(int index)
        {
            if (index < Constants.Size.SHARE_SLOT_SIZE)
                return default;

            return (index - Constants.Size.SHARE_SLOT_SIZE + 1).ToEnum<CloneCharacterType>();
        }

        /// <summary>
        /// 클론 타입에 해당하는 인덱스 반환
        /// </summary>
        public int GetCloneCharacterIndex(CloneCharacterType cloneType)
        {
            if (cloneType == default)
                return -1;

            return (int)cloneType + Constants.Size.SHARE_SLOT_SIZE - 1;
        }

        /// <summary>
        /// 현재 사용중인 셰어링 캐릭터 반환
        /// </summary>
        public IBattleSharingCharacter[] GetSharingCharacters()
        {
            if (sharingCharacterList.size == 0)
                return System.Array.Empty<IBattleSharingCharacter>();

            return sharingCharacterList.ToArray();
        }

        /// <summary>
        /// 현재 사용중인 클론 캐릭터 반환
        /// </summary>
        public IBattleSharingCharacter[] GetCloneCharacters()
        {
            foreach (CloneCharacterType item in System.Enum.GetValues(typeof(CloneCharacterType)))
            {
                IBattleSharingCharacter info = GetCloneCharacter(item);
                if (info == null)
                    continue;

                sharingCharacterBuffer.Add(info);
            }

            return sharingCharacterBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 현재 사용중인 클론캐릭터 반환
        /// </summary>
        public IBattleSharingCharacter GetCloneCharacter(CloneCharacterType type)
        {
            return cloneCharacterDic.ContainsKey(type) ? cloneCharacterDic[type] : null;
        }

        /// <summary>
        /// 공유 가능 남은시간
        /// </summary>
        public float GetRemainTimeForShare()
        {
            return remainTimeForShare.ToRemainTime();
        }

        /// <summary>
        /// 공유 가능 시간 종료
        /// </summary>
        public bool IsFinishedSharingTime()
        {
            return remainTimeForShare.IsFinished();
        }

        /// <summary>
        /// 티켓 수 반환
        /// </summary>
        public int GetShareTicketCount(ShareTicketType ticketType)
        {
            switch (ticketType)
            {
                case ShareTicketType.DailyFree: return dailyFreeShareTicket;
                case ShareTicketType.ChargeItem1: return minutes10ShareTicket;
                case ShareTicketType.ChargeItem2: return minutes30ShareTicket;
                case ShareTicketType.ChargeItem3: return minutes60ShareTicket;
                default: throw new System.ArgumentException($"[올바르지 않은 {nameof(ShareTicketType)}] {nameof(ticketType)} = {ticketType}");
            }
        }

        /// <summary>
        /// 셰어 캐릭터 목록 반환
        /// </summary>
        public UISimpleCharacterShareBar.IInput[] GetShareCharacterList()
        {
            return shareCharacterList.ToArray();
        }

        /// <summary>
        /// 클론 캐릭터 목록 반환
        /// </summary>
        public UISimpleCharacterShareBar.IInput[] GetCloneCharacterList(SimpleCharacterPacket[] infos, int uid, int cid)
        {
            Buffer<UISimpleCharacterShareBar.IInput> buffer = new Buffer<UISimpleCharacterShareBar.IInput>();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].Cid == cid)
                    continue;

                infos[i].Initialize(uid);
                infos[i].Initialize(skillDataRepoImpl); // 스킬 세팅
                infos[i].Initialize(profileDataRepoImpl); // 프로필 세팅
                buffer.Add(infos[i]);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 종료 시 셰어 자동 등록 설정
        /// </summary>
        public bool IsShareExitAutoSetting()
        {
            return isShareExitAutoSetting;
        }

        public bool UpdatableShareFilter(JobFilter[] jobfilter)
        {
            for (int i = 0; i < shareJobFilterAry.Length; i++)
            {
                if (i < jobfilter.Length) // 필터가 있을 때
                {
                    if (shareJobFilterAry[i] == jobfilter[i]) continue;
                    else return true;
                }
                else // 필터가 비었을 때
                {
                    if (shareJobFilterAry[i] == JobFilter.None) continue;
                    else return true;
                }
            }

            return false;
        }

        public ShareviceState GetShareviceState()
        {
            // 레벨업 중..
            if (shareviceTempExp > 0)
            {
                if (shareviceLevelUpRemainTime.ToRemainTime() > 0) return ShareviceState.LevelUpProgress;
                else return ShareviceState.LevelUpComplete;
            }
            else
            {
                return ShareviceState.Normal;
            }
        }
        /// <summary>
        /// 셰어바이스 레벨업에 사용된 경험치 반환
        /// </summary>
        public int GetShareviceTempExp()
        {
            return shareviceTempExp;
        }
        /// <summary>
        /// 셰어바이스 레벨업까지 남은 시간 반환
        /// </summary>
        public RemainTime GetShareviceLevelUpRemainTime()
        {
            return shareviceLevelUpRemainTime;
        }

        /// <summary>
        /// 서버 변경 시 호출: 현재 셰어링 캐릭터 저장과 동시에 초기화
        /// </summary>
        private void OnServerChange(BattleMode pre, BattleMode cur)
        {
            if (!HasSharingCharacters())
                return;

            SaveShareCharacters();
            ClearData();
        }

        /// <summary>
        /// 서버 변경 완료 후 호출: 저장한 셰어링 캐릭터 다시 사용
        /// </summary>
        private void OnSuccessServerChange(BattleMode pre, BattleMode cur)
        {
            // 재접속 후 셰어캐릭터 사용
            if (cur.IsReuseSavedSharingCharacter())
                ReuseShareCharacters();
        }

        public async Task RequestShareCharacterSetting(bool isShare)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", isShare);

            if (!isShare)
            {
                StartShareForceQuit(); // 강제 종료 대기 (응답 없을 때를 대비)
            }

            Response response = await Protocol.REQUEST_SHARE_CHAR_SETTING.SendAsync(sfs);

            if (isShare)
            {
                Analytics.TrackEvent(TrackType.RegisterShare);
            }

            StopShareForceQuit(); // 강제 종료 대기 멈춤

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            SharingState sharingState = response.GetInt("1").ToEnum<SharingState>(); // 내 캐릭터 공유 상태
            SetSharingState(sharingState);

            if (response.ContainsKey("2"))
            {
                int remainTime = response.GetInt("2");
                SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅 (셰어 상태 후!) => 남은시간 흐르지 않는 처리 때문에
            }

            // 내 캐릭터 공유 중지 했을 때, 고용주의 상태가 Pause 일 경우에 보상 정보를 받을 수 있다.
            if (response.ContainsKey("3"))
            {
                SharingRewardPacket sharingRewardPacket = response.GetPacket<SharingRewardPacket>("3"); // 3. 보상 정보
                SetSharingReward(sharingRewardPacket); // 보상 세팅

                SetSharingEmployer(null); // 고용주 정보 초기화
            }
        }

        /// <summary>
        /// 종료 시 셰어 자동 등록 설정
        /// </summary>
        public async Task RequestShareExitAutoSetting()
        {
            const byte DO_NOT_USE_EXIT_AUTO_SETTING = 0; // 자동 설정 하지 않음
            const byte USE_EXIT_AUTO_SETTING = 1; // 자동 설정

            bool isUseExitAutoSetting = !isShareExitAutoSetting; // 자동 설정 여부
            if (!isUseExitAutoSetting)
            {
                string message = LocalizeKey._10239.ToText(); // 해제 시 게임 종료를 통해 셰어 등록이 되지 않습니다.\n해제하시겠습니까?
                if (!await UI.SelectPopup(message))
                    return;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", isUseExitAutoSetting ? USE_EXIT_AUTO_SETTING : DO_NOT_USE_EXIT_AUTO_SETTING);
            Response response = await Protocol.REQUEST_AUTO_SHARE_SETTING.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            SetShareExitAutoSetting(isUseExitAutoSetting); // 종료 시 셰어 자동 설정 값 세팅
        }

        public async Task RequestShareCharacterList(int stageID, int curChapter)
        {
            // 셰어 튜토리얼 중일 때에는 더미 캐릭터 보여주기
            if (Tutorial.current == TutorialType.SharingCharacterEquip)
            {
                DummySharingCharacter[] dummySharingCharacters = agentDataRepo.GetRandomDummySharingAgents(stageID, dummySharingCharacterDic.Keys, COUNT_OF_SHARING_CHARACTER_LIST_PER_PAGE);
                shareCharacterList.AddRange(dummySharingCharacters);
                InvokeUpdateShareCharacterList();
                return;
            }

            // 서버 프로토콜 보내지 않기
            if (!requestShareCharacterListKey.CanSendProtocol())
                return;

            var sfs = Protocol.NewInstance();
            if (requestShareCharacterListKey.HasNextPage())
            {
                sfs.PutInt("1", requestShareCharacterListKey.chapter.Value);
                sfs.PutInt("2", requestShareCharacterListKey.offset.Value);
            }

            Response response = await Protocol.REQUEST_SHARE_CHAR_LIST.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            int listCount = 0;
            if (response.ContainsKey("1"))
            {
                ShareCharacterPacket[] array = response.GetPacketArray<ShareCharacterPacket>("1");
                System.Array.Sort(array); // Sort
                foreach (var item in array)
                {
                    item.Initialize(skillDataRepoImpl); // 스킬 세팅
                    item.Initialize(profileDataRepoImpl); // 프로필 세팅
                }

                listCount = array.Length;
                shareCharacterList.AddRange(array);
            }

            int? chapter = null;
            int? offset = null;

            if (response.ContainsKey("2"))
                chapter = response.GetInt("2");

            if (response.ContainsKey("3"))
                offset = response.GetInt("3");

            int needCount = COUNT_OF_SHARING_CHARACTER_LIST_PER_PAGE - listCount;
            if (needCount > 0)
            {
                DummySharingCharacter[] dummySharingCharacters = agentDataRepo.GetRandomDummySharingAgents(stageID, dummySharingCharacterDic.Keys, needCount);
                shareCharacterList.AddRange(dummySharingCharacters);

                chapter = null;
                offset = null;
            }

            InvokeUpdateShareCharacterList();
            requestShareCharacterListKey.SetData(chapter, offset);
        }

        /// <summary>
        /// 길드 캐릭터 쉐어 목록
        /// </summary>
        public async Task RequestGuildShareCharacterList()
        {
            Response response = await Protocol.REQUEST_GUILD_SHARE_CHAR_LIST.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
            {
                ShareCharacterPacket[] array = response.GetPacketArray<ShareCharacterPacket>("1");
                System.Array.Sort(array); // Sort
                foreach (var item in array)
                {
                    item.Initialize(skillDataRepoImpl); // 스킬 세팅
                    item.Initialize(profileDataRepoImpl); // 프로필 세팅
                }

                shareCharacterList.AddRange(array);
            }

            InvokeUpdateShareCharacterList();
        }

        public async Task RequestShareCharacterUseSetting(CharacterShareFlag flag, int cid, int uid, SharingCharacterType sharingCharacterType)
        {
            // 셰어 튜토리얼 중일 때에는 더미 캐릭터 장착한 것처럼 보여주기
            if (Tutorial.current == TutorialType.SharingCharacterEquip)
            {
                int agentId = Mathf.Abs(cid);
                DummySharingCharacter data = agentDataRepo.GetDummySharingAgent(agentId);
                dummySharingCharacterDic.Add(agentId, data); // 사용중인 셰어캐릭터 추가
                sharingCharacterList.Add(data); // 리스트 추가
                ResetCharacterList(); // 캐릭터 리스트 초기화 (사용중인 셰어캐릭터 변경되면 캐릭터 리스트를 한 번씩 초기화 시킴)
                InvokeUpdateSharingCharacters();
                sharingCharacterList.Clear(); // 리스트 제거 (실제로는 없는 아이)
                dummySharingCharacterDic.Clear(); // 이벤트 호출과 동시에 더미 셰어링 제거 (실제로는 없는 아이)
                return;
            }

#if UNITY_EDITOR
            if (flag == CharacterShareFlag.Release)
                throw new System.ArgumentException($"[올바르지 않은 {nameof(flag)}] {nameof(RequestShareCharacterRelease)} 이용하세요!");

            if (flag == CharacterShareFlag.AutoUseOnlyDummy || flag == CharacterShareFlag.AutoUse)
                throw new System.ArgumentException($"[올바르지 않은 {nameof(flag)}] {nameof(RequestShareCharacterAutoUse)} 이용하세요!");
#endif
            if (flag == CharacterShareFlag.Cancel)
            {
                if (StageEntry.hasSummonMonster)
                {
                    UI.ShowToastPopup(LocalizeKey._90198.ToText()); // 소환된 몬스터 처치 후에 이용 가능합니다.
                    return;
                }
            }

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)flag);

            if (sharingCharacterType == SharingCharacterType.Normal)
            {
                sfs.PutInt("2", cid);
                sfs.PutInt("3", uid);
            }
            else if (sharingCharacterType == SharingCharacterType.Dummy)
            {
                int agentId = Mathf.Abs(cid);
                sfs.PutInt("2", agentId);
            }

            sfs.PutInt("5", (byte)sharingCharacterType);

            UI.ShowIndicator(); // 인디케이터 강제 제어 (특정 경우에만 인디케이터 보여주기)
            Response response = await Protocol.REQUEST_SHARE_CHAR_USE_SETTING.SendAsync(sfs);
            UI.HideIndicator();

            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.RELEASE_SHARE_CHAR_IS_ZERO)
                {
                    ClearData(); // 사용중인 셰어캐릭터 초기화 (먼저 할 것!)
                    return;
                }

                ResetCharacterList(); // 캐릭터 리스트 초기화 (셰어 리스트에서 사라졌을 경우에 대비한 Refresh)
                response.ShowResultCode();
                return;
            }

            int remainTime = response.GetInt("1");

            switch (flag)
            {
                case CharacterShareFlag.Cancel:
                    if (sharingCharacterType == SharingCharacterType.Normal)
                    {
                        sharingCharacterList.Remove(sharingCharacterDic[cid]); // 리스트 제거
                        sharingCharacterDic.Remove(cid); // 사용중인 셰어캐릭터 제거
                    }
                    else if (sharingCharacterType == SharingCharacterType.Dummy)
                    {
                        int agentId = Mathf.Abs(cid);
                        sharingCharacterList.Remove(dummySharingCharacterDic[agentId]); // 리스트 제거
                        dummySharingCharacterDic.Remove(agentId); // 사용중인 더미셰어캐릭터 제거
                    }
                    OnRemoveSharingCharacter?.Invoke(cid);
                    break;

                case CharacterShareFlag.Use:
                    Analytics.TrackEvent(TrackType.Share);

                    if (sharingCharacterType == SharingCharacterType.Normal)
                    {
                        if (response.ContainsKey("2"))
                        {
                            BattleSharingCharacterPacket[] packets = response.GetPacketArray<BattleSharingCharacterPacket>("2");
                            foreach (var item in packets)
                            {
                                item.Initialize(skillDataRepoImpl); // 스킬 세팅
                                item.Initialize(profileDataRepoImpl); // 프로필 세팅

                                sharingCharacterDic.Add(item.Cid, item); // 사용중인 셰어캐릭터 추가
                                sharingCharacterList.Add(item); // 리스트 추가
                                sharingCharacterBuffer.Add(item);
                            }

                            Quest.QuestProgress(QuestType.SHARE_CHAR_USE_COUNT); // 퀘스트 처리: 셰어 캐릭터 고용 횟수
                            IBattleSharingCharacter[] sharingCharacters = sharingCharacterBuffer.GetBuffer(isAutoRelease: true);
                            OnAddSharingCharacters?.Invoke(sharingCharacters);
                        }
                    }
                    else if (sharingCharacterType == SharingCharacterType.Dummy)
                    {
                        int agentId = Mathf.Abs(cid);
                        DummySharingCharacter data = agentDataRepo.GetDummySharingAgent(agentId);
                        dummySharingCharacterDic.Add(agentId, data); // 사용중인 셰어캐릭터 추가
                        sharingCharacterList.Add(data); // 리스트 추가
                        sharingCharacterBuffer.Add(data);
                        Quest.QuestProgress(QuestType.SHARE_CHAR_USE_COUNT); // 퀘스트 처리: 셰어 캐릭터 고용 횟수
                        IBattleSharingCharacter[] sharingCharacters = sharingCharacterBuffer.GetBuffer(isAutoRelease: true);
                        OnAddSharingCharacters?.Invoke(sharingCharacters);
                    }
                    break;
            }

            ResetCharacterList(); // 캐릭터 리스트 초기화 (사용중인 셰어캐릭터 변경되면 캐릭터 리스트를 한 번씩 초기화 시킴)
            InvokeUpdateSharingCharacters();
            SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅 (사용중인 셰어캐릭터 세팅 후!) => 남은시간 흐르지 않는 처리 때문에
        }

        public async Task RequestShareCharacterRelease(bool isSave)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)CharacterShareFlag.Release);

            // 시간 만료에 따른 종료가 아닐 경우
            if (isSave)
            {
                sfs.PutByte("7", 1);
            }

            Response response = await Protocol.REQUEST_SHARE_CHAR_USE_SETTING.SendAsync(sfs);
            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.RELEASE_SHARE_CHAR_IS_ZERO)
                {
                    ClearData(); // 사용중인 셰어캐릭터 초기화 (먼저 할 것!)
                    return;
                }

                response.ShowResultCode();
                return;
            }

            int remainTime = response.GetInt("1");

            if (isSave)
            {
                SaveShareCharacters();
            }

            ClearData(); // 사용중인 셰어캐릭터 초기화 (먼저 할 것!)
            SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅 (사용중인 셰어캐릭터 세팅 후!) => 남은시간 흐르지 않는 처리 때문에
        }

        public async Task RequestShareCharacterAutoUse(bool isOnlyDummy)
        {
            var sfs = Protocol.NewInstance();
            CharacterShareFlag flag = isOnlyDummy ? CharacterShareFlag.AutoUseOnlyDummy : CharacterShareFlag.AutoUse;
            sfs.PutByte("1", (byte)flag);

            UI.ShowIndicator(); // 인디케이터 강제 제어 (특정 경우에만 인디케이터 보여주기)
            Response response = await Protocol.REQUEST_SHARE_CHAR_USE_SETTING.SendAsync(sfs);
            Analytics.TrackEvent(TrackType.Share);
            UI.HideIndicator();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            int remainTime = response.GetInt("1");

            switch (flag)
            {
                case CharacterShareFlag.AutoUseOnlyDummy:
                    if (response.ContainsKey("3"))
                    {
                        int[] dummyAgentIds = response.GetIntArray("3");
                        foreach (int dummyAgentId in dummyAgentIds)
                        {
                            DummySharingCharacter data = agentDataRepo.GetDummySharingAgent(dummyAgentId);
                            dummySharingCharacterDic.Add(dummyAgentId, data); // 사용중인 셰어캐릭터 추가
                            sharingCharacterList.Add(data); // 리스트 추가
                            sharingCharacterBuffer.Add(data);
                        }

                        Quest.QuestProgress(QuestType.SHARE_CHAR_USE_COUNT); // 퀘스트 처리: 셰어 캐릭터 고용 횟수
                        IBattleSharingCharacter[] sharingCharacters = sharingCharacterBuffer.GetBuffer(isAutoRelease: true);
                        OnAddSharingCharacters?.Invoke(sharingCharacters);
                    }
                    break;

                case CharacterShareFlag.AutoUse:
                    if (response.ContainsKey("2"))
                    {
                        BattleSharingCharacterPacket[] packets = response.GetPacketArray<BattleSharingCharacterPacket>("2");
                        foreach (var item in packets)
                        {
                            item.Initialize(skillDataRepoImpl); // 스킬 세팅
                            item.Initialize(profileDataRepoImpl); // 프로필 세팅

                            sharingCharacterDic.Add(item.Cid, item); // 사용중인 셰어캐릭터 추가
                            sharingCharacterList.Add(item); // 리스트 추가
                            sharingCharacterBuffer.Add(item);
                        }
                    }

                    if (response.ContainsKey("3"))
                    {
                        int[] dummyAgentIds = response.GetIntArray("3");
                        foreach (int dummyAgentId in dummyAgentIds)
                        {
                            DummySharingCharacter data = agentDataRepo.GetDummySharingAgent(dummyAgentId);
                            dummySharingCharacterDic.Add(dummyAgentId, data); // 사용중인 셰어캐릭터 추가
                            sharingCharacterList.Add(data); // 리스트 추가
                            sharingCharacterBuffer.Add(data);
                        }
                    }

                    if (sharingCharacterBuffer.size > 0)
                    {
                        Quest.QuestProgress(QuestType.SHARE_CHAR_USE_COUNT); // 퀘스트 처리: 셰어 캐릭터 고용 횟수
                        IBattleSharingCharacter[] sharingCharacters = sharingCharacterBuffer.GetBuffer(isAutoRelease: true);
                        OnAddSharingCharacters?.Invoke(sharingCharacters);
                    }
                    break;
            }

            ResetCharacterList(); // 캐릭터 리스트 초기화 (사용중인 셰어캐릭터 변경되면 캐릭터 리스트를 한 번씩 초기화 시킴)
            InvokeUpdateSharingCharacters();
            SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅 (사용중인 셰어캐릭터 세팅 후!) => 남은시간 흐르지 않는 처리 때문에
        }

        public async Task RequestShareCharacterRewardInfo()
        {
            Response response = await Protocol.REQUEST_SHARE_CHAR_REWARD_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
            {
                SharingRewardPacket sharingRewardPacket = response.GetPacket<SharingRewardPacket>("1"); // 1. 보상 정보
                SetSharingReward(sharingRewardPacket); // 보상 세팅
            }
        }

        public async Task RequestShareCharacterRewardGet()
        {
            Response response = await Protocol.REQUEST_SHARE_CHAR_REWARD_GET.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            // 쉐어 버프로 인해 추가 보상 체크
            bool isZeny = response.ContainsKey("1");
            bool isBaseExp = response.ContainsKey("2");
            bool isJobExp = response.ContainsKey("3");
            OnUpdateShareAddReward?.Invoke(isZeny, isBaseExp, isJobExp);

            SetSharingState(SharingState.None); // 상태 변경
            SetSharingReward(null); // 보상 초기화
            SetSharingEmployer(null); // 고용주 정보 초기화
        }

        public async Task RequestShareCharacterTimeTicketUse(ShareTicketType ticketType)
        {
            // 셰어링캐릭터장착 튜토리얼의 경우에는 굳이 팝업을 보여주지 않습니다.
            if (Tutorial.current == TutorialType.SharingCharacterEquip)
            {
                // Do Nothing
            }
            else
            {
                int timeRate = Mathf.Max(1, GetSharingCharacterSize()); // 시간 배속 고려
                var currentTimeSpan = (GetRemainTimeForShare() * timeRate).ToTimeSpan();
                var maxShareTimeSpan = System.TimeSpan.FromMilliseconds(BasisType.MAX_USE_CHAR_SHARE_TIME.GetInt());
                if (currentTimeSpan.Add(ticketType.ToTimeSpan()) > maxShareTimeSpan)
                {
                    string notice = LocalizeKey._10228.ToText(); // 최대 누적 가능 시간을 초과합니다.\n초과한 시간은 충전 되지 않습니다.\n\n계속하시겠습니까?
                    if (!await UI.SelectPopup(notice))
                        return;
                }
            }

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)ticketType);
            Response response = await Protocol.REQUEST_SHARE_CHAR_TIME_TICKET_USE.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            int remainTime = response.GetInt("1");
            SetRemainTimeForShare(remainTime);

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            if (ticketType == ShareTicketType.DailyFree)
                SetDailyFreeShareTicket(dailyFreeShareTicket - 1);

            if (Tutorial.current == TutorialType.SharingCharacterEquip)
            {
                // Do Nothing
            }
            else
            {
                string message = LocalizeKey._10226.ToText(); // 이용 시간이 정상적으로 충전 되었습니다.
                UI.ShowToastPopup(message);
            }
        }

        public async Task RequestShareJobFilter(JobFilter[] newFilterAry)
        {
            // 비어있는 슬롯은 제외..
            newFilterAry = newFilterAry.Where(v => v > 0).ToArray();

            // 기존 필터설정과 똑같으면 UI만 닫음
            if (!UpdatableShareFilter(newFilterAry))
            {
                OnHideShareFilterUI?.Invoke();
                return;
            }

            var intAry = System.Array.ConvertAll(newFilterAry, value => (int)value);

            var sfs = Protocol.NewInstance();
            sfs.PutIntArray("1", intAry);
            Response response = await Protocol.REQUEST_SHARE_JOB_FILTER.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            SetSharingFilter(intAry);

            // 필터가 갱신된 후에 UI 를 닫음.
            OnHideShareFilterUI?.Invoke();
        }
        /// <summary>
        /// 쉐어바이스 레벨업 요청
        /// </summary>
        /// <param name="shareviceItems"> 쉐어바이스 아이템 목록</param>
        /// <param name="itemsExp">아이템 경험치</param>
        /// <returns></returns>
        public async Task RequestShareviceLevelUp(ShareviceItem[] shareviceItems, int itemsExp)
        {
            if (shareviceItems.Length == 0)
                return;

            var sfs = Protocol.NewInstance();
            var sfsArray = Protocol.NewArrayInstance();
            foreach (var item in shareviceItems)
            {
                var element = Protocol.NewInstance();
                element.PutInt("1", item.itemId);
                element.PutInt("2", item.SelectedCount);
                sfsArray.AddSFSObject(element);
            }

            sfs.PutSFSArray("1", sfsArray);
            Response response = await Protocol.REQUEST_SHARE_VICE_LEVELUP.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            // 셰어바이스 레벨이 변경되는지..
            if (response.ContainsKey("1"))
            {
                shareviceTempExp = itemsExp;
                shareviceLevelUpRemainTime = response.GetLong("1");
            }

            OnUpdateShareviceExperience?.Invoke();
        }
        /// <summary>
        /// 쉐어바이스 레벨업 즉시완료 요청
        /// </summary>
        /// <returns></returns>
        public async Task RequestShareviceLevelUpComplete()
        {
            Response response = await Protocol.REQUEST_SHARE_VICE_LEVELUP_COMP.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            // 레벨업 관련 데이터 초기화
            shareviceTempExp = 0;
            shareviceLevelUpRemainTime = 0;

            OnUpdateShareviceExperience?.Invoke();
        }

        public async Task RequestShareCloneCharacter(CloneCharacterType cloneType, int uid, int cid)
        {
            if (cloneCharacterDic.ContainsKey(cloneType))
            {
                ResultCode.DUPLICATION_FAIL.ShowResultCode();
                return;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)cloneType);
            sfs.PutInt("2", uid);
            sfs.PutInt("3", cid);
            Response response = await Protocol.REQUEST_CLONE_SHARE.SendAsync(sfs);
            Analytics.TrackEvent(TrackType.Share);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
            {
                BattleSharingCharacterPacket packet = response.GetPacket<BattleCloneCharacterPacket_MyCharacter>("1");
                packet.Initialize(skillDataRepoImpl); // 스킬 세팅
                packet.Initialize(profileDataRepoImpl); // 프로필 세팅

                cloneCharacterDic.Add(cloneType, packet);

                Quest.QuestProgress(QuestType.SHARE_CHAR_USE_COUNT); // 퀘스트 처리: 셰어 캐릭터 고용 횟수
                OnAddCloneCharacter?.Invoke(packet);
            }

            int remainTime = response.GetInt("2");

            InvokeUpdateSharingCharacters();
            SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅 (사용중인 셰어캐릭터 세팅 후!) => 남은시간 흐르지 않는 처리 때문에
        }

        public async Task RequestReleaseCloneCharacter(CloneCharacterType cloneType)
        {
            if (!cloneCharacterDic.ContainsKey(cloneType))
            {
                ResultCode.FAIL.ShowResultCode();
                return;
            }

            if (StageEntry.hasSummonMonster)
            {
                UI.ShowToastPopup(LocalizeKey._90198.ToText()); // 소환된 몬스터 처치 후에 이용 가능합니다.
                return;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)cloneType);
            Response response = await Protocol.REQUEST_CLONE_SHARE_RELEASE.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            int remainTime = response.GetInt("1");
            cloneCharacterDic.Remove(cloneType);

            OnRemoveCloneCharacter?.Invoke(cloneType);

            InvokeUpdateSharingCharacters();
            SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅 (사용중인 셰어캐릭터 세팅 후!) => 남은시간 흐르지 않는 처리 때문에
        }

        private async Task RequestShareCharRewardCalcAll()
        {
            // 실제 사용중인 캐릭터가 있을 경우에만 처리 (더미 제외)
            // (애플리케이션 Sleep 상태일 때 셰어취소한 유저가 있을 수 있기 때문에 호출)
            if (sharingCharacterDic.Count == 0)
                return;

            Response response = await Protocol.REQUEST_SHARE_CHAR_REWARD_CALC_ALL.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }
        }

        private async Task RequestShareUseCharList()
        {
            // 실제 사용중인 캐릭터가 있을 경우에만 처리 (더미 제외)
            // (애플리케이션 Sleep 상태일 때 셰어취소한 유저가 있을 수 있기 때문에 호출)
            if (sharingCharacterDic.Count == 0)
                return;

            Response response = await Protocol.REQUEST_SHARE_USE_CHAR_LIST.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            bool isDirty = false;
            if (response.ContainsKey("1"))
            {
                isDirty = true;
                int[] releaseCids = response.GetIntArray("1");
                foreach (var cid in releaseCids)
                {
                    sharingCharacterList.Remove(sharingCharacterDic[cid]); // 리스트 제거
                    sharingCharacterDic.Remove(cid); // 사용중인 셰어캐릭터 제거
                    OnRemoveSharingCharacter?.Invoke(cid);
                }
#if UNITY_EDITOR
                var sb = StringBuilderPool.Get();
                sb.Append("지워지는 셰어캐릭터 수: ").Append(releaseCids.Length);
                foreach (var cid in releaseCids)
                {
                    sb.AppendLine();
                    sb.Append("[cid] ").Append(cid);
                }
                Debug.LogError(sb.Release());
#endif
            }

            if (response.ContainsKey("2"))
            {
                isDirty = true;
                int remainTime = response.GetInt("2");
                SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅 (사용중인 셰어캐릭터 세팅 후!) => 남은시간 흐르지 않는 처리 때문에
            }

            if (isDirty)
                InvokeUpdateSharingCharacters();
        }

        private async Task RequestShareCharRewardCalc(int cid, int uid)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", cid);
            sfs.PutInt("2", uid);
            Response response = await Protocol.REQUEST_SHARE_CHAR_REWARD_CALC.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // 서버에서 싱크가 맞지 않아서 정산하고자 하는 셰어캐릭터가 존재하지 않을 수 있다(고 한다).
            if (response.ContainsKey("1"))
            {
                int remainTime = response.GetInt("1");
                SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅
            }
        }

        /// <summary>
        /// 저장된 셰어링캐릭터 다시 사용
        /// </summary>
        private async Task RequestReuseSharingCharacter()
        {
            var sfs = Protocol.NewInstance();
            if (savedSharingCharacterTupleList.Count > 0)
            {
                var sfsArray = Protocol.NewArrayInstance();
                for (int i = 0; i < savedSharingCharacterTupleList.Count; i++)
                {
                    var sfsObject = Protocol.NewInstance();
                    sfsObject.PutInt("1", savedSharingCharacterTupleList[i].uid);
                    sfsObject.PutInt("2", savedSharingCharacterTupleList[i].cid);
                    sfsArray.AddSFSObject(sfsObject);
                }

                sfs.PutSFSArray("1", sfsArray);
            }

            if (savedAgentSharingCharacterIdList.Count > 0)
            {
                sfs.PutIntArray("2", savedAgentSharingCharacterIdList.ToArray());
            }

            if (savedCloneCharacterTupleDic.ContainsKey(CloneCharacterType.MyCharacter))
            {
                sfs.PutInt("3", savedCloneCharacterTupleDic[CloneCharacterType.MyCharacter].cid);
            }

            var response = await Protocol.REQUEST_SERVER_CHANGE_SHARE_CHAR_REUSE.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                ClearSavedSharingCharacter(); // 저장된 정보 제거
                return;
            }

            int remainTime = response.GetInt("1");
            if (response.ContainsKey("2"))
            {
                BattleSharingCharacterPacket[] packets = response.GetPacketArray<BattleSharingCharacterPacket>("2");
                foreach (var item in packets)
                {
                    item.Initialize(skillDataRepoImpl); // 스킬 세팅
                    item.Initialize(profileDataRepoImpl); // 프로필 세팅

                    sharingCharacterDic.Add(item.Cid, item); // 사용중인 셰어캐릭터 추가
                    sharingCharacterList.Add(item); // 리스트 추가
                }
            }

            if (response.ContainsKey("3"))
            {
                BattleSharingCharacterPacket packet = response.GetPacket<BattleCloneCharacterPacket_MyCharacter>("3");
                packet.Initialize(skillDataRepoImpl); // 스킬 세팅
                packet.Initialize(profileDataRepoImpl); // 프로필 세팅

                cloneCharacterDic.Add(CloneCharacterType.MyCharacter, packet);
            }

            foreach (int dummyAgentId in savedAgentSharingCharacterIdList)
            {
                DummySharingCharacter data = agentDataRepo.GetDummySharingAgent(dummyAgentId);
                dummySharingCharacterDic.Add(dummyAgentId, data); // 사용중인 셰어캐릭터 추가
                sharingCharacterList.Add(data); // 리스트 추가
            }

            // 재접속에서는 OnAddSharingCharacters 를 처리하지 않는다!!
            ClearSavedSharingCharacter(); // 저장된 정보 제거
            InvokeUpdateSharingCharacters(); // 셰어링 캐릭터 Refresh 용
            SetRemainTimeForShare(remainTime); // 남은 보상 시간 세팅 (사용중인 셰어캐릭터 세팅 후!) => 남은시간 흐르지 않는 처리 때문에
        }

        /// <summary>
        /// 셰어 강제 중지
        /// </summary>
        public async Task RequestShareForceQuit()
        {
            Response response = await Protocol.REQUEST_SHARE_FORCE_QUIT.SendAsync();
            OnReceiveShareCharacterSettingCancelOk(response);
        }

        /// <summary>
        /// 셰어캐릭터 공유 일시정지
        /// </summary>
        public async Task RequestPauseShareCharacter()
        {
            if (!HasSharingCharacters())
                return;

            await RequestShareCharacterRelease(isSave: true);
        }

        /// <summary>
        /// 셰어캐릭터 공유 재연결
        /// </summary>
        public void ReuseShareCharacters()
        {
            if ((savedSharingCharacterTupleList.Count == 0) && (savedAgentSharingCharacterIdList.Count == 0) && (savedCloneCharacterTupleDic.Count == 0))
                return;

            RequestReuseSharingCharacter().WrapNetworkErrors();
        }

        /// <summary>
        /// 활성화된 셰어 슬롯 카운트
        /// </summary>
        public int GetShareSlotCount(int jobGrade, bool isOpenShareHope)
        {
            var shareSlot = BasisType.SHARE_OPEN_SLOT_BY_JOB_GRADE.GetInt(jobGrade);
            if (shareSlot >= Constants.Size.HOPE_SHARE_NEED_SIZE)
            {
                // 희망의 영웅 사용 가능(5번째 쉐어)
                if (isOpenShareHope)
                {
                    shareSlot++;
                }
            }
            return shareSlot;
        }

        /// <summary>
        /// 클론 쉐어는 4차 직업에 열림.
        /// </summary>
        public int GetCloneSlotCount(int jobGrade)
        {
            return jobGrade >= BasisType.CLONE_SHARE_JOB_GRADE.GetInt() ? 1 : 0;
        }

        /// <summary>
        /// 셰어 캐릭터 저장
        /// </summary>
        private void SaveShareCharacters()
        {
            if (sharingCharacterDic.Count > 0)
            {
                foreach (var item in sharingCharacterDic.Values)
                {
                    savedSharingCharacterTupleList.Add((item.Uid, item.Cid));
                }
            }

            if (dummySharingCharacterDic.Count > 0)
            {
                foreach (var item in dummySharingCharacterDic.Values)
                {
                    savedAgentSharingCharacterIdList.Add(item.agentId);
                }
            }

            if (cloneCharacterDic.Count > 0)
            {
                foreach (var item in cloneCharacterDic)
                {
                    savedCloneCharacterTupleDic.Add(item.Key, (item.Value.Uid, item.Value.Cid));
                }
            }
        }

        void OnReceiveShareCharacterSettingCancel(Response response)
        {
            int cid = response.GetInt("1"); // 공유철회한 캐릭터 cid

            if (sharingCharacterDic.ContainsKey(cid))
            {
                UISimpleCharacterShareBar.IInput info = sharingCharacterDic[cid];
                int exitedCid = info.Cid;
                int exitedUid = info.Uid;

                string message = LocalizeKey._90195.ToText() // 셰어중이던 케릭터 {NAME}님이 새로운 모험을 향해 떠납니다.
                    .Replace(ReplaceKey.NAME, info.Name);

                UI.ShowToastPopup(message);

                sharingCharacterList.Remove(sharingCharacterDic[cid]); // 리스트 제거
                sharingCharacterDic.Remove(cid); // 셰어캐릭터 제거
                OnRemoveSharingCharacter?.Invoke(cid);
                InvokeUpdateSharingCharacters();

                RequestShareCharRewardCalc(exitedCid, exitedUid).WrapNetworkErrors(); // 중도 철회된 셰어캐릭터 정산
            }
        }

        void OnReceiveShareCharacterUseSettingStop(Response response)
        {
            if (sharingState != SharingState.Sharing)
            {
                Debug.LogError("방어코드 작렬");
                return;
            }

            if (response.ContainsKey("1"))
            {
                SharingRewardPacket sharingRewardPacket = response.GetPacket<SharingRewardPacket>("1"); // 1. 보상 정보
                SetSharingReward(sharingRewardPacket); // 보상 세팅
            }

            SetSharingEmployer(null); // 고용주 정보 초기화
        }

        void OnReceiveShareCharacterUseSettingStart(Response response)
        {
            if (sharingState != SharingState.Sharing)
            {
                Debug.LogError("방어코드 작렬");
                return;
            }

            SharingEmployerPacket sharingEmployerPacket = response.GetPacket<SharingEmployerPacket>("1");
            SetSharingEmployer(sharingEmployerPacket); // 고용주 정보 세팅
        }

        void OnReceiveShareCharacterSettingCancelOk(Response response)
        {
            UI.HideIndicator(); ///<see cref="Protocol.REQUEST_SHARE_CHAR_SETTING"/> 의 응답이 여기로 떨어짐 (고용주 존재할 때의 철회 시)

            StopShareForceQuit(); // 강제 종료 대기 멈춤

            SharingState sharingState = response.GetInt("2").ToEnum<SharingState>(); // 내 캐릭터 공유 상태
            SetSharingState(sharingState);

            if (response.ContainsKey("1"))
            {
                SharingRewardPacket sharingRewardPacket = response.GetPacket<SharingRewardPacket>("1"); // 1. 보상 정보
                SetSharingReward(sharingRewardPacket); // 보상 세팅
            }

            SetSharingEmployer(null); // 고용주 정보 초기화
        }

        void OnReceiveShareCharacterSettingCancelFail(Response response)
        {
            //UI.HideIndicator(); ///<see cref="Protocol.REQUEST_SHARE_CHAR_SETTING"/> 의 응답이 여기로 떨어짐 (고용주 존재할 때의 철회 시)
            // 강제 종료 프로토콜을 날리기 때문에 HideIndicator 를 중복 반복하지 않음

            StopShareForceQuit(); // 강제 종료 대기 멈춤

            RequestShareForceQuit().WrapNetworkErrors(); // 셰어 강제 종료
        }

        private void RequestCancelShareCharacter()
        {
            RequestShareCharacterSetting(false).WrapNetworkErrors();
        }

        internal void SetRemainTimeForShare(int remainTime)
        {
            remainTimeForShare.Set(remainTime);

            UpdateRemainForShareTimeStopwatch(); // ShareTime Stopwatch 업데이트
            OnUpdateRemainTimeForShare?.Invoke();
        }

        private void InvokeUpdateSharingCharacters()
        {
            UpdateRemainForShareTimeStopwatch(); // ShareTime Stopwatch 업데이트
            OnUpdateSharingCharacters?.Invoke();
        }

        private void InvokeUpdateShareCharacterList()
        {
            OnUpdateShareCharacterList?.Invoke();
        }

        private void UpdateRemainForShareTimeStopwatch()
        {
            Timing.KillCoroutines(TAG); // 기존 코루틴 제거

            if (HasSharingCharacters())
            {
                remainTimeForShare.Resume(); // 셰어링 캐릭터 존재: 시간 흐름
                Timing.RunCoroutine(YieldReleaseSharingCharacters(), Segment.RealtimeUpdate, TAG);
            }
            else
            {
                remainTimeForShare.Pause(); // 셰어링 캐릭터 존재하지 않음: 시간 멈춤
            }
        }

        private void StartShareForceQuit()
        {
            Timing.RunCoroutine(YieldShareForceQuit(), nameof(YieldShareForceQuit));
        }

        private void StopShareForceQuit()
        {
            Timing.KillCoroutines(nameof(YieldShareForceQuit));
        }

        IEnumerator<float> YieldReleaseSharingCharacters()
        {
            yield return Timing.WaitUntilTrue(remainTimeForShare.IsFinished);
            RequestShareCharacterRelease(isSave: false).WrapNetworkErrors();
        }

        IEnumerator<float> YieldShareForceQuit()
        {
            yield return Timing.WaitForSeconds(WAIT_TIME_TO_SHARE_FORCE_QUIT);
            RequestShareForceQuit().WrapNetworkErrors();
        }

        bool IEqualityComparer<CloneCharacterType>.Equals(CloneCharacterType x, CloneCharacterType y)
        {
            return x == y;
        }

        int IEqualityComparer<CloneCharacterType>.GetHashCode(CloneCharacterType obj)
        {
            return obj.GetHashCode();
        }
    }
}