using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 배너 데이터
    /// </summary>
    public class EventModel : CharacterEntityModel, IComparer<IEventBanner>
    {
        /// <summary>
        /// 이벤트 룰렛
        /// </summary>
        public class EventRoulette
        {
            RemainTime eventRouletteRemainTime;
            /// <summary>
            /// 일반 룰렛판 보상 그룹 ID
            /// </summary>
            public int NormalGachaGroupId { get; private set; }
            /// <summary>
            /// 레어 룰렛판 보상 그룹 ID
            /// </summary>
            public int RareGachaGroupId { get; private set; }
            /// <summary>
            /// 일반 돌림판 소모되는 재화
            /// </summary>
            public ItemData NormalCostItem { get; private set; }
            /// <summary>
            /// 레어 돌림판 소모되는 재화
            /// </summary>
            public ItemData RareCostItem { get; private set; }
            /// <summary>
            /// 레어 돌림판 활성화 여부
            /// </summary>
            public bool IsActiveRareGacha { get; private set; }
            /// <summary>
            /// 레어 룰렛판 여부
            /// </summary>
            public bool IsRareRoulette { get; private set; }

            public bool IsRemainTime()
            {
                return eventRouletteRemainTime.ToRemainTime() > 0;
            }

            public EventRoulette(EventRoulettePacket packet, ItemDataManager.IItemDataRepoImpl itemDataRepo)
            {
                NormalGachaGroupId = packet.normal_gacha_group_id;
                RareGachaGroupId = packet.rare_gach_group_id;
                eventRouletteRemainTime = packet.remain_time;
                NormalCostItem = itemDataRepo.Get(packet.normal_item_id);
                IsActiveRareGacha = packet.is_active_rare_gacha != 0;
                RareCostItem = IsActiveRareGacha ? itemDataRepo.Get(packet.rare_item_id) : null;
                IsRareRoulette = false;

                Debug.Log($"룰렛판 소모 아이템 일반={packet.normal_item_id}, 레어={packet.rare_item_id}");
                Debug.Log($"레어 룰렛판 활성화 여부={IsActiveRareGacha}");
            }

            /// <summary>
            /// 노멀 가챠그룹 ID 업데이트
            /// </summary>
            /// <param name="id"></param>
            public void UpdateNormalGachaGroupId(int id)
            {
                NormalGachaGroupId = id;
            }

            /// <summary>
            /// 레어 가챠그룹 ID 업데이트
            /// </summary>
            /// <param name="id"></param>
            public void UpdateRareGachaGroupId(int id)
            {
                RareGachaGroupId = id;
            }

            public int GetGachGroupId()
            {
                if (IsRareRoulette)
                    return RareGachaGroupId;

                return NormalGachaGroupId;
            }

            public void ToggleRoulette()
            {
                if (!IsActiveRareGacha)
                {
                    IsRareRoulette = false;
                }
                IsRareRoulette = !IsRareRoulette;
            }

            public ItemData GetCostItem()
            {
                if (IsRareRoulette)
                    return RareCostItem;

                return NormalCostItem;
            }
        }

        private const int ROULETTE_MAX_COUNT = 9; // 스페셜 룰렛 뽑기 최대 횟수
        private const byte DICE_DOUBLE_STATE = 1; // 주사위게임 더블 상태

        /// <summary>
        /// 주사위 굴리기 이벤트
        /// </summary>
        public delegate void DiceRollEvent(byte diceNum1, byte diceNum2, bool isDouble, int eventId, RewardData[] rewards);

        private EventRoulette roulette;

        /// <summary>
        /// 룰렛 이벤트 클래스
        /// </summary>
        public EventRoulette Roulette => roulette;

        private readonly ConnectionManager connectionManager;
        private readonly ItemDataManager itemDataRepo;
        private readonly GachaDataManager gachaDataRepo;
        private readonly EventLoginBonusDataManager eventLoginBonusDataRepo;
        private readonly FindAlphabetDataManager findAlphabetDataRepo;
        private readonly BetterList<IEventBanner> bannerList;
        private readonly Buffer<IEventBanner> bannerBuffer;
        private readonly List<UISpecialRouletteElement.IInput> specialRouletteList; // 스페셜 룰렛 보상 정보

        public RemainTime SpecialRouletteRemainTime { get; private set; } // 스페셜 룰렛 남은시간
        public int SpecialRouletteItemId { get; private set; } // 스페셜 룰렛 뽑기 사용 아이템
        public RemainTime SpecialRouletteChangeRemainTime { get; private set; } // 스페셜 룰렛 변경까지 남은시간
        public int SpecialRouletteUsedCount { get; private set; } // 스페셜 룰렛 뽑기 사용 횟수
        public int SpecialRouletteSGradeRewardGachaId { get; private set; } // 스페셜 룰렛 주요 보상 가챠 ID

        private int rareItemId;
        private int savedRareItemCount;

        private ObscuredInt eventQuizStartDate;
        private ObscuredInt eventQuizSeqIndex;

        private ObscuredInt eventRpsResult;
        private ObscuredInt eventRpsRound;

        private ObscuredInt catCoinGiftCount;
        private ObscuredBool catCoinGiftTodayClear;
        private CatCoinGiftData[] catCoinGiftDatas;
        private System.Array giftTypeValues;

        /// <summary>
        /// 이벤트 퀴즈 시작 시간
        /// </summary>
        public int EventQuizStartDate => eventQuizStartDate;
        /// <summary>
        /// 이벤트 퀴즈 시퀀스 인덱스
        /// </summary>
        public int EventQuizSeqIndex => eventQuizSeqIndex;

        public RpsResultType EventRpsResult => eventRpsResult.ToEnum<RpsResultType>();
        public RpsRoundType EventRpsRound => eventRpsRound.ToEnum<RpsRoundType>();

        public int EventDiceGroupId { get; private set; }
        public int EventDiceStep { get; private set; }
        private byte eventDiceDoubleState; // 주사위게임 더블 상태 (1이면 더블)
        public int EventDiceCompleteCount { get; private set; } // 주사위게임 완주 횟수
        public int EventDiceCompleteRewardStep { get; private set; } // 주사위게임 보상수령한 회차

        /// <summary>
        /// [글자수집] 재료 아이템 ID 목록
        /// </summary>
        private int[] wordCollectionItems;

        /// <summary>
        /// [글자수집] 재료 필요 수량
        /// </summary>
        public int WordCollectionNeedCount { get; private set; }

        /// <summary>
        /// [글자수집] 이벤트 남은 시간
        /// </summary>
        public RemainTime WordCollectionRemainTime { get; private set; }

        /// <summary>
        /// [글자수집] 이벤트 보상 수령한 회차
        /// </summary>
        public int WordCollectionRewardStep { get; private set; }

        /// <summary>
        /// [출석 체크 이벤트] 출석 일수
        /// </summary>
        public int AttendEventDay { get; private set; }

        /// <summary>
        /// [출석 체크 이벤트] 보상 수령한 회차
        /// </summary>
        public int AttendEventRewardStep { get; private set; }

        /// <summary>
        /// 주사위게임 더블 상태
        /// </summary>
        public bool IsEventDiceDouble => eventDiceDoubleState == DICE_DOUBLE_STATE;

        /// <summary>
        /// 전체 이벤트 온버프 포인트 남은 잔여량
        /// </summary>
        public long OnBuffTotalRemainPoint { get; private set; }

        /// <summary>
        /// 스페셜 룰렛 뽑기 결과 이벤트
        /// </summary>
        public System.Action<bool, int> OnSpecialRoulette;

        /// <summary>
        /// 스페셜 룰렛판 변경 이벤트
        /// </summary>
        public System.Action OnSpecialRouletteChange;

        /// <summary>
        /// 이벤트 퀴즈 정보 업데이트
        /// </summary>
        public event System.Action OnUpdateQuizInfo;

        /// <summary>
        /// 이벤트 퀴즈 보상 이벤트
        /// </summary>
        public event System.Action<bool, RewardData> OnQuizReward;

        /// <summary>
        /// 이벤트 가위바위보 정보 업데이트
        /// </summary>
        public event System.Action OnUpdateRpsInfo;

        public event System.Action OnUpdateCatCoinGiftInfo;

        /// <summary>
        /// [주사위 게임] 주사위 굴리기 이벤트
        /// </summary>
        public event DiceRollEvent OnUpdateDiceRollEvent;

        /// <summary>
        /// [주사위 게임] 주사위 완주 보상 이벤트
        /// </summary>
        public event System.Action<RewardData[]> OnDiceCompleteRewardEvent;

        /// <summary>
        /// [주사위 게임] 완주 보상 업데이트
        /// </summary>
        public event System.Action OnUpdateDiceCompleteRewardEvent;

        /// <summary>
        /// [출석체크 이벤트] 출석 체크 보상 이벤트
        /// </summary>
        public event System.Action OnUpdateAttendEventReward;

        /// <summary>
        /// [단어수집] 재료 아이템 수량 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateWordCollectionItemCount;

        /// <summary>
        /// [단어 수집] 완성 보상 이벤트
        /// </summary>
        public event System.Action<RewardData[]> OnWordCollectionCompleteRewardEvent;

        /// <summary>
        /// [단어 수집] 전체 이벤트 온버프 포인트 남은 잔여량 수량 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateOnBuffTotalRemainPoint;

        public EventModel()
        {
            connectionManager = ConnectionManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            gachaDataRepo = GachaDataManager.Instance;
            eventLoginBonusDataRepo = EventLoginBonusDataManager.Instance;
            findAlphabetDataRepo = FindAlphabetDataManager.Instance;
            specialRouletteList = new List<UISpecialRouletteElement.IInput>();
            bannerList = new BetterList<IEventBanner>();
            bannerBuffer = new Buffer<IEventBanner>();

            giftTypeValues = System.Enum.GetValues(typeof(CatCoinGiftType));
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        public override void ResetData()
        {
            base.ResetData();

            roulette = null;
            SetRareItemId(0);
            savedRareItemCount = 0;
            UpdateEventQuizInfo(0, 0);
            EventDiceGroupId = 0;
            EventDiceStep = 0;
            EventDiceCompleteCount = 0;
            EventDiceCompleteRewardStep = 0;
            AttendEventDay = 0;
            AttendEventRewardStep = 0;
            WordCollectionRewardStep = 0;

            bannerList.Clear();
            specialRouletteList.Clear();
        }

        internal void Initialize(SpecialRoulettePacket packet)
        {
            SpecialRouletteRemainTime = packet.remainTime;
            SpecialRouletteItemId = packet.useItemId;
            SpecialRouletteSGradeRewardGachaId = packet.groupId;

            Debug.Log($"스페셜 룰렛 남은 시간={SpecialRouletteRemainTime.ToStringTime()}");
            Debug.Log($"스페셜 룰렛 사용 ItemId={SpecialRouletteItemId}");
            Debug.Log($"스페셜 룰렛 S등급 가챠 테이블 그룹ID={SpecialRouletteSGradeRewardGachaId}");

            SetSpecialRouletteList(packet.gachaIds, packet.receivedGachIds);
        }

        internal void Initialize(EventQuizPacket packet)
        {
            UpdateEventQuizInfo(packet.start_date, packet.seq_Index);
        }

        internal void Initialize(CharacterPacket packet)
        {
            SpecialRouletteChangeRemainTime = packet.specialRouletteChangeRemainTime;
            SpecialRouletteUsedCount = packet.specialRouletteUsedCount;

            Debug.Log($"스페셜 룰렛 변경까지 남은 시간={SpecialRouletteChangeRemainTime.ToStringTime()}");
            Debug.Log($"스페셜 룰렛 사용 횟수={SpecialRouletteUsedCount}");
        }

        internal void Initialize(EventRoulettePacket packet)
        {
            roulette = new EventRoulette(packet, itemDataRepo);

            // 레어 룰렛 아이템 아읻
            ItemData rareItem = roulette.RareCostItem;
            if (rareItem == null)
            {
                SetRareItemId(0);
            }
            else
            {
                SetRareItemId(rareItem.id);
            }
        }

        internal void Initialize(EventRpsPacket packet)
        {
            UpdateEventRpsInfo(packet.result, packet.round);
        }

        internal void Initialize(CatCoinGiftPacket packet)
        {
            UpdateCatCoinGiftInfo(packet.CompleteCount, packet.TodayRewarded);
        }

        internal void Initialize(EventDicePacket packet)
        {
            EventDiceGroupId = packet.group_id;
            EventDiceStep = packet.step;
            eventDiceDoubleState = packet.dice_double_state;
            SetEventDiceCompleteInfo(packet.complete_count, packet.complete_reward_step);
        }

        internal void Initialize(AttendEventPacket packet)
        {
            AttendEventDay = Mathf.Min(packet.day_count, eventLoginBonusDataRepo.MaxAttendEventDay);
            AttendEventRewardStep = packet.reward_step;
            Debug.Log($"[출석체크 이벤트] 출석일수={AttendEventDay}, 보상받은 회차={AttendEventRewardStep}, DayMax={eventLoginBonusDataRepo.MaxAttendEventDay}");
        }

        /// <summary>
        /// [주사위 게임] 완료 정보 세팅
        /// </summary>
        internal void SetEventDiceCompleteInfo(int completeCount, int completeRewardStep)
        {
            EventDiceCompleteCount = completeCount;
            UpdateEventDiceCompleteRewardStep(completeRewardStep);
        }

        internal void Initialize(WordCollectionPacket packet)
        {
            WordCollectionRemainTime = packet.remainTime;
            SetEventWordCollectionRewardStep(packet.completeRewardStep);

            WordCollectionNeedCount = BasisType.WORD_COLLECTION_MATERIAL_NEED_COUNT.GetInt();
            wordCollectionItems = BasisType.WORD_COLLECTION_ITEMS.GetKeyList().ToArray();
            System.Array.Sort(wordCollectionItems, (a, b) => b.CompareTo(a)); // 기초데이터 상세는 무조건 오름차순이라 내림차순으로 돌림
            foreach (var item in wordCollectionItems)
            {
                Entity.Inventory.RemoveItemEvent(item, OnEventWordCollectionItemCount);
                Entity.Inventory.AddItemEvent(item, OnEventWordCollectionItemCount);
            }
            Debug.Log($"[단어수집 이벤트] 남은 시간 = {WordCollectionRemainTime.ToStringTime()}");
            Debug.Log($"[단어수집 이벤트] 보상 받은 회차 = {WordCollectionRewardStep}");
        }

        /// <summary>
        /// [단어 수집] 보상 수령한 회차 세팅
        /// </summary>
        internal void SetEventWordCollectionRewardStep(int completeRewardStep)
        {
            if (WordCollectionRewardStep == completeRewardStep)
                return;

            WordCollectionRewardStep = completeRewardStep;
        }

        private void SetRareItemId(int itemId)
        {
            if (rareItemId == itemId)
                return;

            // 기존 아이템 아이디가 존재할 경우
            if (rareItemId != 0)
            {
                Entity.Inventory.RemoveItemEvent(rareItemId, OnUpdateItem); // 기존 아이템 이벤트 제거
            }

            rareItemId = itemId;

            if (rareItemId != 0)
            {
                Entity.Inventory.AddItemEvent(rareItemId, OnUpdateItem); // 아이템 이벤트 추가
            }
        }

        void OnUpdateItem()
        {
            if (rareItemId == 0)
                return;

            if (roulette == null)
                return;

            ItemData rareItem = roulette.RareCostItem;
            if (rareItem == null)
                return;

            int rareItemCount = Entity.Inventory.GetItemCount(rareItemId);
            if (savedRareItemCount == rareItemCount)
                return;

            if (rareItemCount > savedRareItemCount)
            {
                UIBattleMenu uiBattleMenu = UI.GetUI<UIBattleMenu>();
                if (uiBattleMenu != null)
                {
                    Vector3 endPos = uiBattleMenu.GetRoulettePos();
                    UI.Show<UIItemResult>()
                        .SetItem(rareItem.name_id.ToText(), rareItem.icon_name, endPos);
                }
            }

            savedRareItemCount = rareItemCount;
        }

        private void SetSpecialRouletteList(string gachaIds, string receivedGachIds)
        {
            Debug.Log($"스페셜 룰렛 보상목록={gachaIds}");
            Debug.Log($"스페셜 룰렛 획득목록={receivedGachIds}");
            // 획득한 스페셜 가차 ID 목록
            List<int> receivedGachIdList = new List<int>();
            string[] receivedIds = receivedGachIds.Split(',');
            for (int i = 0; i < receivedIds.Length; i++)
            {
                if (int.TryParse(receivedIds[i], out int id))
                {
                    receivedGachIdList.Add(id);
                }
            }

            // 스페셜 가차 보상 정보 목록
            specialRouletteList.Clear();
            string[] ids = gachaIds.Split(',');
            for (int i = 0; i < ids.Length; i++)
            {
                if (int.TryParse(ids[i], out int gachaId))
                {
                    GachaData gachaData = gachaDataRepo.Get(gachaId);
                    bool isReceived = receivedGachIdList.Contains(gachaData.id);
                    gachaData.SetReceived(isReceived);
                    specialRouletteList.Add(gachaData);

                    Debug.Log($"스페셜 룰렛 Index={i}, 그룹타입={gachaData.group_type}, ID={gachaData.id}, 획득여부={isReceived}, 보상아이템={gachaData.Reward.ItemName}");
                }
            }
        }

        /// <summary>
        /// 유효한 배너 반환
        /// </summary>
        public IEventBanner[] GetEventBanners()
        {
            // 기간 만료된 배너들 제거
            var expiredList = bannerList.FindAll(e => e.RemainTime.ToRemainTime() <= 0);
            foreach (var item in expiredList)
            {
                bannerList.Remove(item);
            }

            // 정리
            foreach (var item in bannerList)
            {
                // 페이스북 타입이면서 한국의 경우에는 처리하지 않음
                if (item.ShortcutType == ShortCutType.FacebookLikePage && GameServerConfig.IsKorea())
                    continue;

                bannerBuffer.Add(item);
            }

            return bannerBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 룰렛 실행
        /// </summary>
        public async Task<Response> RequestPlayRouletteAsync()
        {
            const byte NORMAL_ROULETTE = 1; // 일반 룰렛
            const byte RARE_ROULETTE = 2; // 레어 룰렛

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", roulette.IsRareRoulette ? RARE_ROULETTE : NORMAL_ROULETTE);

            Debug.Log($"현재 가챠 그룹 ID={roulette.GetGachGroupId()}, 레어={roulette.IsRareRoulette}");

            var response = await Protocol.REQUEST_ROULETTE_REWARD.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return response;
            }

            if (response.ContainsKey("cud"))
            {
                Notify(response.GetPacket<CharUpdateData>("cud"));
            }

            // 변경된는 룰렛 가챠그룹ID
            if (response.ContainsKey("2"))
            {
                Debug.Log($"변경되는 가챠 그룹 ID={response.GetInt("2")}, 레어={roulette.IsRareRoulette}");
                if (roulette != null)
                {
                    if (roulette.IsRareRoulette)
                    {
                        roulette.UpdateRareGachaGroupId(response.GetInt("2"));
                    }
                    else
                    {
                        roulette.UpdateNormalGachaGroupId(response.GetInt("2"));
                    }
                }
            }

            if (response.ContainsKey("1"))
            {
                int gacha = response.GetInt("1");
                var data = GachaDataManager.Instance.Get(gacha);
                RewardData reward = new RewardData(data.reward_type, data.reward_value, data.reward_count);
                Debug.Log($"가챠 그룹 ID={data.group_id} 보상={gacha} {reward.ItemId} {reward.ItemName} {reward.Count}");
            }

            return response;
        }

        internal void Initialize(EventBannerPacket[] arrPacket)
        {
            bannerList.Clear();

            if (arrPacket == null)
                return;

            // 해당 텍스쳐 다운로드 후 데이터로 보관.
            foreach (EventBannerPacket item in arrPacket)
            {
                EventBannerData data = new EventBannerData();
                data.Initialize(item, connectionManager.GetResourceUrl(item.textureName));
                bannerList.Add(data);
            }

            // Pos 순으로 정렬.
            bannerList.Sort(Compare);
        }

        public int Compare(IEventBanner x, IEventBanner y)
        {
            return x.Pos.CompareTo(y.Pos);
        }

        /// <summary>
        /// 스페셜 룰렛 진행중 여부
        /// </summary>
        public bool IsOpenSpecialRoulette()
        {
            return SpecialRouletteRemainTime.ToRemainTime() > 0;
        }

        public UISpecialRouletteElement.IInput[] GetSpecialRouletteElementArray()
        {
            int totalRate = 0;
            for (int i = 0; i < specialRouletteList.Count; i++)
            {
                if (!specialRouletteList[i].IsComplete)
                    totalRate += specialRouletteList[i].Rate;
            }

            for (int i = 0; i < specialRouletteList.Count; i++)
            {
                specialRouletteList[i].SetTotalRate(totalRate);
            }
            return specialRouletteList.ToArray();
        }

        /// <summary>
        /// 스페셜 룰렛 뽑기 요청
        /// </summary>
        public async Task RequestSpecialRoulette()
        {
            Response response = await Protocol.REQUEST_SPECIAL_ROULETTE_REWARD.SendAsync();

            if (!response.isSuccess)
            {
                OnSpecialRoulette?.Invoke(false, 0);
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                Notify(response.GetPacket<CharUpdateData>("cud"));
            }

            int rewardGachaId = response.GetInt("1"); // 획득한 가차ID
            SpecialRouletteUsedCount = response.GetInt("2"); // 룰렛 돌린횟수

            Debug.Log($"스페셜 룰렛 획득 보상 가챠ID = {rewardGachaId}");
            Debug.Log($"스페셜 룰렛 돌린 횟수 = {SpecialRouletteUsedCount}");

            foreach (var item in specialRouletteList)
            {
                if (item.Id == rewardGachaId)
                {
                    item.SetReceived(true);
                    break;
                }
            }

            OnSpecialRoulette?.Invoke(true, rewardGachaId);
        }

        /// <summary>
        /// 스페셧 룰렛판 변경
        /// </summary>
        /// <returns></returns>
        public async Task RequestSpecialRouletteChange()
        {
            Response response = await Protocol.REQUEST_SPECIAL_ROULETTE_INIT_BOARD.SendAsync();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                Notify(response.GetPacket<CharUpdateData>("cud"));
            }

            SpecialRouletteUsedCount = 0; // 룰렛 돌린 횟수 초기화
            SpecialRouletteChangeRemainTime = response.GetLong("1"); // 초기화 까지 남은시간
            string gachaIds = response.GetUtfString("2"); // 초기화 후 가차 보드 목록
            Debug.Log($"스페셜 룰렛 변경까지 남은 시간 = {SpecialRouletteChangeRemainTime.ToStringTime()}");

            SetSpecialRouletteList(gachaIds, string.Empty);

            OnSpecialRouletteChange?.Invoke();
        }

        /// <summary>
        /// 이벤트 퀴즈 보상 받기
        /// </summary>
        public async Task RequestEventQuizReward(int id, byte answer, bool isCorrect)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id);
            sfs.PutByte("2", answer);
            Response response = await Protocol.REQUEST_EVENT_QUIZ_REWARD.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            RewardData reward = null;
            if (response.ContainsKey("cud"))
            {
                CharUpdateData cud = response.GetPacket<CharUpdateData>("cud");
                Notify(cud);

                if (cud.rewards != null && cud.rewards.Length > 0)
                    reward = new RewardData(cud.rewards[0].rewardType, cud.rewards[0].rewardValue, cud.rewards[0].rewardCount);
            }

            int startData = response.GetInt("1");
            int seq = response.GetInt("2");

            OnQuizReward?.Invoke(isCorrect, reward); // 보상 이벤트 호출
            UpdateEventQuizInfo(startData, seq); // 퀴즈 정보 업데이트
        }

        /// <summary>
        /// 이벤트 가위바위보 시작
        /// </summary>
        public async Task RequestEventRpsStart()
        {
            Response response = await Protocol.REQUEST_RPS_GAME.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData cud = response.GetPacket<CharUpdateData>("cud");
                Notify(cud);
            }

            var result = response.GetByte("1");
            var round = response.GetByte("2");

            UpdateEventRpsInfo(result, round); // 가위바위보 정보 업데이트
        }

        /// <summary>
        /// 이벤트 가위바위보 초기화
        /// </summary>
        public async Task RequestEventRpsInit()
        {
            Response response = await Protocol.REQUEST_RPS_INIT.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData cud = response.GetPacket<CharUpdateData>("cud");
                Notify(cud);
            }

            var result = response.GetByte("1");
            var round = response.GetByte("2");

            UpdateEventRpsInfo(result, round); // 가위바위보 정보 업데이트
        }

        /// <summary>
        /// 냥다래 받기 이벤트 보상
        /// </summary>
        /// <returns></returns>
        public async Task RequestUserEventQuestReward()
        {
            Response response = await Protocol.REQUEST_USER_EVNET_QUEST_REWARD.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData cud = response.GetPacket<CharUpdateData>("cud");
                Notify(cud);
            }

            var count = response.GetByte("1");

            UpdateCatCoinGiftInfo(count, true); // 냥다래 선물 정보 업데이트
        }

        /// <summary>
        /// [주사위게임] 주사위 굴리기
        /// </summary>
        public async Task RequestDiceRoll()
        {
            Response response = await Protocol.REQUEST_DICE_ROLL.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            RewardData[] rewards = null;
            if (response.ContainsKey("cud"))
            {
                CharUpdateData cud = response.GetPacket<CharUpdateData>("cud");
                Notify(cud);

                rewards = UI.ConvertRewardData(cud.rewards);
            }

            if (response.ContainsKey("1"))
            {
                EventDiceGroupId = response.GetInt("1");
            }

            if (response.ContainsKey("2"))
            {
                EventDiceStep = response.GetInt("2");
            }

            if (response.ContainsKey("4"))
            {
                EventDiceCompleteCount = response.GetInt("4");
            }

            if (response.ContainsKey("5"))
            {
                eventDiceDoubleState = response.GetByte("5");
            }

            byte diceNum1 = response.GetByte("6");
            byte diceNum2 = response.GetByte("7");
            int eventId = response.GetInt("8");
            OnUpdateDiceRollEvent?.Invoke(diceNum1, diceNum2, IsEventDiceDouble, eventId, rewards);
        }

        /// <summary>
        /// 주사위 완주 보상 받기
        /// </summary>
        public async Task RequestDiceReward()
        {
            Response response = await Protocol.REQUEST_DICE_REWARD.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            RewardData[] rewards = null;
            if (response.ContainsKey("cud"))
            {
                CharUpdateData cud = response.GetPacket<CharUpdateData>("cud");
                Notify(cud);

                rewards = UI.ConvertRewardData(cud.rewards);
            }

            OnDiceCompleteRewardEvent?.Invoke(rewards);

            int diceCompleteRewardStep = response.GetInt("1");
            UpdateEventDiceCompleteRewardStep(diceCompleteRewardStep);
        }

        /// <summary>
        /// 룰렛 이벤트 남은 시간 있는지 여부
        /// </summary>
        public bool IsRemainTimeRoulette()
        {
            return SpecialRouletteRemainTime.ToRemainTime() > 0;
        }

        /// <summary>
        /// 룰렛 뽑기시 필요한 아이템 수량
        /// </summary>
        public int GetSpecialRouletteNeedCount()
        {
            return BasisType.SPECIAL_ROULETTE_NEED_COUNT.GetInt(Mathf.Min(ROULETTE_MAX_COUNT, SpecialRouletteUsedCount + 1));
        }

        /// <summary>
        /// 룰렛 뽑기를 최대 횟수까지 사용했는지 여부
        /// </summary>
        public bool IsSpecialRouletteMaxUsed()
        {
            return SpecialRouletteUsedCount == ROULETTE_MAX_COUNT;
        }

        public bool ActivationCatCoinGiftEvent()
        {
            bool isActive = BasisType.CAT_COIN_GIFT_EVENT_FLAG.GetInt() == 1;

            // 이벤트 진행 중
            if (isActive)
            {
                // 이벤트 보상 남았을 때
                if (catCoinGiftCount < giftTypeValues.Length)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// S등급 아이템
        /// </summary>
        public UISpecialRouletteElement.IInput GetSGradeItem()
        {
            if (specialRouletteList == null || specialRouletteList.Count < 5)
                return null;

            return specialRouletteList[4];
        }

        /// <summary>
        /// 스페셜 룰렛 주요 보상 리스트
        /// </summary>
        public UIRewardListElement.IInput[] GetSpecialRewardList()
        {
            UIRewardListElement.IInput[] datas = gachaDataRepo.Gets(GroupType.SpecialRoulette, SpecialRouletteSGradeRewardGachaId);

            int totalRate = 0;
            foreach (var item in datas)
            {
                totalRate += item.Rate;
            }

            foreach (var item in datas)
            {
                item.SetTotalRate(totalRate);
            }

            return datas;
        }

        /// <summary>
        /// 계정단위 냥다래 보상 이벤트 수령가능 상태
        /// </summary>
        public bool CanRewardCatCoinGift(bool isClearQuest = false)
        {
            if (catCoinGiftCount < giftTypeValues.Length // 이벤트 보상 남았을 때
                && !catCoinGiftTodayClear // 오늘 보상 받지 않음
                && isClearQuest) // 수령 가능조건(일퀘보상 받은 상태)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 냥다래 선물 정보 리스트
        /// </summary>
        public CatCoinGiftData[] GetCatCoinGiftInfos(bool isClearQuest = false)
        {
            if (catCoinGiftDatas == null)
            {
                // 냥다래 선물 보상 리스트
                catCoinGiftDatas = new CatCoinGiftData[giftTypeValues.Length];

                for (int i = 0; i < giftTypeValues.Length; i++)
                {
                    var id = (int)(CatCoinGiftType)giftTypeValues.GetValue(i);

                    RewardData reward = new RewardData((byte)RewardType.CatCoinFree, BasisType.CAT_COIN_GIFT_REWARD.GetInt(id), 0);
                    catCoinGiftDatas[i] = new CatCoinGiftData(id, reward);
                }
            }

            // 정보 갱신
            var canReward = !catCoinGiftTodayClear && isClearQuest;
            for (int i = 0; i < catCoinGiftDatas.Length; i++)
            {
                catCoinGiftDatas[i].SetData(catCoinGiftCount, catCoinGiftDatas[i].Id, canReward);
            }

            // 정렬
            List<CatCoinGiftData> progressList = new List<CatCoinGiftData>();
            List<CatCoinGiftData> completeList = new List<CatCoinGiftData>();
            for (int i = 0; i < catCoinGiftDatas.Length; i++)
            {
                // 완료된 상태
                if (catCoinGiftCount >= catCoinGiftDatas[i].CompleteCount)
                {
                    completeList.Add(catCoinGiftDatas[i]);
                }
                else
                {
                    progressList.Add(catCoinGiftDatas[i]);
                }
            }

            progressList.AddRange(completeList);

            return progressList.ToArray();
        }

        /// <summary>
        /// 이벤트 퀴즈 정보 업데이트
        /// </summary>
        private void UpdateEventQuizInfo(int startData, int seqIndex)
        {
            eventQuizStartDate = startData;
            eventQuizSeqIndex = seqIndex;

            OnUpdateQuizInfo?.Invoke();
        }

        /// <summary>
        /// 이벤트 가위바위보 정보 업데이트
        /// </summary>
        private void UpdateEventRpsInfo(byte result, byte round)
        {
            eventRpsResult = result;
            eventRpsRound = round;

            OnUpdateRpsInfo?.Invoke();
        }

        /// <summary>
        /// 냥다래 선물 정보 업데이트
        /// </summary>
        private void UpdateCatCoinGiftInfo(int count, bool isRewarded)
        {
            catCoinGiftCount = count;
            catCoinGiftTodayClear = isRewarded;

            OnUpdateCatCoinGiftInfo?.Invoke();
        }

        /// <summary>
        /// [주사위 이벤트] 주사위게임 보상수령한 회차 업데이트
        /// </summary>
        private void UpdateEventDiceCompleteRewardStep(int completeRewardStep)
        {
            if (EventDiceCompleteRewardStep == completeRewardStep)
                return;

            EventDiceCompleteRewardStep = completeRewardStep;
            OnUpdateDiceCompleteRewardEvent?.Invoke();
        }

        /// <summary>
        /// [출석체크 이벤트] 보상 받기 요청
        /// </summary>
        public async Task RequestAttendEventReward()
        {
            Response response = await Protocol.REQUEST_PROMOTION_LOGIN_BONUS_REWARD.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData cud = response.GetPacket<CharUpdateData>("cud");
                Notify(cud);

                UI.RewardInfo(cud.rewards);
            }

            AttendEventRewardStep = Mathf.Min(AttendEventRewardStep + 1, AttendEventDay);

            OnUpdateAttendEventReward?.Invoke();
        }

        /// <summary>
        /// [출석체크 이벤트] 받을 보상이 있는지 여부
        /// </summary>
        public bool IsAttendEventStandByReward()
        {
            return AttendEventRewardStep < AttendEventDay;
        }

        /// <summary>
        /// [글자수집] 재료 아이템 ID 목록
        /// </summary>
        public int[] GetWordCollectionItems()
        {
            return wordCollectionItems;
        }

        /// <summary>
        /// [글자수집] 재료 수량 변경 이벤트
        /// </summary>
        private void OnEventWordCollectionItemCount()
        {
            OnUpdateWordCollectionItemCount?.Invoke();
        }

        /// <summary>
        /// [글자수집] 이벤트 진행중 여부
        /// </summary>
        /// <returns></returns>
        public bool IsEventWordCollection()
        {
            return WordCollectionRemainTime.ToRemainTime() > 0;
        }

        /// <summary>
        /// [글자수집] 모든 보상을 받았는지 여부
        /// </summary>
        public bool IsAllWordCollectionCompleteReward()
        {
            return WordCollectionRewardStep >= findAlphabetDataRepo.GetMaxRewardCount();
        }

        /// <summary>
        /// [글자수집] 재료 완성 여부
        /// </summary>
        /// <returns></returns>
        public bool IsWordCollectionCompleteMaterial()
        {
            foreach (var item in wordCollectionItems)
            {
                if (Entity.Inventory.GetItemCount(item) < WordCollectionNeedCount)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// [글자수집] 받을 보상이 있는지 여부
        /// </summary>
        public bool IsWordCollectionStandByReward()
        {
            if (!IsEventWordCollection()) // 이벤트 종료
                return false;

            if (IsAllWordCollectionCompleteReward())
                return false;

            return IsWordCollectionCompleteMaterial();
        }

        /// <summary>
        /// [글자수집] 완성 보상 받기
        /// </summary>
        public async Task RequestWordCollectionReward()
        {
            Response response = await Protocol.REQUEST_FIND_ALPHABET_REWARD.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            RewardData[] rewards = null;
            if (response.ContainsKey("cud"))
            {
                CharUpdateData cud = response.GetPacket<CharUpdateData>("cud");
                Notify(cud);

                rewards = UI.ConvertRewardData(cud.rewards);
            }

            SetEventWordCollectionRewardStep(WordCollectionRewardStep + 1);

            OnWordCollectionCompleteRewardEvent?.Invoke(rewards);
            OnEventWordCollectionItemCount();
        }

        /// <summary>
        /// [온버프] 전체 이벤트 온버프 포인트 남은 잔여량 조회
        /// </summary>
        public async Task RequestOnBuffTotalRemainPoint()
        {
            Response response = await Protocol.REQUEST_ONBUFF_TOTAL_REMAIN_POINT.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            long point = response.GetLong("1");

            if (OnBuffTotalRemainPoint == point)
                return;

            OnBuffTotalRemainPoint = point;
            OnUpdateOnBuffTotalRemainPoint?.Invoke();
        }

        /// <summary>
        /// 테스트 코드 (이벤트 출석 데이터 임시 세팅)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void TempAttendEvent(int day, int step)
        {
            AttendEventDay = day;
            AttendEventRewardStep = step;
            OnUpdateAttendEventReward?.Invoke();
        }
    }
}