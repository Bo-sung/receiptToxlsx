using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ShopModel : CharacterEntityModel, IEqualityComparer<ShareTicketType>
    {
        public const int TAB_MIN_VALUE = (int)ShopTabType.Goods;
        public const int TAB_MAX_VALUE = (int)ShopTabType.Consumable;

        public const int TAB_GUILD_SHOP = (int)ShopTabType.Guild; // 길드 상점

        private const string TAG = nameof(ShopModel);

        private readonly GachaDataManager gachaDataRepo;
        private readonly ShopDataManager shopDataRepo;
        private readonly RewardGroupDataManager rewardGroupDataRepo;
        private readonly PaymentRewardDataManager paymentRewardDataRepo;
        private readonly PassDataManager passDataRepo;
        private readonly OnBuffPassDataManager onBuffPassDataRepo;
        private readonly BuildSettings settings;
        private readonly IGamePotImpl gamePotImpl;
        private readonly ConnectionManager connectionManager;

        /// <summary>
        /// 비밀상점 목록 및 구매 횟수
        /// </summary>
        private readonly List<SecretShopInfo> secretShopInfos;
        /// <summary>
        /// 상점 목록
        /// </summary>
        private readonly Dictionary<ObscuredInt, ShopInfo> shopInfoDict;

        /// <summary>
        /// 캐쉬 상품 가격
        /// </summary>
        private readonly Dictionary<string, string> cashShopPrice;

        /// <summary>
        /// 비밀상점 무료 변경까지 남은시간
        /// </summary>
        private RemainTime nextSecretShopTime;

        /// <summary>
        /// 비밀상점 목록 초기화 비용
        /// </summary>
        public int SecretShopInitCatCoin => BasisType.SECRET_SHOP_COOL_INIT_CAT_COIN.GetInt();

        /// <summary>
        /// 이벤트 호출 주기
        /// </summary>
        private const float CALL_WAIT_TIME = 30f;

        /// <summary>
        /// 챕터 패키지
        /// </summary>
        public short PayChapterStep { get; private set; } // -1:미구매, 0: 구매, 1: 1챕터 보상 받음

        /// <summary>
        /// 레벨업 패키지
        /// </summary>
        public short PayJobLevelStep { get; private set; } // -1:미구매, 0: 구매, 1: 1레벨 보상 보상 받음

        /// <summary>
        /// 무료 시나리오 패키지
        /// </summary>
        public short FreeChapterStep { get; private set; } // 보상 받은 스탭

        /// <summary>
        /// 무료 레벨업 패키지
        /// </summary>
        public short FreeJobLevelStep { get; private set; } // 보상 받은 스탭

        /// <summary>
        /// 금빛 패키지 남은시간
        /// </summary>
        public RemainTime TreePackRemainTime { get; private set; }

        /// <summary>
        /// 매일매일 패키지 정보
        /// </summary>
        private readonly Dictionary<int, UserEverydayGoodsPacket> everyDayGoodsListDic;

        /// <summary>
        /// 캐쉬 상품 구매하여 메일에 보유중인 상점 아이템 목록
        /// </summary>
        private readonly List<int> hasMailShopItemList;

        /// <summary>
        /// 유저 ID
        /// </summary>
        public int Uid { get; private set; }

        /// <summary>
        /// 캐릭터 ID
        /// </summary>
        public int Cid { get; private set; }

        /// <summary>
        /// 게임서버 채널 인덱스
        /// </summary>
        public int channelIndex { get; private set; }

        /// <summary>
        /// 셰어영구버프 활성유무
        /// </summary>
        public bool IsShareBuff { get; private set; }

        /// <summary>
        /// 첫결제 보상 상태 (0 : 첫결제 하지 않은 상태, 1: 첫결제 후 보상이 나간 상태)
        /// </summary>
        public byte FirstPaymentState { get; private set; }

        /// <summary>
        /// 마일리지
        /// </summary>
        public int Mileage { get; private set; }

        /// <summary>
        /// 마일리지 보상 단계
        /// </summary>
        public int MileageRewardStep { get; private set; }

        /// <summary>
        /// 직업 레벨 패키지 상점 ID
        /// </summary>
        public int JobLevelPackageShopId { get; private set; }

        /// <summary>
        /// 직업 레벨 패키지 구입 가능 남은시간
        /// </summary>
        public RemainTime JobLevelPackageRemainTime { get; private set; }

        /// <summary>
        /// 직업 레벨 패키지 사라짐 경고 팝업 여부
        /// </summary>
        private bool isNeedShowJobLevelPopup;

        /// <summary>
        /// 고객감사 일반 진행 단계
        /// </summary>
        public int CustomerRewardNormalStep { get; private set; }
        /// <summary>
        /// 고객감사 프리미엄 진행 단계
        /// </summary>
        public int CustomerRewardPremiumStep { get; private set; }
        /// <summary>
        /// 고객감사 일반 보상수령 남은시간
        /// </summary>
        public RemainTime CustomerRewardNormalRemainTime { get; private set; }
        /// <summary>
        /// 고객감사 프리미엄 보상수령 남은시간
        /// </summary>
        public RemainTime CustomerRewardPremiumRemainTime { get; private set; }

        public readonly PassInfo labyrinthPass;
        public readonly PassInfo onBuffPass;
        private ObscuredInt onBuffMvpPoint; // 온버프 Mvp 사냥 포인트
        private ObscuredBool isReceivedOnBuffPoint; // 온버프 포인트 획득 여부

        public int OnBuffMvpPoint => onBuffMvpPoint;
        public bool IsReceivedOnBuffPoint => isReceivedOnBuffPoint;

        #region 이벤트

        /// <summary>
        /// 매일매일 패키지 정보 갱신
        /// </summary>
        public event Action OnUpdateEveryDayGoods;

        /// <summary>
        /// 비밀상점 무료변경 체크 이벤트
        /// </summary>
        public event Action OnSecretShopFree;

        /// <summary>
        /// 상점 구매 횟수 정보 초기화 이벤트 (무료 아이템에 한함)
        /// </summary>
        public event Action OnResetFreeItemBuyCount;

        /// <summary>
        /// 상점 구매 성공 이벤트
        /// </summary>
        public event Action OnPurchaseSuccess;

        /// <summary>
        /// 달성패키지 보상 획득 이벤트
        /// </summary>
        public event Action OnRewardPackageAchieve;
        /// <summary>
        /// 직업 레벨 패키지 업데이트
        /// </summary>
        public event Action OnUpdatePackageJobLevel;
        /// <summary>
        /// 마일리지 보상 획득 이벤트
        /// </summary>
        public event Action OnUpdateMileageReward;
        /// <summary>
        /// 상점 우편함 받기 이벤트
        /// </summary>
        public event Action OnUpdateShopMail;
        /// <summary>
        /// 비밀상점 목록 변경 이벤트
        /// </summary>
        public event Action OnUpdateSecretShop;
        /// <summary>
        /// 고객감사 정보 변경 이벤트
        /// </summary>
        public event Action OnUpdateCustomerRewardInfo;
        /// <summary>
        /// 온버프 MVP 처치 포인트 이벤트
        /// </summary>
        public event Action OnUpdateOnBuffMvpPoint;
        /// <summary>
        /// 온버프 포인트 획득 여부
        /// </summary>
        public event Action OnUpdateReceivedOnBuffPoint;

        #endregion

        public ShopModel()
        {
            gachaDataRepo = GachaDataManager.Instance;
            shopDataRepo = ShopDataManager.Instance;
            settings = BuildSettings.Instance;
            gamePotImpl = GamePotSystem.Instance;
            connectionManager = ConnectionManager.Instance;
            rewardGroupDataRepo = RewardGroupDataManager.Instance;
            paymentRewardDataRepo = PaymentRewardDataManager.Instance;
            passDataRepo = PassDataManager.Instance;
            onBuffPassDataRepo = OnBuffPassDataManager.Instance;

            secretShopInfos = new List<SecretShopInfo>();
            shopInfoDict = new Dictionary<ObscuredInt, ShopInfo>(ObscuredIntEqualityComparer.Default);
            cashShopPrice = new Dictionary<string, string>(StringComparer.Ordinal);
            everyDayGoodsListDic = new Dictionary<int, UserEverydayGoodsPacket>(IntEqualityComparer.Default);
            hasMailShopItemList = new List<int>();
            labyrinthPass = new PassInfo();
            onBuffPass = new PassInfo();
        }

        public override void AddEvent(UnitEntityType type)
        {
            // 캐쉬 결제 성공 이벤트 추가
            Protocol.RECEIVE_PURCHASE_SUCCESS.AddEvent(OnResponsePurchaseSuccess);
            gamePotImpl.OnPurchaseItemListSuccess += OnPurchaseItemListSuccess;
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            // 캐쉬 결제 성공 이벤트 제거
            Protocol.RECEIVE_PURCHASE_SUCCESS.RemoveEvent(OnResponsePurchaseSuccess);
            gamePotImpl.OnPurchaseItemListSuccess -= OnPurchaseItemListSuccess;
            Timing.KillCoroutines(TAG);
        }

        public override void ResetData()
        {
            everyDayGoodsListDic.Clear();
            hasMailShopItemList.Clear();
            SetJobLevelPackageInfo(0, 0L);

            Timing.KillCoroutines(TAG);
        }

        internal void Initialize(UserInfoData packet)
        {
            Uid = packet.uid;
            IsShareBuff = packet.is_share_buff != 0;
            FirstPaymentState = packet.first_purchase_state;
            Mileage = packet.mileage;
            MileageRewardStep = packet.mileage_reward_step;
            TreePackRemainTime = packet.tree_pack_remaintime;

            if (Uid == 0)
            {
                Debug.LogError($"UID 잘못된 세팅 Uid={Uid}");
            }
            Debug.Log($"마일리지 = {Mileage}, 마일리지 스탭 = {MileageRewardStep}");
            Debug.Log($"금빛 패키지 남은 시간 = {TreePackRemainTime.ToStringTime(@"dd\:hh\:mm\:ss")}");
        }

        internal void Initialize(CharacterPacket packet)
        {
            Cid = packet.cid;
            PayChapterStep = packet.scenario_step;
            PayJobLevelStep = packet.job_level_step;
            FreeChapterStep = packet.free_scenario_step;
            FreeJobLevelStep = packet.free_job_level_step;
            onBuffMvpPoint = packet.onBuffMvpPoint;
            isReceivedOnBuffPoint = packet.isReceivedOnBuffPoint;

            SetJobLevelPackageInfo(packet.jobLevelPackageShopId, packet.joblevelPackageReaminTime);
        }

        internal void Initialize(CustomerRewardPacket packet)
        {
            CustomerRewardNormalStep = packet.NormalStep;
            CustomerRewardNormalRemainTime = packet.NormalRemainTime;
            CustomerRewardPremiumStep = packet.PremiumStep;
            CustomerRewardPremiumRemainTime = packet.PremiumRemainTime;

            // 고객감사 정보 갱신
            OnUpdateCustomerRewardInfo?.Invoke();
        }

        private bool SetJobLevelPackageInfo(int? shopId, long? remainTime)
        {
            bool isDirty = false;

            if (shopId.HasValue)
            {
                isDirty = true;
                JobLevelPackageShopId = shopId.Value;
            }

            if (remainTime.HasValue)
            {
                isDirty = true;
                JobLevelPackageRemainTime = remainTime.Value;
            }

            if (!isDirty)
                return false;

            bool hasJobLevelPackage = JobLevelPackageShopId > 0 && JobLevelPackageRemainTime.ToRemainTime() > 0f;
            SetIsNeedShowJobLevelPopup(hasJobLevelPackage);

            if (hasJobLevelPackage)
            {
                Debug.Log($"직업 레벨 달성 패키지: {JobLevelPackageShopId}, {JobLevelPackageRemainTime.ToStringTime()}");
            }

            return hasJobLevelPackage; // 변경된 값이 존재하면서 레벨패키지가 존재할 경우
        }

        internal void Initialize(ShopPacket shopPacket)
        {
            SetSecretShop(shopPacket?.charSecretShops); // 비밀상점 목록
            nextSecretShopTime = shopPacket == null ? 0L : shopPacket.nextSecretShopTime; // 비밀상점 상품 변경까지 남은시간

            SetShop(); // 상점 목록

            Timing.RunCoroutineSingleton(UpdateSecretShop(), TAG, SingletonBehavior.Overwrite);
            Timing.RunCoroutineSingleton(UpdateResetBuyCount(), TAG, SingletonBehavior.Overwrite);
        }

        /// <summary>
        /// 우편한 상점 탭에 보유중이인 상점ID 목록
        /// </summary>
        internal void Initialize(int[] shopIds)
        {
            hasMailShopItemList.Clear();
            hasMailShopItemList.AddRange(shopIds);
        }

        internal void Initialize(PassPacket packet)
        {
            labyrinthPass.Initialize(packet);
        }

        internal void Initialize(PassSeasonPacket packet)
        {
            labyrinthPass.Initialize(packet);
        }

        internal void Initialize(OnBuffPassPacket packet)
        {
            onBuffPass.Initialize(packet);
        }

        internal void Initialize(OnBuffPassSeasonPacket packet)
        {
            onBuffPass.Initialize(packet);
        }

        /// <summary>
        /// 직럽레벨 패키지 오픈
        /// </summary>
        internal void UpdateJobLevelInfo(int? jobLevelPackageShopId, long? joblevelPackageReaminTime)
        {
            if (SetJobLevelPackageInfo(jobLevelPackageShopId, joblevelPackageReaminTime))
            {
                UI.ShortCut<UIPackageJobLevel>();
            }

            OnUpdatePackageJobLevel?.Invoke();
        }

        /// <summary>
        /// 패스 경험치 업데이트
        /// </summary>
        internal void UpdatePassExp(PassType passType, int? passExp)
        {
            if (!passExp.HasValue)
                return;

            switch (passType)
            {
                case PassType.Labyrinth:
                    labyrinthPass.UpdatePassExp(passExp.Value);
                    break;

                case PassType.OnBuff:
                    onBuffPass.UpdatePassExp(passExp.Value);
                    break;
            }
        }

        /// <summary>
        /// 온버프 mvp 포인트 업데이트
        /// </summary>
        internal void UpdateOnBuffMvpPoint(int? value)
        {
            if (!onBuffMvpPoint.Replace(value))
                return;

            OnUpdateOnBuffMvpPoint?.Invoke();
        }

        /// <summary>
        /// 온버프 포인트 획득 업데이트
        /// </summary>
        internal void UpdateReceivedOnBuffPoint(bool value)
        {
            if (!isReceivedOnBuffPoint.Replace(value))
                return;

            OnUpdateReceivedOnBuffPoint?.Invoke();
        }

        /// <summary>
        /// 우편함에 있는 상품 정보 제거
        /// </summary>
        /// <param name="shopId"></param>
        internal void ReceiveShopMail(int shopId)
        {
            if (!shopInfoDict.ContainsKey(shopId))
                return;

            // 우편함 상점 탭에 있는 상품 제거
            if (hasMailShopItemList.Contains(shopId))
            {
                hasMailShopItemList.Remove(shopId);
            }

            ShopInfo info = shopInfoDict[shopId];

            // 특수 패키지 활성화
            switch (info.PackageType)
            {
                case PackageType.SharePackage: // 셰어 정산 버프 활성화
                    IsShareBuff = true;
                    break;

                // 카프라, 냥다래 패키지(28일) 정보 갱신 
                case PackageType.KafraPackage:
                case PackageType.CatCoinPackage:
                    RequestEveryDayGoodsInfo().WrapNetworkErrors();
                    break;

                case PackageType.LevelAchievePackage:
                    PayJobLevelStep = 0;
                    break;

                case PackageType.ScenarioPackage:
                    PayChapterStep = 0;
                    break;

                case PackageType.FirstPaymentReward:
                    FirstPaymentState = 1;
                    break;
            }

            if (info.Mileage > 0)
            {
                Mileage += info.Mileage; // 마일리지 업데이트
                Debug.Log($"마일리지 업데이트: 추가 마일리지={info.Mileage}, 토탈 마일리지={Mileage}");
            }

            // 시즌 패스 구매
            switch (info.ShopConditionType)
            {
                case ShopConditionType.SeasonPass:
                    labyrinthPass.SetActivePass();
                    break;

                case ShopConditionType.OnBuffPass:
                    onBuffPass.SetActivePass();
                    break;
            }

            OnUpdateShopMail?.Invoke();
        }

        /// <summary>
        /// 나무 보상 증가 남은시간 업데이트
        /// </summary>
        /// <param name="remainTime"></param>
        internal void UpdateTreeRemainTime(long remainTime)
        {
            TreePackRemainTime = remainTime;
            Debug.Log($"금빛 패키지 남은 시간 = {TreePackRemainTime.ToStringTime(@"dd\:hh\:mm\:ss")}");
        }

        internal void SetChannelIndex(int channelIndex)
        {
            this.channelIndex = channelIndex;
        }

        internal void UpdateCustomerRewardInfo(CustomerRewardType type, int step, long remainTime)
        {
            if (type == CustomerRewardType.NORMAL) // 일반
            {
                CustomerRewardNormalStep = step;
                CustomerRewardNormalRemainTime = remainTime;
            }
            else // 프리미엄
            {
                CustomerRewardPremiumStep = step;
                CustomerRewardPremiumRemainTime = remainTime;
            }

            // 고객감사 정보 갱신
            OnUpdateCustomerRewardInfo?.Invoke();
        }

        private void SetSecretShop(CharSecretShop[] charSecretShops)
        {
            // 데이터 초기화
            secretShopInfos.Clear();

            // 비밀상점 정보
            foreach (var item in charSecretShops.OrEmptyIfNull())
            {
                SecretShopInfo info = new SecretShopInfo();
                GachaData data = gachaDataRepo.Get(item.gacha_data_id);
                info.SetData(data);
                info.SetReward(data.GetRewardData());
                info.Initialize(item);
                secretShopInfos.Add(info);
            }
            secretShopInfos.Sort(SortSecretShop);
        }

        /// <summary>
        /// 상점 정보 초기화
        /// </summary>
        private void SetShop()
        {
            shopInfoDict.Clear();
            foreach (var item in shopDataRepo.GetShopDatas().Values.OrEmptyIfNull())
            {
                ShopInfo info = new ShopInfo();
                info.SetData(item);
                info.SetReward();

                shopInfoDict.Add(item.id, info);
            }

            gamePotImpl.GetPurchaseDetailListAsync(); // 결제 상품 정보 목록 요청
        }

        /// <summary>
        /// 결제 상품 목록 요청 성공
        /// </summary>
        private void OnPurchaseItemListSuccess(NPurchaseItem[] items)
        {
            const string HONG_KONG_CURRENCY_CODE = "HKD";
            const string TAIWAN_NTD = "NTD";
            const string TAIWAN_TWD = "TWD";

            // 캐쉬 상품 목록 가격 세팅
            foreach (var item in items)
            {
                string priceWithCurrency;
                if (string.Equals(item.price_currency_code, HONG_KONG_CURRENCY_CODE)
                    || string.Equals(item.price_currency_code, TAIWAN_NTD)
                    || string.Equals(item.price_currency_code, TAIWAN_TWD))
                {
                    priceWithCurrency = GetCustomPriceText(item.price_with_currency);
                }
                else
                {
                    priceWithCurrency = item.price_with_currency;
                }

                // 캐쉬 상품 표시 가격 세팅
                foreach (var shopInfo in shopInfoDict.Values)
                {
                    if (shopInfo.CoinType != CoinType.Cash)
                        continue;

                    if (shopInfo.ProductId == item.productId)
                        shopInfo.SetPrice(priceWithCurrency);
                }
            }
        }

        /// <summary>
        /// 커스텀 Text 처리 (첫번째 자리에서 반올림: 홍콩)
        /// </summary>
        private string GetCustomPriceText(string priceWithCurrency)
        {
            int index = FindDigitIndex(priceWithCurrency);
            if (index == -1)
                return priceWithCurrency;

            string substring = priceWithCurrency.Substring(index);
            if (!float.TryParse(substring, out float result))
                return priceWithCurrency;

            string currency = priceWithCurrency.Substring(0, index);
            return string.Concat(currency, result.ToString("N0"), ".00"); // 소수 첫번째 자리에서 반올림
        }

        /// <summary>
        /// 처음으로 숫자로 된 인덱스 반환
        /// </summary>
        private int FindDigitIndex(string text)
        {
            if (string.IsNullOrEmpty(text))
                return -1;

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 비밀상점 목록 반환
        /// </summary>
        /// <returns></returns>
        public SecretShopInfo[] GetSecretShops()
        {
            return secretShopInfos.ToArray();
        }

        /// <summary>
        /// 비밀상점 무료 변경까지 남은시간
        /// </summary>
        /// <returns></returns>
        public float GetRemainFreeChangeSecretShopTime()
        {
            return nextSecretShopTime;
        }

        /// <summary>
        /// 비밀상점 무료 변경 가능 여부
        /// </summary>
        public bool IsSecretShopFree()
        {
            return GetRemainFreeChangeSecretShopTime() <= 0;
        }

        /// <summary>
        /// 상점 무료 물품 보유 여부
        /// </summary>
        public bool HasFreeShopItem()
        {
            for (int i = TAB_MIN_VALUE; i < TAB_MAX_VALUE; i++)
            {
                if (HasFreeShopItem(i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 길드 상점 무료 물품 보유 여부
        /// </summary>
        public bool HasFreeGuildShopItem()
        {
            return HasFreeShopItem(TAB_GUILD_SHOP);
        }

        /// <summary>
        /// 상점 무료 물품 보유 여부
        /// </summary>
        public bool HasFreeShopItem(int tab)
        {
            Job job = Entity.Character.Job;
            int jobLevel = Entity.Character.JobLevel;
            foreach (var item in shopInfoDict.Values)
            {
                // Tab 조건이 맞지 않은 상품
                if (item.Tab != tab)
                    continue;

                // 상점 탭에서 표시 안 함
                if (item.ShopVisableType == ShopVisableType.NotOnDisplay)
                    continue;

                // 유료 상품이 아님
                if (item.CoinType != CoinType.Free)
                    continue;

                // 이미 구매한 물품
                if (!item.CanBuyLimit)
                    continue;

                // 직업 레벨 초과
                if (item.ShopVisableType == ShopVisableType.JobLevel && item.JobLevel > jobLevel)
                    continue;

                // 기간이 지난 상품
                if (item.SellType == SellType.SellPeriodProduct && item.RemainTime.ToRemainTime() <= 0f)
                    continue;

                // 직업 필터 체크
                if (item.JobFilter != JobFilter.None)
                {
                    bool hasFilter = false;
                    JobFilter[] array = item.JobFilter.FlagsToArray();
                    JobFilter jobFilter = job.ToJobFilter();
                    foreach (JobFilter filter in array)
                    {
                        if (filter == JobFilter.None)
                            continue;

                        if (jobFilter.HasFlag(filter))
                        {
                            hasFilter = true;
                            break;
                        }
                    }

                    if (!hasFilter)
                        continue;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 수령 가능한 마일리지 보상 존재 여부
        /// </summary>
        public bool HasMileageReward()
        {
            int mileage = Mileage; // 마일리지 
            int nextMileageStep = MileageRewardStep + 1; // 다음스텝
            PaymentRewardData mileageData = paymentRewardDataRepo.Get(mileage, nextMileageStep);
            if (mileageData != null)
            {
                if (mileage >= mileageData.GetMileage())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 수령 가능한 패키지 상점 보상 존재
        /// </summary>
        public bool HasPackageNotice(PackageType packageType)
        {
            switch (packageType)
            {
                case PackageType.GeneralPackage:
                    return false;

                case PackageType.PopUpProductPackage:
                    return false;

                case PackageType.SharePackage:
                    return false;

                case PackageType.KafraPackage:
                    return HasEveryDayPackageReward(packageType); // 매일 패키지 보상 존재

                case PackageType.CatCoinPackage:
                    return HasEveryDayPackageReward(packageType); // 매일 패키지 보상 존재

                case PackageType.LevelAchievePackage:
                    return HasJobAchievePackageReward(); // 레벨달성 패키지 보상 존재

                case PackageType.ScenarioPackage:
                    return HasScenarioAchievePackageReward(); // 시나리오달성 패키지 보상 존재

                case PackageType.FirstPaymentReward:
                    return false;

                case PackageType.TreePackage:
                    return false;

                case PackageType.BattlePassPackage:
                    return false;

                case PackageType.OnBuffPassPackage:
                    return false;
            }

            throw new ArgumentException($"[올바르지 않은 {nameof(PackageType)}] {nameof(packageType)} = {packageType}");
        }

        /// <summary>
        /// 수령 가능한 레벨 달성패키지 보상 존재
        /// </summary>
        private bool HasJobAchievePackageReward()
        {
            int jobLevel = Entity.Character.JobLevel;

            // 무료 레벨 달성패키지 체크
            int freeJobAchievePackageGroupId = rewardGroupDataRepo.GetRewardGroupId(ShopConditionType.JobLevel, isFree: true); // 레벨 달성패키지 GroupId (무료)
            int nextFreeJobAchieveStep = FreeJobLevelStep + 1; // 다음 스텝 (무료)
            RewardGroupData freeJobAchieveData = rewardGroupDataRepo.GetByGroupIndex(freeJobAchievePackageGroupId, nextFreeJobAchieveStep);
            if (freeJobAchieveData != null)
            {
                if (jobLevel >= freeJobAchieveData.GetConditionValue())
                    return true;
            }

            // 유료 레벨 달성패키지 체크
            if (PayJobLevelStep != -1) // 유료 레벨 달성패키지 스텝 존재
            {
                int payJobAchievePackageGroupId = rewardGroupDataRepo.GetRewardGroupId(ShopConditionType.JobLevel, isFree: false); // 레벨 달성패키지 GroupId (유료)
                int nextPayJobAchieveStep = PayJobLevelStep + 1; // 다음 스텝 (유료)
                RewardGroupData payJobAchieveData = rewardGroupDataRepo.GetByGroupIndex(payJobAchievePackageGroupId, nextPayJobAchieveStep);
                if (payJobAchieveData != null)
                {
                    if (jobLevel >= payJobAchieveData.GetConditionValue())
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 수령 가능한 시나리오 달성패키지 보상 존재
        /// </summary>
        private bool HasScenarioAchievePackageReward()
        {
            int clearStageId = Mathf.Max(0, Entity.Dungeon.FinalStageId - 1);

            // 무료 시나리오 달성패키지 체크
            int freeScenarioAchievePackageGroupId = rewardGroupDataRepo.GetRewardGroupId(ShopConditionType.Scenario, isFree: true); // 시나리오 달성패키지 GroupId (무료)
            int nextFreeScenarioAchieveStep = FreeChapterStep + 1; // 다음 스텝 (무료)
            RewardGroupData freeScenarioAchieveData = rewardGroupDataRepo.GetByGroupIndex(freeScenarioAchievePackageGroupId, nextFreeScenarioAchieveStep);
            if (freeScenarioAchieveData != null)
            {
                if (clearStageId >= freeScenarioAchieveData.GetConditionValue())
                    return true;
            }

            // 유료 시나리오 달성패키지 체크
            if (PayChapterStep != -1) // 유료 시나리오 달성패키지 스텝 존재
            {
                int payScenarioAchievePackageGroupId = rewardGroupDataRepo.GetRewardGroupId(ShopConditionType.Scenario, isFree: false); // 시나리오 달성패키지 GroupId (유료)
                int nextPayScenarioAchieveStep = PayChapterStep + 1; // 다음 스텝 (유료)
                RewardGroupData payScenarioAchieveData = rewardGroupDataRepo.GetByGroupIndex(payScenarioAchievePackageGroupId, nextPayScenarioAchieveStep);
                if (payScenarioAchieveData != null)
                {
                    if (clearStageId >= payScenarioAchieveData.GetConditionValue())
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 수령 가능한 매일패키지 보상 존재
        /// </summary>
        private bool HasEveryDayPackageReward(PackageType packageType)
        {
            foreach (var item in shopInfoDict.Values)
            {
                // 패키지 타입이 아님
                if (item.PackageType != packageType)
                    continue;

                // 상점 탭에서 표시 안함
                if (item.ShopVisableType == ShopVisableType.NotOnDisplay)
                    continue;

                // 패키지 상태가 보상 받을 수 있음 상태
                if (GetEveryDayPackageState(item.ID) == EveryDayPackageState.StandByReward)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 상점 목록 반환
        /// </summary>
        public ShopInfo[] GetShopInfos(int tab, Job job)
        {
            List<ShopInfo> list = new List<ShopInfo>();

            foreach (var item in shopInfoDict.Values)
            {
                // 탭이 다름
                if (item.Tab != tab)
                    continue;

                // 상점 탭에서 표시 안함
                if (item.ShopVisableType == ShopVisableType.NotOnDisplay)
                    continue;

                // 기간 상품 체크
                if (item.SellType == SellType.SellPeriodProduct)
                {
                    // 기간이 지남
                    if (item.RemainTime.ToRemainTime() <= 0)
                        continue;
                }

                // 직업 필터 없는 아이템
                if (item.JobFilter == JobFilter.None)
                {
                    list.Add(item);
                    continue;
                }

                // 직업 필터 체크
                bool hasFilter = false;
                JobFilter[] array = item.JobFilter.FlagsToArray();
                JobFilter jobFilter = job.ToJobFilter();
                foreach (JobFilter filter in array)
                {
                    if (filter == JobFilter.None)
                        continue;

                    // 구매 가능
                    if (jobFilter.HasFlag(filter))
                    {
                        hasFilter = true;
                        break;
                    }
                }
                if (!hasFilter)
                    continue;

                list.Add(item);
            }

            return list.ToArray();
        }

        /// <summary>
        /// 캐릭터 셰어 충전티켓 반환
        /// </summary>
        public ShopInfo GetInfo(ShareTicketType ticketType)
        {
            switch (ticketType)
            {
                case ShareTicketType.ChargeItem1:
                    return GetInfo(BasisShop.ChargeItem1.GetID());

                case ShareTicketType.ChargeItem2:
                    return GetInfo(BasisShop.ChargeItem2.GetID());

                case ShareTicketType.ChargeItem3:
                    return GetInfo(BasisShop.ChargeItem3.GetID());
            }

            return null;
        }

        public ShopInfo GetInfo(int shopId)
        {
            return shopInfoDict.ContainsKey(shopId) ? shopInfoDict[shopId] : null;
        }

        public void SetIsNeedShowJobLevelPopup(bool value)
        {
            isNeedShowJobLevelPopup = value;
        }

        public bool GetIsNeedShowJobLevelPopup()
        {
            return isNeedShowJobLevelPopup;
        }

        public bool HasPaymentHistory()
        {
            return FirstPaymentState == 1;
        }

        /// <summary>
        /// 고객감사 보상 타입별 광고제거 조건
        /// </summary>
        public bool HasSumPaymentType(CustomerRewardType type)
        {
            switch (type)
            {
                case CustomerRewardType.NORMAL:
                    return Mileage >= BasisType.CUSTOMER_REWARD_NORMAL_MILEAGE.GetInt();

                case CustomerRewardType.PREMIUM:
                    return Mileage >= BasisType.CUSTOMER_REWARD_PREMIUM_MILEAGE.GetInt();
            }

            return false;
        }

        /// <summary>
        /// 고객 보상 스텝 반환
        /// </summary>
        public int GetCustomerRewardStep(CustomerRewardType type)
        {
            switch (type)
            {
                case CustomerRewardType.NORMAL:
                    return CustomerRewardNormalStep;

                case CustomerRewardType.PREMIUM:
                    return CustomerRewardPremiumStep;
            }

            return 0;
        }

        /// <summary>
        /// 고객 보상 남은 시간 반환
        /// </summary>
        public RemainTime GetCustomerRemainTime(CustomerRewardType type)
        {
            switch (type)
            {
                case CustomerRewardType.NORMAL:
                    return CustomerRewardNormalRemainTime;

                case CustomerRewardType.PREMIUM:
                    return CustomerRewardPremiumRemainTime;
            }

            return 0L;
        }

        public void AddUpdatePassExpEvent(PassType passType, Action action)
        {
            GetPassInfo(passType).OnUpdatePassExp += action;
        }

        public void RemoveUpdatePassExpEvent(PassType passType, Action action)
        {
            GetPassInfo(passType).OnUpdatePassExp -= action;
        }

        public void AddUpdateBuyPassExpEvent(PassType passType, Action action)
        {
            GetPassInfo(passType).OnUpdateBuyPassExp += action;
        }

        public void RemoveUpdateBuyPassExpEvent(PassType passType, Action action)
        {
            GetPassInfo(passType).OnUpdateBuyPassExp -= action;
        }

        public void AddUpdatePassRewardEvent(PassType passType, Action action)
        {
            GetPassInfo(passType).OnUpdatePassReward += action;
        }

        public void RemoveUpdatePassRewardEvent(PassType passType, Action action)
        {
            GetPassInfo(passType).OnUpdatePassReward -= action;
        }

        #region 프로토콜 처리

        #region 비밀상점       

        /// <summary>
        /// 비밀상점 목록 변경 요청
        /// </summary>     
        public async Task<bool> RequestSecretShopInit(bool useResetItem)
        {
            if (useResetItem)
            {
                string title = LocalizeKey._90021.ToText(); // 비밀상점 상품 변경
                string description = LocalizeKey._90212.ToText(); // 변경권을 소모하여 상품을 변경하시겠습니까?
                if (!await UI.SelectPopup(title, description))
                    return false;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", useResetItem);

            var response = await Protocol.SECRET_SHOP_INIT_LIST.SendAsync(sfs);
            if (response.isSuccess)
            {
                // 1. 비밀상점 목록
                var packet = response.GetPacketArray<CharSecretShop>("1");
                SetSecretShop(packet);

                // 2. 쿨타임 0으로 비밀상점 목록 받은 경우
                if (response.ContainsKey("2"))
                    nextSecretShopTime = response.GetLong("2") + 1000; // 보정값 1초 추가

                // cud. 캐릭터 업데이트 데이터 (냥다래 구입으로 비밀상점 목록 받은 경우)
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                OnSecretShopFree?.Invoke();
                OnUpdateSecretShop?.Invoke();
            }
            else
            {
                // 비밀상점 목록 변경에 실패하였습니다.
                UI.ConfirmPopup(LocalizeKey._90089);
            }
            return response.isSuccess;
        }

        /// <summary>
        /// 비밀 상점 구입
        /// </summary>
        /// <returns></returns>
        public async Task RequestSecretShopPurchase(SecretShopInfo info)
        {
            if (info.CoinType == CoinType.CatCoin || info.CoinType == CoinType.Zeny)
            {
                string title = LocalizeKey._5.ToText(); // 알람
                string description = LocalizeKey._90023.ToText(); // 상품을 구매하시겠습니까?
                if (!await UI.CostPopup(info.CoinType, info.Cost, title, description))
                    return;
            }

            UI.ShowIndicator();

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", 1);
            sfs.PutInt("2", info.GachaID);
            var response = await Protocol.SHOP_PURCHASE.SendAsync(sfs);

            UI.HideIndicator();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            Quest.QuestProgress(QuestType.SECRET_SHOP_BUY_COUNT); // 비밀 상점 구매 횟수

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                UI.RewardToast(charUpdateData.rewards); // 획득 보상 보여줌 (토스트팝업)
            }

            info.BuyShop();
        }
        #endregion

        /// <summary>
        /// 상점 상품 정보
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RequestShopInfoList()
        {
            var response = await Protocol.REQUEST_ITEMSHOP_LIMIT_LIST.SendAsync();
            if (response.isSuccess)
            {
                // 구매 횟수 정보(기간단위 초기화 상품)
                if (response.ContainsKey("1"))
                {
                    UserLimitedGoodsPacket[] userLimitedGoods = response.GetPacketArray<UserLimitedGoodsPacket>("1");
                    foreach (var item in userLimitedGoods)
                    {
                        if (shopInfoDict.ContainsKey(item.shopId))
                        {
                            shopInfoDict[item.shopId].SetBuyCount(item.count);
                            shopInfoDict[item.shopId].SetResetRemainTime(item.remainTime);
                        }
                    }
                }

                // 다음 첫구매까지 남은 시간 정보
                if (response.ContainsKey("2"))
                {
                    UserFirstGoodsPacket[] userFirstGoodsPackets = response.GetPacketArray<UserFirstGoodsPacket>("2");
                    foreach (var item in userFirstGoodsPackets)
                    {
                        if (shopInfoDict.ContainsKey(item.shopId))
                        {
                            shopInfoDict[item.shopId].SetFirstRemainTime(item.remainTime);
                        }
                    }
                }

                // 매일매일 패키지(카프라,냥다래 프리미엄) 정보
                if (response.ContainsKey("3"))
                {
                    everyDayGoodsListDic.Clear();
                    foreach (var item in response.GetPacketArray<UserEverydayGoodsPacket>("3"))
                    {
                        everyDayGoodsListDic.Add(item.shopId, item);
                    }
                }

                // 특정 기간에만 살수 있는 기간제 상품 정보(기간제 상품)
                if (response.ContainsKey("5"))
                {
                    PeriodProductPacket[] userLimitedGoods = response.GetPacketArray<PeriodProductPacket>("5");
                    foreach (var item in userLimitedGoods)
                    {
                        if (shopInfoDict.ContainsKey(item.shopId))
                        {
                            shopInfoDict[item.shopId].SetBuyRemainTime(item.remainTime);
                        }
                    }
                }

                OnResetFreeItemBuyCount?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }

            return response.isSuccess;
        }

        /// <summary>
        /// 일반 상점 구매
        /// </summary>
        public async Task RequestNormalShopPurchase(int shopId)
        {
            if (!shopInfoDict.ContainsKey(shopId))
                return;

            ShopInfo info = shopInfoDict[shopId];

            // 다중 구매인 경우 개수 확인
            int buyCount = 0;
            #region 다중 구매가 아니어도 팝업은 띄움.
            if (info.CoinType == CoinType.Free)
            {
                buyCount = 1;
            }
            else
            {
                buyCount = await ConfirmBuyCountAsync(info);
            }

            #endregion

            if (buyCount <= 0) // 구매하지 않음
                return;

            if (info.CoinType == CoinType.OnBuffPoint)
            {
                if (!GetPassInfo(PassType.OnBuff).IsActivePass())
                {
                    UIOnBuffPassRequired.SelectResult result = await UI.Show<UIOnBuffPassRequired>().ShowAsync();
                    if (result == UIOnBuffPassRequired.SelectResult.Confirm)
                        ShowPackageUI(PassType.OnBuff);

                    return;
                }
            }

            if (!info.GetCanBuy(buyCount))
            {
                Debug.LogError("구매 제한 횟수 초과");
            }

            // 구매 비용 보유 체크
            int needCoin = buyCount * info.Cost;
            if (!info.CoinType.Check(needCoin))
                return;

            UI.ShowIndicator();

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", 2); // 1:비밀상점,2:일반 상점,3:임시
            sfs.PutInt("2", info.ID);
            if (buyCount > 1)
            {
                sfs.PutInt("4", buyCount);
            }
            var response = await Protocol.SHOP_PURCHASE.SendAsync(sfs);

            UI.HideIndicator();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            info.AddBuyCount(buyCount); // 클라에서 구매 횟수 증가
            switch (info.LimitDayType) // 클라에서 구매 제한 시간 세팅
            {
                case LimitDayType.Lmit_6H:
                    info.SetResetRemainTime((long)TimeSpan.FromHours(6).TotalMilliseconds);
                    break;

                case LimitDayType.Lmit_12H:
                    info.SetResetRemainTime((long)TimeSpan.FromHours(12).TotalMilliseconds);
                    break;

                case LimitDayType.Lmit_24H:
                    info.SetResetRemainTime((long)TimeSpan.FromHours(24).TotalMilliseconds);
                    break;

                case LimitDayType.Lmit_36H:
                    info.SetResetRemainTime((long)TimeSpan.FromHours(36).TotalMilliseconds);
                    break;

                case LimitDayType.Lmit_48H:
                    info.SetResetRemainTime((long)TimeSpan.FromHours(48).TotalMilliseconds);
                    break;

                case LimitDayType.Lmit_60H:
                    info.SetResetRemainTime((long)TimeSpan.FromHours(60).TotalMilliseconds);
                    break;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
            }

            OnPurchaseSuccess?.Invoke();
        }

        /// <summary>
        /// 다중 구매 팝업
        /// </summary>
        public async Task<int> ConfirmBuyCountAsync(ShopInfo info)
        {
            UIAmountBuyData data = new UIAmountBuyData();
            data.shopInfo = info;

            PopupType popupType = PopupType.None;
            int buyCount = 1;
            UI.Show<UIAmountBuy>().Show(data.OnClickedEvent((_popupType, _buyCount) =>
            {
                popupType = _popupType;
                buyCount = _buyCount;
            }));
            await Awaiters.Until(() => popupType != PopupType.None);

            if (popupType == PopupType.Cancel)
                return 0; // 0개 (구매하지 않음)

            return buyCount;
        }

        #endregion

        #region 결제

        /// <summary>
        /// 캐쉬 상품 구매
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task RequestCashShopPurchase(int shopId, bool isShowNotiPopup)
        {
            if (!shopInfoDict.ContainsKey(shopId))
                return;

            if (hasMailShopItemList.Contains(shopId))
            {
                Debug.LogError("이미 구매한 상품");
                return;
            }

            ShopInfo info = shopInfoDict[shopId];

            if (info.CoinType != CoinType.Cash)
            {
                Debug.LogError("캐쉬 결제만 가능");
                return;
            }

            #region 구매확인 팝업

            if (info.PackageType == PackageType.None)
            {
                var buyCount = await ConfirmBuyCountAsync(info);
                if (buyCount <= 0) // 구매하지 않음
                    return;
            }

            #endregion

            // 게스트 팝업
            if (gamePotImpl.IsGuestLogin())
            {
                string title = LocalizeKey._5.ToText(); // 알림
                string cancelText = LocalizeKey._2.ToText(); // 취소

                string accountText = LocalizeKey._90297.ToText(); // 현재 게스트 계정으로 연동 중입니다.\n게임 데이터 보존을 위해 계정 연동을 해주세요.
                string accountConfirmText = LocalizeKey._90298.ToText(); // 계정 연동
                UIGuestNoticePopup popup = UI.Show<UIGuestNoticePopup>();
                if (await popup.Show(title, accountText, cancelText, accountConfirmText) == UIGuestNoticePopup.SelectResult.Confirm)
                {
                    UI.Show<UIOption>();
                    return;
                }

                // Popup이 닫힐 때까지 대기
                while (popup)
                {
                    await Task.Delay(25);
                }

                string purchaseText = LocalizeKey._90299.ToText(); // 게스트 계정으로 구매 시\n구매 기록이 초기화되는 문제가 발생할 수 있습니다.\n\n그래도 구매하시겠습니까?
                string purchaseConfirmText = LocalizeKey._90300.ToText(); // 구매
                popup = UI.Show<UIGuestNoticePopup>();
                if (await popup.Show(title, purchaseText, cancelText, purchaseConfirmText) == UIGuestNoticePopup.SelectResult.Cancel)
                    return;

                // Popup이 닫힐 때까지 대기
                while (popup)
                {
                    await Task.Delay(25);
                }
            }

            // Protocol.RECEIVE_PURCHASE_SUCCESS (265. 결제 성공 이벤트)
            // 캐쉬 결제 성공 이벤트에서 인티케이터 끄는 처리를 위해 
            UI.ShowIndicator(60f);

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", 2); // 1:비밀상점,2:일반 상점,3:임시
            sfs.PutInt("2", info.ID);
#if UNITY_EDITOR
            sfs.PutByte("3", 99); // 에디터에서 캐쉬 상품 결제 할때 사용
#else      
            sfs.PutByte("3", (byte)settings.StoreType);
#endif
            var response = await Protocol.SHOP_PURCHASE.SendAsync(sfs);
            if (!response.isSuccess)
            {
                UI.HideIndicator();
                response.ShowResultCode();
                return;
            }

            if (Application.isEditor)
                return;

            // UID가 0일 경우 결제 요청 X
            if (Uid == 0)
                return;

            // 플렛폼별 캐쉬 결제 요청
            int serverIndex = connectionManager.GetSelectServerGroupId();
            string serverPosition = connectionManager.GetServerPosition();
            string uniqueId = $"{serverIndex}_{channelIndex}_{Uid}_{Cid}_{info.ID}_{serverPosition}";
            gamePotImpl.Purchase(info.ProductId, uniqueId);
        }

        /// <summary>
        /// 결제 성공
        /// </summary>
        /// <param name="response"></param>
        void OnResponsePurchaseSuccess(Response response)
        {
            int shopId = response.GetInt("1");

            if (!shopInfoDict.ContainsKey(shopId))
                return;

            ShopInfo info = shopInfoDict[shopId];
            info.AddBuyCount(); // 클라에서 구매 횟수 증가

            // 특수 패키지의 경우 일정기간 또는 영구적으로 재구매 불가
            // 우편함에 상품이 있을때 재구매 불가처리
            switch (info.PackageType)
            {
                case PackageType.SharePackage:
                case PackageType.KafraPackage:
                case PackageType.CatCoinPackage:
                case PackageType.LevelAchievePackage:
                case PackageType.ScenarioPackage:
                case PackageType.TreePackage:
                case PackageType.BattlePassPackage:
                case PackageType.OnBuffPassPackage:
                    if (!hasMailShopItemList.Contains(info.ID))
                        hasMailShopItemList.Add(info.ID);
                    break;

                // 직업 레벨 패키지 1회만 구입 가능
                case PackageType.PopUpProductPackage:
                    UpdateJobLevelInfo(0, 0);
                    break;
            }

            UI.HideIndicator();
            NotifyAddAlarm(AlarmType.MailShop); // 메일함 상점 탭 빨간점!
            OnPurchaseSuccess?.Invoke();

            UI.Show<UIMailShortCutPopup>().Set(info.Name);
        }

        /// <summary>
        /// 우편함 상점 탭으로 이동
        /// </summary>
        private void ShortCutMail()
        {
            if (GameServerConfig.IsOnBuff())
            {
                UI.ShortCut<UIMailOnBuff>().Set(tabIndex: 2);
            }
            else
            {
                UI.ShortCut<UIMail>().Set(tabIndex: 2);
            }
        }

        /// <summary>
        /// 메일함에 보유중인 상점 아이템
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public bool HasMailShopItem(int shopId)
        {
            return hasMailShopItemList.Contains(shopId);
        }

        #endregion

        #region 패키지

        /// <summary>
        /// 매일매일 패키지 내 보상 정보
        /// 구매 후, 보상 받은 후 정보 갱신 필요, UI입장시 갱신 필요
        /// </summary>
        /// <returns></returns>
        public async Task RequestEveryDayGoodsInfo(bool isShowIndicator = true)
        {
            if (isShowIndicator)
                UI.ShowIndicator();

            Response response = await Protocol.EVERYDAY_GOODS_MY_INFO.SendAsync();

            if (isShowIndicator)
                UI.HideIndicator();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
            {
                everyDayGoodsListDic.Clear();
                foreach (var item in response.GetPacketArray<UserEverydayGoodsPacket>("1"))
                {
                    everyDayGoodsListDic.Add(item.shopId, item);
                }
            }

            OnUpdateEveryDayGoods?.Invoke();
        }

        /// <summary>
        /// 매일매일 패키지 보상 받기
        /// </summary>
        /// <returns></returns>
        public async Task RequestEveryDayGoodsReward(int shopId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", shopId);
            Response response = await Protocol.EVERYDAY_GOODS_GET.SendAsync(sfs);

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

                UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
            }

            RequestEveryDayGoodsInfo().WrapNetworkErrors(); // 패키지 정보 갱신
        }

        /// <summary>
        /// 레벨업 패키지 보상 받기
        /// </summary>
        /// <returns></returns>
        public async Task RequestPayJobLevelReward(int step, bool isFree)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", step);
            sfs.PutBool("2", isFree);
            Response response = await Protocol.REQUEST_PAY_JOB_LEVEL_REWARD.SendAsync(sfs);

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

                UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
            }

            if (isFree)
            {
                FreeJobLevelStep = (short)step;
            }
            else
            {
                PayJobLevelStep = (short)step;
            }
            OnRewardPackageAchieve?.Invoke();
        }

        /// <summary>
        /// 시나리오 패키지 보상 받기
        /// </summary>
        /// <returns></returns>
        public async Task RequestPayScenarioReward(int step, bool isFree)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", step);
            sfs.PutBool("2", isFree);
            Response response = await Protocol.REQUEST_PAY_SCENARIO_REWARD.SendAsync(sfs);

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

                UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
            }

            if (isFree)
            {
                FreeChapterStep = (short)step;
            }
            else
            {
                PayChapterStep = (short)step;
            }
            OnRewardPackageAchieve?.Invoke();
        }

        /// <summary>
        /// 매일매일 패키지 상태
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public EveryDayPackageState GetEveryDayPackageState(int shopId)
        {
            if (HasMailShopItem(shopId))
                return EveryDayPackageState.CompletePurchase;

            // 패키지 정보가 없음 구매가능
            if (!everyDayGoodsListDic.ContainsKey(shopId))
                return EveryDayPackageState.AvailablePurchase;

            UserEverydayGoodsPacket packet = everyDayGoodsListDic[shopId];

            // isReward가 true이고 getCount이 - 1일 경우 재구입이 가능한상태이다.
            if (packet.isReward && packet.getCount == -1)
                return EveryDayPackageState.AvailablePurchase;

            // 받을 보상이 있음
            if (packet.isReward && packet.getCount != -1)
                return EveryDayPackageState.StandByReward;

            // 이미 보상을 받은 상태
            return EveryDayPackageState.ReceivedReward;
        }

        /// <summary>
        /// 보상 받은 횟수
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public int GetRewardCount(int shopId)
        {
            if (!everyDayGoodsListDic.ContainsKey(shopId))
                return 0;

            UserEverydayGoodsPacket packet = everyDayGoodsListDic[shopId];

            return packet.getCount;
        }

        #endregion

        /// <summary>
        /// 마일리지 보상 요청
        /// </summary>
        public async Task RequestMileageReward()
        {
            Response response = await Protocol.REQUEST_MILEAGE_REWARD_GET.SendAsync();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            MileageRewardStep += 1;

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
            }

            OnUpdateMileageReward?.Invoke();
        }

        /// <summary>
        /// 고객감사 보상요청
        /// </summary>
        public async Task RequestCustomerReward(CustomerRewardType type)
        {
            // 광고 체크
            bool isFree = HasSumPaymentType(type);

            // 보상 요청
            const byte FREE_FLAG = 1;
            const byte NO_FREE_FLAG = 0;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)type);
            sfs.PutByte("2", isFree ? FREE_FLAG : NO_FREE_FLAG);

            var response = await Protocol.REQUEST_AD_REWARD.SendAsync(sfs);
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

                UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
            }

            byte customerRewardType = response.GetByte("1");
            int step = response.GetInt("2");
            long remainTime = response.GetLong("3");
            UpdateCustomerRewardInfo(customerRewardType.ToEnum<CustomerRewardType>(), step, remainTime);
        }

        /// <summary>
        /// 상품 교환
        /// </summary>
        public async Task RequestExcnage(int id, int count)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id);
            sfs.PutInt("2", count);
            Response response = await Protocol.REQUEST_KAF_EXCHANGE.SendAsync(sfs);
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

                UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
            }
        }

        /// <summary>
        /// 30초에 한번씩 비밀상점 무료변경 가능한지 체크
        /// </summary>
        private IEnumerator<float> UpdateSecretShop()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(CALL_WAIT_TIME);
                OnSecretShopFree?.Invoke();
            }
        }

        private IEnumerator<float> UpdateResetBuyCount()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(CALL_WAIT_TIME);

                bool isInvoke = false;
                foreach (var item in shopInfoDict.Values)
                {
                    // 상점 탭에서 표시 안 함
                    if (item.ShopVisableType == ShopVisableType.NotOnDisplay)
                        continue;

                    // 유료 상품이 아님
                    if (item.CoinType != CoinType.Free)
                        continue;

                    // 단위 기간 상품이 아님
                    if (item.SellType != SellType.ResetPeriodProduct)
                        continue;

                    // 남은 시간이 없음
                    if (!item.IsSetResetRemainTime)
                        continue;

                    // 영구구매횟수 제한
                    if (item.LimitDayType == LimitDayType.NoLimit)
                        continue;

                    // 구매 가능한 물품
                    if (item.CanBuyLimit)
                        continue;

                    // 남은 시간 존재
                    if (item.ResetRemainTime.ToRemainTime() > 0f)
                        continue;

                    item.SetBuyCount(0); // 구매 횟수 초기화!
                    isInvoke = true;
                }

                if (isInvoke)
                    OnResetFreeItemBuyCount?.Invoke();
            }
        }

        /// <summary>
        /// 교역 & 생산 광고 스킵 조건 (마일리지)
        /// </summary>
        public bool IsSkipTradeProductionAd()
        {
            return Mileage >= BasisType.TRADE_PRODUCTION_MILEAGE.GetInt();
        }

        /// <summary>
        /// 패스 무료 보상 받았는지 여부
        /// </summary>
        public bool IsReceivePassFree(PassType passType, int level)
        {
            return GetPassInfo(passType).IsReceivePassFree(level);
        }

        /// <summary>
        /// 패스 유료 보상 받았는지 여부
        /// </summary>
        public bool IsReceivePassPay(PassType passType, int level)
        {
            return GetPassInfo(passType).IsReceivePassPay(level);
        }

        public int GetPassSeason(PassType passType)
        {
            return GetPassInfo(passType).GetSeason();
        }

        public RemainTime GetSeasonRemainTime(PassType passType)
        {
            return GetPassInfo(passType).GetSeasonRemainTime();
        }

        public int GetPassFreeStep(PassType passType)
        {
            return GetPassInfo(passType).GetPassFreeStep();
        }

        public int GetPassPayStep(PassType passType)
        {
            return GetPassInfo(passType).GetPassPayStep();
        }

        /// <summary>
        /// 패스 보상 받기
        /// </summary>
        public async Task RequestPassReward(PassType passType, byte passFlag, int level)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", passFlag); // (1:무료, 2:유료)
            sfs.PutInt("2", level);
            Response response = await GetRewardProtocol(passType).SendAsync(sfs);
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

                UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
            }

            GetPassInfo(passType).Receive(passFlag, level);
        }

        /// <summary>
        /// 패스 경험치 구매
        /// </summary>
        public async Task RequestBuyPassExp()
        {
            Response response = await Protocol.REQUEST_PASS_BUY_POINT.SendAsync();
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

            labyrinthPass.InvokeBuyPassExp();
        }

        /// <summary>
        /// 온버프 MVP 처치포인트 => 온버프 변환
        /// </summary>
        public async Task RequestOnBuffMvpPointGet()
        {
            Response response = await Protocol.REQUEST_ONBUFF_MVP_POINT_GET.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            UpdateReceivedOnBuffPoint(true);

            bool isCoinZero = response.GetBool("1");
            if (isCoinZero)
            {
                // Protocol.RECEIVE_ONBUFF_COIN_ZERO 에서 Toast 처리
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._90327.ToText()); // 우편함으로 OnBuff 포인트가 지급되었습니다.
            }
        }

        /// <summary>
        /// 패스 노출 여부 (종료시에는 패스 컨텐츠 모두 보이지 않도록 처리)
        /// </summary>
        public bool IsDisplayPass(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    return true; // 라비린스 패스는 종료되지 않는다

                case PassType.OnBuff:
                    return GameServerConfig.IsOnBuff();
            }

            return false;
        }

        /// <summary>
        /// 패스 활성화 여부
        /// </summary>
        public bool IsActivePass(PassType passType)
        {
            if (!IsDisplayPass(passType))
                return false;

            return GetPassInfo(passType).IsActivePass();
        }

        /// <summary>
        /// 시즌 패스 진행중 여부
        /// </summary>
        public bool IsBattlePass(PassType passType)
        {
            if (!IsDisplayPass(passType))
                return false;

            return GetPassInfo(passType).IsBattlePass();
        }

        /// <summary>
        /// 패스 경험치
        /// </summary>
        public int GetPassExp(PassType passType)
        {
            return GetPassInfo(passType).GetPassExp();
        }

        /// <summary>
        /// 수령 가능한 패스 보상 존재
        /// </summary>
        public bool IsPassRewardNotice(PassType passType)
        {
            if (!IsDisplayPass(passType))
                return false;

            IPassLevel passLevel = GetPassLevel(passType);

            if (IsActivePass(passType))
            {
                // 패스 유료 보상 받을 수 있음
                for (int i = 1; i <= passLevel.Level; i++)
                {
                    if (!IsReceivePassPay(passType, i))
                        return true;
                }
            }
            else
            {
                // 패스 무료 보상 받을 수 있음
                for (int i = 1; i <= passLevel.Level; i++)
                {
                    if (!IsReceivePassFree(passType, i))
                        return true;
                }
            }
            return false;
        }

        public IPassLevel GetPassLevel(PassType passType)
        {
            int passExp = GetPassExp(passType);
            return GetPassDataRepo(passType).GetLevel(passExp);
        }

        public int GetLastPassExp(PassType passType)
        {
            return GetPassDataRepo(passType).GetLastPassExp();
        }

        public int GetLastPassLevel(PassType passType)
        {
            return GetPassDataRepo(passType).GetLastPassLevel();
        }

        public IPassData GetPassData(PassType passType, int level)
        {
            return GetPassDataRepo(passType).Get(level);
        }

        public IEnumerable<IPassData> GetPassDataEnumerable(PassType passType)
        {
            return GetPassDataRepo(passType).GetEnumerable();
        }

        private PassInfo GetPassInfo(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    return labyrinthPass;

                case PassType.OnBuff:
                    return onBuffPass;
            }

            throw new ArgumentException($"유효하지 않은 처리: {nameof(passType)} = {passType}");
        }

        private IPassDataRepoImpl GetPassDataRepo(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    return passDataRepo;

                case PassType.OnBuff:
                    return onBuffPassDataRepo;
            }

            throw new ArgumentException($"유효하지 않은 처리: {nameof(passType)} = {passType}");
        }

        public BasisShop GetBasisShop(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    return BasisShop.SeasonPass;

                case PassType.OnBuff:
                    return BasisShop.OnBuffPass;
            }

            throw new ArgumentException($"유효하지 않은 처리: {nameof(passType)} = {passType}");
        }

        public void ShowPackageUI(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    UI.Show<UIPackageBattlePass>();
                    break;

                case PassType.OnBuff:
                    UI.Show<UIPackageOnBuffPass>();
                    break;

                default:
                    throw new ArgumentException($"유효하지 않은 처리: {nameof(passType)} = {passType}");
            }
        }

        private Protocol GetRewardProtocol(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    return Protocol.REQUEST_PASS_REWARD;

                case PassType.OnBuff:
                    return Protocol.REQUEST_ONBUFF_PASS_REWARD;
            }

            throw new ArgumentException($"유효하지 않은 처리: {nameof(passType)} = {passType}");
        }

        int SortSecretShop(SecretShopInfo x, SecretShopInfo y)
        {
            return x.OpenChapter.CompareTo(y.OpenChapter);
        }

        bool IEqualityComparer<ShareTicketType>.Equals(ShareTicketType x, ShareTicketType y)
        {
            return x == y;
        }

        int IEqualityComparer<ShareTicketType>.GetHashCode(ShareTicketType obj)
        {
            return obj.GetHashCode();
        }
    }
}