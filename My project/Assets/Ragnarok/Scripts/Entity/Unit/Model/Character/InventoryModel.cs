using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using Sfs2X.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public interface IInventoryModel
    {
        ItemInfo GetItemInfo(long no);
        int GetItemCount(int itemId);
        int GetJobLevel();
    }

    /// <summary>
    /// 인벤토리 정보
    /// </summary>
    public class InventoryModel : CharacterEntityModel, IInventoryModel, BattleItemInfo.IValue
    {
        public interface IInputItemValue
        {
            long No { get; }
            int ItemId { get; }
            int ItemCount { get; }
            int TierPer { get; }
            int ItemLevel { get; }
            byte ItemPos { get; }
            long EquippedCardNo1 { get; }
            long EquippedCardNo2 { get; }
            long EquippedCardNo3 { get; }
            long EquippedCardNo4 { get; }
            bool IsLock { get; }
            int ItemTranscend { get; }
            int ItemChangedElement { get; }
            int ElementLevel { get; }
            byte TradeFlag { get; }
        }

        public delegate void InvenWeightEvent();
        public delegate void ItemUpdateEvent();
        public delegate void EquipmentEvent();
        public delegate void UpdateStatusEvent();
        public delegate void CostumeEvent();

        private readonly ItemDataManager itemDataRepo;
        private readonly SoundManager soundManager;
        private readonly CostumeDataManager costumeDataRepo;
        private readonly BoxDataManager boxDataRepo;
        private readonly GachaDataManager gachaDataRepo;
        private readonly DarkTreeRewardDataManager darkTreeRewardDataRepo;
        private readonly NabihoDataManager nabihoDataRepo;
        private readonly NabihoIntimacyDataManager nabihoIntimacyDataRepo;

        private readonly EquipmentItemAttackPowerInfo equipmentItemAttackPowerInfo;

        /// <summary>
        /// 아이템 정보
        /// </summary>
        public readonly List<ItemInfo> itemList;

        /// <summary>
        /// 아이템 정보
        /// Key: no, Value: ItemInfo
        /// </summary>
        private readonly Dictionary<ObscuredLong, ItemInfo> itemDic;

        /// <summary>
        /// ItemId 아이템 정보변경 이벤트
        /// </summary>
        private Dictionary<int, Action> itemEvent;

        /// <summary>
        /// 아이템 버퍼
        /// </summary>
        private readonly Buffer<ItemInfo> itemBuffer;

        /// <summary>
        /// 어둠의 나무 정보
        /// </summary>
        private readonly DarkTreeInfo darkTreeInfo;

        private ObscuredInt maxInvenWeight;
        private ObscuredInt invenWeightBuyCount;
        private ObscuredInt currentInvenWeight;
        private ObscuredInt serverTotalItemAtk, serverTotalItemMatk, serverTotalItemDef, serverTotalItemMdef; // 멀티플레이어 전용

        public int InvenWeightBuyCount => invenWeightBuyCount;
        public int CurrentInvenWeight => currentInvenWeight;
        public int MaxInvenWeight => maxInvenWeight;

        /// <summary>
        /// [멀티플레이어 전용] 아이템으로 인한 물리공격력 총합
        /// </summary>
        public int ServerTotalItemAtk => serverTotalItemAtk;

        /// <summary>
        /// [멀티플레이어 전용] 아이템으로 인한 마법공격력 총합
        /// </summary>
        public int ServerTotalItemMatk => serverTotalItemMatk;

        /// <summary>
        /// [멀티플레이어 전용] 아이템으로 인한 물리방어력 총합
        /// </summary>
        public int ServerTotalItemDef => serverTotalItemDef;

        /// <summary>
        /// [멀티플레이어 전용] 아이템으로 인한 마법방어력 총합
        /// </summary>
        public int ServerTotalItemMdef => serverTotalItemMdef;

        /// <summary>
        /// 가방 무게 초과 여부
        /// </summary>
        public bool IsWeightOver => CurrentInvenWeight >= MaxInvenWeight;

        /// <summary>
        /// 어둠의 나무 정보
        /// </summary>
        public IDarkTreeInfo DarkTree => darkTreeInfo;

        /// <summary>
        /// 보유한 장비들의 전투력 테이블
        /// </summary>
        public EquipmentItemAttackPowerInfo EquipmentItemAttackPowerInfo => equipmentItemAttackPowerInfo;

        /// <summary>
        /// [나비호] 의뢰 정보 목록
        /// </summary>
        public Dictionary<int, NabihoPacket> NabihoDic { get; private set; }

        /// <summary>
        /// [나비호] 레벨 체크 용
        /// </summary>
        private readonly BetterList<int> nabihoNeedLevelCheckBuffer;

        /// <summary>
        /// [나비호] 친밀도 경험치
        /// </summary>
        public int NabihoExp { get; private set; }

        #region 이벤트

        /// <summary>
        /// 인벤 무게 변경 시 호출 (등록과 동시에 호출)
        /// </summary>
        public event InvenWeightEvent OnUpdateInvenWeight;

        /// <summary>
        /// 아이템 정보 변경되었을 경우
        /// </summary>
        public event ItemUpdateEvent OnUpdateItem;

        /// <summary>
        /// 장비 정보 변경되었을 경우 (장착, 해제, 교체 등)
        /// </summary>
        public event EquipmentEvent OnUpdateEquipment;

        /// <summary>
        /// 스탯이 변경되었을 경우 (장비 장착, 해제, 강화, 인챈트, 등급 변경 등)
        /// </summary>
        public event UpdateStatusEvent OnUpdateStatus;

        /// <summary>
        /// 코스튬 정보 변경되었을 경우 (장착, 해제, 교체)
        /// </summary>
        public event CostumeEvent OnUpdateCostume;

        /// <summary>
        /// 코스튬 변경 시 호출
        /// </summary>
        public event Action OnChangeCostume;

        /// <summary>
        /// 장비 아이템 획득 시 호출 (자동 장착을 위해)
        /// </summary>
        public event Action<EquipmentItemInfo> OnObtainEquipment;

        /// <summary>
        /// 장비 강화 후 이벤트 호출 (카드 슬롯 해금 여부) 
        /// 장비 강화 연출, 장비 카드 슬롯 해금 연출
        /// </summary>
        public event Action<bool, int> OnEquipItemLevelUp;

        /// <summary>
        /// 장비 속성 변경 이벤트
        /// </summary>
        public event Action<bool> OnChangeElement;

        /// <summary>
        /// 장비 초월 이벤트
        /// </summary>
        public event Action<bool> OnTierUp;

        /// <summary>
        /// 장비 분해 이벤트
        /// </summary>
        public event Action OnDisassemble;

        /// <summary>
        /// [나비호] 나비호 정보 갱신
        /// </summary>
        public event Action OnUpdateNabiho;

        #endregion

        public InventoryModel()
        {
            itemDataRepo = ItemDataManager.Instance;
            soundManager = SoundManager.Instance;
            costumeDataRepo = CostumeDataManager.Instance;
            boxDataRepo = BoxDataManager.Instance;
            gachaDataRepo = GachaDataManager.Instance;
            darkTreeRewardDataRepo = DarkTreeRewardDataManager.Instance;
            nabihoDataRepo = NabihoDataManager.Instance;
            nabihoIntimacyDataRepo = NabihoIntimacyDataManager.Instance;

            itemList = new List<ItemInfo>();
            itemDic = new Dictionary<ObscuredLong, ItemInfo>(ObscuredLongEqualityComparer.Default);
            itemEvent = new Dictionary<int, Action>();
            equipmentItemAttackPowerInfo = new EquipmentItemAttackPowerInfo(this);
            itemBuffer = new Buffer<ItemInfo>();
            darkTreeInfo = new DarkTreeInfo();
            NabihoDic = new Dictionary<int, NabihoPacket>(IntEqualityComparer.Default);
            nabihoNeedLevelCheckBuffer = new BetterList<int>();
        }

        public override void AddEvent(UnitEntityType type)
        {
            equipmentItemAttackPowerInfo.AddEvent();
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            equipmentItemAttackPowerInfo.RemoveEvent();
        }

        public override void ResetData()
        {
            base.ResetData();

            serverTotalItemAtk = 0;
            serverTotalItemMatk = 0;
            serverTotalItemDef = 0;
            serverTotalItemMdef = 0;

            itemList.Clear();
            itemDic.Clear();
            itemEvent.Clear();
            itemBuffer.Release();

            if (Entity.type == UnitEntityType.Player)
            {
                darkTreeInfo.ResetData();
                Timing.KillCoroutines(nameof(YieldCheckNabihoRemainTime));
            }

            NabihoDic.Clear();
            NabihoExp = 0;
        }

        /// <summary>
        /// 인벤 무게 정보 세팅
        /// </summary>
        internal void SetInvenWeight(int maxInvenWeight, int invenWeightBuyCount)
        {
            this.maxInvenWeight = maxInvenWeight;
            this.invenWeightBuyCount = invenWeightBuyCount;
            OnUpdateInvenWeight?.Invoke();
        }

        /// <summary>
        /// 상점 특수아이템 구매시 가방 무게 증가 
        /// </summary>
        internal void AddInvenWeight()
        {
            this.maxInvenWeight += BasisType.FIRST_PURCHASE_REWARD_INVEN_WEIGHT.GetInt();
            OnUpdateInvenWeight?.Invoke();
        }

        internal void Initialize(IInputItemValue[] arrData)
        {
            Initialize();

            foreach (var item in arrData.OrEmptyIfNull())
            {
                SetItem(item);
            }

            Reload();

            OnUpdateItem?.Invoke();
            UpdateInventoryWeight();
        }

        /// <summary>
        /// 멀티 플레이어 전용
        /// </summary>
        internal void Initialize(BattleItemInfo.IValue itemStatusValue, int weaponItemId, int armorItemId, ElementType weaponChangedElement, int weaponElementLevel, ElementType armorChangedElement, int armorElementLevel, ItemInfo.IEquippedItemValue[] equippedItems)
        {
            Initialize();

            long assignItemNo = 0; // 아이템에 임시 ItemNo 부여

            foreach (var item in equippedItems.OrEmptyIfNull())
            {
                int itemId = item.ItemId;
                if (itemId == 0)
                    continue;

                ItemData data = itemDataRepo.Get(itemId);
                if (data == null)
                {
                    Debug.LogError($"[아이템] 데이터가 존재하지 않습니다: {nameof(itemId)} = {itemId}");
                    continue;
                }

                ItemGroupType itemGroupType = data.ItemGroupType;

                if (itemGroupType == ItemGroupType.Equipment)
                {
                    // 카드 먼저 세팅 (장비에 카드가 장착되기 때문에)
                    long[] cardsNo = new long[Constants.Size.MAX_EQUIPPED_CARD_COUNT];
                    for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; i++)
                    {
                        int? cardId = item.GetEquippedCardId(i);
                        int? cardLevel = item.GetEquippedCardLevel(i);
                        if (!cardId.HasValue || !cardLevel.HasValue)
                        {
                            cardsNo[i] = 0;
                            continue;
                        }

                        ItemData cardData = itemDataRepo.Get(cardId.Value);

                        ItemInfo cardInfo = Create(cardData.ItemGroupType, cardData.event_id);
                        itemList.Add(cardInfo);
                        itemDic.Add(++assignItemNo, cardInfo);

                        cardInfo.SetItemNo(assignItemNo);
                        cardInfo.SetItemCount(itemCount: 1);
                        cardInfo.SetEquipped(true);
                        cardInfo.SetItemInfo(cardLevel.Value, 0, 0, 0L, 0L, 0L, 0L, isLock: false);
                        cardInfo.SetData(cardData); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)
                        cardsNo[i] = assignItemNo;
                    }

                    ItemInfo equipmentInfo = Create(itemGroupType, data.event_id);
                    itemList.Add(equipmentInfo);
                    itemDic.Add(++assignItemNo, equipmentInfo);

                    equipmentInfo.SetItemNo(assignItemNo);
                    equipmentInfo.SetItemCount(itemCount: 1);
                    equipmentInfo.SetItemInfo(tier: 0, itemLevel: item.ItemLevel, data.GetEquipmentSlotType().ToByteValue(), cardsNo[0], cardsNo[1], cardsNo[2], cardsNo[3], isLock: false, item.ItemTranscend, item.ItemChangedElement, item.ElementLevel);
                    equipmentInfo.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)
                }
                else if (itemGroupType == ItemGroupType.Costume)
                {
                    ItemInfo coustumeInfo = Create(itemGroupType, data.event_id);
                    itemList.Add(coustumeInfo);
                    itemDic.Add(++assignItemNo, coustumeInfo);

                    coustumeInfo.SetItemNo(assignItemNo);
                    coustumeInfo.SetItemCount(itemCount: 1);
                    coustumeInfo.SetItemInfo(tier: 0, itemLevel: 0, data.GetCostumeSlotType().ToByteValue(), 0, 0, 0, 0, isLock: false, 0, 0, 0);
                    coustumeInfo.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)
                }
            }

            if (weaponItemId != 0)
            {
                ItemData data = itemDataRepo.Get(weaponItemId);
                if (data == null)
                {
                    Debug.LogError($"[무기 세팅] 데이터가 존재하지 않습니다: {nameof(weaponItemId)} = {weaponItemId}");
                    return;
                }

                ItemInfo info = GetItemInfo(ItemEquipmentSlotType.Weapon);

                if (info == null)
                {
                    ItemGroupType type = data.ItemGroupType;
                    info = Create(type, data.event_id);
                    itemList.Add(info);
                    itemDic.Add(++assignItemNo, info);

                    info.SetItemNo(assignItemNo);
                    info.SetItemCount(itemCount: 1);
                    info.SetItemInfo(tier: 0, itemLevel: 0, (byte)ItemEquipmentSlotType.Weapon, 0L, 0L, 0L, 0L, isLock: false, 0, (int)weaponChangedElement, weaponElementLevel);
                    info.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)
                }
            }

            if (armorItemId != 0)
            {
                ItemData data = itemDataRepo.Get(armorItemId);
                if (data == null)
                {
                    Debug.LogError($"[방어구 세팅] 데이터가 존재하지 않습니다: {nameof(armorItemId)} = {armorItemId}");
                    return;
                }

                ItemInfo info = GetItemInfo(ItemEquipmentSlotType.Armor);

                if (info == null)
                {
                    ItemGroupType type = data.ItemGroupType;
                    info = Create(type, data.event_id);
                    itemList.Add(info);
                    itemDic.Add(++assignItemNo, info);

                    info.SetItemNo(assignItemNo);
                    info.SetItemCount(itemCount: 1);
                    info.SetItemInfo(tier: 0, itemLevel: 0, (byte)ItemEquipmentSlotType.Armor, 0L, 0L, 0L, 0L, isLock: false, 0, (int)armorChangedElement, armorElementLevel);
                    info.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)
                }
            }

            if (itemStatusValue != null)
            {
                serverTotalItemAtk = itemStatusValue.TotalItemAtk;
                serverTotalItemMatk = itemStatusValue.TotalItemMatk;
                serverTotalItemDef = itemStatusValue.TotalItemDef;
                serverTotalItemMdef = itemStatusValue.TotalItemMdef;
            }

            Reload();

            OnUpdateItem?.Invoke();
            UpdateInventoryWeight();
        }

        /// <summary>
        /// 클론 플레이어 전용
        /// </summary>
        internal void Initialize(BattleItemInfo.IValue itemStatusValue)
        {
            // 이 전에 Weapon 등을 세팅하므로 Initialize 를 하지 않는다.
            if (itemStatusValue == null)
            {
                serverTotalItemAtk = 0;
                serverTotalItemMatk = 0;
                serverTotalItemDef = 0;
                serverTotalItemMdef = 0;
            }
            else
            {
                serverTotalItemAtk = itemStatusValue.TotalItemAtk;
                serverTotalItemMatk = itemStatusValue.TotalItemMatk;
                serverTotalItemDef = itemStatusValue.TotalItemDef;
                serverTotalItemMdef = itemStatusValue.TotalItemMdef;
            }

            Reload();
        }

        /// <summary>
        /// 어둠의 나무
        /// </summary>
        internal void Initialize(DarkTreePacket packet)
        {
            darkTreeInfo.Initialize(packet.time, darkTreeRewardDataRepo.Get(packet.rewardId), packet.point);
        }

        /// <summary>
        /// 나비호 정보
        /// </summary>
        internal void Initialize(NabihoPacket[] packets)
        {
            NabihoDic.Clear();
            foreach (var item in packets)
            {
                NabihoDic.Add(item.NabihoId, item);

                Debug.Log($"[나비호] " +
                    $"ID={item.NabihoId}, " +
                    $"남은시간={item.RemainTime.ToRemainTime().ToStringTimeConatinsDay()}, " +
                    $"광고={item.AdCount}, " +
                    $"광고 남은시간={item.AdRemainTime.ToRemainTime().ToStringTimeConatinsDay()}");
            }

            CheckNabihoRemainTime();
        }

        internal void UpdateNabiho(NabihoPacket packet)
        {
            if (NabihoDic.ContainsKey(packet.NabihoId))
            {
                NabihoDic[packet.NabihoId] = packet;
            }
            else
            {
                NabihoDic.Add(packet.NabihoId, packet);
            }

            Debug.Log($"[나비호] " +
                    $"ID={packet.NabihoId}, " +
                    $"남은시간={packet.RemainTime.ToRemainTime().ToStringTimeConatinsDay()}, " +
                    $"광고={packet.AdCount}, " +
                    $"광고 남은시간={packet.AdRemainTime.ToRemainTime().ToStringTimeConatinsDay()}");

            CheckNabihoRemainTime();
        }

        internal void RemoveNabiho(int nabihoId)
        {
            if (NabihoDic.ContainsKey(nabihoId))
                NabihoDic.Remove(nabihoId);

            CheckNabihoRemainTime();
        }

        /// <summary>
        /// 나비호 친밀도 경험치
        /// </summary>
        internal void SetNabihoExp(int exp)
        {
            NabihoExp = exp;
            Debug.Log($"[나비호] 친밀도 경험치={exp}");
        }

        /// <summary>
        /// [나비호] 광고 횟수 초기화
        /// </summary>
        internal void ResetNabihoInfo()
        {
            foreach (var item in NabihoDic.Values)
            {
                item.ResetAdCount();
            }

            OnUpdateNabiho?.Invoke();
        }

        /// <summary>
        /// [나비호] 알림 표시 여부
        /// </summary>
        public bool IsNoticeNobiho()
        {
            nabihoNeedLevelCheckBuffer.Clear();
            nabihoNeedLevelCheckBuffer.Add(NabihoData.GROUP_EQUIPMENT);
            nabihoNeedLevelCheckBuffer.Add(NabihoData.GROUP_BOX);
            nabihoNeedLevelCheckBuffer.Add(NabihoData.GROUP_SPECIAL);

            foreach (var item in NabihoDic.Values)
            {
                // 완료 존재
                if (item.RemainTime.ToRemainTime() <= 0)
                    return true;

                // 시간 단축 존재
                NabihoData data = nabihoDataRepo.Get(item.NabihoId);
                if (data == null)
                    continue;

                int maxAdCount = BasisType.REF_NABIHO_INFO.GetInt(data.groupType);
                if (item.AdCount < maxAdCount && item.AdRemainTime.ToRemainTime() <= 0)
                    return true;

                // 그룹 체크 제거
                nabihoNeedLevelCheckBuffer.Remove(data.groupType);
            }

            // 추가 진행 가능 레벨 확인
            if (nabihoNeedLevelCheckBuffer.size > 0)
            {
                int level = nabihoIntimacyDataRepo.GetLevel(NabihoExp);
                for (int i = 0; i < nabihoNeedLevelCheckBuffer.size; i++)
                {
                    int checkLevel = 0;
                    switch (nabihoNeedLevelCheckBuffer[i])
                    {
                        case NabihoData.GROUP_EQUIPMENT:
                            checkLevel = nabihoDataRepo.GetEquipmentNeedLevel();
                            break;

                        case NabihoData.GROUP_BOX:
                            checkLevel = nabihoDataRepo.GetBoxNeedLevel();
                            break;

                        case NabihoData.GROUP_SPECIAL:
                            checkLevel = nabihoDataRepo.GetSpecialNeedLevel();
                            break;
                    }

                    if (checkLevel == -1)
                        continue;

                    if (level >= checkLevel)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 다른 플레이어 외형 정보만 변경 (멀티플레이어 전용)
        /// </summary>
        internal void UpdateCostumeWithWeapon(int weaponItemId, int[] equipCostumeIds)
        {
            Initialize();

            long assignItemNo = 0; // 아이템에 임시 ItemNo 부여

            if (weaponItemId != 0)
            {
                ItemData data = itemDataRepo.Get(weaponItemId);
                if (data == null)
                {
                    Debug.LogError($"[무기 세팅] 데이터가 존재하지 않습니다: {nameof(weaponItemId)} = {weaponItemId}");
                    return;
                }

                ItemGroupType type = data.ItemGroupType;
                ItemInfo info = Create(type, data.event_id);
                itemList.Add(info);
                itemDic.Add(++assignItemNo, info);

                info.SetItemNo(assignItemNo);
                info.SetItemCount(itemCount: 1);
                info.SetItemInfo(tier: 0, itemLevel: 0, (byte)ItemEquipmentSlotType.Weapon, 0L, 0L, 0L, 0L, isLock: false, 0, 0, 0);
                info.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)
            }

            foreach (var costumeItemId in equipCostumeIds.OrEmptyIfNull())
            {
                ItemData data = itemDataRepo.Get(costumeItemId);
                if (data == null)
                {
                    Debug.LogError($"[코스튬 세팅] 데이터가 존재하지 않습니다: {nameof(costumeItemId)} = {costumeItemId}");
                    return;
                }

                ItemGroupType type = data.ItemGroupType;
                ItemInfo info = Create(type, data.event_id);
                itemList.Add(info);
                itemDic.Add(++assignItemNo, info);

                info.SetItemNo(assignItemNo);
                info.SetItemCount(itemCount: 1);
                info.SetItemInfo(tier: 0, itemLevel: 0, (byte)data.GetCostumeSlotType(), 0L, 0L, 0L, 0L, isLock: false, 0, 0, 0);
                info.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)
            }

            OnUpdateItem?.Invoke();
        }

        internal void UpdateData(UpdateItemData[] items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                switch (item.dirtyType)
                {
                    case DirtyType.Insert:
                        AddItem(item.characterItemData);
                        break;

                    case DirtyType.Update:
                        UpdateItem(item.characterItemData);
                        break;

                    case DirtyType.Delete:
                        RemoveItem(item.characterItemData);
                        break;
                }
                if (itemEvent.ContainsKey(item.characterItemData.ItemId))
                {
                    itemEvent[item.characterItemData.ItemId]?.Invoke();
                }
            }

            Reload();

            OnUpdateItem?.Invoke();
            UpdateInventoryWeight();
        }

        public void ForceReload()
        {
            OnUpdateItem?.Invoke();
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        private void Initialize()
        {
            itemList.Clear();
            itemDic.Clear();
        }

        /// <summary>
        /// 장착한 장비 아이템 리스트
        /// </summary>
        public ItemInfo[] GetEquippedItems()
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].EquippedSlotType != ItemEquipmentSlotType.None)
                    itemBuffer.Add(itemList[i]);
            }

            return itemBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 현재 보유한 어둠의 나무 재료 아이템 목록
        /// </summary>
        public IEnumerable<ItemInfo> GetDarkTreeMaterials()
        {
            return itemList
                .FindAll(a => a is PartsItemInfo && a.DarkTreePoint > 0)
                .OrderByDescending(a => a.DarkTreePoint)
                .ThenBy(a => a.ItemId);
        }

        public void AddItemEvent(int itemId, Action action)
        {
            if (itemEvent.ContainsKey(itemId))
            {
                itemEvent[itemId] += action;
            }
            else
            {
                itemEvent.Add(itemId, action);
            }
        }

        public void RemoveItemEvent(int itemId, Action action)
        {
            if (itemEvent.ContainsKey(itemId))
            {
                itemEvent[itemId] -= action;
            }
        }

        /// <summary>
        /// 아이템 반환
        /// </summary>
        public ItemInfo GetItemInfo(long no)
        {
            if (no == 0L)
                return null;

            ObscuredLong key = no;
            return itemDic.ContainsKey(key) ? itemDic[key] : null;
        }

        /// <summary>
        /// 특정 슬롯에 장착한 아이템 정보
        /// </summary>
        public ItemInfo GetItemInfo(ItemEquipmentSlotType slotType)
        {
            return itemList.Find(a => a.EquippedSlotType == slotType);
        }

        public CostumeData GetCostumeData(int id)
        {
            return costumeDataRepo.Get(id);
        }

        public int GetItemCount(int itemId)
        {
            return itemDic.Values.Where(a => a.ItemId == itemId).Sum(a => a.ItemCount);
        }

        public int GetJobLevel()
        {
            return Entity.Character.JobLevel;
        }

        public ItemInfo FindOrCreateIfNull(int itemId)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].ItemId == itemId)
                    return itemList[i];
            }

            return CreateItmeInfo(itemId);
        }

        /// <summary>
        /// 남은 쿨타임 시간 설정
        /// </summary>
        [Obsolete]
        public void SetEndCooldown(int itemId, float endCooldown)
        {
            var info = itemList.Find(a => a.ItemId == itemId);
            if (info == null)
                return;

            info.SetRemainCoolDown(endCooldown);
        }

        /// <summary>
        /// 인벤 확장
        /// </summary>
        public async Task RequestInvenExpand()
        {
            if (MaxInvenWeight == BasisType.MAX_INVEN_CNT.GetInt())
            {
                string description = LocalizeKey._90047.ToText(); // 더이상 가방 무게를 늘릴 수 없습니다.
                UI.ConfirmPopup(description);
                return;
            }

            var needCoint = BasisType.PRICE_INVEN_CNT.GetInt() + (BasisType.INC_PRICE_INCEN_CNT.GetInt() * InvenWeightBuyCount);
            string title = LocalizeKey._90001.ToText(); // 가방 무게 확장
            string message = LocalizeKey._90002.ToText() // 가방 무게가 {WEIGHT} 증가합니다.
                .Replace("{WEIGHT}", (BasisType.BUY_INVEN_CNT.GetInt() * 0.1f).ToString("0.#"));
            if (!await UI.CostPopup(CoinType.CatCoin, needCoint, title, message))
                return;

            var response = await Protocol.INVEN_EXPAND.SendAsync();
            if (response.isSuccess)
            {
                var obj = response.GetSFSObject("1");

                int maxInvenWeight = obj.GetInt("1");
                int invenWeightBuyCount = obj.GetShort("2");
                SetInvenWeight(maxInvenWeight, invenWeightBuyCount);

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 아이템 장착,교체,해제
        /// </summary>
        public async Task RequestItemEquip(ItemInfo equipment)
        {
            string popupMessage = equipment.GetEquiqWarningMessage(isPopupMessage: true);
            if (!string.IsNullOrEmpty(popupMessage))
            {
                UI.ConfirmPopup(popupMessage);
                return;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", equipment.IsEquipped ? 0L : equipment.ItemNo); // 이미 장착중인 아이템 = 해제(0L)
            sfs.PutByte("2", (byte)equipment.SlotType);

            var response = await Protocol.ITEM_EQUIP.SendAsync(sfs);
            if (response.isSuccess)
            {
                if (!equipment.IsEquipped)
                    Quest.QuestProgress(QuestType.ITEM_EQUIP); // 장비 장착

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                OnUpdateEquipment?.Invoke();
                OnUpdateStatus?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// [자동 장착 기능] 여러 개의 아이템 장착 (이미 장착되어있으면 덮어씌움)
        /// </summary>
        public async Task RequestMultiItemEquip(List<ItemInfo> equipmentList)
        {
            var sfs = Protocol.NewInstance();

            IEnumerable<long> itemNoList = from item in equipmentList select item.ItemNo;
            sfs.PutLongArray("3", itemNoList.ToArray());

            var slotTypeList = from item in equipmentList select item.SlotType.ToByteValue();
            sfs.PutByteArray("4", new ByteArray(slotTypeList.ToArray()));

            var response = await Protocol.ITEM_EQUIP.SendAsync(sfs);
            if (!response.isSuccess)
                return;

            foreach (var equipment in equipmentList)
            {
                if (!equipment.IsEquipped)
                    Quest.QuestProgress(QuestType.ITEM_EQUIP); // 장비 장착
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            OnUpdateEquipment?.Invoke();

            OnUpdateStatus?.Invoke();

            UI.ShowToastPopup(LocalizeKey._90256.ToText()); // 자동 장착이 완료되었습니다.
        }

        /// <summary>
        /// 확성기(소모품) 사용
        /// </summary>
        public async Task<Response> RequestUseLoudSpeaker(ItemInfo itemInfo, string message)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", itemInfo.ItemId); // 아이템 테이블 ID
            sfs.PutUtfString("4", message); // 메세지

            var response = await Protocol.USE_CONSUMABLE_ITEM.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                UI.ShowToastPopup(LocalizeKey._21105.ToText()); // 확성기를 사용했습니다.
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        /// <summary>
        /// 소모품 사용
        /// </summary>
        public async Task<Response> RequestUseConsumableItem(ItemInfo itemInfo, long equipmentId = 0, int useBoxCount = 1)
        {
            // 박스 아이템은 개봉전 무게 체크
            if (itemInfo.IsWeightCheckForBoxOpen && !UI.CheckInvenWeight())
                return null;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", itemInfo.ItemId); // 아이템 테이블 ID
            if (equipmentId != 0)
                sfs.PutLong("2", equipmentId);
            if (useBoxCount > 1)
                sfs.PutInt("3", useBoxCount);

            var response = await Protocol.USE_CONSUMABLE_ITEM.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    if (equipmentId != 0) // 장비 아이템 등급 변경인 경우 전투력 변동 측정.
                    {
                        OnUpdateStatus?.Invoke();
                    }
                }

                if (response.ContainsKey("1"))
                {
                    bool isInvenOver = response.GetBool("1");
                    if (isInvenOver)
                    {
                        UI.ConfirmPopup(LocalizeKey._90025.ToText(), LocalizeKey._90024.ToText()); // 가방 무게가 초과하였습니다.
                    }
                }

                // 성별 변환
                if (itemInfo.ConsumableItemType == ConsumableItemType.TranssexualPotion)
                {
                    Entity.Character.ChangeGender(); // 젠더 변경

                    string message = LocalizeKey._90239.ToText(); // 성별이 변경되었습니다.
                    UI.ShowToastPopup(message);
                }

                // 소모품 이면서 버프 아이템의 경우
                if (itemInfo.ItemGroupType == ItemGroupType.ConsumableItem && itemInfo.IsBuff)
                {
                    Analytics.TrackEvent(TrackType.Buff);

                    foreach (BattleOption item in itemInfo)
                    {
                        if (item.battleOptionType == BattleOptionType.None)
                            continue;

                        string battleOptionDescription = item.GetDescription();
                        if (string.IsNullOrEmpty(battleOptionDescription))
                            continue;

                        UI.ShowToastPopup(battleOptionDescription);
                    }
                }

                // [듀얼 충천]
                if (itemInfo.ConsumableItemType == ConsumableItemType.DuelTicket)
                {
                    if (response.ContainsKey("2"))
                    {
                        NotifyDuelPoint(response.GetInt("2"));
                        UI.ShowToastPopup(LocalizeKey._90260.ToText()); // 듀얼 포인트가 충전되었습니다.
                    }
                }
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        /// <summary>
        /// 장비 분해 요청
        /// </summary>
        public async Task RequestItemDisassemble(long[] disassembleArray, int type, ItemGroupType itemType)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLongArray("1", disassembleArray);
            sfs.PutInt("2", type); // 1:일반 분해, 2:초월분해

            var response = await Protocol.ITEM_DISASSEMBLE.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                }

                // 퀘스트 처리
                if (itemType == ItemGroupType.Equipment)
                {
                    Quest.QuestProgress(QuestType.ITEM_BREAK, questValue: 1); // 장비템 분해 횟수
                }

                OnDisassemble?.Invoke(); // 분해 성공 이벤트
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 장비에 카드 장착
        /// </summary>
        public async Task RequestMultiCardEquip(long equipmentNo, CardEquipSender[] cards)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", equipmentNo);

            bool isEquip = false;
            var sfsArray = Protocol.NewArrayInstance();
            for (int i = 0; i < cards.Length; i++)
            {
                var sfsObject = Protocol.NewInstance();
                sfsObject.PutLong("1", cards[i].ItemNo);
                sfsObject.PutByte("2", cards[i].SlotIndex);
                sfsArray.AddSFSObject(sfsObject);

                // 1개 슬롯이라도 장착일때
                if (cards[i].ItemNo > 0)
                    isEquip = true;
            }

            sfs.PutSFSArray("2", sfsArray);

            var response = await Protocol.ENCHANT_PARTS_ITEM.SendAsync(sfs);

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

            foreach (var item in cards)
            {
                if (item.ItemNo == 0)
                    continue;

                ItemInfo card = GetItemInfo(item.ItemNo);
                // 새로 획득한 카드 인첸트시 New 체거
                if (card != null && card.IsNew)
                    card.SetNew(false);
            }

            // 퀘스트 체크
            if (isEquip)
                Quest.QuestProgress(QuestType.CARD_ENCHANT_COUNT); // 장비에 카드 인챈트 횟수

            OnUpdateStatus?.Invoke();

            Tutorial.Run(TutorialType.CardEnchant);
        }

        /// <summary>
        /// [카드 자동 장착 기능] 여러 개의 카드를 장착 (이미 장착되어있으면 덮어씌움)
        /// </summary>
        public async Task RequestAutoCardEquip(long equipItemNo)
        {
            ItemInfo equipment = GetItemInfo(equipItemNo);

            // 장착 가능한 카드리스트
            List<ItemInfo> cardList = itemList.FindAll(card => card is CardItemInfo && !card.IsEquipped && equipment.CanEquipCard(card.ClassType));

            int slotSize = 0;

            // 현재 장비에 장착중인 카드 추가
            for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; i++)
            {
                if (equipment.IsOpenCardSlot(i))
                {
                    slotSize++;
                }

                // 현재 장비에 장착중인 카드 리스트에 포함
                ItemInfo card = equipment.GetCardItem(i);
                if (card != null)
                    cardList.Add(card);
            }

            // 전투력 높은 순으로 슬롯 개수 만큼
            List<ItemInfo> sortedCardList = cardList
                .OrderByDescending(x => x.BattleScore) // 전투력 높은순
                .ThenByDescending(x => x.IsEquipped) // 장착중인 카드 순
                .Take(slotSize)
                .ToList();

#if UNITY_EDITOR
            foreach (var item in sortedCardList)
            {
                Debug.Log($"카드={item.Name},{item.ItemNo} {nameof(item.BattleScore)}={item.BattleScore}, {nameof(item.IsEquipped)}={item.IsEquipped}");
            }
#endif

            if (sortedCardList is null || sortedCardList.Count == 0)
            {
                Debug.LogError("장착에 필요한 카드가 음슴");
                return;
            }

            List<CardEquipSender> senderList = new List<CardEquipSender>();

            for (int i = 0; i < slotSize; i++)
            {
                ItemInfo card = equipment.GetCardItem(i);
                if (card != null)
                {
                    // 중복 아이템이 없는 경우
                    if (!sortedCardList.Remove(card))
                    {
                        ItemInfo item = sortedCardList.First();
                        if (item != null)
                        {
                            CardEquipSender sender = new CardEquipSender(item.ItemNo, (byte)(i + 1));
                            senderList.Add(sender);
                            sortedCardList.Remove(item);
                        }
                    }
                }
                else
                {
                    ItemInfo item = sortedCardList.First();
                    if (item != null)
                    {
                        CardEquipSender sender = new CardEquipSender(item.ItemNo, (byte)(i + 1));
                        senderList.Add(sender);
                        sortedCardList.Remove(item);
                    }
                }
            }

#if UNITY_EDITOR
            foreach (var item in senderList)
            {
                Debug.Log($"카드={item.ItemNo}, {nameof(item.SlotIndex)}={item.SlotIndex}");
            }
#endif

            await RequestMultiCardEquip(equipment.ItemNo, senderList.ToArray());
        }

        /// <summary>
        /// [카드 자동 장착 기능] 특정 슬롯에 카드를 자동 장착 (이미 장착되어있으면 덮어씌움)
        /// </summary>
        public async Task RequestAutoCardEquip(long equipItemNo, byte slotIndex)
        {
            ItemInfo equipment = GetItemInfo(equipItemNo);

            // 자동 장착을 위한 최소 전투력
            int minBattleScore = 0;
            ItemInfo equippedCard = equipment.GetCardItem(slotIndex - 1);
            if (equippedCard != null)
                minBattleScore = equippedCard.BattleScore;

            // 쉐도우 장비에 5성 쉐도우 카드 장착중인지 체크
            bool isEquipLimit = false;
            if (equipment.IsShadow)
            {
                for (int i = 0; i < equipment.GetMaxCardSlot(); i++)
                {
                    ItemInfo card = equipment.GetCardItem(i);
                    if (card != null)
                    {
                        if (card.Rating == 5)
                        {
                            isEquipLimit = true;
                            break;
                        }
                    }
                }
            }

            // 장착 가능한 카드리스트          
            List<ItemInfo> cardList = new List<ItemInfo>();
            foreach (var item in itemList)
            {
                if (item.ItemGroupType != ItemGroupType.Card)
                    continue;

                if (item.IsEquipped)
                    continue;

                if (item.IsShadow != equipment.IsShadow)
                    continue;

                if (!equipment.CanEquipCard(item.ClassType))
                    continue;

                if (item.BattleScore <= minBattleScore)
                    continue;

                if (isEquipLimit && item.Rating == 5)
                    continue;

                cardList.Add(item);
            }

            if (cardList.Count == 0)
            {
                if (equipment.IsShadow)
                {
                    UI.ShowToastPopup(LocalizeKey._90289.ToText()); // 장착 가능한 쉐도우 카드가 없습니다.
                    return;
                }
                UI.Show<UINoCard>();
                return;
            }

            ItemInfo sortedCard = cardList
                .OrderByDescending(x => x.BattleScore) // 전투력 높은순                
                .First();

            Debug.Log($"카드={sortedCard.Name},{sortedCard.ItemNo} {nameof(sortedCard.BattleScore)}={sortedCard.BattleScore}");

            List<CardEquipSender> senderList = new List<CardEquipSender>()
            {
                new CardEquipSender(sortedCard.ItemNo, slotIndex),
            };

#if UNITY_EDITOR
            foreach (var item in senderList)
            {
                Debug.Log($"카드={item.ItemNo}, {nameof(item.SlotIndex)}={item.SlotIndex}");
            }
#endif

            await RequestMultiCardEquip(equipment.ItemNo, senderList.ToArray());
        }

        /// <summary>
        /// 카드 제련
        /// </summary>
        public async Task<Response> RequestSmeltCard(ItemInfo info, ItemInfo material)
        {
            soundManager.PlaySfx(Constants.SFX.UI.CARD_LEVEL_UP);
            int preCardLevel = info.CardLevel;
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", info.ItemNo);
            sfs.PutLongArray("2", new long[] { material.ItemNo, 1 });
            var response = await Protocol.SMELT_CARD.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                if (info.CardLevel > preCardLevel)
                {
                    // 카드 레벨업
                    Quest.QuestProgress(QuestType.CARD_LEVEL_COUNT); // 카드 레벨업 횟수
                }

                Analytics.TrackEvent($"Card{info.CardLevel}");
                Quest.QuestProgress(QuestType.CARD_LEVEL, conditionValue: info.CardLevel); // 카드 특정 레벨 도달 횟수

                OnUpdateStatus?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        /// <summary>
        /// 장비 강화
        /// </summary>
        public async Task RequestEquipmentLevelUp(ItemInfo equipment)
        {
            soundManager.PlaySfx(Constants.SFX.UI.EQUIPMENT_LEVEL_UP);

            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", equipment.ItemNo);

            var response = await Protocol.REQUEST_EQUIP_ITEM_LEVELUP.SendAsync(sfs);
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

            ItemInfo item = GetItemInfo(equipment.ItemNo);
            int nextSmelt = item.Smelt;

            bool isUnLock = false;
            for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; i++)
            {
                if (nextSmelt == item.GetCardSlotOpenLevel(i))
                {
                    isUnLock = true;
                    break;
                }
            }

            Analytics.TrackEvent($"Equipment{nextSmelt}");

            // 퀘스트 처리
            Quest.QuestProgress(QuestType.ITEM_UPGRADE); // 장비템 제련 횟수
            Quest.QuestProgress(QuestType.ITEM_UPGRADE_MAX, questValue: nextSmelt); // 장비템 제련도 도달
            Quest.QuestProgress(QuestType.ITEM_LEVEL_UPGRADE, conditionValue: nextSmelt); // 장비템 제련도 도달 횟수

            switch (item.Rating)
            {
                case 1:
                    Quest.QuestProgress(QuestType.ITEM_RANK_1_LEVEL_COUNT, conditionValue: nextSmelt); // 1랭크 특정 장비 강화도 도달 횟수
                    break;

                case 2:
                    Quest.QuestProgress(QuestType.ITEM_RANK_2_LEVEL_COUNT, conditionValue: nextSmelt); // 2랭크 특정 장비 강화도 도달 횟수
                    break;

                case 3:
                    Quest.QuestProgress(QuestType.ITEM_RANK_3_LEVEL_COUNT, conditionValue: nextSmelt); // 3랭크 특정 장비 강화도 도달 횟수
                    break;

                case 4:
                    Quest.QuestProgress(QuestType.ITEM_RANK_4_LEVEL_COUNT, conditionValue: nextSmelt); // 4랭크 특정 장비 강화도 도달 횟수
                    break;

                case 5:
                    Quest.QuestProgress(QuestType.ITEM_RANK_5_LEVEL_COUNT, conditionValue: nextSmelt); // 5랭크 특정 장비 강화도 도달 횟수
                    break;
            }


            OnUpdateStatus?.Invoke();
            OnEquipItemLevelUp?.Invoke(isUnLock, nextSmelt);

            Tutorial.Run(TutorialType.CardEquip);
        }

        /// <summary>
        /// 무게 업데이트
        /// </summary>
        void UpdateInventoryWeight()
        {
            currentInvenWeight = GetTotalWeight();
            OnUpdateInvenWeight?.Invoke();
        }

        /// <summary>
        /// 여유 무게가 있는지 확인
        /// </summary>
        public bool HaveWeight(int addWeight)
        {
            if (currentInvenWeight + addWeight <= maxInvenWeight)
                return true;

            // 무게 부족
            return false;
        }

        /// <summary>
        /// 아이템 세팅
        /// </summary>
        private void SetItem(IInputItemValue packet)
        {
            ObscuredLong itemNo = packet.No;

            ItemData data = itemDataRepo.Get(packet.ItemId);
            if (data == null)
            {
                Debug.LogError($"[아이템 추가] 데이터가 존재하지 않습니다: {nameof(itemNo)} = {itemNo.ToString()}, {nameof(packet.ItemId)} = {packet.ItemId}");
                return;
            }

            ItemInfo info;

            ItemGroupType type = data.ItemGroupType;
            if (itemDic.ContainsKey(itemNo))
            {
                Debug.LogError($"[아이템 추가] 이미 존재하는 아이템입니다: {nameof(itemNo)} = {itemNo.ToString()}");
                return;
            }
            else
            {
                // 아이템 추가
                info = Create(type, data.event_id);
                itemList.Add(info);
                itemDic.Add(itemNo, info);
            }

            info.SetItemNo(packet.No);
            info.SetItemCount(packet.ItemCount);
            info.SetItemInfo(packet.TierPer, packet.ItemLevel, packet.ItemPos, packet.EquippedCardNo1, packet.EquippedCardNo2, packet.EquippedCardNo3, packet.EquippedCardNo4, packet.IsLock, packet.ItemTranscend, packet.ItemChangedElement, packet.ElementLevel);
            info.SetTrade(packet.TradeFlag);
            info.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        private void AddItem(IInputItemValue packet)
        {
            ObscuredLong itemNo = packet.No;

            ItemData data = itemDataRepo.Get(packet.ItemId);
            if (data == null)
            {
                Debug.LogError($"[아이템 추가] 데이터가 존재하지 않습니다: {nameof(itemNo)} = {itemNo.ToString()}, {nameof(packet.ItemId)} = {packet.ItemId}");
                return;
            }

            ItemInfo info;

            ItemGroupType type = data.ItemGroupType;
            if (itemDic.ContainsKey(itemNo))
            {
                info = itemDic[itemNo];

                if (info.IsStackable)
                {
                    // 겹쳐지는 아이템의 경우 Update이지만 Add로 들어올 수 있다 (타이밍 문제)
                    // 업데이트로 강제 처리
                    UpdateItem(packet);
                    return;
                }
                else
                {
                    Debug.LogError($"[아이템 추가] 이미 존재하는 아이템입니다: {nameof(itemNo)} = {itemNo.ToString()}");
                    return;
                }
            }
            else
            {
                // 아이템 추가
                info = Create(type, data.event_id);
                info.SetNew(true);
                itemList.Add(info);
                itemDic.Add(itemNo, info);
            }

            // 퀘스트 처리
            Quest.QuestProgress(QuestType.ITEM_GAIN, packet.ItemId, packet.ItemCount); // 특정 아이템 획득
            Quest.QuestProgress(QuestType.ITEM_GAIN_TYPE, (int)(data.ItemGroupType), packet.ItemCount); // 아이템 종류 획득 개수

            info.SetItemNo(packet.No);
            info.SetItemCount(packet.ItemCount);
            info.SetItemInfo(packet.TierPer, packet.ItemLevel, packet.ItemPos, packet.EquippedCardNo1, packet.EquippedCardNo2, packet.EquippedCardNo3, packet.EquippedCardNo4, packet.IsLock, packet.ItemTranscend, packet.ItemChangedElement, packet.ElementLevel);
            info.SetTrade(packet.TradeFlag);
            info.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)

            // 장비 획득 이벤트
            if (type == ItemGroupType.Equipment)
            {
                OnObtainEquipment?.Invoke(info as EquipmentItemInfo);
            }
            // 코스튬획득 이벤트
            else if (type == ItemGroupType.Costume)
            {
                OnUpdateCostume?.Invoke();
            }

            // 도감 획득
            switch (info.BookType)
            {
                case BookType.Equipment:
                    NotyfyBookRecord(BookTabType.Equipment, info.BookIndex);
                    break;

                case BookType.Card:
                    NotyfyBookRecord(BookTabType.Card, info.BookIndex);
                    break;

                case BookType.Costume:
                    NotyfyBookRecord(BookTabType.Costume, info.BookIndex);
                    break;

                case BookType.Special:
                    NotyfyBookRecord(BookTabType.Special, info.BookIndex);
                    break;

                case BookType.OnBuff:
                    NotyfyBookRecord(BookTabType.OnBuff, info.BookIndex);
                    break;
            }
        }

        /// <summary>
        /// 아이템 업데이트
        /// </summary>
        private void UpdateItem(IInputItemValue packet)
        {
            ObscuredLong itemNo = packet.No;

            ItemData data = itemDataRepo.Get(packet.ItemId);
            if (data == null)
            {
                Debug.LogError($"[아이템 업데이트] 데이터가 존재하지 않습니다: {nameof(itemNo)} = {itemNo.ToString()}, {nameof(packet.ItemId)} = {packet.ItemId}");
                return;
            }

            ItemInfo info;
            if (!itemDic.ContainsKey(itemNo))
            {
                Debug.LogError($"[아이템 업데이트] 존재하지 않는 아이템입니다: {nameof(itemNo)} = {itemNo.ToString()}");
                return;
            }
            else
            {
                info = itemDic[itemNo];
            }

            // 퀘스트 처리
            if (info.IsStackable)
            {
                int questValue = packet.ItemCount - info.ItemCount;
                if (questValue > 0)
                {
                    Quest.QuestProgress(QuestType.ITEM_GAIN, packet.ItemId, questValue); // 특정 아이템 획득
                    Quest.QuestProgress(QuestType.ITEM_GAIN_TYPE, (int)data.ItemGroupType, questValue); // 아이템 종류 획득 개수
                }
                else if (questValue < 0)
                {
                    Quest.QuestProgress(QuestType.ITEM_USE, packet.ItemId, Mathf.Abs(questValue)); // 특정 아이템 사용 횟수
                    Quest.QuestProgress(QuestType.ITEM_USE_TYPE, (int)data.ItemGroupType, Mathf.Abs(questValue)); // 아이템 종류 사용 횟수
                }
            }

            info.SetItemNo(packet.No);
            info.SetItemCount(packet.ItemCount);
            info.SetItemInfo(packet.TierPer, packet.ItemLevel, packet.ItemPos, packet.EquippedCardNo1, packet.EquippedCardNo2, packet.EquippedCardNo3, packet.EquippedCardNo4, packet.IsLock, packet.ItemTranscend, packet.ItemChangedElement, packet.ElementLevel);
            info.SetTrade(packet.TradeFlag);
            info.SetData(data); // 주의: 세팅은 마지막에 할 것! (미리 값을 세팅한 후에 데이터 이벤트 호출)

            // 도감 획득
            switch (info.BookType)
            {
                case BookType.Equipment:
                    NotyfyBookRecord(BookTabType.Equipment, info.BookIndex);
                    break;

                case BookType.Card:
                    NotyfyBookRecord(BookTabType.Card, info.BookIndex);
                    break;

                case BookType.Costume:
                    NotyfyBookRecord(BookTabType.Costume, info.BookIndex);
                    break;

                case BookType.Special:
                    NotyfyBookRecord(BookTabType.Special, info.BookIndex);
                    break;

                case BookType.OnBuff:
                    NotyfyBookRecord(BookTabType.OnBuff, info.BookIndex);
                    break;
            }
        }

        public async Task RequestItemElementChange(ItemInfo targetItem, ItemInfo elementStone)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", targetItem.ItemNo);
            sfs.PutLong("2", elementStone.ItemNo);
            var response = await Protocol.REQUEST_ITEM_ELEMENT_CHANGE.SendAsync(sfs);
            if (!response.isSuccess)
            {
                OnChangeElement?.Invoke(false);
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                var update = response.GetPacket<CharUpdateData>("cud");
                Notify(update);
                if (targetItem.IsEquipped)
                    Entity.ReloadStatus();
            }

            OnChangeElement?.Invoke(true);
        }

        public async Task RequestItemTierUp(ItemInfo targetItem, List<ItemInfo> selectedEquipments)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", targetItem.ItemNo);

            if (selectedEquipments != null)
            {
                long[] nos = new long[selectedEquipments.Count];
                for (int i = 0; i < selectedEquipments.Count; ++i)
                    nos[i] = selectedEquipments[i].ItemNo;
                sfs.PutLongArray("2", nos);
            }

            Debug.Log($"장비={targetItem.Name}, 초월={targetItem.ItemTranscend}");

            var response = await Protocol.REQUEST_ITEM_TIER_UP.SendAsync(sfs);
            if (!response.isSuccess)
            {
                OnTierUp?.Invoke(false);
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                response.GetPacket<CharUpdateData>("cud");
                var update = response.GetPacket<CharUpdateData>("cud");
                Notify(update);
                if (targetItem.IsEquipped)
                    Entity.ReloadStatus();
            }

            Debug.Log($"장비={targetItem.Name}, 초월={targetItem.ItemTranscend}");
            Quest.QuestProgress(QuestType.ITEM_TIER_UP_COUNT, conditionValue: targetItem.ItemTranscend); // 장비 초월 특정레벨 도달 횟수

            OnTierUp?.Invoke(true);
        }

        public async Task<bool> RequestCardRestore(ItemInfo targetItem)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", targetItem.ItemNo);

            var response = await Protocol.REQUEST_RESTORE_MON_CARD.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            if (response.ContainsKey("cud"))
            {
                response.GetPacket<CharUpdateData>("cud");
                var update = response.GetPacket<CharUpdateData>("cud");
                Notify(update);
                if (targetItem.IsEquipped)
                    Entity.ReloadStatus();
            }

            return true;
        }

        public async Task<bool> RequestCardReset(ItemInfo targetItem)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", targetItem.ItemNo);

            var response = await Protocol.REQUEST_RESET_MON_CARD.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            if (response.ContainsKey("cud"))
            {
                response.GetPacket<CharUpdateData>("cud");
                var update = response.GetPacket<CharUpdateData>("cud");
                Notify(update);
                if (targetItem.IsEquipped)
                    Entity.ReloadStatus();
            }

            return true;
        }

        /// <summary>
        /// 아이템 제거 (몬스터조각은 일부러 처리하지 않는다)
        /// </summary>
        private void RemoveItem(IInputItemValue packet)
        {
            ObscuredLong itemNo = packet.No;

            ItemData data = itemDataRepo.Get(packet.ItemId);
            if (data == null)
            {
                Debug.LogError($"[아이템 업데이트] 데이터가 존재하지 않습니다: {nameof(itemNo)} = {itemNo.ToString()}, {nameof(packet.ItemId)} = {packet.ItemId}");
                return;
            }

            ItemInfo info;
            if (!itemDic.ContainsKey(itemNo))
            {
                Debug.LogError($"[아이템 제거] 존재하지 않는 아이템입니다: {nameof(itemNo)} = {itemNo.ToString()}");
                return;
            }
            else
            {
                info = itemDic[itemNo];
            }

            // 퀘스트 처리
            if (info.IsStackable)
            {
                int questValue = info.ItemCount;
                if (questValue > 0)
                {
                    Quest.QuestProgress(QuestType.ITEM_USE, packet.ItemId, questValue); // 특정 아이템 사용 횟수
                    Quest.QuestProgress(QuestType.ITEM_USE_TYPE, (int)data.ItemGroupType, questValue); // 아이템 종류 사용 횟수
                }
            }

            if (data.ItemGroupType == ItemGroupType.MonsterPiece)
            {
                info.SetItemCount(0);
            }
            else
            {
                info.Reload(isEquipCard: false);

                // 아이템 제거
                itemList.Remove(info);
                itemDic.Remove(itemNo);
            }
        }

        /// <summary>
        /// 타입에 맞는 아이템 정보 생성
        /// </summary>
        private ItemInfo Create(ItemGroupType itemGroupType, int eventId)
        {
            switch (itemGroupType)
            {
                case ItemGroupType.Equipment:
                    return new EquipmentItemInfo(this);

                case ItemGroupType.ConsumableItem:
                    if (eventId > 0)
                    {
                        return new BoxItemInfo();
                    }
                    return new ConsumableItemInfo();

                case ItemGroupType.Card:
                    return new CardItemInfo();

                case ItemGroupType.ProductParts:
                    return new PartsItemInfo();

                case ItemGroupType.Costume:
                    return new CostumeItemInfo();

                case ItemGroupType.MonsterPiece:
                    return new MonsterPieceItemInfo();

                default:
                    throw new ArgumentException($"[올바르지 않은 {nameof(itemGroupType)}] {itemGroupType} {(int)itemGroupType}");
            }
        }

        /// <summary>
        /// 아이템 총 무게
        /// </summary>
        private int GetTotalWeight()
        {
            int result = 0;
            for (int i = 0; i < itemList.Count; i++)
            {
                result += (itemList[i].Weight * itemList[i].ItemCount);
            }

            return result;
        }

        /// <summary>
        /// 아이템 다시 로드 (카드 세팅)
        /// </summary>
        private void Reload()
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                itemList[i].Reload(isEquipCard: true);
            }
        }

        /// <summary>
        /// 아이템 잠금
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task RequestItemLock(ItemInfo info)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", info.ItemNo);
            sfs.PutByte("2", info.IsLock ? (byte)0 : (byte)1);
            var response = await Protocol.CHAR_ITEM_LOCK.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        public void SetHideNew(ItemInfo[] arrayInfo)
        {
            if (arrayInfo == null || arrayInfo.Length == 0)
                return;

            bool isDirty = false;
            foreach (var item in arrayInfo.OrEmptyIfNull())
            {
                if (!item.IsNew)
                    continue;

                isDirty = true;
                item.SetNew(false);
            }

            if (isDirty)
            {
                OnUpdateItem?.Invoke();
            }
        }

        public void SetHideNew(ItemInfo info)
        {
            if (!info.IsNew)
                return;

            info.SetNew(false);
            OnUpdateItem?.Invoke();
        }

        /// <summary>
        /// ItemId로 ItemInfo 생성 (보유하지 않은 아이템 정보필요시 사용)
        /// </summary>
        public ItemInfo CreateItmeInfo(int itemId)
        {
            ItemData data = itemDataRepo.Get(itemId);
            if (data == null)
            {
                Debug.LogError($"[아이템정보 생성] 데이터가 존재하지 않습니다: {nameof(itemId)} = {itemId}");
                return null;
            }
            ItemInfo info = Create(data.ItemGroupType, data.event_id);
            info.SetData(data);
            return info;
        }

        /// <summary>
        /// 인벤토리에 아이템 추가. (획득 이벤트 등을 발생시키지 않는다.)
        /// </summary>
        public void AddToItemList(ItemInfo info)
        {
            var existItem = itemList.Find(e => e.ItemId == info.ItemId && e.IsStackable);
            if (existItem != null)
                existItem.SetItemCount(existItem.ItemCount + info.ItemCount);
            else
            {
                itemList.Add(info);
                itemDic.Add(info.ItemNo, info);
            }
        }

        /// <summary>
        /// 인벤토리에 존재하는 특정 아이템을 제거하거나 차감한다. (소모 이벤트 등을 발생시키지 않는다.)
        /// </summary>
        public void RemoveFromItemList(long itemNo, int itemID, int count)
        {
            var existItem = itemList.Find(e => e.ItemId == itemID && e.ItemNo == itemNo);
            if (existItem != null)
            {
                if (existItem.ItemCount > count)
                    existItem.SetItemCount(existItem.ItemCount - count);
                else
                {
                    itemList.Remove(existItem);
                    itemDic.Remove(existItem.ItemNo);
                }
                return;
            }

            Debug.LogError($"존재하지 않는 아이템을 삭제 시도함. ID: {itemID}");
        }

        /// <summary>
        /// 해당 슬롯타입의 가장 강한 장비 반환
        /// </summary>
        public EquipmentItemInfo GetStrongestEquipmentInSlotType(ItemEquipmentSlotType slotType, List<EquipmentItemInfo> equipmentList = null)
        {
            if (equipmentList is null)
            {
                equipmentList = (from item in this.itemList
                                 where item is EquipmentItemInfo && item.SlotType == slotType
                                 let equipment = item as EquipmentItemInfo
                                 select equipment).ToList();
            }

            Job job = Entity.Character.Job;

            // 만약 slotType이 Weapon이고, 그 안에 내 직업추천타입이 있다면 직업추천타입 안에서만 비교한다.
            bool isSelectJobAppropriateOnly = slotType == ItemEquipmentSlotType.Weapon && equipmentList.Exists(e => IsJobRecommendClassType(e.ClassType, job));

            EquipmentItemInfo strongestItem = null;
            int strongestAP = -1;
            foreach (var equipment in equipmentList)
            {
                // 직업추천무기만 고르는 경우, 내 직업추천무기가 아니면 생략
                if (isSelectJobAppropriateOnly && !IsJobRecommendClassType(equipment.ClassType, job))
                {
                    continue;
                }

                int itemAP = equipment.GetAttackPower();
                if (strongestAP < itemAP || strongestItem == null)
                {
                    strongestAP = itemAP;
                    strongestItem = equipment;
                }
            }

            return strongestItem;
        }

        /// <summary>
        /// 착용중인 장비보다 더 강한 장비 존재 여부
        /// </summary>
        /// <returns></returns>
        public bool HasStrongerEquipment()
        {
            if (HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.HeadGear) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.Armor) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.Weapon) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.Garment) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.Accessory1) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.Accessory2))
                return true;

            return false;
        }

        /// <summary>
        /// 착용중인 장비보다 더 강한 장비 존재 여부
        /// </summary>
        /// <returns></returns>
        public bool HasStrongerShadowEquipment()
        {
            if (HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.ShadowHeadGear) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.ShadowArmor) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.ShadowWeapon) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.ShadowGarment) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.ShadowAccessory1) ||
                HasStrongerEquipmentInSlotType(ItemEquipmentSlotType.ShadowAccessory2))
                return true;

            return false;
        }

        /// <summary>
        /// 현재 장비보다 강한 장비가 있는지 체크
        /// </summary>
        public bool HasStrongerEquipmentInSlotType(ItemEquipmentSlotType slotType)
        {
            // 쉐도우 장비 타입 일 경우
            switch (slotType)
            {
                case ItemEquipmentSlotType.ShadowHeadGear:
                case ItemEquipmentSlotType.ShadowGarment:
                case ItemEquipmentSlotType.ShadowArmor:
                case ItemEquipmentSlotType.ShadowWeapon:
                case ItemEquipmentSlotType.ShadowAccessory1:
                case ItemEquipmentSlotType.ShadowAccessory2:
                    return HasStrongerShadowEquipmentInSlotType(slotType);
            }

            List<ItemInfo> equipmentList = itemList.FindAll(e => e is EquipmentItemInfo && e.SlotType == slotType);

            Job job = Entity.Character.Job;

            // 무기면 직업추천타입 여부를 따로 검사한다.
            bool isSelectJobAppropriateOnly = slotType == ItemEquipmentSlotType.Weapon && equipmentList.Exists(e => IsJobRecommendClassType(e.ClassType, job));
            if (isSelectJobAppropriateOnly)
            {
                // 리스트 안에 하나라도 내 직업추천무기가 있다면, 
                // 직업추천무기가 아닌 장비들은 전부 리스트에서 제외한다.
                equipmentList.RemoveAll(e => !IsJobRecommendClassType(e.ClassType, job));
            }

            // 리스트가 없으면 false 반환
            if (equipmentList.Count == 0)
                return false;

            // 현재 미 장착 중이라면 true 반환
            var currentEquipment = itemList.Find(a => a.EquippedSlotType == slotType) as EquipmentItemInfo;
            if (currentEquipment is null)
                return true;

            // 현재 내 무기가 직업추천무기가 아니라면 true 반환
            if (isSelectJobAppropriateOnly && !IsJobRecommendClassType(currentEquipment.ClassType, job))
                return true;

            // 내 장착템보다 강한 템이 있으면 바로 True 반환
            int currentEquipmentAP = currentEquipment.GetAttackPower();
            foreach (var item in equipmentList)
            {
                if (item == currentEquipment)
                    continue;

                EquipmentItemInfo thisItem = item as EquipmentItemInfo;
                int thisAP = thisItem.GetAttackPower();
                if (thisAP > currentEquipmentAP)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 현재 장비보다 강한 장비가 있는지 체크
        /// </summary>
        private bool HasStrongerShadowEquipmentInSlotType(ItemEquipmentSlotType slotType)
        {
            List<ItemInfo> equipmentList = itemList.FindAll(e => e is EquipmentItemInfo && e.SlotType == slotType);

            // 리스트가 없으면 false 반환
            if (equipmentList.Count == 0)
                return false;

            // 현재 미 장착 중이라면 true 반환
            var currentEquipment = itemList.Find(a => a.EquippedSlotType == slotType) as EquipmentItemInfo;
            if (currentEquipment is null)
                return true;

            return false;
        }

        public bool EquipableCostume()
        {
            if (EquipableCostume(ItemEquipmentSlotType.CostumeWeapon) ||
                EquipableCostume(ItemEquipmentSlotType.CostumeHat) ||
                EquipableCostume(ItemEquipmentSlotType.CostumeFace) ||
                EquipableCostume(ItemEquipmentSlotType.CostumeCape) ||
                EquipableCostume(ItemEquipmentSlotType.CostumePet) ||
                EquipableCostume(ItemEquipmentSlotType.CostumeTitle))
                return true;

            return false;
        }

        /// <summary>
        /// 장착 가능한 코스튬이 있는지
        /// </summary>
        public bool EquipableCostume(ItemEquipmentSlotType slotType)
        {
            var equippedCostume = itemList.Find(a => a.EquippedSlotType == slotType);
            if (equippedCostume != null) // 슬롯에 코스튬이 장착된 상태
                return false;

            return itemList.Find(a => a.SlotType == slotType);
        }

        /// <summary>
        /// 해당 클래스타입이 내 직업의 추천 타입인지
        /// </summary>
        public bool IsJobRecommendClassType(EquipmentClassType classType, Job job)
        {
            EquipmentClassType appropriateEquipmentClassType = job.GetJobAppropriateEquipmentClassTypes();
            return appropriateEquipmentClassType.HasFlag(classType);
        }

        /// <summary>
        /// [코스튬] 장착,교체,해제
        /// </summary>
        public async Task RequestCostumeEquip(ItemInfo costume)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", costume.IsEquipped ? 0L : costume.ItemNo); // 이미 장착중인 아이템 = 해제(0L)
            sfs.PutByte("2", (byte)costume.SlotType);

            var response = await Protocol.COSTUME_EQUIP.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                OnUpdateCostume?.Invoke();
                OnChangeCostume?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        public void ShowBoxItemRewardLog(ItemInfo info)
        {
#if UNITY_EDITOR
            if (!DebugUtils.IsLogBoxReward)
                return;

            if (info == null)
                return;

            if (info.ItemType == ItemType.Box)
            {
                BoxData boxData = boxDataRepo.Get(info.EventId);

                if (boxData == null)
                    return;

                var sb = StringBuilderPool.Get();

                sb.AppendLine($"{info.ItemId}\t{info.Name}");
                sb.AppendLine($"ID\t아이템이름\t수량\t확률");

                RewardData[] rewards = boxData.rewards;

                for (int i = 0; i < rewards.Length; i++)
                {
                    if (rewards[i].RewardType == RewardType.None)
                        continue;

                    if (rewards[i].RewardType == RewardType.RefGacha)
                    {
                        GachaData[] gachaData = gachaDataRepo.Gets(rewards[i].RewardValue);

                        foreach (var item in gachaData)
                        {
                            RewardData reward = item.Reward;

                            if (reward.RewardType == RewardType.Item)
                            {
                                sb.AppendLine($"{reward.ItemId}\t{reward.ItemName}\t{reward.Count}\t{item.rate}");
                            }
                            else
                            {
                                sb.AppendLine($"\t{reward.ItemName}\t{reward.Count}\t{item.rate}");
                            }
                        }
                    }
                    else if (rewards[i].RewardType == RewardType.Item)
                    {
                        sb.AppendLine($"{rewards[i].ItemId}\t{rewards[i].ItemName}\t{rewards[i].Count}");
                    }
                    else
                    {
                        sb.AppendLine($"\t{rewards[i].ItemName}\t{rewards[i].Count}");
                    }

                    if (rewards.Length > 0)
                        sb.AppendLine($"============");
                }

                Debug.Log(sb.Release());
            }
#endif
        }

        /// <summary>
        /// 상자 구성품 정보
        /// </summary>
        public List<BoxRewardGroup> GetBoxRewardList(int itemId)
        {
            ItemData itemData = itemDataRepo.Get(itemId);

            if (itemData == null)
                return null;

            BoxData boxData = boxDataRepo.Get(itemData.event_id);

            if (boxData == null)
                return null;

            RewardData[] rewards = boxData.rewards;

            BoxRewardGroup fixedRewardList = null; // 확정으로 획득하는 보상 목록
            Dictionary<int, BoxRewardGroup> rateRewardDic = new Dictionary<int, BoxRewardGroup>(IntEqualityComparer.Default); // 확률적으로 획득하는 보상 목록

            for (int i = 0; i < rewards.Length; i++)
            {
                if (rewards[i].RewardType == RewardType.None)
                    continue;

                if (rewards[i].RewardType == RewardType.RefGacha) // 확률적으로 획득하는 보상
                {
                    GachaData[] gachaData = gachaDataRepo.Gets(rewards[i].RewardValue);

                    if (!rateRewardDic.ContainsKey(i))
                        rateRewardDic.Add(i, new BoxRewardGroup(rewards[i].RewardCount));

                    foreach (var item in gachaData)
                    {
                        rateRewardDic[i].AddReward(new BoxRewardInfo(item.Reward, item.rate, isFixed: false));
                    }
                    rateRewardDic[i].Sort();
                    rateRewardDic[i].SetTotalRate();
                }
                else // 확정으로 획득하는 보상 목록
                {
                    if (fixedRewardList == null)
                        fixedRewardList = new BoxRewardGroup(giveCount: 1);

                    fixedRewardList.AddReward(new BoxRewardInfo(rewards[i], rate: 0, isFixed: true));
                }
            }

            List<BoxRewardGroup> boxRewards = new List<BoxRewardGroup>();
            if (fixedRewardList != null)
                boxRewards.Add(fixedRewardList);

            boxRewards.AddRange(rateRewardDic.Values);

            return boxRewards;
        }

        /// <summary>
        /// 쉐도우 장비 카드 슬롯 오픈 요청
        /// </summary>
        public async void RequestShadowItemOpenCardSlot(long itemNo)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", itemNo);

            var response = await Protocol.REQUEST_SHADOW_ITEM_OPEN_CARD_SLOT.SendAsync(sfs);
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
        }

        /// <summary>
        /// 어둠의 나무 보상 선택
        /// </summary>
        public async Task RequestDarkTreeSelectReward(int rewardId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", rewardId);

            var response = await Protocol.REQUEST_DARK_TREE_SELECT_REWARD.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            darkTreeInfo.SelectReward(darkTreeRewardDataRepo.Get(rewardId));
        }

        /// <summary>
        /// 어둠의 나무 재료 넣기
        /// </summary>
        public async Task RequestDarkTreeRegPoint((int id, int count)[] items)
        {
            int plusPoint = 0;
            var sfs = Protocol.NewInstance();
            var sfsArray = Protocol.NewArrayInstance();
            foreach (var item in items)
            {
                var element = Protocol.NewInstance();
                element.PutInt("1", item.id);
                element.PutInt("2", item.count);
                sfsArray.AddSFSObject(element);

                ItemData itemData = itemDataRepo.Get(item.id);
                if (itemData == null)
                    continue;

                plusPoint += (itemData.skill_rate * item.count);
            }
            sfs.PutSFSArray("1", sfsArray);

            var response = await Protocol.REQUEST_DARK_TREE_REG_POINT.SendAsync(sfs);
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

            darkTreeInfo.PlusPoint(plusPoint);
        }

        /// <summary>
        /// 어둠의 나무 시작
        /// </summary>
        public async Task RequestDarkTreeStart()
        {
            var response = await Protocol.REQUEST_DARK_TREE_START.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            darkTreeInfo.Run();
        }

        /// <summary>
        /// 어둠의 나무 보상 받기
        /// </summary>
        public async Task RequestDarkTreeGetReward()
        {
            var response = await Protocol.REQUEST_DARK_TREE_GET_REWARD.SendAsync();
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

                UI.RewardInfo(charUpdateData.rewards);
            }

            darkTreeInfo.ResetData();
        }

        /// <summary>
        /// [나비호] 의뢰 정보
        /// </summary>
        [System.Obsolete]
        public async Task RequestNabihoInfo()
        {
            var response = await Protocol.REQUEST_NABIHO_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
                Initialize(response.GetPacketArray<NabihoPacket>("1"));

            SetNabihoExp(response.GetInt("2"));

            OnUpdateNabiho?.Invoke();
        }

        /// <summary>
        /// [나비호] 의뢰 아이템 선택
        /// </summary>
        public async Task RequestNabihoItemSelect(int nabihoId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", nabihoId);
            var response = await Protocol.REQUEST_NABIHO_ITEM_SELECT.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
                UpdateNabiho(response.GetPacket<NabihoPacket>("1"));

            OnUpdateNabiho?.Invoke();
        }

        /// <summary>
        /// [나비호] 의뢰 아이템 선택 취소
        /// </summary>
        public async Task RequestNabihoItemSelectCancel(int nabihoId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", nabihoId);
            var response = await Protocol.REQUEST_NABIHO_ITEM_SELECT_CANCEL.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            RemoveNabiho(nabihoId);

            OnUpdateNabiho?.Invoke();
        }

        /// <summary>
        /// [나비호] 의뢰 아이템 광고 시청후 시간 단축
        /// </summary>
        public async Task RequestNabihoItemAdTimeReduction(int nabihoId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", nabihoId);
            var response = await Protocol.REQUEST_NABIHO_ITEM_AD_TIME_REDUCTION.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
                UpdateNabiho(response.GetPacket<NabihoPacket>("1"));

            OnUpdateNabiho?.Invoke();
        }

        /// <summary>
        /// [나비호] 의뢰 아이템 수령
        /// </summary>
        public async Task RequestNabihoItemSelectGet(int nabihoId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", nabihoId);
            var response = await Protocol.REQUEST_NABIHO_ITEM_SELECT_GET.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            RemoveNabiho(nabihoId);

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                UI.RewardInfo(charUpdateData.rewards);
            }

            OnUpdateNabiho?.Invoke();
        }

        /// <summary>
        /// [나비호] 선물 - 친밀도 증가
        /// </summary>
        public async Task RequestNabihoSendPresent(int count)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", count);
            var response = await Protocol.REQUEST_NABIHO_SEND_PRESENT.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            SetNabihoExp(response.GetInt("1"));

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                UI.RewardInfo(charUpdateData.rewards);
            }

            OnUpdateNabiho?.Invoke();
        }

        private void CheckNabihoRemainTime()
        {
            Timing.KillCoroutines(nameof(YieldCheckNabihoRemainTime));

            // 이미 Notice 가 보일 경우에는 Check 할 필요 없음
            if (IsNoticeNobiho())
                return;

            bool needCheckTime = false;
            float waitSeconds = float.MaxValue;
            float tempTime;
            foreach (var item in NabihoDic.Values)
            {
                // RemainTime
                tempTime = item.RemainTime.ToRemainTime() * 0.001f;
                if (tempTime > 0f)
                {
                    needCheckTime = true;
                    waitSeconds = Mathf.Min(waitSeconds, tempTime);
                }

                // AdRemainTime
                tempTime = item.AdRemainTime.ToRemainTime() * 0.001f;
                if (tempTime > 0f)
                {
                    needCheckTime = true;
                    waitSeconds = Mathf.Min(waitSeconds, tempTime);
                }
            }

            if (needCheckTime)
            {
                Timing.RunCoroutine(YieldCheckNabihoRemainTime(waitSeconds), Segment.RealtimeUpdate, nameof(YieldCheckNabihoRemainTime));
            }
        }

        IEnumerator<float> YieldCheckNabihoRemainTime(float seconds)
        {
            yield return Timing.WaitForSeconds(seconds + 0.1f); // 혹시 모르니 Delay 를 추가로 더 준다.
            OnUpdateNabiho?.Invoke();
        }

        int BattleItemInfo.IValue.TotalItemAtk => ServerTotalItemAtk;
        int BattleItemInfo.IValue.TotalItemMatk => ServerTotalItemMatk;
        int BattleItemInfo.IValue.TotalItemDef => ServerTotalItemDef;
        int BattleItemInfo.IValue.TotalItemMdef => ServerTotalItemMdef;

        #region 테스트 코드

        /// <summary>
        /// 장비 장착 테스트 프로토콜
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public async void RequestItemEquip(long itemNo, byte slot)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", itemNo); // 이미 장착중인 아이템 = 해제(0L)
            sfs.PutByte("2", slot);

            var response = await Protocol.ITEM_EQUIP.SendAsync(sfs);
            if (response.isSuccess)
            {
                if (itemNo != 0)
                    Quest.QuestProgress(QuestType.ITEM_EQUIP); // 장비 장착

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                OnUpdateEquipment?.Invoke();
                OnUpdateStatus?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 장비 강화
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public async void RequestEquipmentLevelUp(long itemNo)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", itemNo);

            var response = await Protocol.REQUEST_EQUIP_ITEM_LEVELUP.SendAsync(sfs);
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

            ItemInfo item = GetItemInfo(itemNo);
            int nextSmelt = item.Smelt;

            bool isUnLock = false;
            for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; i++)
            {
                if (nextSmelt == item.GetCardSlotOpenLevel(i))
                {
                    isUnLock = true;
                    break;
                }
            }

            Analytics.TrackEvent($"Equipment{nextSmelt}");

            // 퀘스트 처리
            Quest.QuestProgress(QuestType.ITEM_UPGRADE); // 장비템 제련 횟수
            Quest.QuestProgress(QuestType.ITEM_UPGRADE_MAX, questValue: nextSmelt); // 장비템 제련도 도달
            Quest.QuestProgress(QuestType.ITEM_LEVEL_UPGRADE, conditionValue: nextSmelt); // 장비템 제련도 도달 횟수

            switch (item.Rating)
            {
                case 1:
                    Quest.QuestProgress(QuestType.ITEM_RANK_1_LEVEL_COUNT, conditionValue: nextSmelt); // 1랭크 특정 장비 강화도 도달 횟수
                    break;

                case 2:
                    Quest.QuestProgress(QuestType.ITEM_RANK_2_LEVEL_COUNT, conditionValue: nextSmelt); // 2랭크 특정 장비 강화도 도달 횟수
                    break;

                case 3:
                    Quest.QuestProgress(QuestType.ITEM_RANK_3_LEVEL_COUNT, conditionValue: nextSmelt); // 3랭크 특정 장비 강화도 도달 횟수
                    break;

                case 4:
                    Quest.QuestProgress(QuestType.ITEM_RANK_4_LEVEL_COUNT, conditionValue: nextSmelt); // 4랭크 특정 장비 강화도 도달 횟수
                    break;

                case 5:
                    Quest.QuestProgress(QuestType.ITEM_RANK_5_LEVEL_COUNT, conditionValue: nextSmelt); // 5랭크 특정 장비 강화도 도달 횟수
                    break;
            }


            OnUpdateStatus?.Invoke();
            OnEquipItemLevelUp?.Invoke(isUnLock, nextSmelt);
        }

        #endregion
    }
}