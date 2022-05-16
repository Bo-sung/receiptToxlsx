using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ItemDataManager : Singleton<ItemDataManager>, IDataManger, ItemDataManager.IItemDataRepoImpl
    {
        public interface IItemDataRepoImpl
        {
            ItemData Get(int id);
        }

        private readonly Dictionary<ObscuredInt, ItemData> dataDic;
        private readonly List<ItemData> monsterPieceItems;
        private readonly Buffer<ItemData> buffer;

        public ResourceType DataType => ResourceType.ItemDataDB;

        public IEnumerable<ItemData> EntireItems => dataDic.Values;

        public ItemDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, ItemData>(ObscuredIntEqualityComparer.Default);
            monsterPieceItems = new List<ItemData>();
            buffer = new Buffer<ItemData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataDic.Clear();
            monsterPieceItems.Clear();
            buffer.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ItemData data = new ItemData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);

                    // 몬스터조각 따로 관리
                    if (data.ItemGroupType == ItemGroupType.MonsterPiece)
                        monsterPieceItems.Add(data);
                }
            }
        }

        public ItemData Get(int id)
        {
            if (id == 0)
                return null;

            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"아이템 데이터가 존재하지 않습니다: id = {id}");
                return null;
            }

            return dataDic[id];
        }

        /// <summary>
        /// 몬스터조각 아이디
        /// </summary>
        public List<ItemData> GetMonsterPieceItems()
        {
            return monsterPieceItems;
        }

        /// <summary>
        /// 해당 상자 아이템을 반환한다.
        /// </summary>
        public ItemData GetBoxItem(int boxId)
        {
            foreach (var item in dataDic.Values)
            {
                if (item.ItemType != ItemType.Box)
                    continue;

                if (item.event_id == boxId)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 속성석 반환
        /// </summary>
        public ItemData GetElementStone(ElementType elementType, int elementLevel = 0)
        {
            int elementTypeValue = (int)elementType;
            foreach (var item in dataDic.Values)
            {
                if (item.element_type == elementTypeValue && item.GetElementStoneLevel() == elementLevel)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 길드전 버프 재료 반환
        /// </summary>
        public ItemData[] GetGuildBattleBuffMaterial()
        {
            foreach (var item in dataDic.Values)
            {
                if (item.ItemType != ItemType.ProductParts)
                    continue;

                if (item.matk_max == 0)
                    continue;

                buffer.Add(item);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 큐펫 경험치 재료 반환
        /// </summary>
        public ItemData[] GetCupetExpMaterial()
        {
            foreach (var item in dataDic.Values)
            {
                if (item.ItemType != ItemType.ProductParts)
                    continue;

                if (item.matk_min == 0)
                    continue;

                buffer.Add(item);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                ItemGroupType itemGroupType = item.ItemGroupType;
                ItemType itemType = item.ItemType;

                switch (itemGroupType)
                {
                    case ItemGroupType.None:
                        throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 타입 미설정 {nameof(itemGroupType)} {itemGroupType}");

                    case ItemGroupType.Equipment:
                        {
                            if (itemType != ItemType.Equipment)
                            {
                                throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 타입 매칭 오류 {nameof(itemGroupType)}={itemGroupType}, {nameof(itemType)}={itemType}");
                            }
                            break;
                        }
                    case ItemGroupType.Card:
                        {
                            if (itemType != ItemType.Card)
                            {
                                throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 타입 매칭 오류 {nameof(itemGroupType)}={itemGroupType}, {nameof(itemType)}={itemType}");
                            }
                            break;
                        }
                    case ItemGroupType.ProductParts:
                        {
                            if (itemType != ItemType.ProductParts && itemType != ItemType.CardSmeltMaterial && itemType != ItemType.ShareVice)
                            {
                                throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 타입 매칭 오류 {nameof(itemGroupType)}={itemGroupType}, {nameof(itemType)}={itemType}");
                            }
                            break;
                        }
                    case ItemGroupType.ConsumableItem:
                        {
                            if (itemType != ItemType.ConsumableItem && itemType != ItemType.Box)
                            {
                                throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 타입 매칭 오류 {nameof(itemGroupType)}={itemGroupType}, {nameof(itemType)}={itemType}");
                            }
                            break;
                        }
                    case ItemGroupType.Costume:
                        {
                            if (itemType != ItemType.Costume)
                            {
                                throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 타입 매칭 오류 {nameof(itemGroupType)}={itemGroupType}, {nameof(itemType)}={itemType}");
                            }
                            break;
                        }
                    case ItemGroupType.MonsterPiece:
                        {
                            if (itemType != ItemType.MonsterPiece)
                            {
                                throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 타입 매칭 오류 {nameof(itemGroupType)}={itemGroupType}, {nameof(itemType)}={itemType}");
                            }
                            break;
                        }
                }

                switch (itemGroupType)
                {
                    case ItemGroupType.Equipment:
                    case ItemGroupType.ConsumableItem:
                        {
                            BattleOptionType battleOptionType = item.battle_option_type_1.ToEnum<BattleOptionType>();
                            if (battleOptionType != BattleOptionType.None)
                            {
                                if (item.value1_b1 == 0 && item.value2_b1 == 0)
                                {
                                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 배틀옵션1 값 미설정");
                                }
                                if (item.value1_b1 != 0 && item.value2_b1 != 0 && !battleOptionType.IsConditionalOption())
                                {
                                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 배틀옵션1 Value2 안쓰는 타입에 값 설정");
                                }
                            }

                            battleOptionType = item.battle_option_type_2.ToEnum<BattleOptionType>();
                            if (battleOptionType != BattleOptionType.None)
                            {
                                if (item.value1_b2 == 0 && item.value2_b2 == 0)
                                {
                                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 배틀옵션2 값 미설정");
                                }
                                if (item.value1_b2 != 0 && item.value2_b2 != 0 && !battleOptionType.IsConditionalOption())
                                {
                                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 배틀옵션2 Value2 안쓰는 타입에 값 설정");
                                }
                            }

                            battleOptionType = item.battle_option_type_3.ToEnum<BattleOptionType>();
                            if (battleOptionType != BattleOptionType.None)
                            {
                                if (item.value1_b3 == 0 && item.value2_b3 == 0)
                                {
                                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 배틀옵션3 값 미설정");
                                }
                                if (item.value1_b3 != 0 && item.value2_b3 != 0 && !battleOptionType.IsConditionalOption())
                                {
                                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 배틀옵션3 Value2 안쓰는 타입에 값 설정");
                                }
                            }

                            battleOptionType = item.battle_option_type_4.ToEnum<BattleOptionType>();
                            if (battleOptionType != BattleOptionType.None)
                            {
                                if (item.value1_b4 == 0 && item.value2_b4 == 0)
                                {
                                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 배틀옵션4 값 미설정");
                                }
                                if (item.value1_b4 != 0 && item.value2_b4 != 0 && !battleOptionType.IsConditionalOption())
                                {
                                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 배틀옵션4 Value2 안쓰는 타입에 값 설정");
                                }
                            }
                        }
                        break;
                    case ItemGroupType.Card:
                        {
                            VerifyCardOption(item.battle_option_type_1);
                            VerifyCardOption(item.battle_option_type_2);
                            VerifyCardOption(item.battle_option_type_3);
                            VerifyCardOption(item.battle_option_type_4);
                        }
                        break;
                }

                ItemSourceCategoryType getItemSourceCategoryType = item.item_get_bit_type.ToEnum<ItemSourceCategoryType>(); // 아이템 획득처
                ItemSourceCategoryType useItemSourceCategoryType = item.item_use_bit_type.ToEnum<ItemSourceCategoryType>(); // 아이템 사용처                

                if (getItemSourceCategoryType.HasUseItemSourceCategoryType())
                {
                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id},[{item.name_id.ToText()}] 획득처에 사용처 타입이 있음{getItemSourceCategoryType}");
                }
                if (useItemSourceCategoryType.HasGetItemSourceCategoryType())
                {
                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id},[{item.name_id.ToText()}] 사용처에 획득처 타입이 있음{useItemSourceCategoryType}");
                }
                if (getItemSourceCategoryType.HasObsoleteItemSourceCategoryType())
                {
                    throw new System.Exception($"10.아이템 테이블 오류 ID={item.id},[{item.name_id.ToText()}] 획득처에 사용하지 않는 타입이 있음{getItemSourceCategoryType}");
                }

                List<ItemSourceDetailData> sourceDatas = new List<ItemSourceDetailData>();
                BoxData[] refGachaBoxes = BoxDataManager.Instance.GetRefGachaTypeBoxes();

                if (getItemSourceCategoryType.HasFlag(ItemSourceCategoryType.StageDrop))
                {
                    sourceDatas.Clear();

                    StageData[] stageDatas = StageDataManager.Instance.GetStagesCanDropItem(item.id);
                    foreach (var stageData in stageDatas)
                    {
                        string stageName = stageData.name_id.ToText();
                        int stageId = stageData.id;
                        sourceDatas.Add(new ItemSourceDetailData(getItemSourceCategoryType, text: stageName, value_1: stageId));
                    }

                    if (sourceDatas.Count == 0)
                    {
                        throw new System.Exception($"10.아이템 테이블 오류 ID={item.id},[{item.name_id.ToText()}] 스테이지에 드롭 아이템이 없다.");
                    }
                }

                // 획득처 중 상자 타입 포함인 아이템
                if (getItemSourceCategoryType.HasFlag(ItemSourceCategoryType.Box))
                {
                    sourceDatas.Clear();

                    // (아이템이 들어있는 상자)
                    BoxData[] boxDatas = BoxDataManager.Instance.GetBoxesDropItem(item.id);
                    foreach (var boxData in boxDatas)
                    {
                        BoxType boxtype = boxData.box_type.ToEnum<BoxType>();
                        if (!boxtype.IsItemSourceCategoryType())
                            continue;

                        BoxItemInfo boxItemInfo = new BoxItemInfo();
                        ItemData boxItemData = GetBoxItem(boxData.id);
                        if (boxItemData == null)
                            throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 박스 테이블 ID={boxData.id} 획득처 오류, 상자에 아이템이 없다");

                        boxItemInfo.SetData(boxItemData);
                        sourceDatas.Add(new ItemSourceDetailData(getItemSourceCategoryType, itemInfo: boxItemInfo));
                    }

                    GachaData[] gachaDatas = GachaDataManager.Instance.GetGachasDropItem(item.id);
                    foreach (var gachaData in gachaDatas)
                    {
                        if (gachaData.group_type != GroupType.List.ToIntValue()) // 가챠테이블 1만 사용
                            continue;

                        BoxItemInfo boxItemInfo = FindGachaBoxItem(gachaData);
                        if (boxItemInfo == null)
                            continue;

                        if (!boxItemInfo.BoxType.IsItemSourceCategoryType())
                            continue;

                        sourceDatas.Add(new ItemSourceDetailData(getItemSourceCategoryType, itemInfo: boxItemInfo));
                    }

                    // 중복 제거
                    sourceDatas = sourceDatas.Distinct().ToList();

                    if (sourceDatas.Count == 0)
                    {
                        throw new System.Exception($"10.아이템 테이블 오류 ID={item.id},[{item.name_id.ToText()}] 획득처 상자에 아이템이 없다.");
                    }
                }

                if (getItemSourceCategoryType.HasFlag(ItemSourceCategoryType.UseMake))
                {
                    sourceDatas.Clear();

                    MakeData[] makeDatas = MakeDataManager.Instance.GetMakesNeedThisItem(item.id);
                    foreach (MakeData makeData in makeDatas)
                    {
                        ItemData itemData = Get(makeData.result);

                        if (itemData == null)
                            throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 없는 제작아이템 = {makeData.result}");

                        // 타입별로 아이템 인포 생성
                        ItemInfo retItemInfo = null;

                        switch (itemData.ItemGroupType)
                        {
                            case ItemGroupType.Equipment:
                                {
                                    EquipmentItemInfo equipmentItemInfo = new EquipmentItemInfo(Entity.player.Inventory);
                                    equipmentItemInfo.SetData(itemData);
                                    retItemInfo = equipmentItemInfo;
                                }
                                break;
                            case ItemGroupType.Card:
                                {
                                    CardItemInfo cardItemInfo = new CardItemInfo();
                                    cardItemInfo.SetData(itemData);
                                    retItemInfo = cardItemInfo;
                                }
                                break;
                            case ItemGroupType.ProductParts:
                                {
                                    PartsItemInfo partsItemInfo = new PartsItemInfo();
                                    partsItemInfo.SetData(itemData);
                                    retItemInfo = partsItemInfo;
                                }
                                break;
                            case ItemGroupType.ConsumableItem:
                                {
                                    if (itemData.ItemType == ItemType.Box)
                                    {
                                        BoxItemInfo boxItemInfo = new BoxItemInfo();
                                        boxItemInfo.SetData(itemData);
                                        retItemInfo = boxItemInfo;
                                    }
                                    else
                                    {
                                        ConsumableItemInfo consumableItemInfo = new ConsumableItemInfo();
                                        consumableItemInfo.SetData(itemData);
                                        retItemInfo = consumableItemInfo;
                                    }
                                }
                                break;

                            default:
                                throw new System.Exception($"10.아이템 테이블 오류 ID={item.id},[{item.name_id.ToText()}] 사용처(제작) 잘못된 타입 {itemData.id}, {itemData.name_id.ToText()}, {itemData.ItemGroupType} ({itemData.ItemType})");
                        }

                        sourceDatas.Add(new ItemSourceDetailData(getItemSourceCategoryType, itemInfo: retItemInfo));
                    }

                    if (sourceDatas.Count == 0)
                    {
                        throw new System.Exception($"10.아이템 테이블 오류 ID={item.id},[{item.name_id.ToText()}] 획득처 제작재료 아이템이 없다.");
                    }
                }

                BoxItemInfo FindGachaBoxItem(GachaData gachaData)
                {
                    // gacha.id == box.value_n (type_n == 99)
                    // box.value_n == item.event_id (item_type == 2)

                    int boxId = default;
                    foreach (var gachaBox in refGachaBoxes)
                    {
                        if (gachaBox.ContainsGachaBox(gachaData.group_id))
                        {
                            boxId = gachaBox.id;
                        }
                    }

                    if (boxId == default)
                        return null;


                    ItemData boxItemData = GetBoxItem(boxId);
                    if (boxItemData == null)
                    {
                        return null;
                    }

                    BoxItemInfo boxInfo = new BoxItemInfo();
                    boxInfo.SetData(boxItemData);

                    return boxInfo;
                }

                void VerifyCardOption(int id)
                {
                    if (id == 0)
                        return;

                    var option = CardOptionDataManager.Instance.Get(id);
                    if (option == null)
                    {
                        throw new System.Exception($"10.아이템 테이블 오류 ID={item.id}, 없는 카드 옵션 = {id}");
                    }
                    else
                    {
                        BattleOptionType battleOptionType = option.battle_option_type.ToEnum<BattleOptionType>();
                        if (battleOptionType != BattleOptionType.None)
                        {
                            if (option.value_1_prob == 0 && option.value_2_prob == 0)
                            {
                                throw new System.Exception($"42.카드옵션 테이블 오류 ID={option.id}, 배틀옵션 값 미설정");
                            }

                            if (option.value_1_prob != 0 && option.option_value_1_rate == 0)
                            {
                                throw new System.Exception($"42.카드옵션 테이블 오류 ID={option.id}, {nameof(option.option_value_1_rate)} 값이 없음");
                            }
                            if (option.value_2_prob != 0 && option.option_value_2_rate == 0)
                            {
                                throw new System.Exception($"42.카드옵션 테이블 오류 ID={option.id}, {nameof(option.option_value_2_rate)} 값이 없음");
                            }
                            if (battleOptionType.IsConditionalOption() && option.option_value_2_rate == 0)
                            {
                                throw new System.Exception($"42.카드옵션 테이블 오류 ID={option.id}, {nameof(option.option_value_2_rate)} 값이 없음");
                            }
                            if (option.marking_level == 1)
                            {
                                throw new System.Exception($"42.카드옵션 테이블 오류 ID={option.id}, {nameof(option.marking_level)}은 0 또는 1초과 값이어야함");
                            }
                        }
                    }
                }
            }
#endif
        }
    }
}