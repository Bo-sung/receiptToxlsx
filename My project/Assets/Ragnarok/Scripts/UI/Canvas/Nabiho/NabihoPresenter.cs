using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UINabiho"/>
    /// </summary>
    public sealed class NabihoPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly NabihoDataManager nabihoDataRepo;
        private readonly NabihoIntimacyDataManager nabihoIntimacyDataRepo;
        private readonly ItemDataManager itemDataRepo;
        private readonly int nabihoExpId;

        private readonly NabihoSelectBarInfo equipmentInfo;
        private readonly NabihoSelectBarInfo boxInfo;
        private readonly NabihoSelectBarInfo specialInfo;

        // <!-- Managers --!>
        private readonly IronSourceManager ironSourceManager;

        // <!-- Event --!>
        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public event System.Action OnRefresh;
        public event System.Action OnUpdateNabihoItem;

        private int selectedId;

        public NabihoPresenter()
        {
            goodsModel = Entity.player.Goods;
            inventoryModel = Entity.player.Inventory;
            nabihoDataRepo = NabihoDataManager.Instance;
            nabihoIntimacyDataRepo = NabihoIntimacyDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            nabihoExpId = BasisItem.NabihoExp.GetID();

            equipmentInfo = new NabihoSelectBarInfo(NabihoData.GROUP_EQUIPMENT);
            boxInfo = new NabihoSelectBarInfo(NabihoData.GROUP_BOX);
            specialInfo = new NabihoSelectBarInfo(NabihoData.GROUP_SPECIAL);

            ironSourceManager = IronSourceManager.Instance;

            RefreshNabihoInfo();
        }

        public override void AddEvent()
        {
            inventoryModel.OnUpdateNabiho += OnUpateNabiho;
            inventoryModel.AddItemEvent(nabihoExpId, OnNabihoItem);
        }

        public override void RemoveEvent()
        {
            inventoryModel.OnUpdateNabiho -= OnUpateNabiho;
            inventoryModel.RemoveItemEvent(nabihoExpId, OnNabihoItem);
        }

        void OnUpateNabiho()
        {
            RefreshNabihoInfo();
            OnRefresh?.Invoke();
        }

        void OnNabihoItem()
        {
            OnUpdateNabihoItem?.Invoke();
        }

        public int GetEquipmentNeedLevel()
        {
            return nabihoDataRepo.GetEquipmentNeedLevel();
        }

        public int GetBoxNeedLevel()
        {
            return nabihoDataRepo.GetBoxNeedLevel();
        }

        public int GetSpecialNeedLevel()
        {
            return nabihoDataRepo.GetSpecialNeedLevel();
        }

        public bool IsMaxLevel()
        {
            return GetCurrentLevel() == nabihoIntimacyDataRepo.GetMaxLevel();
        }

        public int GetCurrentLevel()
        {
            return nabihoIntimacyDataRepo.GetLevel(inventoryModel.NabihoExp);
        }

        public int GetCurrentExp()
        {
            // 최대 레벨일 경우에는 이전 데이터의 정보를 가져와서 보여줌
            if (IsMaxLevel())
            {
                int preLevel = GetCurrentLevel() - 1;
                NabihoIntimacyData preLevelData = nabihoIntimacyDataRepo.Get(preLevel);
                if (preLevelData == null)
                    return 0;

                return preLevelData.TotalNeedExp - preLevelData.PreTotalNeedExp;
            }

            NabihoIntimacyData data = GetCurrentLevelData();
            if (data == null)
                return 0;

            return inventoryModel.NabihoExp - data.PreTotalNeedExp;
        }

        public int GetMaxExp()
        {
            // 최대 레벨일 경우에는 cur 와 max 가 동일
            if (IsMaxLevel())
            {
                return GetCurrentExp();
            }

            NabihoIntimacyData data = GetCurrentLevelData();
            if (data == null)
                return 0;

            return data.TotalNeedExp - data.PreTotalNeedExp;
        }

        public int GetReduceMinutes()
        {
            NabihoIntimacyData data = GetCurrentLevelData();
            if (data == null)
                return 0;

            return data.ReduceMinute;
        }

        public UINabihoSelectBar.IInput GetEquipmentInfo()
        {
            return equipmentInfo;
        }

        public UINabihoSelectBar.IInput GetBoxInfo()
        {
            return boxInfo;
        }

        public UINabihoSelectBar.IInput GetSpecialInfo()
        {
            return specialInfo;
        }

        public RewardData GetMaterial()
        {
            int itemCount = inventoryModel.GetItemCount(nabihoExpId);
            return new RewardData(RewardType.Item, nabihoExpId, itemCount);
        }

        public int GetMaterialNameId()
        {
            ItemData data = itemDataRepo.Get(nabihoExpId);
            if (data == null)
                return 194239; // 도람 강아지풀

            return data.name_id;
        }

        public NabihoRewardElement.IInput[] GetSelectEquipmentInfos()
        {
            return nabihoDataRepo.GetEquipments();
        }

        public NabihoRewardElement.IInput[] GetSelectBoxInfos()
        {
            return nabihoDataRepo.GetBoxes();
        }

        public NabihoRewardElement.IInput[] GetSelectSpecialInfos()
        {
            return nabihoDataRepo.GetSpecials();
        }

        public void ShowAd(int id)
        {
            selectedId = id;
            ironSourceManager.ShowRewardedVideo(IronSourceManager.PlacementNameType.None, false, false, OnCompleteRewardVideo);
        }

        private void OnCompleteRewardVideo()
        {
            inventoryModel.RequestNabihoItemAdTimeReduction(selectedId).WrapNetworkErrors();
        }

        public void RequestCancel(int id)
        {
            inventoryModel.RequestNabihoItemSelectCancel(id).WrapNetworkErrors();
        }

        public void RequestReward(int id)
        {
            inventoryModel.RequestNabihoItemSelectGet(id).WrapNetworkErrors();
        }

        public void RequestSendPresent(int giftCount)
        {
            inventoryModel.RequestNabihoSendPresent(giftCount).WrapNetworkErrors();
        }

        public void RequestItemSelect(int id)
        {
            inventoryModel.RequestNabihoItemSelect(id).WrapNetworkErrors();
        }

        private NabihoIntimacyData GetCurrentLevelData()
        {
            int currentLevel = GetCurrentLevel();
            return nabihoIntimacyDataRepo.Get(currentLevel);
        }

        private void RefreshNabihoInfo()
        {
            equipmentInfo.ResetData();
            boxInfo.ResetData();
            specialInfo.ResetData();

            foreach (NabihoPacket item in inventoryModel.NabihoDic.Values)
            {
                NabihoData data = nabihoDataRepo.Get(item.NabihoId);
                switch (data.groupType)
                {
                    case NabihoData.GROUP_EQUIPMENT:
                        equipmentInfo.Initialize(item, data);
                        break;

                    case NabihoData.GROUP_BOX:
                        boxInfo.Initialize(item, data);
                        break;

                    case NabihoData.GROUP_SPECIAL:
                        specialInfo.Initialize(item, data);
                        break;
                }
            }
        }

        private class NabihoSelectBarInfo : UINabihoSelectBar.IInput
        {
            public int AdMaxCount { get; }

            public int Id { get; private set; }
            public RewardData Reward { get; private set; }
            public RemainTime RemainTime { get; private set; }
            public RemainTime AdCooldownTime { get; private set; }
            public int AdRemainCount { get; private set; }

            public NabihoSelectBarInfo(int group)
            {
                AdMaxCount = BasisType.REF_NABIHO_INFO.GetInt(group);
            }

            public void ResetData()
            {
                Id = 0;
                Reward = null;
                RemainTime = 0f;
                AdCooldownTime = 0f;
                AdRemainCount = 0;
            }

            public void Initialize(NabihoPacket packet, NabihoData data)
            {
                Id = packet.NabihoId;
                Reward = data == null ? null : data.Reward;
                RemainTime = packet.RemainTime;
                AdCooldownTime = packet.AdRemainTime;
                AdRemainCount = AdMaxCount - packet.AdCount;
            }
        }
    }
}