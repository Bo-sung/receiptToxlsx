using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIPass"/>
    /// </summary>
    public class PassPresenter : ViewPresenter
    {
        private readonly PassType passType;

        // <!-- Models --!>
        private readonly QuestModel questModel;
        private readonly ShopModel shopModel;
        private readonly GoodsModel goodsModel;
        private readonly UserModel userModel;

        // <!-- Data --!>
        public readonly int onBuffMvpMaxPoint;
        private BetterList<PassRewardElement> passRewardElements;

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수 변경시 호출
        /// </summary>
        public event System.Action OnStandByReward
        {
            add { questModel.OnStandByReward += value; }
            remove { questModel.OnStandByReward -= value; }
        }

        public event System.Action OnUpdatePass;

        public event System.Action OnUpdateBuyPassExp;

        public event System.Action OnUpdateOnBuffPoint
        {
            add => goodsModel.OnUpdateOnBuffPoint += value;
            remove => goodsModel.OnUpdateOnBuffPoint -= value;
        }

        public event System.Action OnUpdateOnBuffMvpPoint
        {
            add => shopModel.OnUpdateOnBuffMvpPoint += value;
            remove => shopModel.OnUpdateOnBuffMvpPoint -= value;
        }

        public event System.Action OnUpdateReceivedOnBuffPoint
        {
            add => shopModel.OnUpdateReceivedOnBuffPoint += value;
            remove => shopModel.OnUpdateReceivedOnBuffPoint -= value;
        }

        public PassPresenter(PassType passType)
        {
            this.passType = passType;
            questModel = Entity.player.Quest;
            shopModel = Entity.player.ShopModel;
            goodsModel = Entity.player.Goods;
            userModel = Entity.player.User;
            onBuffMvpMaxPoint = BasisOnBuffInfo.DailyMvpKillCount.GetInt();
        }

        public override void AddEvent()
        {
            shopModel.AddUpdatePassExpEvent(passType, InvokeUpdatePass);
            shopModel.AddUpdateBuyPassExpEvent(passType, InvokeUpdateBuyPassExp);
            shopModel.AddUpdatePassRewardEvent(passType, InvokeUpdatePass);
            goodsModel.OnUpdateOnBuffMyPoint += OnUpdateOnBuffMyPoint;
        }

        public override void RemoveEvent()
        {
            shopModel.RemoveUpdatePassExpEvent(passType, InvokeUpdatePass);
            shopModel.RemoveUpdateBuyPassExpEvent(passType, InvokeUpdateBuyPassExp);
            shopModel.RemoveUpdatePassRewardEvent(passType, InvokeUpdatePass);
            goodsModel.OnUpdateOnBuffMyPoint -= OnUpdateOnBuffMyPoint;

            if (passRewardElements != null)
                passRewardElements.Clear();
        }

        private void InvokeUpdatePass()
        {
            UpdatePassRewardElements();
            OnUpdatePass?.Invoke();
        }

        private void InvokeUpdateBuyPassExp()
        {
            UpdatePassRewardElements();
            OnUpdateBuyPassExp?.Invoke();
        }

        /// <summary>
        /// 패스 현재 시즌
        /// </summary>
        public int GetSeason()
        {
            return shopModel.GetPassSeason(passType);
        }

        /// <summary>
        /// 패스 활성 여부
        /// </summary>
        public bool IsActivePass()
        {
            return shopModel.IsActivePass(passType);
        }

        /// <summary>
        /// 패스 시즌 남은 시간
        /// </summary>
        public RemainTime GetSeasonRemainTime()
        {
            return shopModel.GetSeasonRemainTime(passType);
        }

        /// <summary>
        /// 패스 경험치 정보
        /// </summary>
        public IPassLevel GetExpInfo()
        {
            return shopModel.GetPassLevel(passType);
        }

        public int GetLastPassExp()
        {
            return shopModel.GetLastPassExp(passType);
        }

        /// <summary>
        /// 패스 일일 퀘스트 목록
        /// </summary>
        public UIPassQuestElement.IInput[] GetPassDailyQuests()
        {
            return questModel.GetPassDailyQuests(passType);
        }

        /// <summary>
        /// 패스 시즌 퀘스트 목록
        /// </summary>
        public UIPassQuestElement.IInput[] GetPassSeasonQuests()
        {
            return questModel.GetPassSeasonQuests(passType);
        }

        /// <summary>
        /// 패스 경험치 구매
        /// </summary>
        public async void RequestBuyPassExp()
        {
            IPassLevel output = GetExpInfo();
            int needCoin = BasisType.PASS_LEVEL_UP_CAT_COIN.GetInt();
            string title = LocalizeKey._39811.ToText(); // 패스 레벨 구매
            string message = LocalizeKey._39812.ToText() // 패스 경험치 {COUNT}을 획득하시겠습니까?
                .Replace(ReplaceKey.COUNT, output.MaxExp - output.CurExp);
            if (!await UI.CostPopup(CoinType.CatCoin, needCoin, title, message))
                return;

            shopModel.RequestBuyPassExp().WrapNetworkErrors();
        }

        /// <summary>
        /// 패스 보상 받기 요청
        /// </summary>
        public void RequestPassReward((byte passFlag, int level) item)
        {
            shopModel.RequestPassReward(passType, item.passFlag, item.level).WrapNetworkErrors();
        }

        /// <summary>
        /// 패스 일일 퀘스트 보상 요청
        /// </summary>
        public void RequestPassDailyQuestReward(int questId)
        {
            foreach (var item in questModel.GetPassDailyQuests(passType))
            {
                if (item.ID == questId)
                {
                    questModel.RequestQuestRewardAsync(item).WrapNetworkErrors();
                    break;
                }
            }
        }

        /// <summary>
        /// 패스 시즌 퀘스트 보상 요청
        /// </summary>
        public void RequestPassSeasonQuestReward(int questId)
        {
            foreach (var item in questModel.GetPassSeasonQuests(passType))
            {
                if (item.ID == questId)
                {
                    questModel.RequestQuestRewardAsync(item).WrapNetworkErrors();
                    break;
                }
            }
        }

        public void BuyPass()
        {
            shopModel.ShowPackageUI(passType);
        }

        public int GetCurLevelIndex()
        {
            for (int i = 0; i < passRewardElements.size; i++)
            {
                if (passRewardElements[i].Level == passRewardElements[i].CurLevel)
                    return i;
            }
            return 0;
        }

        public int GetLastPassLevel()
        {
            return shopModel.GetLastPassLevel(passType);
        }

        /// <summary>
        /// 마지막 보상 아이콘 이름
        /// </summary>
        public string GetLastRewardIconName()
        {
            int lastPassLevel = GetLastPassLevel();
            IPassData data = shopModel.GetPassData(passType, lastPassLevel);
            foreach (var item in data.GetFreeRewards())
            {
                return item.IconName;
            }

            return string.Empty;
        }

        /// <summary>
        /// 온버프 MVP 처치 포인트
        /// </summary>
        public int GetOnBuffMvpPoint()
        {
            return shopModel.OnBuffMvpPoint;
        }

        /// <summary>
        /// 온버프 포인트 받을 수 있는 상태
        /// </summary>
        public bool CanRequestOnBuffPoint()
        {
            if (shopModel.IsReceivedOnBuffPoint)
                return false;

            return GetOnBuffMvpPoint() == onBuffMvpMaxPoint;
        }

        /// <summary>
        /// 보유한 온버프 포인트
        /// </summary>
        public int GetTotalOnBuffPoint()
        {
            return goodsModel.OnBuffPoint;
        }

        public bool IsRewardTabNotice()
        {
            if (!shopModel.IsBattlePass(passType))
                return false;

            return shopModel.IsPassRewardNotice(passType);
        }

        public bool IsQuestTabNotice()
        {
            if (!shopModel.IsBattlePass(passType))
                return false;

            return questModel.IsPassQuestStandByReward(passType);
        }

        private void UpdatePassRewardElements()
        {
            if (passRewardElements == null)
                return;

            IPassLevel expInfo = GetExpInfo();
            int level = expInfo.Level;
            int freeStep = shopModel.GetPassFreeStep(passType);
            int payStep = shopModel.GetPassPayStep(passType);
            bool isActivePass = IsActivePass();
            foreach (var item in passRewardElements)
            {
                item.SetCurLevel(level);
                item.SetFreeStep(freeStep);
                item.SetPayStep(payStep);
                item.SetIsActivePass(isActivePass);
            }
        }

        public RewardData GetOnBuffFreeReward()
        {
            int point = BasisOnBuffInfo.FreeOnBuffRewardPoints.GetInt();
            return GetOnBuffReward(point);
        }

        public RewardData GetOnBuffPremiumReward()
        {
            int point = BasisOnBuffInfo.PremiumOnBuffRewardPoints.GetInt();
            return GetOnBuffReward(point);
        }

        private RewardData GetOnBuffReward(int value)
        {
            return new RewardData(RewardType.OnBuffPointMail, value, 0);
        }

        public UIBasePassRewardElement.IInput[] GetPassRewards()
        {
            if (passRewardElements == null)
            {
                passRewardElements = new BetterList<PassRewardElement>();

                int lastPasslevel = GetLastPassLevel();
                IPassLevel expInfo = GetExpInfo();
                int level = expInfo.Level;
                int freeStep = shopModel.GetPassFreeStep(passType);
                int payStep = shopModel.GetPassPayStep(passType);
                bool isActivePass = IsActivePass();
                foreach (IPassData item in shopModel.GetPassDataEnumerable(passType))
                {
                    PassRewardElement temp = new PassRewardElement(item, lastPasslevel, CheckReceivePassFree, CheckReceivePassPay, CheckLastRewardCount);
                    temp.SetCurLevel(level);
                    temp.SetFreeStep(freeStep);
                    temp.SetPayStep(payStep);
                    temp.SetIsActivePass(isActivePass);
                    passRewardElements.Add(temp);
                }
            }
            return passRewardElements.ToArray();
        }

        public void TryGetOnBuffPoint()
        {
            shopModel.RequestOnBuffMvpPointGet().WrapNetworkErrors();
        }

        public void RequestOnBuffMyPoint()
        {
            goodsModel.RequestOnBuffMyPoint().WrapNetworkErrors();
        }

        public bool IsOnBuffAccountLink()
        {
            return userModel.IsOnBuffAccountLink();
        }

        /// <summary>
        /// 라비린스 패스 배너에 사용하는 텍스쳐 이름
        /// </summary>
        public string GetTextureName()
        {
            if (GameServerConfig.IsOnBuff())
                return Constants.UITexute.LABYRINTY_PASS_ONBUFF;

            return Constants.UITexute.LABYRINTY_PASS;
        }

        private void OnUpdateOnBuffMyPoint()
        {
            UI.ShowToastPopup(LocalizeKey._90331.ToText()); // OnBuff 포인트가 동기화되었습니다.
        }

        private bool CheckReceivePassFree(int level)
        {
            return shopModel.IsReceivePassFree(passType, level);
        }

        private bool CheckReceivePassPay(int level)
        {
            return shopModel.IsReceivePassPay(passType, level);
        }

        private int CheckLastRewardCount()
        {
            int lastPasslevel = GetLastPassLevel();
            IPassLevel expInfo = GetExpInfo();

            if (expInfo.Level != lastPasslevel)
                return 0;

            return (expInfo.CurExp / expInfo.MaxExp) + 1;
        }

        private class PassRewardElement : UIBasePassRewardElement.IInput
        {
            public int Level { get; }
            public int LastPassLevel { get; }
            public int CurLevel { get; private set; }
            public RewardData[] FreeRewards { get; }
            public RewardData[] PayRewards { get; }
            public int FreeStep { get; private set; }
            public int PayStep { get; private set; }
            public bool IsActivePass { get; private set; }

            public bool IsReceivePassFree => checkReceivePassFree(Level);
            public bool IsReceivePassPay => checkReceivePassPay(Level);
            public int LastRewardCount => checkLastRewardCount();

            private readonly System.Func<int, bool> checkReceivePassFree;
            private readonly System.Func<int, bool> checkReceivePassPay;
            private readonly System.Func<int> checkLastRewardCount;

            public PassRewardElement(IPassData data, int lastPasslevel, System.Func<int, bool> checkReceivePassFree, System.Func<int, bool> checkReceivePassPay, System.Func<int> checkLastRewardCount)
            {
                Level = data.PassLevel;
                LastPassLevel = lastPasslevel;
                FreeRewards = data.GetFreeRewards();
                PayRewards = data.GetPayRewards();
                this.checkReceivePassFree = checkReceivePassFree;
                this.checkReceivePassPay = checkReceivePassPay;
                this.checkLastRewardCount = checkLastRewardCount;
            }

            public void SetCurLevel(int level)
            {
                CurLevel = level;
            }

            public void SetFreeStep(int step)
            {
                FreeStep = step;
            }

            public void SetPayStep(int step)
            {
                PayStep = step;
            }

            public void SetIsActivePass(bool isActivePass)
            {
                IsActivePass = isActivePass;
            }
        }
    }
}