using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIShop"/>
    /// </summary>
    public sealed class ShopPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void ShowShopDefault();
            void ShowShopSecret();
            void UpdateZeny(long zeny);
            void UpdateCatCoin(long catCoin);
            void UpdateGuildCoin(long guildCoin);
            void SetLockSecretShop(bool isActive);
            void UpdatePackageTabNotice();
            void UpdateDefaultTabNotice();
            void UpdateMileageNotice();
        }

        private readonly IView view;
        private readonly GoodsModel goodsModel;
        private readonly ShopModel shopModel;
        private readonly QuestModel questModel;
        private readonly DungeonModel dungeonModel;
        private readonly StageDataManager stageDataRepo;
        private readonly CharacterModel characterModel;
        private readonly InventoryModel invenModel;
        private readonly IronSourceManager ironSourceManager;

        private int shopId;

        public event System.Action PlaySlotAnim;
        public event System.Action OnUpdateRewardPackageAchieve;
        public event System.Action OnUpdateEveryDayGoods;

        public event System.Action OnStandByReward
        {
            add { questModel.OnStandByReward += value; }
            remove { questModel.OnStandByReward -= value; }
        }

        public ShopPresenter(IView view)
        {
            this.view = view;
            goodsModel = Entity.player.Goods;
            shopModel = Entity.player.ShopModel;
            questModel = Entity.player.Quest;
            dungeonModel = Entity.player.Dungeon;
            stageDataRepo = StageDataManager.Instance;
            characterModel = Entity.player.Character;
            invenModel = Entity.player.Inventory;
            ironSourceManager = IronSourceManager.Instance;
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += view.UpdateZeny;
            goodsModel.OnUpdateCatCoin += view.UpdateCatCoin;
            goodsModel.OnUpdateGuildCoin += view.UpdateGuildCoin;
            shopModel.OnPurchaseSuccess += view.Refresh;
            shopModel.OnRewardPackageAchieve += OnRewardPackageAchieve;
            shopModel.OnPurchaseSuccess += view.UpdateDefaultTabNotice;
            shopModel.OnUpdateMileageReward += view.UpdateMileageNotice;
            characterModel.OnUpdateJobLevel += OnUpdateJobLevel;
            dungeonModel.OnUpdateClearedStage += view.UpdatePackageTabNotice;
            shopModel.OnUpdateEveryDayGoods += InvokeUpdateEveryDayGoods;
            shopModel.OnUpdateSecretShop += OnUpdateSecretShop;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= view.UpdateZeny;
            goodsModel.OnUpdateCatCoin -= view.UpdateCatCoin;
            goodsModel.OnUpdateGuildCoin -= view.UpdateGuildCoin;
            shopModel.OnPurchaseSuccess -= view.Refresh;
            shopModel.OnRewardPackageAchieve -= OnRewardPackageAchieve;
            shopModel.OnPurchaseSuccess -= view.UpdateDefaultTabNotice;
            shopModel.OnUpdateMileageReward -= view.UpdateMileageNotice;
            characterModel.OnUpdateJobLevel -= OnUpdateJobLevel;
            dungeonModel.OnUpdateClearedStage -= view.UpdatePackageTabNotice;
            shopModel.OnUpdateEveryDayGoods -= InvokeUpdateEveryDayGoods;
            shopModel.OnUpdateSecretShop -= OnUpdateSecretShop;
        }

        public void AddUpdatePassExpEvent(PassType passType, System.Action action)
        {
            shopModel.AddUpdatePassExpEvent(passType, action);
        }

        public void RemoveUpdatePassExpEvent(PassType passType, System.Action action)
        {
            shopModel.RemoveUpdatePassExpEvent(passType, action);
        }

        public void AddUpdatePassRewardEvent(PassType passType, System.Action action)
        {
            shopModel.AddUpdatePassRewardEvent(passType, action);
        }

        public void RemoveUpdatePassRewardEvent(PassType passType, System.Action action)
        {
            shopModel.RemoveUpdatePassRewardEvent(passType, action);
        }

        void OnUpdateJobLevel(int jobLevel)
        {
            view.UpdatePackageTabNotice();
        }

        void OnRewardPackageAchieve()
        {
            view.UpdatePackageTabNotice();
            OnUpdateRewardPackageAchieve?.Invoke();
        }

        void InvokeUpdateEveryDayGoods()
        {
            view.UpdatePackageTabNotice();
            OnUpdateEveryDayGoods?.Invoke();
        }

        /// <summary>
        /// 비밀상점 목록 초기화 비용
        /// </summary>
        public int SecretShopInitCatCoin => shopModel.SecretShopInitCatCoin;

        /// <summary>
        /// 비밀상점 콘텐츠 오픈 여부
        /// </summary>
        /// <returns></returns>
        public bool IsOpenSecretShop()
        {
            return questModel.IsOpenContent(ContentType.SecretShop, isShowPopup: false);
        }

        /// <summary>
        /// 비밀상점 초기화시 에러 있는지 여부 체크
        /// 에러 있으면 다시 비밀상점UI 들어올때까지 자동 목록변경 불가
        /// </summary>
        private bool hasSecetShopError;

        public void ShowShopDefault()
        {
            view.ShowShopDefault();
        }

        public void ShowShopSecret()
        {
            if (!questModel.IsOpenContent(ContentType.SecretShop))
            {
                return;
            }
            hasSecetShopError = false;
            view.ShowShopSecret();
        }

        /// <summary>
        /// 비밀상점 정보 반환
        /// </summary>
        /// <returns></returns>
        public SecretShopInfo[] GetSecretShopInfos()
        {
            return shopModel.GetSecretShops();
        }

        /// <summary>
        /// 비밀상점 무료 변경까지 남은시간
        /// </summary>
        /// <returns></returns>
        public float GetRemainFreeChangeSecretShopTime()
        {
            return shopModel.GetRemainFreeChangeSecretShopTime();
        }

        public bool IsSecretShopFree()
        {
            return shopModel.IsSecretShopFree();
        }

        /// <summary>
        /// 비밀상점 목록 자동변경
        /// </summary>
        public async void SecretShopInit()
        {
            bool isFree = GetRemainFreeChangeSecretShopTime() <= 0;
            if (!isFree || hasSecetShopError)
                return;

            hasSecetShopError = !await shopModel.RequestSecretShopInit(useResetItem: false);
            if (!hasSecetShopError)
            {
                view.Refresh();
                PlaySlotAnim?.Invoke();
            }
        }

        /// <summary>
        /// 비밀상점 목록 리셋권으로 변경
        /// </summary>
        public void SecetShopInitUseResetItem()
        {
            shopModel.RequestSecretShopInit(useResetItem: true).WrapNetworkErrors();
        }

        /// <summary>
        /// 비밀 상점 목록 변경 이벤트
        /// </summary>
        private void OnUpdateSecretShop()
        {
            view.Refresh();
            PlaySlotAnim?.Invoke();
        }

        /// <summary>
        /// 비밀상점 리셋권 구매 팝업 띄우기
        /// </summary>
        public void ShowPurchaseSecretShopResetItem()
        {
            shopModel.RequestNormalShopPurchase(BasisShop.SecretShopReset.GetID()).WrapNetworkErrors();
        }

        /// <summary>
        /// 리셋권 개수 반환
        /// </summary>
        public int GetSecretShopResetItemCount()
        {
            return invenModel.GetItemCount(BasisItem.SecretShopReset.GetID());
        }

        /// <summary>
        /// 리셋권 스프라이트 이름 반환
        /// </summary>
        public string GetSecretShopResetItemIconName()
        {
            return ItemDataManager.Instance.Get(BasisItem.SecretShopReset.GetID()).icon_name;
        }

        /// <summary>
        /// 비밀상점 구매
        /// </summary>
        public async Task RequestSecretShopPurchase(SecretShopInfo info)
        {
            await shopModel.RequestSecretShopPurchase(info);
            view.Refresh();
        }

        /// <summary>
        /// 상점 목록
        /// </summary>
        public ShopInfo[] GetShopInfos(int tab)
        {
            return shopModel.GetShopInfos(tab, characterModel.Job);
        }

        public bool HasPackageNotice(PackageType packageType)
        {
            return shopModel.HasPackageNotice(packageType);
        }

        /// <summary>
        /// 상점 무료 물품 보유 여부
        /// </summary>
        public bool GetHasNotice(int tab)
        {
            return shopModel.HasFreeShopItem(tab);
        }

        /// <summary>
        /// 수령 가능한 마일리지 보상 존재 여부
        /// </summary>
        public bool GetHasMileageNotice()
        {
            return shopModel.HasMileageReward();
        }

        /// <summary>
        /// 수령 가능한 패키지 상점 보상 존재
        /// </summary>
        public bool GetHasPackageNotice(PackageType packageType)
        {
            return shopModel.HasPackageNotice(packageType);
        }

        public async Task<bool> RequestItemShopLimitList()
        {
            return await shopModel.RequestShopInfoList();
        }

        /// <summary>
        /// 상점 구매
        /// </summary>
        public void RequestShopPurchase(ShopInfo info)
        {
            // 기간제 아이템 시간 초과
            if (info.IsPeriedItem() && info.RemainTime.ToRemainTime() <= 0)
            {
                UI.ShowToastPopup(LocalizeKey._90219.ToText()); // 기간제 상품 판매시간이 종료되었습니다.
                view.Refresh();
                return;
            }

            if (!info.CanBuyLimit)
            {
                Debug.LogError("구매 제한 횟수 초과");
                return;
            }

            if (info.CoinType == CoinType.Cash)
            {
                shopModel.RequestCashShopPurchase(info.ID, isShowNotiPopup: true).WrapNetworkErrors();
                return;
            }

            if (info.CoinType == CoinType.Free)
            {
                shopId = info.ID;
                bool isBeginner = characterModel.JobGrade() < BasisType.AD_NEED_JOB_GRADE.GetInt();
                ironSourceManager.ShowRewardedVideo(IronSourceManager.PlacementNameType.ShopFreeRewardedVideo, HasPaymentHistory(), isBeginner, OnCompleteRewardVideo);
                return;
            }

            shopModel.RequestNormalShopPurchase(info.ID).WrapNetworkErrors();
        }

        /// <summary>
        /// 광고 완료 후 보상 받기
        /// </summary>
        private void OnCompleteRewardVideo()
        {
            shopModel.RequestNormalShopPurchase(shopId).WrapNetworkErrors();
        }

        public void SetLockSecretShop()
        {
            view.SetLockSecretShop(!questModel.IsOpenContent(ContentType.SecretShop, false));
        }

        /// <summary>
        /// 비밀상점 아이템 해금 여부
        /// </summary>
        /// <param name="openChapter"></param>
        /// <returns></returns>
        public bool IsOpenSecetShopItem(int openChapter)
        {
            StageData stageData = stageDataRepo.Get(dungeonModel.FinalStageId);
            if (stageData == null)
                return false;
            int lastChapter = stageData.chapter;
            return openChapter <= lastChapter;
        }

        /// <summary>
        /// 패키지 구매 팝업 보기
        /// </summary>
        /// <param name="info"></param>
        public void ShowPackagePopup(ShopInfo info)
        {
            // TODO 패키지 구매 팝업으로 변경
#if UNITY_EDITOR
            ShopData data = ShopDataManager.Instance.Get(info.ID);
            Debug.Log($"상점ID={info.ID}");
            Debug.Log(data.GetDump());
#endif

            switch (info.PackageType)
            {
                case PackageType.GeneralPackage:
                    UI.Show<UIPackageGeneral>().Set(info.ID);
                    break;

                case PackageType.SharePackage:
                    UI.Show<UIPackageShare>().Set(info.ID);
                    break;

                case PackageType.KafraPackage:
                    UI.Show<UIPackageKafra>().Set(info.ID);
                    break;

                case PackageType.CatCoinPackage:
                    UI.Show<UIPackageCatCoin>().Set(info.ID);
                    break;

                case PackageType.LevelAchievePackage:
                case PackageType.ScenarioPackage:
                    UI.Show<UIPackageAchieve>().Set(info.ID);
                    break;

                case PackageType.TreePackage:
                    UI.Show<UIPackageTree>().Set(info.ID);
                    break;

                case PackageType.OnBuffPassPackage:
                    UI.Show<UIPackageOnBuffPass>();
                    break;
            }
        }

        /// <summary>
        /// 특수 패키지 상품 구매 가능 여부
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool CanBuyLimit(ShopInfo info)
        {
            // 우편함에 있는 상품
            if (shopModel.HasMailShopItem(info.ID))
                return false;

            switch (info.PackageType)
            {
                case PackageType.SharePackage:
                    return !shopModel.IsShareBuff;

                case PackageType.KafraPackage:
                case PackageType.CatCoinPackage:
                    return shopModel.GetEveryDayPackageState(info.ID) == EveryDayPackageState.AvailablePurchase;

                case PackageType.LevelAchievePackage:
                    return shopModel.PayJobLevelStep == -1;

                case PackageType.ScenarioPackage:
                    return shopModel.PayChapterStep == -1;

                case PackageType.TreePackage:
                    return shopModel.TreePackRemainTime.ToRemainTime() <= 0;

                case PackageType.BattlePassPackage:
                    return !shopModel.IsActivePass(PassType.Labyrinth) && IsBattlePass(PassType.Labyrinth);

                case PackageType.OnBuffPassPackage:
                    return !shopModel.IsActivePass(PassType.OnBuff) && IsBattlePass(PassType.OnBuff);
            }
            return false;
        }

        public bool HasPaymentHistory()
        {
            return shopModel.HasPaymentHistory();
        }

        public int GetJobLevel()
        {
            return characterModel.JobLevel;
        }

        public bool IsBattlePass(PassType passType)
        {
            return shopModel.IsBattlePass(passType);
        }

        public bool IsDisplayPass(PassType passType)
        {
            return shopModel.IsDisplayPass(passType);
        }

        /// <summary>
        /// 패스 알림 체크
        /// </summary>
        public bool IsPassNotice(PassType passType)
        {
            // 시즌 패스 진행중일때만 체크
            if (IsBattlePass(passType))
            {
                // 수령 가능한 패스 보상 존재
                if (shopModel.IsPassRewardNotice(passType))
                    return true;

                // 수령 가능한 패스 퀘스트 보상 존재
                if (questModel.IsPassQuestStandByReward(passType))
                    return true;
            }

            return false;
        }
    }
}