using Ragnarok.View;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDarkTreeMaterialSelect"/>
    /// </summary>
    public class DarkTreeMaterialSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly InventoryModel inventoryModel;

        // <!-- Event --!>
        public event System.Action OnUpdateMaterialSelect;
        public event System.Action OnFinished;

        private BetterList<DarkTreeMaterial> darkTreeMaterials;

        public DarkTreeMaterialSelectPresenter()
        {
            inventoryModel = Entity.player.Inventory;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
            if (darkTreeMaterials == null)
                return;

            for (int i = 0; i < darkTreeMaterials.size; i++)
            {
                darkTreeMaterials[i].OnUpdateSelectedCount -= OnUpdateSelectedCount;
            }
            darkTreeMaterials.Clear();
        }

        /// <summary>
        /// 어둠의 나무 재료 목록 반환
        /// </summary>
        public DarkTreeMaterialElement.IInput[] GetArrayData()
        {
            if (darkTreeMaterials == null)
            {
                darkTreeMaterials = new BetterList<DarkTreeMaterial>();

                foreach (ItemInfo itemInfo in inventoryModel.GetDarkTreeMaterials())
                {
                    DarkTreeMaterial temp = new DarkTreeMaterial(itemInfo, IsCheckMax);
                    temp.OnUpdateSelectedCount += OnUpdateSelectedCount;

                    darkTreeMaterials.Add(temp);
                }
            }

            return darkTreeMaterials.ToArray();
        }

        /// <summary>
        /// 현재 선택한 보상
        /// </summary>
        public RewardData GetCurrentReward()
        {
            return inventoryModel.DarkTree.GetSelectedReward();
        }

        /// <summary>
        /// 현재 포인트 반환
        /// </summary>
        public int GetCurPoint()
        {
            int curPoint = inventoryModel.DarkTree.GetCurPoint();
            int plusPoint = GetPlusPoint();
            return curPoint + plusPoint;
        }

        /// <summary>
        /// 최대 필요포인트 반환
        /// </summary>
        public int GetMaxPoint()
        {
            return inventoryModel.DarkTree.GetMaxPoint();
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
            if (darkTreeMaterials == null)
                return 0;

            int plusPoint = 0;
            for (int i = 0; i < darkTreeMaterials.size; i++)
            {
                plusPoint += darkTreeMaterials[i].GetSelectedPoint();
            }

            return plusPoint;
        }

        /// <summary>
        /// 최대 포인트 도달 여부
        /// </summary>
        private bool IsCheckMax()
        {
            int curPoint = GetCurPoint();
            int maxPoint = GetMaxPoint();
            return curPoint >= maxPoint;
        }

        /// <summary>
        /// 선택 카운트 변경 시 호출
        /// </summary>
        private void OnUpdateSelectedCount()
        {
            OnUpdateMaterialSelect?.Invoke();
        }

        /// <summary>
        /// 서버 호출 (어둠의 나무 재료 선택)
        /// </summary>
        private async Task AsyncRequestSelectMatereial()
        {
            // 추가된 것이 없을 경우
            if (GetPlusPoint() == 0)
            {
                OnFinished?.Invoke();
                return;
            }

            string message = LocalizeKey._9038.ToText(); // 수확 재료로 쓰이는 아이템은 모두 사라집니다.\n선택된 재료를 수확에 사용하시겠습니까?
            if (!await UI.SelectPopup(message))
                return;

            BetterList<(int id, int count)> list = new BetterList<(int id, int count)>();
            foreach (var item in darkTreeMaterials)
            {
                int selectedCount = item.GetSelectedCount();

                // 선택된 아이템이 없을 경우
                if (selectedCount == 0)
                    continue;

                list.Add((item.ItemInfo.ItemId, selectedCount));
            }

            await inventoryModel.RequestDarkTreeRegPoint(list.ToArray());
            OnFinished?.Invoke();
        }

        private class DarkTreeMaterial : DarkTreeMaterialElement.IInput
        {
            public ItemInfo ItemInfo { get; }
            public int MaxCount { get; }
            public int Point => ItemInfo.DarkTreePoint;
            private readonly System.Func<bool> checkMax;

            private int selectedCount;

            public event System.Action OnUpdateSelectedCount;

            public DarkTreeMaterial(ItemInfo itemInfo, System.Func<bool> checkMax)
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

                return ItemInfo.DarkTreePoint * selectedCount;
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