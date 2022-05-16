using UnityEngine;

namespace Ragnarok
{
    public class SpecialRoulettePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly EventModel eventModel;
        private readonly InventoryModel invenModel;
        private readonly CharacterModel characterModel;
        private readonly GoodsModel goodsModel;

        // <!-- Repositories --!>
        private readonly GachaDataManager gachaRepo;
        private readonly ItemDataManager itemDataRepo;

        /// <summary>
        /// 룰렛 판 즉시 변경 냥다래 책정 단위
        /// </summary>
        private int changeTime;

        /// <summary>
        /// 룰렛 판 즉시 변경에 사용되는 냥다래
        /// </summary>
        private int basisCatCoin;

        /// <summary>
        /// 룰렛 뽑기 결과 가차 ID
        /// </summary>
        private int rewardGachId;

        /// <summary>
        /// 룰렛 뽑기 제한 레벨
        /// </summary>
        private int jobLevelLimit;

        /// <summary>
        /// 룰렛 뽑기 요청 중
        /// </summary>
        public bool IsSendRoulette { get; private set; }


        // <!-- Event --!>     
        /// <summary>
        /// 소모 아이템 수량 변경 이벤트
        /// </summary>
        public event InventoryModel.ItemUpdateEvent OnUpdateUseItem
        {
            add { invenModel.OnUpdateItem += value; }
            remove { invenModel.OnUpdateItem -= value; }
        }

        /// <summary>
        /// 룰렛판 변경 성공 이벤트
        /// </summary>
        public event System.Action OnRouletteChange
        {
            add { eventModel.OnSpecialRouletteChange += value; }
            remove { eventModel.OnSpecialRouletteChange -= value; }
        }

        /// <summary>
        /// 룰렛 뽑기 성공 이벤트
        /// </summary>
        public System.Action OnUpdateRoulette;

        /// <summary>
        /// 룰렛 뽑기 연출 시작
        /// </summary>
        public System.Action OnPlayRouletteEffect;

        public System.Action OnUpdateNotice;

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public SpecialRoulettePresenter()
        {
            eventModel = Entity.player.Event;
            invenModel = Entity.player.Inventory;
            characterModel = Entity.player.Character;
            goodsModel = Entity.player.Goods;
            gachaRepo = GachaDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            changeTime = BasisType.SPECIAL_ROULETTE_CHANGE_TIME.GetInt();
            basisCatCoin = BasisType.SPECIAL_ROULETTE_CHANGE_CAT_COIN.GetInt();
            jobLevelLimit = BasisType.SPECIAL_ROULETTE_JOB_LEVEL_LIMIT.GetInt();
        }

        public override void AddEvent()
        {
            eventModel.OnSpecialRoulette += OnSpecialRoulette;
            characterModel.OnUpdateJobLevel += InvokeOnUpdateNotice;
        }

        public override void RemoveEvent()
        {
            eventModel.OnSpecialRoulette -= OnSpecialRoulette;
            characterModel.OnUpdateJobLevel -= InvokeOnUpdateNotice;
        }

        public void Initialize()
        {
            IsSendRoulette = false;
        }

        /// <summary>
        /// 룰렛판 보상 목록
        /// </summary>
        /// <returns></returns>
        public UISpecialRouletteElement.IInput[] GetSpecialRouletteElementArray()
        {
            return eventModel.GetSpecialRouletteElementArray();
        }

        /// <summary>
        /// 소모 아이템 아이콘 이름
        /// </summary>
        /// <returns></returns>
        public string GetOwnedIconName()
        {
            ItemData itemData = itemDataRepo.Get(eventModel.SpecialRouletteItemId);

            if (itemData == null)
            {
                return string.Empty;
            }

            return itemData.icon_name;
        }

        /// <summary>
        /// 소모 재화 이름
        /// </summary>
        /// <returns></returns>
        public string GetOwnedItemName()
        {
            ItemData itemData = itemDataRepo.Get(eventModel.SpecialRouletteItemId);

            if (itemData == null)
            {
                return string.Empty;
            }

            return itemData.name_id.ToText();
        }

        /// <summary>
        /// 소모 아이템 보유 수량
        /// </summary>
        /// <returns></returns>
        public int GetOwnedCount()
        {
            return invenModel.GetItemCount(eventModel.SpecialRouletteItemId);
        }

        /// <summary>
        /// 소모 아이템 정보 보기
        /// </summary>
        public void ShowUseItemInfo()
        {
            ItemData itemData = itemDataRepo.Get(eventModel.SpecialRouletteItemId);
            var info = new PartsItemInfo();
            info.SetData(itemData);
            UI.ShowItemInfo(info);
        }

        /// <summary>
        /// 룰렛 주요 보상 정보 보기
        /// </summary>
        public void ShowRewerdList()
        {
            int titleKey = LocalizeKey._5900; // 주요 보상 리스트
            UI.Show<UIRewardList>().Set(titleKey, eventModel.GetSpecialRewardList());
        }

        /// <summary>
        /// 룰렛 뽑기시 필요한 아이템 수량
        /// </summary>
        /// <returns></returns>
        public int GetNeedCount()
        {
            return eventModel.GetSpecialRouletteNeedCount();
        }

        /// <summary>
        /// 룰렛 변경까지 남은 시간
        /// </summary>
        /// <returns></returns>
        public RemainTime GetChangeRemainTime()
        {
            return eventModel.SpecialRouletteChangeRemainTime;
        }

