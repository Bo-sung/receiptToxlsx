using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <see cref="UIItemSourceDetail"/>
    public class ItemSourceDetailPresenter : ViewPresenter, IEqualityComparer<ItemSourceDetailData>/*, IComparer<ItemSourceDetailData>*/
    {
        public interface IView
        {
        }

        IView view;
        StageDataManager stageDataRepo;
        ItemDataManager itemDataRepo;
        BoxDataManager boxDataRepo;
        GachaDataManager gachaDataRepo;
        MakeDataManager makeDataRepo;
        InventoryModel invenModel;
        DungeonModel dungeonModel;

        /// <summary>
        /// RewardType이 RefGacha(99)인 상자 리스트
        /// </summary>
        BoxData[] refGachaBoxes;

        private bool isShortcutStart;
        public int ItemId { get; private set; }

        public ItemSourceDetailPresenter(IView view)
        {
            this.view = view;

            stageDataRepo = StageDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            boxDataRepo = BoxDataManager.Instance;
            gachaDataRepo = GachaDataManager.Instance;
            makeDataRepo = MakeDataManager.Instance;
            invenModel = Entity.player.Inventory;
            dungeonModel = Entity.player.Dungeon;

            refGachaBoxes = boxDataRepo.GetRefGachaTypeBoxes();
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isShortcutStart)
            {
                isShortcutStart = false;
                UI.Close<UIItemSourceDetail>();
            }
        }

        public ItemSourceDetailData[] GetDetailSlotData(UIItemSourceDetail.Input input)
        {
            ItemId = input.itemInfo.ItemId;
            List<ItemSourceDetailData> sourceDatas = new List<ItemSourceDetailData>();

            switch (input.categoryType)
            {
                case ItemSourceCategoryType.StageDrop:
                    // 스테이지 테이블
                    StageData[] stageDatas = stageDataRepo.GetStagesCanDropItem(input.itemInfo.ItemId);
                    foreach (var stageData in stageDatas)
                    {
                        string stageName = stageData.name_id.ToText();
                        int stageId = stageData.id;
                        sourceDatas.Add(new ItemSourceDetailData(input.categoryType, text: stageName, value_1: stageId));
                    }

                    //// 스테이지 내림차순 정렬
                    //sourceDatas.Sort(this);

                    break;

                case ItemSourceCategoryType.Box:
                    // 상자,가챠 테이블

                    // 상자 테이블 (아이템이 들어있는 상자)
                    BoxData[] boxDatas = boxDataRepo.GetBoxesDropItem(input.itemInfo.ItemId);
                    foreach (var boxData in boxDatas)
                    {
                        BoxType boxtype = boxData.box_type.ToEnum<BoxType>();
                        if (!boxtype.IsItemSourceCategoryType())
                            continue;

                        BoxItemInfo boxItemInfo = new BoxItemInfo();
                        ItemData boxItemData = itemDataRepo.GetBoxItem(boxData.id);

                        if (boxItemData == null)
                        {
                            Debug.LogError($"Box Item Data를 찾지 못했다. {boxData.id}");
                            continue;
                        }

                        boxItemInfo.SetData(boxItemData);
                        sourceDatas.Add(new ItemSourceDetailData(input.categoryType, itemInfo: boxItemInfo));
                    }

                    // 가챠
                    GachaData[] gachaDatas = gachaDataRepo.GetGachasDropItem(input.itemInfo.ItemId);
                    foreach (var gachaData in gachaDatas)
                    {
                        if (gachaData.group_type != GroupType.List.ToIntValue()) // 가챠테이블 1만 사용
                            continue;

                        BoxItemInfo boxItemInfo = FindGachaBoxItem(gachaData);
                        if (boxItemInfo == null)
                            continue;

                        if (!boxItemInfo.BoxType.IsItemSourceCategoryType())
                            continue;

                        sourceDatas.Add(new ItemSourceDetailData(input.categoryType, itemInfo: boxItemInfo));
                    }

                    // 중복 제거
                    sourceDatas = sourceDatas.Distinct(this).ToList();
                    break;

                case ItemSourceCategoryType.UseMake:
                    // 제작 테이블 (제작 재료)
                    MakeData[] makeDatas = makeDataRepo.GetMakesNeedThisItem(input.itemInfo.ItemId);
                    foreach (MakeData makeData in makeDatas)
                    {
                        ItemData itemData = itemDataRepo.Get(makeData.result);

                        if (itemData == null)
                        {
                            Debug.LogError($"잘못된 Result ID : {makeData.result}");
                            continue;
                        }

                        // 타입별로 아이템 인포 생성
                        ItemInfo retItemInfo = null;
                        switch (itemData.ItemGroupType)
                        {                           
                            case ItemGroupType.Equipment:
                                {
                                    EquipmentItemInfo equipmentItemInfo = new EquipmentItemInfo(invenModel);
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
                                    if(itemData.ItemType == ItemType.Box)
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
                            case ItemGroupType.Costume:
                                {
                                    CostumeItemInfo costumeItemInfo = new CostumeItemInfo();
                                    costumeItemInfo.SetData(itemData);
                                    retItemInfo = costumeItemInfo;
                                }
                                break;

                            default:
                                Debug.LogError($"잘못된 타입 : {itemData.ItemGroupType} ({itemData.ItemType})");
                                break;
                        }                  
                       

                        sourceDatas.Add(new ItemSourceDetailData(input.categoryType, itemInfo: retItemInfo));
                    }
                    break;
            }


            return sourceDatas.ToArray();
        }

        /// <summary>
        /// GachaData로 해당 상자 아이템의 ItemInfo 찾아서 반환.
        /// </summary>
        private BoxItemInfo FindGachaBoxItem(GachaData gachaData)
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


            ItemData boxItemData = itemDataRepo.GetBoxItem(boxId);
            if (boxItemData == null)
            {
                return null;
            }

            BoxItemInfo boxInfo = new BoxItemInfo();
            boxInfo.SetData(boxItemData);

            return boxInfo;
        }

        /// <summary>
        /// 텍스트만 보여주는 리스트인지 체크.
        /// </summary>
        public bool IsTextView(ItemSourceCategoryType type)
        {
            switch (type)
            {
                case ItemSourceCategoryType.StageDrop:
                case ItemSourceCategoryType.DungeonDrop:
                case ItemSourceCategoryType.WorldBoss:
                case ItemSourceCategoryType.EndlessTower:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 해당 스테이지로 이동
        /// </summary>
        public void GoToStage(int stageId)
        {
            if (Entity.player.Dungeon.LastEnterStageId == stageId)
            {
                UIManager.Instance.ShortCut();
                return;
            }

            StartStage(stageId).WrapNetworkErrors();
        }

        /// <summary>
        /// 해당 던전으로 이동
        /// </summary>
        public void GoToDungeon(int dungeonId)
        {

        }

        /// <summary>
        /// 스테이지 오픈 여부
        /// </summary>
        public bool IsStageOpend(int stageId)
        {
            return dungeonModel.IsStageOpend(stageId);
        }

        /// <summary>
        /// 스테이지 진행중 여부
        /// </summary>
        public bool IsPlayingStage(int stageId)
        {
            return dungeonModel.LastEnterStageId == stageId;
        }

        private async Task StartStage(int stageId)
        {
            isShortcutStart = true;            
            dungeonModel.StartBattleStageMode(StageMode.Normal, stageId);
        }

        /// <summary>
        /// 아이템 정보 팝업
        /// </summary>
        public void ShowBoxItemInfo(BoxItemInfo boxItemInfo)
        {
            // 상세창 2종 닫기
            UI.Close<UIItemSourceDetail>();
            UI.Close<UIItemSource>();

            // 아이템 정보 창 닫기
            UI.Close<UIEquipmentInfo>();
            UI.Close<UIEquipmentInfoSimple>();
            UI.Close<UIConsumableInfo>();
            UI.Close<UIPartsInfo>();
            UI.Close<UICardInfo>();
            UI.Close<UICardInfoSimple>();
            UI.Close<UICostumeInfo>();

            // 상자 정보 띄우기. (사용 버튼이 뜨지 않게 하기 위해 CoolTime을 Max로 둔다.)
            boxItemInfo.SetRemainCoolDown(float.MaxValue);
            UI.Show<UIConsumableInfo>(boxItemInfo);
        }

        /// <summary>
        /// 해당 아이템의 제작UI로 이동
        /// </summary>
        public void GoToMake(int itemId, int materialItemId)
        {
            UI.ShortCut<UIMake>(new UIMake.Input(itemId, materialItemId));
        }

        /// <summary>
        /// 갈 수 있는 스테이지인지 체크.
        /// </summary>
        public bool IsOpenedStage(int stageId)
        {
            return Entity.player.Dungeon.IsStageOpend(stageId);
        }

        /// <summary>
        /// 스테이지 목록에서 내가 갈 수 있는 최상위 스테이지의 Progress 반환
        /// (datas가 내림차순 데이터라는 가정이 있음.)
        /// </summary>
        public float GetProgressHighestStage(ItemSourceDetailData[] datas)
        {
            int index = 0;
            foreach (var data in datas)
            {
                if (IsOpenedStage(data.value_1))
                {
                    return (index + 7) / (float)datas.Length; // 한 페이지에 8개 목록이 보이므로 7을 더해준다.
                }
                index++;
            }
            return 1f;
        }

        bool IEqualityComparer<ItemSourceDetailData>.Equals(ItemSourceDetailData x, ItemSourceDetailData y)
        {
            if (x.categoryType == ItemSourceCategoryType.DungeonDrop) // 던전 : 동일한 이름의 던전은 중복 제거
            {
                return x.text.Equals(y.text);
            }
            else if (x.categoryType == ItemSourceCategoryType.Box) // 상자 : 동일한 상자는 중복 제거
            {
                return x.itemInfo.ItemId.Equals(y.itemInfo.ItemId);
            }

            return default;
        }

        int IEqualityComparer<ItemSourceDetailData>.GetHashCode(ItemSourceDetailData obj)
        {
            return GetHashCode();
        }

        //int IComparer<ItemSourceDetailData>.Compare(ItemSourceDetailData x, ItemSourceDetailData y)
        //{
        //    if (x.categoryType == ItemSourceCategoryType.StageDrop) // 스테이지 : 내림차순
        //    {
        //        return x.value_1.CompareTo(y.value_1) * -1;
        //    }

        //    return 0;
        //}
    }
}