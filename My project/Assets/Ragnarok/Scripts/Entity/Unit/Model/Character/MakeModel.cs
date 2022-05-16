using Sfs2X.Entities.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class MakeModel : CharacterEntityModel
    {
        private MakeDataManager makeDataRepo;
        private ItemDataManager itemDataRepo;

        /// <summary>
        /// 제작목록
        /// </summary>
        public readonly List<MakeInfo> makeList;
        /// <summary>
        ///  선택한 장비 정보
        /// </summary>
        public readonly Dictionary<int, List<ItemInfo>> selectItemDict;
        /// <summary>
        /// 제작 결과 정보 목록
        /// </summary>
        private readonly List<MakeResultInfo> makeResultInfos;
        /// <summary>
        /// 제작 성공 수
        /// </summary>
        public int successCount { get; private set; }
        /// <summary>
        /// 제작 실패 수
        /// </summary>
        public int failCount { get; private set; }
        public int MakeCount { get; private set; }

        public MakeModel()
        {
            makeDataRepo = MakeDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            makeList = new List<MakeInfo>();
            selectItemDict = new Dictionary<int, List<ItemInfo>>();
            makeResultInfos = new List<MakeResultInfo>();
            MakeCount = 1;
        }

        public override void AddEvent(UnitEntityType type)
        {

        }

        public override void RemoveEvent(UnitEntityType type)
        {

        }

        internal void Initialize()
        {
            foreach (var info in makeList)
            {
                if (info.Reward != null)
                {
                    if (info != null && info.EnableType == EnableType.Enable)
                    {
                        if (info.MaterialInfos != null)
                        {
                            for (int i = 0; i < info.MaterialInfos.Length; i++)
                            {
                                Entity.Inventory.RemoveItemEvent(info.MaterialInfos[i].ItemId, info.SetItemCount);
                            }
                        }
                        Entity.Goods.OnUpdateZeny -= info.CheckZeny;
                    }
                }
            }

            makeList.Clear();

            // 테이블 수 만큼 제작 목록을 만들을 준다
            foreach (var item in makeDataRepo.GetList())
            {
                if (item.Value.enable_type == 2)
                {
                    MakeInfo info = new MakeInfo(Entity.Inventory);
                    info.SetData(item.Value);
                    info.SetItemCount();
                    for (int i = 0; i < info.MaterialInfos.Length; i++)
                    {
                        Entity.Inventory.AddItemEvent(info.MaterialInfos[i].ItemId, info.SetItemCount);
                    }
                    Entity.Goods.OnUpdateZeny += info.CheckZeny;

                    makeList.Add(info);
                }
                else
                {
                    MakeInfo info = new MakeInfo(Entity.Inventory);
                    info.SetData(item.Value);
                    makeList.Add(info);
                }
            }
        }

        /// <summary>
        /// 제작 메인 탭 인덱스
        /// </summary>
        public int MainTab { get; private set; }
        /// <summary>
        /// 제작 서브 탭 인덱스
        /// </summary>
        public int SubTab { get; private set; }
        /// <summary>
        /// 제작에서 선택한 아이템 정보
        /// </summary>
        public MakeInfo SelectMakeInfo { get; private set; }

        /// <summary>
        /// 선택된 재료(장비) 정보
        /// </summary>
        public MaterialInfo SelectMaterialInfo { get; private set; }

        public void SetUIMake(int mainTab, int subTab)
        {
            MainTab = mainTab;
            SubTab = subTab;
        }

        /// <summary>
        /// 제작아이템 선택
        /// </summary>
        /// <param name="info"></param>
        public void SetSelectMakeInfo(MakeInfo info)
        {
            SelectMakeInfo = info;
            ResetSelectItemInfo();
        }

        /// <summary>
        /// 재료(장비) 선택
        /// </summary>
        /// <param name="info"></param>
        public void SetSelectMaterialInfo(MaterialInfo info)
        {
            SelectMaterialInfo = info;
        }

        public void ResetSelectItemInfo()
        {
            selectItemDict.Clear();
        }

        /// <summary>
        /// 선택된 재료 아이템 수
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        public int GetSelectItemCount(int slotIndex)
        {
            if (!selectItemDict.ContainsKey(slotIndex))
                return 0;

            return selectItemDict[slotIndex].Count;
        }

        /// <summary>
        /// 선택된 재료인지 여부
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool IsSelect(ItemInfo info)
        {
            var index = SelectMaterialInfo.SlotIndex;
            if (!selectItemDict.ContainsKey(index))
                return false;

            if (selectItemDict[index].Contains(info))
                return true;

            return false;
        }

        /// <summary>
        /// 재료 선택 토글
        /// </summary>
        /// <param name="info"></param>
        public void SelectItemInfo(ItemInfo info)
        {
            var index = SelectMaterialInfo.SlotIndex;

            if (IsSelect(info))
            {
                selectItemDict[index].Remove(info);
                return;
            }

            // 잠금, 장착중인 아이템 선택 불가
            if (info.IsLock || info.IsEquipped)
            {
                return;
            }

            // 카드가 장착 되어있는 아이템은 선택 불가
            for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; ++i)
            {
                ItemInfo card = info.GetCardItem(i);
                if (card != null)
                {
                    return;
                }
            }

            if (!selectItemDict.ContainsKey(index))
            {
                selectItemDict.Add(index, new List<ItemInfo>() { info });
            }
            else if (selectItemDict[index].Count < SelectMaterialInfo.Count)
            {
                selectItemDict[index].Add(info);
            }
        }

        public MakeInfo[] GetMakeInfos(int mainTab, int subTab)
        {
            return makeList.FindAll(a => a.MainTab == mainTab && a.SubTab == subTab).ToArray();
        }

        /// <summary>
        /// 특정 아이템의 탭 정보를 반환
        /// </summary>
        public (int mainTab, int subTab) GetMakeTab(int itemId, int materialId)
        {
            MakeInfo makeInfo = GetMakeInfo(itemId, materialId);

            if (makeInfo == null)
            {
                return (-1, -1);
            }
            else
            {
                return (makeInfo.MainTab, makeInfo.SubTab);
            }
        }

        public MakeInfo GetMakeInfo(int itemId, int materialId)
        {
            if (makeList.Exists(e => e.EnableType == EnableType.Enable && e.ResultID == itemId))
            {
                // 동일한 제작템이 여러개 있으면, 탭의 인덱스가 낮은쪽 기준으로 보여줌.
                List<MakeInfo> makeInfos = makeList.FindAll(e => e.EnableType == EnableType.Enable && e.ResultID == itemId && (materialId == 0 ? true : e.HasMaterial(materialId)));

                MakeInfo makeInfo = null;
                int minTab = int.MaxValue;

                foreach (var item in makeInfos)
                {
                    if (item.MainTab < minTab)
                    {
                        minTab = item.MainTab;
                        makeInfo = item;
                    }
                }

                return makeInfo;
            }
            else
            {
                return null;
            }
        }

        public bool IsMakeTab(int mainTab)
        {
            var make = makeList.FindAll(a => a.MainTab == mainTab && a.IsMake);
            return make != null && make.Count > 0;
        }

        public bool IsMakeSubTab(int mainTab, int subTab)
        {
            var make = makeList.FindAll(a => a.MainTab == mainTab && a.IsMake);
            if (make == null || make.Count == 0)
                return false;

            var subMake = make.FindAll(a => a.SubTab == subTab);
            return subMake != null && subMake.Count > 0;
        }

        public bool IsMake()
        {
            var make = makeList.FindAll(a => a.IsMake);
            return make != null && make.Count > 0;
        }

        public void SetMakeCount(int count)
        {
            MakeCount = count;
        }

        /// <summary>
        /// 아이템 제작
        /// </summary>
        public async Task<bool> RequestMakeItem(MakeInfo info, int count)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.ID);

            // 제작시 장비가 들어간 경우
            if (!info.IsStackable)
            {
                Dictionary<int, List<long>> itemDict = new Dictionary<int, List<long>>();
                foreach (var list in selectItemDict.OrEmptyIfNull())
                {
                    foreach (var item in list.Value.OrEmptyIfNull())
                    {
                        if (!itemDict.ContainsKey(item.ItemId))
                        {
                            itemDict.Add(item.ItemId, new List<long>());
                        }
                        itemDict[item.ItemId].Add(item.ItemNo);
                    }
                }

                if (itemDict.Count > 0)
                {
                    SFSArray array = new SFSArray();

                    foreach (var item in itemDict)
                    {
                        SFSObject sfsObject = new SFSObject();
                        sfsObject.PutInt("1", item.Key);
                        sfsObject.PutLongArray("2", item.Value.ToArray());
                        array.AddSFSObject(sfsObject);
                    }

                    sfs.PutSFSArray("2", array);
                }
                count = 1;
            }
            else
            {
                // 제작 아이템 수량
                sfs.PutInt("3", count);
            }

            var response = await Protocol.MAKE_ITEM.SendAsync(sfs);
            if (response.isSuccess)
            {
                ResetSelectItemInfo();

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    // 제작 결과 정보 세팅
                    successCount = charUpdateData.rewards == null ? 0 : charUpdateData.rewards.Length;
                    failCount = count - successCount;

                    makeResultInfos.Clear();
                    for (int i = 0; i < successCount; i++)
                    {
                        var resultInfo = new MakeResultInfo(isSuccess: true);
                        resultInfo.SetData(info.Reward);
                        makeResultInfos.Add(resultInfo);
                    }

                    for (int i = 0; i < failCount; i++)
                    {
                        var resultInfo = new MakeResultInfo(isSuccess: false);
                        resultInfo.SetData(info.Reward);
                        makeResultInfos.Add(resultInfo);
                    }

                    makeResultInfos.Shuffle();

                    // 제작 결과 연출
                    UI.Show<UIMakeResult>();
                }

                // 퀘스트 처리
                int itemId = makeDataRepo.Get(info.ID).result;
                ItemData itemData = itemDataRepo.Get(itemId);
                ItemType itemType = itemData.ItemType;
                int rating = itemData.rating;

                Quest.QuestProgress(QuestType.ITEM_MAKING, questValue: MakeCount); // 아이템 제작 횟수
                if (itemType == ItemType.Equipment)
                {
                    Quest.QuestProgress(QuestType.ITEM_STAR_UPGRADE, conditionValue: rating); // 장비템 특정 성급 제작 횟수
                }
                Quest.QuestProgress(QuestType.ITEM_MAKING_TYPE, (int)itemData.ItemGroupType, questValue: MakeCount); // 아이템 종류 제작 횟수
                Quest.QuestProgress(QuestType.MAKE_ITEM_COUNT, conditionValue: itemId); // 특정 아이템 제작 횟수

                SetMakeCount(1);
            }
            else
            {
                response.ShowResultCode();
            }

            return response.isSuccess;
        }

        public MakeResultInfo[] GetMakeResultInfos()
        {
            return makeResultInfos.ToArray();
        }
    }
}