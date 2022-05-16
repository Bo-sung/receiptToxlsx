using System.Linq;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICupetSelect"/>
    /// </summary>
    public class CupetSelectPresenter : ViewPresenter
    {
        private const int MAX_CUPET_COUNT = 3; // 최대 선택 가능 수량

        // <!-- Models --!>
        private readonly CupetListModel cupetListModel;

        // <!-- Data --!>
        private readonly BetterList<CupetSelectElement> cupetSelectElements;
        private readonly Buffer<int> selectedIds;

        // <!-- Event --!>
        public event System.Action OnUpdateList;

        public CupetSelectPresenter()
        {
            cupetListModel = Entity.player.CupetList;
            cupetSelectElements = new BetterList<CupetSelectElement>();
            selectedIds = new Buffer<int>();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
            if (cupetSelectElements == null)
                return;

            for (int i = 0; i < cupetSelectElements.size; i++)
            {
                cupetSelectElements[i].OnClickedSelect -= OnClickedSelect;
            }
            cupetSelectElements.Clear();
        }

        public UICupetSelectElement.IInput[] GetCupetArray(int[] filterIds)
        {
            cupetSelectElements.Clear();

            foreach (CupetEntity item in cupetListModel.GetArray())
            {
                // 미보유 큐펫 필터
                if (!item.Cupet.IsInPossession)
                    continue;

                // 리스트 목록에서 필터링 해야할 목록 (좌측 큐펫 세팅시 우측 큐펫 필터링)
                if (filterIds != null && filterIds.Contains(item.Cupet.CupetID))
                    continue;

                CupetSelectElement temp = new CupetSelectElement(item.Cupet, CheckSelect);
                temp.OnClickedSelect += OnClickedSelect;
                cupetSelectElements.Add(temp);
            }
            cupetSelectElements.Sort(SortCupet);

            return cupetSelectElements.ToArray();
        }

        /// <summary>
        /// 큐펫 선택 상태 체크 & index
        /// </summary>
        private (bool isSelect, int index) CheckSelect(int cupetId)
        {
            return (selectedIds.Contains(cupetId), selectedIds.IndexOf(cupetId));
        }

        /// <summary>
        /// 큐펫 선택 & 해제
        /// </summary>
        private void OnClickedSelect(int cupetId)
        {
            if (selectedIds.Contains(cupetId))
            {
                selectedIds.Remove(cupetId);
            }
            else
            {
                // 선택할 수 있는 최대 큐펫 수량
                if (selectedIds.size == MAX_CUPET_COUNT)
                    return;

                selectedIds.Add(cupetId);
            }
            OnUpdateList?.Invoke();
        }

        /// <summary>
        /// 미리 선택된 큐펫 세팅
        /// </summary>
        public void SetSelectedIds(int[] ids)
        {
            if (ids == null)
                return;

            selectedIds.AddRange(ids);
        }

        public int[] GetSelectedIds()
        {
            return selectedIds.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 큐펫 정보 보기
        /// </summary>
        public void ShowCupetInfo(ICupetModel cupet)
        {
            UI.Show<UIOtherCupetInfo>().SetEntity(cupet.CupetID, cupet.Rank, cupet.Level);
        }

        private int SortCupet(CupetSelectElement a, CupetSelectElement b)
        {
            int result0 = b.IsSelect.CompareTo(a.IsSelect); // 1순위: IsSelect True 먼저
            int result1 = result0 == 0 ? a.Index.CompareTo(b.Index) : result0; // 2순위: Index 낮은 순
            int result2 = result1 == 0 ? b.CupetModel.Rank.CompareTo(a.CupetModel.Rank) : result1; // 3순위: Rank 높은 순
            return result2 == 0 ? b.CupetModel.Level.CompareTo(a.CupetModel.Level) : result2; // 4순위: Level 높은 순
        }

        private class CupetSelectElement : UICupetSelectElement.IInput
        {
            public CupetModel CupetModel { get; }
            public bool IsSelect => checkSelect(CupetModel.CupetID).isSelect;
            public int Index => checkSelect(CupetModel.CupetID).index;

            private readonly System.Func<int, (bool isSelect, int index)> checkSelect;

            public event System.Action<int> OnClickedSelect;

            public CupetSelectElement(CupetModel cupetModel, System.Func<int, (bool isSelect, int index)> checkSelect)
            {
                CupetModel = cupetModel;
                this.checkSelect = checkSelect;
            }

            public void OnSelect()
            {
                OnClickedSelect?.Invoke(CupetModel.CupetID);
            }
        }
    }
}