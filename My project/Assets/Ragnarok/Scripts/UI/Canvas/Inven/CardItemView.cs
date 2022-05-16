using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CardItemView : UISubCanvas
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIButtonHelper btnSort;

        // 등급 필터
        [SerializeField] UIToggleHelper toggleRank;
        [SerializeField] UISprite rankArrow;
        [SerializeField] UIButtonHelper[] btnRank;
        [SerializeField] UIGridHelper starGrid;

        // 분해
        [SerializeField] UIButtonHelper btnCrash;
        [SerializeField] UIButtonHelper btnAllSelect;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnCrashConfirm;

        [SerializeField] public TweenAlpha publicTweenAlpha;
        [SerializeField] UILabelHelper emptyListNotice;

        /// <summary>
        /// 오름 차순 여부
        /// </summary>
        private bool isAscending = false;

        /// <summary>
        /// 필터 등급
        /// </summary>
        private int rank = 0;

        InvenPresenter presenter;
        ItemInfo[] arrayInfo;


        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnSort.OnClick, OnClickedBtnSort);

            EventDelegate.Add(btnCrash.OnClick, OnClickedBtnDisassembleMode);
            EventDelegate.Add(btnCrashConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnSort.OnClick, OnClickedBtnSort);
            EventDelegate.Add(btnAllSelect.OnClick, OnClickedBtnAllDisassemble);

            EventDelegate.Add(toggleRank.OnChange, OnToggleRank);
            EventDelegate.Add(btnRank[0].OnClick, OnClickBtnRankAll);
            EventDelegate.Add(btnRank[1].OnClick, OnClickBtnRank1);
            EventDelegate.Add(btnRank[2].OnClick, OnClickBtnRank2);
            EventDelegate.Add(btnRank[3].OnClick, OnClickBtnRank3);
            EventDelegate.Add(btnRank[4].OnClick, OnClickBtnRank4);
            EventDelegate.Add(btnRank[5].OnClick, OnClickBtnRank5);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnSort.OnClick, OnClickedBtnSort);

            EventDelegate.Remove(btnCrash.OnClick, OnClickedBtnDisassembleMode);
            EventDelegate.Remove(btnCrashConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnSort.OnClick, OnClickedBtnSort);
            EventDelegate.Remove(btnAllSelect.OnClick, OnClickedBtnAllDisassemble);

            EventDelegate.Remove(toggleRank.OnChange, OnToggleRank);
            EventDelegate.Remove(btnRank[0].OnClick, OnClickBtnRankAll);
            EventDelegate.Remove(btnRank[1].OnClick, OnClickBtnRank1);
            EventDelegate.Remove(btnRank[2].OnClick, OnClickBtnRank2);
            EventDelegate.Remove(btnRank[3].OnClick, OnClickBtnRank3);
            EventDelegate.Remove(btnRank[4].OnClick, OnClickBtnRank4);
            EventDelegate.Remove(btnRank[5].OnClick, OnClickBtnRank5);
        }

        protected override void OnShow()
        {
            toggleRank.Set(false);

            UpdateView();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnCrash.LocalKey = LocalizeKey._6016; // 카드분해
            btnCrashConfirm.LocalKey = LocalizeKey._6006; // 분해
            btnCancel.LocalKey = LocalizeKey._6007; // 취소      
            btnAllSelect.LocalKey = LocalizeKey._6013; // 전체 선택

            btnSort.LocalKey = LocalizeKey._6008; // 정렬
            btnRank[0].Text = ItemEquipmentSlotType.None.ToText();
            emptyListNotice.LocalKey = LocalizeKey._22300;
        }

        public void Initialize(InvenPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void SortItem()
        {
            arrayInfo = presenter.GetCardItemInfos(rank);
            System.Array.Sort(arrayInfo, SortByCustom);
        }

        private void UpdateView()
        {
            // 카드 정보            
            var array = presenter.GetCardItemInfos(rank);
            // 아이템 추가 또는 삭제 체크
            var intersect = arrayInfo.Intersect(array);

            if (array.Length != intersect.Count())
            {
                arrayInfo = intersect.Union(array).ToArray();
            }
            else
            {
                arrayInfo = intersect.ToArray();
            }
            wrapper.Resize(arrayInfo.Length);
            emptyListNotice.gameObject.SetActive(arrayInfo.Length == 0);

            btnCrash.IsEnabled = arrayInfo.Length > 0;
            btnSort.IsEnabled = arrayInfo.Length > 0;

            if (presenter.IsDisassembleMode)
            {
                btnCrash.SetActive(false);
                btnCrashConfirm.SetActive(true);
                btnCancel.SetActive(true);
                btnAllSelect.SetActive(true);
            }
            else
            {
                btnCrash.SetActive(true);
                btnCrashConfirm.SetActive(false);
                btnCancel.SetActive(false);
                btnAllSelect.SetActive(false);
            }

            if (rank == 0)
            {
                toggleRank.LocalKey = LocalizeKey._6010; // 등급
            }
            else
            {
                toggleRank.Text = string.Empty;
            }
            starGrid.SetValue(rank);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UICardInfoInventorySlot ui = go.GetComponent<UICardInfoInventorySlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        /// <summary>
        /// 정렬 버튼
        /// </summary>
        private void OnClickedBtnSort()
        {
            isAscending = !isAscending;
            SortItem();
            UpdateView();

            UI.ShowToastPopup(LocalizeKey._90257.ToText()); // 정렬이 완료되었습니다.
        }

        /// <summary>
        /// 랭크 토글
        /// </summary>
        private void OnToggleRank()
        {
            rankArrow.flip = toggleRank.Value ? UIBasicSprite.Flip.Vertically : UIBasicSprite.Flip.Nothing;
        }

        private void SetRank(int rank)
        {
            toggleRank.Value = false;
            if (this.rank == rank)
                return;
            this.rank = rank;
            presenter.ClearAllDisassemble();
            SortItem();
            UpdateView();
        }

        private void OnClickBtnRankAll()
        {
            SetRank(0);
        }

        private void OnClickBtnRank1()
        {
            SetRank(1);
        }

        private void OnClickBtnRank2()
        {
            SetRank(2);
        }

        private void OnClickBtnRank3()
        {
            SetRank(3);
        }

        private void OnClickBtnRank4()
        {
            SetRank(4);
        }

        private void OnClickBtnRank5()
        {
            SetRank(5);
        }

        /// <summary>
        /// 분해모드 버튼 이벤트
        /// </summary>
        private void OnClickedBtnDisassembleMode()
        {
            presenter.SetDisassembleMode(true);
        }

        /// <summary>
        /// 분해 전체선택
        /// </summary>
        private void OnClickedBtnAllDisassemble()
        {
            presenter.SelectAllDisassemble(ItemGroupType.Card, rank);
        }

        /// <summary>
        /// 분해 버튼
        /// </summary>
        private void OnClickedBtnConfirm()
        {
            presenter.RequestItemDisassemble(ItemGroupType.Card);
        }

        /// <summary>
        /// 취소 버튼
        /// </summary>
        private void OnClickedBtnCancel()
        {
            presenter.SetDisassembleMode(false);
        }

        private int SortByCustom(ItemInfo x, ItemInfo y)
        {
            // 신규, 등급, 레벨, 아이템ID
            int sortType = isAscending ? -1 : 1;
            int result0 = y.IsNew.CompareTo(x.IsNew);
            int result1 = result0 == 0 ? sortType * y.Rating.CompareTo(x.Rating) : result0;
            int result2 = result1 == 0 ? sortType * y.CardLevel.CompareTo(x.CardLevel) : result1;
            int result3 = result2 == 0 ? y.ItemId.CompareTo(x.ItemId) : result2;
            return result3;
        }

        public void HideAllNew()
        {
            presenter.HideNew(arrayInfo);
        }
    }
}