using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDungeon"/>
    /// </summary>
    public class DungeonPresenter : BaseDungeonPresenter, IDungeonDetailImpl, IEqualityComparer<DungeonType>
    {
        private readonly WorldBossDataManager worldBossDataRepo;
        private readonly DefenceDungeonDataManager defenceDungeonDataRepo;
        private readonly ClickerDungeonDataManager clickerDungeonDataRepo;
        private readonly CentralLabDataManager centralLabDataRepo;
        private readonly DungeonInfoDataManager dungeonInfoDataRepo;
        private readonly IronSourceManager ironSourceManager;
        private readonly EndlessTowerDataManager endlessTowerDataRepo;

        private readonly GoodsModel goodsModel;
        private readonly DailyModel dailyModel;
        private readonly CharacterModel characterModel;
        private readonly ShopModel shopModel;
        private readonly QuestModel questModel;
        private readonly InventoryModel inventoryModel;
        private readonly int dungoenTicketItemId;

        /******************** Pool ********************/
        private readonly Dictionary<DungeonType, DungeonDetailElement[]> dungeonDetailElementDic;

        DungeonElement[] dungeonElements;
        private DungeonType dungeonType;

        public event System.Action OnUpdateGoodsZeny;
        public event System.Action OnUpdateGoodsCatCoin;
        public event System.Action OnUpdateView;

        public DungeonPresenter()
        {
            worldBossDataRepo = WorldBossDataManager.Instance;
            defenceDungeonDataRepo = DefenceDungeonDataManager.Instance;
            clickerDungeonDataRepo = ClickerDungeonDataManager.Instance;
            centralLabDataRepo = CentralLabDataManager.Instance;
            dungeonInfoDataRepo = DungeonInfoDataManager.Instance;
            ironSourceManager = IronSourceManager.Instance;
            endlessTowerDataRepo = EndlessTowerDataManager.Instance;

            goodsModel = Entity.player.Goods;
            dailyModel = Entity.player.Daily;
            characterModel = Entity.player.Character;
            shopModel = Entity.player.ShopModel;
            questModel = Entity.player.Quest;
            inventoryModel = Entity.player.Inventory;

            dungoenTicketItemId = BasisItem.DungeonClearTicket.GetID();
            dungeonDetailElementDic = new Dictionary<DungeonType, DungeonDetailElement[]>(this);
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += InvokeUpdateGoodsZeny;
            goodsModel.OnUpdateCatCoin += InvokeUpdateGoodsCatCoin;
            dailyModel.OnResetDailyEvent += InvokeUpdateView;
            characterModel.OnUpdateJobLevel += OnUpdateJobLevel;
            dungeonModel.OnUpdateView += InvokeUpdateView;
            dungeonModel.OnFastClear += InvokeUpdateView;

            inventoryModel.AddItemEvent(dungoenTicketItemId, InvokeUpdateView);

            OnUpdateJobLevel(characterModel.JobLevel);
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= InvokeUpdateGoodsZeny;
            goodsModel.OnUpdateCatCoin -= InvokeUpdateGoodsCatCoin;
            dailyModel.OnResetDailyEvent -= InvokeUpdateView;
            characterModel.OnUpdateJobLevel -= OnUpdateJobLevel;
            dungeonModel.OnUpdateView -= InvokeUpdateView;
            dungeonModel.OnFastClear -= InvokeUpdateView;

            inventoryModel.RemoveItemEvent(dungoenTicketItemId, InvokeUpdateView);
        }

        public long GetZeny()
        {
            return goodsModel.Zeny;
        }

        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }

        void InvokeUpdateGoodsZeny(long value)
        {
            OnUpdateGoodsZeny?.Invoke();
        }

        void InvokeUpdateGoodsCatCoin(long value)
        {
            OnUpdateGoodsCatCoin?.Invoke();
        }

        void OnUpdateJobLevel(int jobLevel)
        {
            if (dungeonElements != null)
            {
                foreach (var item in dungeonElements)
                {
                    if (item.ConditionType == DungeonOpenConditionType.JobLevel)
                        item.InvokeUpdateEvent();
                }
            }

            foreach (var elements in dungeonDetailElementDic.Values)
            {
                foreach (var item in elements)
                {
                    if (item.ConditionType == DungeonOpenConditionType.JobLevel)
                        item.InvokeUpdateEvent();
                }
            }

            InvokeUpdateView();
        }

        private void InvokeUpdateView()
        {
            OnUpdateView?.Invoke();
        }

        public bool CanEnterDungeon(DungeonType dungeonType)
        {
            return dungeonModel.GetFreeEntryCount(dungeonType) > 0;
        }

        /// <summary>
        /// 던전 정보 반환
        /// </summary>
        public DungeonElement[] GetDungeonElements()
        {
            return dungeonElements ?? (dungeonElements = CreateDungeonElements());
        }

        /// <summary>
        /// 던전 상세 정보 반환
        /// </summary>
        public DungeonDetailElement[] GetDungeonDetailElements(DungeonType dungeonType)
        {
            if (!dungeonDetailElementDic.ContainsKey(dungeonType))
                dungeonDetailElementDic.Add(dungeonType, CreateDungeonDetailElements(dungeonType));

            return dungeonDetailElementDic[dungeonType];
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Dungeon()
        {
            questModel.RemoveNewOpenContent(ContentType.Dungeon); // 신규 컨텐츠 플래그 제거 (던전)
        }

        /// <summary>
        /// 던전 시작
        /// </summary>
        public async void RequestStartDungeon(DungeonType dungeonType, int id)
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            if (battleManager.Mode == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                return;
            }

            if (!await dungeonModel.IsEnterDungeon(dungeonType))
                return;

            battleManager.StartBattle(GetBattleMode(dungeonType), id);
        }

        /// <summary>
        /// 던전 소탕
        /// </summary>
        public void RequestFastClearDungeon(DungeonType dungeonType, int id, bool isFree)
        {
            dungeonModel.RequestFastClearDungeon(dungeonType, id, isFree).WrapNetworkErrors();
        }

        /// <summary>
        /// 중앙실험실 가능 여부
        /// </summary>
        public bool IsCheckCentralLab()
        {
            return characterModel.IsCheckJobGrade(Constants.OpenCondition.NEED_CENTRAL_LAB_JOB_GRADE, true);
        }

        /// <summary>
        /// 아이콘 이름 반환
        /// </summary>
        public string[] GetIcons(int dungeonInfoId)
        {
            List<string> ret = new List<string>();
            DungeonInfoData dungeonInfoData = dungeonInfoDataRepo.Get(dungeonInfoId);
            ret.Add(dungeonInfoData.icon_name_1);
            ret.Add(dungeonInfoData.icon_name_2);
            ret.Add(dungeonInfoData.icon_name_3);
            return ret.ToArray();
        }

        /// <summary>
        /// 던전 이름 반환
        /// </summary>
        public string[] GetTitles(int dungeonInfoId)
        {
            List<string> ret = new List<string>();
            DungeonInfoData dungeonInfoData = dungeonInfoDataRepo.Get(dungeonInfoId);
            if (dungeonInfoData.name_id_1 > 0)
                ret.Add(dungeonInfoData.name_id_1.ToText());
            if (dungeonInfoData.name_id_2 > 0)
                ret.Add(dungeonInfoData.name_id_2.ToText());
            if (dungeonInfoData.name_id_3 > 0)
                ret.Add(dungeonInfoData.name_id_3.ToText());
            return ret.ToArray();
        }

        /// <summary>
        /// 던전 설명 반환
        /// </summary>
        public string[] GetDescriptions(int dungeonInfoId)
        {
            List<string> ret = new List<string>();
            DungeonInfoData dungeonInfoData = dungeonInfoDataRepo.Get(dungeonInfoId);
            ret.Add(dungeonInfoData.des_id_1.ToText());
            ret.Add(dungeonInfoData.des_id_2.ToText());
            ret.Add(dungeonInfoData.des_id_3.ToText());
            return ret.ToArray();
        }

        public void RequestFreeReward(DungeonType dungeonType)
        {
            this.dungeonType = dungeonType;
            bool hasPaymentHistory = shopModel.HasPaymentHistory();
            bool isBeginner = characterModel.JobGrade() < BasisType.AD_NEED_JOB_GRADE.GetInt();
            ironSourceManager.ShowRewardedVideo(IronSourceManager.PlacementNameType.DungeonFreeRewardedVideo, hasPaymentHistory, isBeginner, OnCompleteRewardVideo);            
        }

        /// <summary>
        /// 광고 완료 후 보상 받기
        /// </summary>
        private void OnCompleteRewardVideo()
        {
            dungeonModel.RequestFreeReward((int)dungeonType).WrapNetworkErrors();
        }

        /// <summary>
        /// <see cref="DungeonModel.IsOpend(DungeonType)"/>
        /// 던전 정보 생성
        /// </summary>
        private DungeonElement[] CreateDungeonElements()
        {
            return new DungeonElement[]
            {
                new DungeonElement(worldBossDataRepo.GetList()[0], this),
                new DungeonElement(clickerDungeonDataRepo.GetByIndex(DungeonType.ZenyDungeon, 0), this),
                new DungeonElement(clickerDungeonDataRepo.GetByIndex(DungeonType.ExpDungeon, 0), this),
                new DungeonElement(defenceDungeonDataRepo.GetList()[0], this),
                new DungeonElement(centralLabDataRepo.GetByIndex(0), this),
                new DungeonElement(endlessTowerDataRepo.DungeonGroupInfo, this),
            };
        }

        /// <summary>
        /// 던전 상세 정보 생성
        /// </summary>
        private DungeonDetailElement[] CreateDungeonDetailElements(DungeonType dungeonType)
        {
            IDungeonGroup[] dungeonGroups;

            if (dungeonType == DungeonType.ZenyDungeon)
            {
                dungeonGroups = clickerDungeonDataRepo.GetArray(DungeonType.ZenyDungeon);
            }
            else if (dungeonType == DungeonType.ExpDungeon)
            {
                dungeonGroups = clickerDungeonDataRepo.GetArray(DungeonType.ExpDungeon);
            }
            else if (dungeonType == DungeonType.Defence)
            {
                dungeonGroups = defenceDungeonDataRepo.GetList().ToArray();
            }
            else
            {
                throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }

            DungeonDetailElement[] ouptut = new DungeonDetailElement[dungeonGroups.Length];
            for (int i = 0; i < ouptut.Length; i++)
            {
                ouptut[i] = new DungeonDetailElement(dungeonGroups[i], this);
            }

            return ouptut;
        }

        /// <summary>
        /// 전투 타입 반환
        /// </summary>
        private BattleMode GetBattleMode(DungeonType dungeonType)
        {
            if (dungeonType == DungeonType.ZenyDungeon || dungeonType == DungeonType.ExpDungeon)
                return BattleMode.ClickerDungeon;

            if (dungeonType == DungeonType.Defence)
                return BattleMode.Defence;

            throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
        }

        /// <summary>
        /// 던전 오픈 여부
        /// </summary>
        bool IDungeonDetailImpl.IsOpend(DungeonType dungeonType, int id, bool isShowPopup)
        {
            return dungeonModel.IsOpened(dungeonType, id, isShowPopup);
        }

        /// <summary>
        /// 던전 클리어 여부 (소탕을 하려면 클리어 여부를 알아야 한다)
        /// </summary>
        public bool IsCleared(DungeonType dungeonType, int id)
        {
            return dungeonModel.IsCleared(dungeonType, id);
        }

        /// <summary>
        /// 클리어한 던전 난이도
        /// </summary>
        int IDungeonDetailImpl.GetClearedDifficulty(DungeonType dungeonType)
        {
            return dungeonModel.GetClearedDifficulty(dungeonType);
        }

        /// <summary>
        /// 던전 입장 가능 여부
        /// </summary>
        bool IDungeonDetailImpl.CanEnter(DungeonType dungeonType, int id, bool isShowPopup)
        {
            return dungeonModel.CanEnter(dungeonType, id, isShowPopup);
        }

        /// <summary>
        /// 소탕권 보유 수
        /// </summary>
        int IDungeonDetailImpl.GetClearTicketCount(DungeonType dungeonType)
        {
            return inventoryModel.GetItemCount(dungoenTicketItemId);
        }      

        /// <summary>
        /// 몬스터 정보
        /// </summary>
        UIMonsterIcon.IInput[] IDungeonDetailImpl.GetMonsterInfos(DungeonType dungeonType, (int monsterId, MonsterType type, int monsterLevel)[] monsterInfos)
        {
            if (monsterInfos is null)
                return null;

            UIMonsterIcon.IInput[] result = null;
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                case DungeonType.ExpDungeon:
                    result = new ClickerMonsterInfo[monsterInfos.Length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = new ClickerMonsterInfo(dungeonType);
                    }
                    break;

                case DungeonType.Defence:
                    result = new MonsterInfo[monsterInfos.Length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        MonsterData data = monsterDataRepo.Get(monsterInfos[i].monsterId);
                        if (data == null)
                            continue;

                        MonsterInfo info = new MonsterInfo(monsterInfos[i].type == MonsterType.Boss);
                        info.SetData(data);
                        info.SetLevel(monsterInfos[i].monsterLevel);
                        result[i] = info;
                    }
                    break;
            }

            return result;
        }

        bool IEqualityComparer<DungeonType>.Equals(DungeonType x, DungeonType y)
        {
            return x == y;
        }

        int IEqualityComparer<DungeonType>.GetHashCode(DungeonType obj)
        {
            return obj.GetHashCode();
        }
    }
}