        /// <summary>
        /// 뽑기 요청
        /// </summary>
        public void RequestRoulette()
        {
            if(IsJobLevelLimit())
            {
                UI.ShowToastPopup(LocalizeKey._90282.ToText().Replace(ReplaceKey.LEVEL, jobLevelLimit)); // JOB Lv이 부족하여 불가능합니다.(JOB Lv {LEVEL} 필요)
                return;
            }

            if (!IsRemainTimeRoulette())
            {
                Debug.Log($"룰렛 남은 시간이 없음");
                UI.ShowToastPopup(LocalizeKey._90254.ToText()); // 이벤트 기간이 아닙니다.
                return;
            }

            if (!HasUseItem())
            {
                Debug.Log($"룰렛 소모아이템 수량 부족 보유수량={GetOwnedCount()}, 필요수량={GetNeedCount()}");
                string text = LocalizeKey._90000.ToText()
                    .Replace("{COIN}", GetOwnedItemName()); // {COIN}(이)가 부족합니다.
                UI.ShowToastPopup(text); //  {COIN}(이)가 부족합니다.
                return;
            }

            if (IsMaxUsed())
            {
                Debug.Log($"룰렛 최대 횟수까지 뽑았음 돌린횟수={eventModel.SpecialRouletteUsedCount}");
                return;
            }

            IsSendRoulette = true;
            eventModel.RequestSpecialRoulette().WrapNetworkErrors();
        }

        /// <summary>
        /// 룰렛판 변경 요청
        /// </summary>
        public async void RequestChangeRoulette()
        {
            if (IsJobLevelLimit())
            {
                UI.ShowToastPopup(LocalizeKey._90282.ToText().Replace(ReplaceKey.LEVEL, jobLevelLimit)); // JOB Lv이 부족하여 불가능합니다.(JOB Lv {LEVEL} 필요)
                return;
            }

            if (!IsRemainTimeRoulette())
            {
                Debug.Log($"룰렛 남은 시간이 없음");
                UI.ShowToastPopup(LocalizeKey._90254.ToText()); // 이벤트 기간이 아닙니다.
                return;
            }

            if (GetChangeRemainTime().ToRemainTime() > 0)
            {
                Debug.Log($"룰렛 변경가능 남은시간={GetChangeRemainTime().ToStringTime()}");

                string title = LocalizeKey._5.ToText(); // 알람
                string description = LocalizeKey._90253.ToText(); // 냥다래를 소모하여 룰렛판을 변경하시겠습니까?
                int needCoin = GetNeedChangeCatCoin();
                if (!await UI.CostPopup(CoinType.CatCoin, needCoin, title, description))
                    return;
            }

            UISpecialRouletteElement.IInput item = eventModel.GetSGradeItem();
            if (item == null)
                return;

            // S등급 아이템 미획득
            if (!item.IsComplete)
            {
                string text = LocalizeKey._90250.ToText()
                    .Replace(ReplaceKey.ITEM, item.Reward.ItemName); // 현재 주요 보상은 [62AEE4][C]{ITME}[/c][-]입니다.\n정말로 판을 변경하시겠습니까?
                UI.Show<UISelectRewardPopup>().Set(item.Reward, text, OnSelectRewardPopup);
                return;
            }

            eventModel.RequestSpecialRouletteChange().WrapNetworkErrors();
        }

        /// <summary>
        /// S등급 아이템 미획득 상태로 룰벤판 변경
        /// </summary>
        void OnSelectRewardPopup()
        {
            eventModel.RequestSpecialRouletteChange().WrapNetworkErrors();
        }

        /// <summary>
        /// 룰렛 뽑기 결과
        /// </summary>
        /// <param name="gachaId"></param>
        void OnSpecialRoulette(bool isSuccess, int gachaId)
        {
            IsSendRoulette = false;

            if (!isSuccess)
                return;

            rewardGachId = gachaId;
            OnPlayRouletteEffect?.Invoke();
        }

        /// <summary>
        /// 뽑기 결과 팝업
        /// </summary>
        public void ShowRewardPopup()
        {
            GachaData gachaData = gachaRepo.Get(rewardGachId);
            var input = new UISingleReward.Input(UISingleReward.Mode.JUST_REWARD, gachaData.Reward, gachaData.Reward.IconName);
            UI.Show<UISingleReward>(input);
            OnUpdateRoulette?.Invoke();
        }

        /// <summary>
        /// 룰렛 뽑기를 최대 횟수까지 사용했는지 여부
        /// </summary>
        public bool IsMaxUsed()
        {
            return eventModel.IsSpecialRouletteMaxUsed();
        }

        /// <summary>
        /// 룰렛 이벤트 남은 시간 있는지 여부
        /// </summary>
        public bool IsRemainTimeRoulette()
        {
            return eventModel.IsRemainTimeRoulette();
        }

        /// <summary>
        /// 소모 아이템 뽑기 사용가능까지 보유중인지 여부
        /// </summary>
        public bool HasUseItem()
        {
            return GetOwnedCount() >= GetNeedCount();
        }

        /// <summary>
        /// 룰렛 뽑기 버튼 알림 표시 정보
        /// </summary>
        public bool GetRouletteButtonNotice()
        {
            if (!IsRemainTimeRoulette())
                return false;

            if (IsMaxUsed())
                return false;

            return HasUseItem();
        }

        /// <summary>
        /// 룰렛판 변경시 필요한 냥다래
        /// </summary>
        public int GetNeedChangeCatCoin()
        {
            int time = (int)(eventModel.SpecialRouletteChangeRemainTime.ToRemainTime() / changeTime);
            return basisCatCoin + (basisCatCoin * time);
        }

        /// <summary>
        /// 직업 레벨 제한 여부
        /// </summary>
        /// <returns></returns>
        public bool IsJobLevelLimit()
        {
            return characterModel.JobLevel < jobLevelLimit;
        }

        private void InvokeOnUpdateNotice(int jobLevel)
        {
            OnUpdateNotice?.Invoke();
        }
    }
}