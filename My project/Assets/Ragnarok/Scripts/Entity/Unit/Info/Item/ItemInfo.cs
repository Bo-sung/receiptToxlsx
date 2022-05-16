using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 아이템 정보
    /// <see cref="EquipmentItemInfo"/> 장비
    /// <see cref="CardItemInfo"/> 카드
    /// <see cref="PartsItemInfo"/> 재료 (제작재료, 카드 제련 재료)
    /// <see cref="ConsumableItemInfo"/> 소모품
    /// <see cref="BoxItemInfo"/> 박스
    /// <see cref="CostumeItemInfo"/> 코스튬
    /// <see cref="MonsterPieceItemInfo"/> 몬스터조각
    /// </summary>
    public abstract class ItemInfo : DataInfo<ItemData>, IEnumerable<BattleOption>, IEnumerable<CardBattleOption>, IUIData, InventoryModel.IInputItemValue
    {
        public interface IEquippedItemValue
        {
            int ItemId { get; }
            int ItemLevel { get; }
            int ItemTranscend { get; }
            int ItemChangedElement { get; }
            int ElementLevel { get; }
            int? GetEquippedCardId(int index);
            int? GetEquippedCardLevel(int index);
        }

        #region 공통

        ObscuredLong itemNo; // 아이템 서버 고유 값
        ObscuredInt itemCount; // 아이템 개수
        bool itemIsNew; // 아이템 새로 획득 여부
        byte tradeFlag; // 거래 여부

        /// <summary>
        /// [공통] 보유 여부
        /// </summary>
        public bool IsInPossession => itemNo > 0;

        /// <summary>
        /// [공통] 아이템 서버 고유 값 (프로토콜 등에 사용)
        /// </summary>
        public long ItemNo => itemNo;

        /// <summary>
        /// [공통] 아이템 id
        /// </summary>
        public int ItemId => data.id;

        /// <summary>
        /// [공통] 아이템 개수
        /// </summary>
        public int ItemCount => itemCount;

        /// <summary>
        /// [공통] 아이템 타입
        /// </summary>
        public ItemGroupType ItemGroupType => data.ItemGroupType;

        /// <summary>
        /// [공통] 아이템 세부 타입
        /// </summary>
        public ItemType ItemType => data.ItemType;

        /// <summary>
        /// [공통] 겹쳐짐 여부
        /// </summary>
        public abstract bool IsStackable { get; }

        /// <summary>
        /// [공통] 아이템 이름
        /// </summary>
        public string Name => data.name_id.ToText();

        /// <summary>
        /// [공통] 아이템 설명
        /// </summary>
        public string Description => GetItemDescription();

        /// <summary>
        /// [공통] 판매 가격
        /// </summary>
        public int Price => data.price;

        /// <summary>
        /// [공통] 판매 상한가
        /// </summary>
        public int MaxPrice => data.price_max_limit;

        /// <summary>
        /// [공통] 아이템 무게
        /// </summary>
        public int Weight => data.weight;

        /// <summary>
        /// [공통] UI상에 표시 되는 무게
        /// </summary>
        public string TotalWeightText => $"{Weight * 0.1f:0.#}";

        /// <summary>
        /// [공통] 아이콘 이름
        /// </summary>
        public string IconName => data.icon_name;

        /// <summary>
        /// [공통] 아이템 새로 획득
        /// </summary>
        public bool IsNew => itemIsNew;

        /// <summary>
        /// [공통] 노점에서 소모되는 Ro Point
        /// </summary>
        public int RoPoint => data.point_value;

        /// <summary>
        /// [공통] 사용처 카테고리
        /// </summary>
        public ItemSourceCategoryType Use_ClassBitType => data.item_use_bit_type.ToEnum<ItemSourceCategoryType>();

        /// <summary>
        /// [공통] 획득처 카테고리
        /// </summary>
        public ItemSourceCategoryType Get_ClassBitType => data.item_get_bit_type.ToEnum<ItemSourceCategoryType>();

        /// <summary>
        /// [공통] 이벤트 ID
        /// </summary>
        public int EventId => data.event_id;

        long InventoryModel.IInputItemValue.No => ItemNo;
        int InventoryModel.IInputItemValue.TierPer => Tier;
        int InventoryModel.IInputItemValue.ItemLevel => Smelt;
        byte InventoryModel.IInputItemValue.ItemPos => (byte)EquippedSlotType;
        long InventoryModel.IInputItemValue.EquippedCardNo1 => GetEquippedCardNo(0);
        long InventoryModel.IInputItemValue.EquippedCardNo2 => GetEquippedCardNo(1);
        long InventoryModel.IInputItemValue.EquippedCardNo3 => GetEquippedCardNo(2);
        long InventoryModel.IInputItemValue.EquippedCardNo4 => GetEquippedCardNo(3);
        byte InventoryModel.IInputItemValue.TradeFlag => tradeFlag;

        #endregion

        /// <summary>
        /// [장비/카드] 장착 여부
        /// </summary>
        public virtual bool IsEquipped => default;

        /// <summary>
        /// [장비] 착용 가능한 슬롯
        /// </summary>
        public virtual ItemEquipmentSlotType SlotType => default;

        /// <summary>
        /// [장비, 코스튬] 현재 착용중인 장비 슬롯 위치
        /// </summary>
        public virtual ItemEquipmentSlotType EquippedSlotType { get => default; set { } }

        /// <summary>
        /// [장비] 아이템 속성 => 평타 스킬에 속성 적용
        /// [재료] 속성석 속성
        /// </summary>
        public virtual ElementType ElementType => default;

        /// <summary>
        /// [장비] 장비 등급
        /// </summary>
        public virtual int Rating => default;

        /// <summary>
        /// [장비] 프리팹 이름
        /// </summary>
        public virtual string PrefabName => default;

        /// <summary>
        /// [장비] 장비 티어
        /// </summary>
        public virtual int Tier => default;

        /// <summary>
        /// [장비] 장비 제련도
        /// </summary>
        public virtual int Smelt => default;

        /// <summary>
        /// [장비] 장비 최대 제련도
        /// [카드제련재료] 제련 최대 레벨
        /// </summary>
        public virtual int MaxSmelt => default;

        /// <summary>
        /// [장비] 장비 최대 제련도 여부
        /// </summary>
        public bool IsMaxSmelt => Smelt == MaxSmelt;

        /// <summary>
        /// [장비/카드] 장비 클래스 타입
        /// </summary>
        public virtual EquipmentClassType ClassType => default;

        /// <summary>
        /// [장비] 장비에 카드 인챈트 여부
        /// </summary>
        public virtual bool IsCardEnchanted => default;

        /// <summary>
        /// [장비/카드] 잠금 여부
        /// </summary>
        public virtual bool IsLock => default;

        /// <summary>
        /// [장비] 아이템 초월 횟수
        /// </summary>
        public virtual int ItemTranscend => default;

        /// <summary>
        /// [장비] 변경된 장비 속성
        /// </summary>
        public virtual int ItemChangedElement => default;

        /// <summary>
        /// [장비] 장비에 카드 장착중 여부
        /// </summary>
        public virtual bool IsEquippedCard => default;

        /// <summary>
        /// [카드] 장착여부에 따른 카드 이미지
        /// </summary>
        public virtual string GetSlotIconName()
        {
            return default;
        }

        /// <summary>
        /// [카드] 카드 레벨
        /// </summary>
        public virtual int CardLevel => default;

        /// <summary>
        /// [카드] 카드 레벨 최대값
        /// </summary>
        public virtual int MaxCardLevel => default;

        /// <summary>
        /// [카드] 카드 레벨업 가능 여부
        /// </summary>
        public bool CanLevelUp => CardLevel < MaxCardLevel;

        /// <summary>
        /// [카드] 카드 레벨 최대값
        /// </summary>
        public virtual bool IsCardMaxLevel => default;

        /// <summary>
        /// [카드] UI 표시용 카드 레벨
        /// </summary>
        public virtual string GetCardLevelView()
        {
            return default;
        }

        /// <summary>
        /// [소모품/박스] 쿨타임
        /// </summary>
        public virtual long Cooldown => default;

        /// <summary>
        /// [소모품/박스] 남은 쿨타임 시간
        /// </summary>
        public virtual float RemainCooldown => default;

        /// <summary>
        /// [소모품] 타겟 타입
        /// </summary>
        public virtual TargetType TargetType => default;

        /// <summary>
        /// [소모품] 지속 시간
        /// </summary>
        public virtual long Duration => default;

        /// <summary>
        /// [소모품] 버프 아이템
        /// </summary>
        public bool IsBuff => Duration > 0;

        /// <summary>
        /// [박스] 상자 타입
        /// </summary>
        public virtual BoxType BoxType => default;

        /// <summary>
        /// [박스] 상자 오픈 시 무게 체크 여부
        /// </summary>
        public virtual bool IsWeightCheckForBoxOpen => default;

        /// <summary>
        /// [확성기] 확성기 아이템 여부
        /// </summary>
        public virtual ConsumableItemType ConsumableItemType => default;

        /// <summary>
        /// [코스튬] 코스튬 아이템 타입
        /// </summary>
        public virtual CostumeType CostumeType => default;

        /// <summary>
        /// [코스튬] 코스튬 테이블 ID
        /// </summary>
        public virtual int CostumeDataId => default;

        /// <summary>
        /// [무기,코스튬] 이펙트 이름
        /// </summary>
        public string EffectName => data.effect_name;

        /// <summary>
        /// [코스튬] 몸 장착 타입
        /// </summary>
        public CostumeBodyType CostumeBodyType => data.element_type.ToEnum<CostumeBodyType>();

        /// <summary>
        /// [셰어바이스] 경험치 량
        /// </summary>
        public int ExpValue => data.event_id;

        /// <summary>
        /// [도감] 도감 내 획득 딕셔너리 인덱스
        /// </summary>
        public int BookIndex => data.dic_id;

        /// <summary>
        /// [도감] 도감 내 표시 순서
        /// </summary>
        public int BookOrder => data.dic_order;

        /// <summary>
        /// [코스튬] 코스튬 칭호 이름 ID
        /// </summary>
        public int CostumeTitleID => data.skill_id;

        /// <summary>
        /// [장비, 카드] 쉐도우 아이템 여부
        /// </summary>
        public virtual bool IsShadow => default;

        /// <summary>
        /// [장비, 카드] 아이템 세부 타입
        /// </summary>
        public virtual ItemDetailType ItemDetailType => default;

        /// <summary>
        /// [장비] 쉐도우 장비 오픈된 카드 슬롯 개수
        /// </summary>
        public virtual int CardSlotCount => default;

        /// <summary>
        /// [공통] 거래 가능 여부
        /// </summary>
        public bool CanTrade => tradeFlag == 0 && RoPoint > 0;

        /// <summary>
        /// [공통] 아이템 서버 고유 값 세팅 (몬스터 조각의 경우 데이터가 없을 경우가 있기 때문에 필요)
        /// </summary>
        public void SetItemNo(long no)
        {
            itemNo = no; // 아이템 서버 고유 no
        }

        /// <summary>
        /// [공통] 아이템 개수 세팅 (몬스터 조각의 경우 Info Remove 를 타지 않고 Count = 0 으로 처리하기 때문에 필요)
        /// </summary>
        public void SetItemCount(int itemCount)
        {
            this.itemCount = itemCount;
        }

        /// <summary>
        /// [공통] 아이템 정보 세팅
        /// </summary>
        public abstract void SetItemInfo(int tier, int itemLevel, byte itemPos, long equippedCardNo1, long equippedCardNo2, long equippedCardNo3, long equippedCardNo4, bool isLock, int itemTranscend = 0, int itemChangedElement = 0, int itemElementLevel = 0);

        /// <summary>
        /// [공통] 거래 여부 세팅
        /// </summary>
        public void SetTrade(byte flag)
        {
            tradeFlag = flag;
        }

        /// <summary>
        /// [공통] 장착 경고 메시지
        /// </summary>
        public virtual string GetEquiqWarningMessage(bool isPopupMessage)
        {
            return string.Empty;
        }

        /// <summary>
        /// [공통] 구매 경고 메시지
        /// </summary>
        public virtual string GetPurchaseWarningMessage()
        {
            return string.Empty;
        }

        /// <summary>
        /// [공통] 다시로드
        /// </summary>
        public abstract void Reload(bool isEquipCard);

        /// <summary>
        /// [장비] 장착한 카드 정보
        /// </summary>
        protected virtual long GetEquippedCardNo(int index)
        {
            return 0L;
        }

        /// <summary>
        /// [장비] 카드 정보 반환
        /// </summary>
        public virtual ItemInfo GetCardItem(int index)
        {
            return default;
        }

        /// <summary>
        /// [장비] 카드 슬롯 해금 여부
        /// </summary>
        public virtual bool IsOpenCardSlot(int index)
        {
            return default;
        }

        /// <summary>
        /// [장비] 카드 슬롯 해금 레벨
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual int GetCardSlotOpenLevel(int index)
        {
            return default;
        }

        /// <summary>
        /// [장비] 장착 가능 카드 체크
        /// </summary>
        public virtual bool CanEquipCard(EquipmentClassType classType)
        {
            return default;
        }

        /// <summary>
        /// [장비] 현재 제련의 스탯 반환
        /// </summary>
        public Status GetStatus()
        {
            return GetStatus(Tier);
        }

        /// <summary>
        /// [장비] 특정 제련의 스탯 반환
        /// </summary>
        protected virtual Status GetStatus(int tier)
        {
            return default;
        }

        /// <summary>
        /// [장비/카드/소모품] 현재 제련의 전투 옵션 반환
        /// </summary>
        public virtual IEnumerable<BattleOption> GetBattleOptionCollection()
        {
            return GetBattleOptionCollection(Smelt);
        }

        /// <summary>
        /// [장비/소모품] 특정 제련의 전투 옵션 반환
        /// </summary>
        public virtual IEnumerable<BattleOption> GetBattleOptionCollection(int smelt)
        {
            return default;
        }

        /// <summary>
        /// [카드] 옵션의 다음 레벨의 최소, 최대값 // 현재레벨의 최소,최대값, 퍼센트
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<CardBattleOption> GetCardBattleOptionCollection()
        {
            return default;
        }

        /// <summary>
        /// [소모품/박스] 쿨타임 만기 시간 설정
        /// </summary>
        public virtual void SetRemainCoolDown(float endCooldown)
        {
        }

        /// <summary>
        /// [소모품/박스] 재사용 대기시간 존재 여부
        /// </summary>
        public virtual bool IsCooldown()
        {
            return default;
        }

        /// <summary>
        /// [카드] 장착 여부 세팅 (카드의 경우)
        /// </summary>
        public virtual void SetEquipped(bool isEquipped)
        {
        }

        /// <summary>
        /// [장비/카드] 티어,카드레벨에 따른 백그라운드 이미지 이름
        /// 인벤토리에서는 이미지타입이 다름
        /// </summary>
        public virtual string GetBackSpriteName(bool isInventory, bool supportTranscend = false)
        {
            return Constants.CommonAtlas.UI_COMMON_BG_ITEM_01;
        }

        /// <summary>
        /// [장비] 잠금 버튼 배경색
        /// </summary>
        public virtual Color GetLockBackColor(bool supportTranscend = false) { return default; }

        /// <summary>
        /// [카드] 카드제련 에 필요한 제니
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public virtual int NeedCardSmeltZeny => default;

        /// <summary>
        /// [카드] 옵션 비율
        /// </summary>
        /// <returns></returns>
        public virtual float OptionRate => default;

        /// <summary>
        /// [카드] 제련 성공확률 (만분률)
        /// </summary>
        public virtual float SuccessRate => default;

        /// <summary>
        /// [카드제련재료] 제련 경고 표시 레벨
        /// </summary>
        public virtual int WaringSmeltLevel => default;

        /// <summary>
        /// [카드제련재료] 카드제련재료 여부
        /// </summary>
        public virtual bool IsSmeltMaterial => default;

        /// <summary>
        /// [카드] 인덱스별 옵션 값
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual long GetCardOptionValue(int index) => default;

        /// <summary>
        /// [카드] 전투력
        /// </summary>
        /// <returns></returns>
        public virtual int BattleScore => default;

        /// <summary>
        /// 카드 옵션 패널 텍스쳐 이름
        /// </summary>
        /// <returns></returns>
        public virtual string CardOptionPanelName => default;

        /// <summary>
        /// [장비] 속성 레벨
        /// [재료] 속성석 레벨
        /// </summary>
        public virtual int ElementLevel => -1;

        /// <summary>
        /// 속성석 여부
        /// </summary>
        public virtual bool IsElementStone => default;

        /// <summary>
        /// [공통] 도감 타입
        /// </summary>
        public BookType BookType => data.BookType;

        /// <summary>
        /// [재료] 어둠의 나무 포인트
        /// </summary>
        public virtual int DarkTreePoint => 0;

        /// <summary>
        /// [재료] 큐펫 경험치 포인트
        /// </summary>
        public virtual int CupetExpPoint => 0;

        /// <summary>
        /// [재료] 길드전 버프 포인트
        /// </summary>
        public virtual int GuildBattleBuffExpPoint => 0;

        public IEnumerator<BattleOption> GetEnumerator()
        {
            return GetBattleOptionCollection().GetEnumerator();
        }

        IEnumerator<CardBattleOption> IEnumerable<CardBattleOption>.GetEnumerator()
        {
            return GetCardBattleOptionCollection().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ItemInfo ShallowCopy()
        {
            return (ItemInfo)this.MemberwiseClone();
        }

        public void SetNew(bool isNew)
        {
            itemIsNew = isNew;
        }

        private string GetItemDescription()
        {
            if (GameServerConfig.IsKorea())
            {
                BasisUrl basisUrl = BasisDetailDataManager.Instance.GetItemDescUrl(ItemId);
                return basisUrl.AppendText(GetDescription(), useColor: true);
            }

            return GetDescription();
        }

        protected virtual string GetDescription()
        {
            // 거래 가능
            if (CanTrade)
                return data.des_id.ToText();

            return StringBuilderPool.Get()
                .Append(data.des_id.ToText())
                .Append(LocalizeKey._30508.ToText()) // [c][cdcdcd] (거래 불가)[-][/c]
                .Release();
        }

        /// <summary>
        /// 속성 표시되는 레벨
        /// </summary>
        public string GetElementLevelText()
        {
            int elementLevel = ElementLevel;
            if (elementLevel < 0)
                return string.Empty;

            return (elementLevel + 1).ToString(); // 값을 추가하여 보여줌
        }

        public virtual int GetMaxCardSlot()
        {
            return Constants.Size.MAX_EQUIPPED_CARD_COUNT;
        }

        public struct Status
        {
            /// <summary>
            /// 물리공격력/마법공격력/물리방어력/마법방어력
            /// </summary>
            public readonly int atk, matk, def, mdef;

            public Status(int atk, int matk, int def, int mdef)
            {
                this.atk = atk;
                this.matk = matk;
                this.def = def;
                this.mdef = mdef;
            }

            public IEnumerable<BattleOption> GetBattleOptions()
            {
                if (atk != 0)
                    yield return new BattleOption(BattleOptionType.Atk, atk, 0);

                if (matk != 0)
                    yield return new BattleOption(BattleOptionType.MAtk, matk, 0);

                if (def != 0)
                    yield return new BattleOption(BattleOptionType.Def, def, 0);

                if (mdef != 0)
                    yield return new BattleOption(BattleOptionType.MDef, mdef, 0);
            }

            public static Status operator +(Status a, Status b)
            {
                int atk = (a.atk + b.atk);
                int matk = (a.matk + b.matk);
                int def = (a.def + b.def);
                int mdef = (a.mdef + b.mdef);
                return new Status(atk, matk, def, mdef);
            }
        }
    }
}