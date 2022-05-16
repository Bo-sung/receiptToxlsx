using Ragnarok.View;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICupetExpMaterialSelect"/>
    /// </summary>
    public class CupetExpMaterialSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly InventoryModel inventoryModel;
        private readonly CupetListModel cupetListModel;

        // <!-- Repositories --!>
        private readonly ExpDataManager expDataRepo;
        private readonly ItemDataManager itemDataRepo;

        // <!-- Event --!>
        public event System.Action OnUpdateMaterialSelect;
        public event System.Action OnFinished;

        public event System.Action OnUpdateCupetList
        {
            add { cupetListModel.OnUpdateCupetList += value; }
            remove { cupetListModel.OnUpdateCupetList -= value; }
        }

        private BetterList<CupetExpMaterial> materials;
        private CupetModel selectedCupet;
        private int expPoint;
        private int[] maxPoints;
        private int maxPoint;

        public CupetExpMaterialSelectPresenter()
        {
            inventoryModel = Entity.player.Inventory;
            cupetListModel = Entity.player.CupetList;

            expDataRepo = ExpDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
            if (materials == null)
                return;

            for (int i = 0; i < materials.size; i++)
            {
                materials[i].OnUpdateSelectedCount -= OnUpdateSelectedCount;
            }
            materials.Clear();
        }

        /// <summary>
        /// 재료 목록 반환
        /// </summary>
        public DarkTreeMaterialElement.IInput[] GetArrayData()
        {
            if (materials == null)
            {
                materials = new BetterList<CupetExpMaterial>();

                ItemData[] finds = itemDataRepo.GetCupetExpMaterial();
                if (finds != null)
                {
                    foreach (var item in finds)
                    {
                        ItemInfo itemInfo = inventoryModel.FindOrCreateIfNull(item.id);
                        CupetExpMaterial temp = new CupetExpMaterial(itemInfo, IsCheckMax);
                        temp.OnUpdateSelectedCount += OnUpdateSelectedCount;

                        materials.Add(temp);
                    }
                }
            }

            return materials.ToArray();
        }

        /// <summary>
        /// 큐펫 선택
        /// </summary>
        public void SelectCupet(int cupetId)
        {
            selectedCupet = cupetListModel.Get(cupetId).Cupet;

            expPoint = selectedCupet.Exp;
            maxPoints = expDataRepo.GetMaxPoints(ExpDataManager.ExpType.Cupet, selectedCupet.GetMaxLevel());

            maxPoint = 0;
            for (int i = 0; i < maxPoints.Length; i++)
            {
                maxPoint += maxPoints[i];
            }
        }

        /// <summary>
        /// 현재 선택한 큐펫
        /// </summary>
        public CupetModel GetCurrentCupet()
        {
            return selectedCupet;
        }

        /// <summary>
        /// 현재 포인트 반환
        /// </summary>
        public int GetCurPoint()
        {
            return expPoint + GetPlusPoint();
        }

        /// <summary>
        /// 레벨 별 필요포인트 반환
        /// </summary>
        public int[] GetMaxPoints()
        {
            return maxPoints;
        }

        /// <summary>
        /// 선택한 재료로 서버 호출
        /// </summary>
        public void RequestSelectMatereial()
        {
            AsyncRequestSelectMatereial().WrapNetworkErrors();
        }

        /// <summary>
        /// 추가 포인트
        /// </summary>
        private int GetPlusPoint()
        {
            if (materials == null)
                return 0;

            int plusPoint = 0;
            for (int i = 0; i < materials.size; i++)
            {
                plusPoint += materials[i].GetSelectedPoint();
            }

            return plusPoint;
        }

        /// <summary>
        /// 최대 포인트 도달 여부
        /// </summary>
        private bool IsCheckMax()
        {
            return GetCurPoint() >= maxPoint;
        }

        /// <summary>
        /// 선택 카운트 변경 시 호출
        /// </summary>
        private void OnUpdateSelectedCount()
        {
            OnUpdateMaterialSelect?.Invoke();
        }

        private async Task AsyncRequestSelectMatereial()
        {
            // 추가된 것이 없을 경우
            if (GetPlusPoint() == 0)
            {
                OnFinished?.Invoke();
                return;
            }

            string message = LocalizeKey._33904.ToText(); // 강화 재료로 기부한 아이템은 모두 사라집니다.\n선택된 재료를 기부에 사용하시겠습니까?
            if (!await UI.SelectPopup(message))
                return;

            BetterList<(int id, int count)> list = new BetterList<(int id, int count)>();
            foreach (var item in materials)
            {
                int selectedCount = item.GetSelectedCount();

                // 선택된 아이템이 없을 경우
                if (selectedCount == 0)
                    continue;

                list.Add((item.ItemInfo.ItemId, selectedCount));
            }

            cupetListModel.RequestCupetExpUp(selectedCupet.CupetID, list.ToArray()).WrapNetworkErrors();
        }

        private class CupetExpMaterial : DarkTreeMaterialElement.IInput
        {
            public ItemInfo ItemInfo { get; }
            public int MaxCount { get; }
            public int Point => ItemInfo.CupetExpPoint;
            private readonly System.Func<bool> checkMax;

            private int selectedCount;

            public event System.Action OnUpdateSelectedCount;

            public CupetExpMaterial(ItemInfo itemInfo, System.Func<bool> checkMax)
            {
                ItemInfo = itemInfo;
                MaxCount = ItemInfo.ItemCount;
                this.checkMax = checkMax;
            }

            public int GetSelectedCount()
            {
                return selectedCount;
            }

            public void MinusCount()
            {
                SetSelectedCount(selectedCount - 1);
            }

            public void PlusCount()
            {
                if (checkMax())
                {
                    UI.ShowToastPopup(LocalizeKey._9041.ToText()); // 더 이상 추가할 수 없습니다.
                    return;
                }

                SetSelectedCount(selectedCount + 1);
            }

            public void ToggleSelect()
            {
                if (selectedCount == 0)
                {
                    SelectMaxCount();
                }
                else
                {
                    SelectMinCount();
                }
            }

            /// <summary>
            /// 선택하여 추가된 포인트
            /// </summary>
            public int GetSelectedPoint()
            {
                if (selectedCount == 0)
                    return 0;

                return ItemInfo.CupetExpPoint * selectedCount;
            }

            private void SetSelectedCount(int value)
            {
                if (selectedCount == value)
                    return;

                selectedCount = value;
                OnUpdateSelectedCount?.Invoke();
            }

            private void SelectMaxCount()
            {
                // 최대 수치까지 선택하기 전 체크
                if (checkMax())
                {
                    UI.ShowToastPopup(LocalizeKey._9041.ToText()); // 더 이상 추가할 수 없습니다.
                    return;
                }

                // 최대 수치까지 선택
                while (!checkMax() && selectedCount < MaxCount)
                {
                    ++selectedCount;
                }

                OnUpdateSelectedCount?.Invoke();
            }

            private void SelectMinCount()
            {
                SetSelectedCount(0); // 0으로 초기화
            }
        }
    }
}