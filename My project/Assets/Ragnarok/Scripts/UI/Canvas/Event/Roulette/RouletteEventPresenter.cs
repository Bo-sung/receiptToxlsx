using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIRouletteEvent"/>
    /// </summary>
    public class RouletteEventPresenter : ViewPresenter
    {
        public interface IView
        {
            void PlayRoulette(int rewardIndex);
            void OnUpdateCost();
        }

        const int GACHA_REWARDS_LENGTH = 10;

        /******************** Models ********************/
        private readonly EventModel eventModel;
        private readonly InventoryModel invenModel;

        /******************** Repositories ********************/
        private readonly GachaDataManager gachaRepo;

        /******************** Models ********************/
        private readonly UIManager uiManager;

        /******************** Events ********************/
        public event System.Action OnCloseReward;

        private readonly IView view;
        private EventModel.EventRoulette roulette;

        private GachaData[] gachaDatas;
        private RewardData[] convertedRewardDatas; // 아이템 형식으로 나타내기 위해 GachaData -> RewardData로 변환
        private ItemData costItemData; // 사용되는 재화 정보

        public GachaData[] GachaDatas => gachaDatas;
        public RewardData[] ConvertedRewardDatas => convertedRewardDatas;
        public ItemData CostItem => costItemData;

        public bool IsInvalid => (roulette == null || gachaDatas.Length != GACHA_REWARDS_LENGTH || costItemData == null || !roulette.IsRemainTime());

        private RewardData reward; // 룰렛 뽑기 보상

        private readonly int needNormalRouletteCoinCount;
        private readonly int needRareRouletteCoinCount;
        private readonly int eventCoinItemId;
        public readonly int eventCoinMaxCount;

        public RouletteEventPresenter(IView view)
        {
            this.view = view;

            eventModel = Entity.player.Event;
            invenModel = Entity.player.Inventory;
            gachaRepo = GachaDataManager.Instance;
            uiManager = UIManager.Instance;
            needNormalRouletteCoinCount = BasisType.NORMAL_ROULETTE_PIECE.GetInt();
            needRareRouletteCoinCount = BasisType.RARE_ROULETTE_PIECE.GetInt();
            eventCoinItemId = BasisItem.EventCoin.GetID();
            eventCoinMaxCount = BasisType.EVENT_COIN_MAX_COUNT.GetInt();

            UpdateRouletteData();
        }

        public override void AddEvent()
        {
            invenModel.OnUpdateItem += OnUpdateItem;
            uiManager.OnUIClose += OnCloseUi;
        }

        public override void RemoveEvent()
        {
            invenModel.OnUpdateItem -= OnUpdateItem;
            uiManager.OnUIClose -= OnCloseUi;
        }

        /// <summary>
        /// 룰렛 데이더 갱신
        /// </summary>
        public void UpdateRouletteData()
        {
            roulette = eventModel.Roulette;

            if (roulette == null)
            {
                UI.ConfirmPopup("Roulette Event Data Error.");
                return;
            }

            if (roulette.IsRareRoulette)
            {
                gachaDatas = gachaRepo.Gets(GroupType.RareRoulette, roulette.RareGachaGroupId);
            }
            else
            {
                gachaDatas = gachaRepo.Gets(GroupType.NormalRoulette, roulette.NormalGachaGroupId);
            }

            if (gachaDatas.Length != GACHA_REWARDS_LENGTH)
            {
                UI.ConfirmPopup("GachaTable Error.");
                return;
            }

            convertedRewardDatas = System.Array.ConvertAll(gachaDatas, e => e.GetRewardData());
            costItemData = roulette.GetCostItem();

            if (costItemData == null)
            {
                UI.ConfirmPopup("Roulette Event Cost Item is NULL.");
                return;
            }
        }

        public bool isSendRquest = false;

        /// <summary>
        /// 룰렛 실행
        /// </summary>
        public async void RequestPlayRoulette()
        {
            isSendRquest = true;
            int gachaId = await RequestPlayRouletteAsync();
            if (gachaId == default)
                return;

            // 룰렛 돌리기 
            int rewardIndex = -1;
            for (int i = 0; i < gachaDatas.Length; i++)
            {
                if (gachaDatas[i].id == gachaId)
                {
                    rewardIndex = i;
                    break;
                }
            }

            if (rewardIndex == -1)
                return;

            reward = GetRewardData(rewardIndex);
            view.PlayRoulette(rewardIndex);
            UpdateRouletteData(); // 룰렛데이터 갱신

            isSendRquest = false;
        }

        private RewardData GetRewardData(int rewardIndex)
        {
            return convertedRewardDatas[rewardIndex];
        }

        public void ShowRewardUI()
        {
            if (reward == null)
                return;

            var input = new UISingleReward.Input(UISingleReward.Mode.JUST_REWARD, reward, reward.IconName);
            UI.Show<UISingleReward>(input);
        }

        public void PlayRouletteSfx()
        {
            SoundManager.Instance.PlaySfx("[SYSTEM] BOX_runout");
            SoundManager.Instance.PlayUISfx(Sfx.UI.RouletteLoop01); //RouletteLoop01
        }

        /// <summary>
        /// 보유 재화 개수 반환
        /// </summary>
        public int GetCostItemCount()
        {
            return Entity.player.Inventory.GetItemCount(costItemData.id);
        }

        /// <summary>
        /// 룰렛 뽑기 필요 개수
        /// </summary>
        public int GetNeedCount()
        {
            if (roulette.IsRareRoulette)
                return needRareRouletteCoinCount;

            return needNormalRouletteCoinCount;
        }

        private async Task<int> RequestPlayRouletteAsync()
        {
            Response response = await eventModel.RequestPlayRouletteAsync();
            if (!response.isSuccess)
                return default;

            int gachaId = response.GetInt("1");

            return gachaId;
        }

        public void ShowCostItemData()
        {
            var info = new PartsItemInfo();
            info.SetData(costItemData);
            UI.Show<UIPartsInfo>(info);
        }

        void OnUpdateItem()
        {
            view.OnUpdateCost();
        }

        void OnCloseUi(ICanvas canvas)
        {
            if (canvas is UISingleReward)
            {
                OnCloseReward?.Invoke();
            }
        }

        public void ChangeRoulette()
        {
            roulette.ToggleRoulette();
            UpdateRouletteData(); // 룰렛데이터 갱신
        }

        public string GetTitle()
        {
            if (roulette.IsRareRoulette)
                return LocalizeKey._11104.ToText(); // 돌림판 타이틀2

            return LocalizeKey._11103.ToText(); // 돌림판 타이틀
        }

        /// <summary>
        /// 룰렛판 변경 버튼 노티 여부
        /// </summary>
        /// <returns></returns>
        public bool HasNoticeChangeButton()
        {
            if (roulette == null)
                return false;

            if (roulette.IsRareRoulette)
            {
                if (roulette.NormalCostItem == null)
                    return false;

                int normalCount = invenModel.GetItemCount(roulette.NormalCostItem.id);
                return needNormalRouletteCoinCount <= normalCount;
            }

            if (!roulette.IsActiveRareGacha)
                return false;

            if (roulette.RareCostItem == null)
                return false;

            int rareCount = invenModel.GetItemCount(roulette.RareCostItem.id);
            return needRareRouletteCoinCount <= rareCount;
        }

        public bool IsRareRoulette()
        {
            if (roulette == null)
                return false;

            return roulette.IsRareRoulette;
        }

        public bool IsActiveRareRoulette()
        {
            if (roulette == null)
                return false;

            return roulette.IsActiveRareGacha;
        }

        /// <summary>
        /// 이벤트 주화 최대치 체크여부
        /// </summary>
        public bool IsCheckMaxEventCoin()
        {
            if (roulette == null)
                return false;

            ItemData costItem = roulette.GetCostItem();
            if (costItem == null)
                return false;

            return costItem.id == eventCoinItemId;
        }
    }
}