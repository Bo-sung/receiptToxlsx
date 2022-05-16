using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class EquipmentItemView : UISubCanvas
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIButtonHelper btnCrash;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnSort;
        [SerializeField] UIToggleHelper toggleSlot;
        [SerializeField] UISprite slotArrow;
        [SerializeField] UIButtonHelper[] btnSlot;
        [SerializeField] UIToggleHelper toggleRank;
        [SerializeField] UISprite rankArrow;
        [SerializeField] UIButtonHelper[] btnRank;
        [SerializeField] UIGridHelper starGrid;
        [SerializeField] UIButtonHelper btnAllSelect;
        [SerializeField] UIButtonHelper btnAutoEquip;
        [SerializeField] UILabelHelper emptyListNotice;

        [SerializeField] public TweenAlpha publicTweenAlpha;
        [SerializeField] UIWidget firstItemWidget;

        /// <summary>
        /// 오름 차순 여부
        /// </summary>
        private bool isAscending = false;

        /// <summary>
        /// 펄터 슬롯타입
        /// </summary>
        private ItemEquipmentSlotType slotType = ItemEquipmentSlotType.None;

        /// <summary>
        /// 필터 등급
        /// </summary>
        private int rank = 0;

        InvenPresenter presenter;
        ItemInfo[] arrayInfo;

        public UIWidget FirstItemWidget { get { return firstItemWidget; } }

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnCrash.OnClick, OnClickedBtnDisassembleMode);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnSort.OnClick, OnClickedBtnSort);
            EventDelegate.Add(btnAllSelect.OnClick, OnClickedBtnAllDisassemble);

            EventDelegate.Add(toggleSlot.OnChange, OnToggleSlotType);
            EventDelegate.Add(btnSlot[0].OnClick, OnClickBtnSlotAll);
            EventDelegate.Add(btnSlot[1].OnClick, OnClickBtnSlotHeadGear);
            EventDelegate.Add(btnSlot[2].OnClick, OnClickBtnSlotGarment);
            EventDelegate.Add(btnSlot[3].OnClick, OnClickBtnSlotArmor);
            EventDelegate.Add(btnSlot[4].OnClick, OnClickBtnSlotWeapon);
            EventDelegate.Add(btnSlot[5].OnClick, OnClickBtnSlotAccessory1);
            EventDelegate.Add(btnSlot[6].OnClick, OnClickBtnSlotAccessory2);

            EventDelegate.Add(toggleRank.OnChange, OnToggleRank);
            EventDelegate.Add(btnRank[0].OnClick, OnClickBtnRankAll);
            EventDelegate.Add(btnRank[1].OnClick, OnClickBtnRank1);
            EventDelegate.Add(btnRank[2].OnClick, OnClickBtnRank2);
            EventDelegate.Add(btnRank[3].OnClick, OnClickBtnRank3);
            EventDelegate.Add(btnRank[4].OnClick, OnClickBtnRank4);
            EventDelegate.Add(btnRank[5].OnClick, OnClickBtnRank5);

            EventDelegate.Add(btnAutoEquip.OnClick, OnClickBtnAutoEquip);
        }

        protected override void OnClose()
        {
            presenter.SetDisassembleMode(false, false);

            EventDelegate.Remove(btnCrash.OnClick, OnClickedBtnDisassembleMode);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnSort.OnClick, OnClickedBtnSort);
            EventDelegate.Remove(btnAllSelect.OnClick, OnClickedBtnAllDisassemble);

            EventDelegate.Remove(toggleSlot.OnChange, OnToggleSlotType);
            EventDelegate.Remove(btnSlot[0].OnClick, OnClickBtnSlotAll);
            EventDelegate.Remove(btnSlot[1].OnClick, OnClickBtnSlotHeadGear);
            EventDelegate.Remove(btnSlot[2].OnClick, OnClickBtnSlotGarment);
            EventDelegate.Remove(btnSlot[3].OnClick, OnClickBtnSlotArmor);
            EventDelegate.Remove(btnSlot[4].OnClick, OnClickBtnSlotWeapon);
            EventDelegate.Remove(btnSlot[5].OnClick, OnClickBtnSlotAccessory1);
            EventDelegate.Remove(btnSlot[6].OnClick, OnClickBtnSlotAccessory2);

            EventDelegate.Remove(toggleRank.OnChange, OnToggleRank);
            EventDelegate.Remove(btnRank[0].OnClick, OnClickBtnRankAll);
            EventDelegate.Remove(btnRank[1].OnClick, OnClickBtnRank1);
            EventDelegate.Remove(btnRank[2].OnClick, OnClickBtnRank2);
            EventDelegate.Remove(btnRank[3].OnClick, OnClickBtnRank3);
            EventDelegate.Remove(btnRank[4].OnClick, OnClickBtnRank4);
            EventDelegate.Remove(btnRank[5].OnClick, OnClickBtnRank5);

            EventDelegate.Remove(btnAutoEquip.OnClick, OnClickBtnAutoEquip);
        }

        protected override void OnShow()
        {
            toggleSlot.Set(false);
            toggleRank.Set(false);

            UpdateView();
        }

        protected override void OnHide()
        {
            presenter.SetDisassembleMode(false, false);
        }

        protected override void OnLocalize()
        {
            btnCrash.LocalKey = LocalizeKey._6005; // 장비분해
            btnConfirm.LocalKey = LocalizeKey._6006; // 분해
            btnCancel.LocalKey = LocalizeKey._6007; // 취소
            btnSort.LocalKey = LocalizeKey._6008; // 정렬       
            btnAllSelect.LocalKey = LocalizeKey._6013; // 전체 선택
            btnAutoEquip.LocalKey = LocalizeKey._6014; // 자동 장착
            btnSlot[0].Text = ItemEquipmentSlotType.None.ToText();
            btnSlot[1].Text = ItemEquipmentSlotType.HeadGear.ToText();
            btnSlot[2].Text = ItemEquipmentSlotType.Garment.ToText();
            btnSlot[3].Text = ItemEquipmentSlotType.Armor.ToText();
            btnSlot[4].Text = ItemEquipmentSlotType.Weapon.ToText();
            btnSlot[5].Text = ItemEquipmentSlotType.Accessory1.ToText();
            btnSlot[6].Text = ItemEquipmentSlotType.Accessory2.ToText();
            btnRank[0].Text = ItemEquipmentSlotType.None.ToText();
            emptyListNotice.LocalKey = LocalizeKey._22300;
        }

        public void Initialize(InvenPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void SortItem()
        {
            arrayInfo = presenter.GetEquipmentItemInfos(rank, slotType);
            System.Array.Sort(arrayInfo, SortByCustom);
        }

        private void UpdateView()
        {
            // 장비 정보     
            var array = presenter.GetEquipmentItemInfos(rank, slotType);

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

            btnCrash.IsEnabled = arrayInfo.FirstOrDefault(a => !a.IsEquipped && !a.IsLock) != default(ItemInfo);
            btnSort.IsEnabled = arrayInfo.Length > 0;
            btnAutoEquip.IsEnabled = arrayInfo.Length > 0;


            if (presenter.IsDisassembleMode)
            {
                btnCrash.SetActive(false);
                btnConfirm.SetActive(true);
                btnCancel.SetActive(true);
                btnAllSelect.SetActive(true);
                btnAutoEquip.SetActive(false);
            }
            else
            {
                btnCrash.SetActive(true);
                btnConfirm.SetActive(false);
                btnCancel.SetActive(false);
                btnAllSelect.SetActive(false);
                btnAutoEquip.SetActive(true);
            }

            if (slotType == ItemEquipmentSlotType.None)
            {
                toggleSlot.LocalKey = LocalizeKey._6009; // 종류
            }
            else
            {
                toggleSlot.Text = slotType.ToText();
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
            UIEquipmentInfoSlot ui = go.GetComponent<UIEquipmentInfoSlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        /// <summary>
        /// 분해모드 버튼 이벤트
        /// </summary>
        private void OnClickedBtnDisassembleMode()
        {
            wrapper.SetProgress(0);
            presenter.SetDisassembleMode(true);
        }

        /// <summary>
        /// 분해 전체선택
        /// </summary>
        private void OnClickedBtnAllDisassemble()
        {
            presenter.SelectAllDisassemble(ItemGroupType.Equipment, rank, slotType);
        }

        /// <summary>
        /// 분해 버튼
        /// </summary>
        private void OnClickedBtnConfirm()
        {
            presenter.RequestItemDisassemble(ItemGroupType.Equipment);
        }

        /// <summary>
        /// 취소 버튼
        /// </summary>
        private void OnClickedBtnCancel()
        {
            wrapper.SetProgress(0);
            presenter.SetDisassembleMode(false);
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
        /// 장비 종류 토클
        /// </summary>
        private void OnToggleSlotType()
        {
            slotArrow.flip = toggleSlot.Value ? UIBasicSprite.Flip.Vertically : UIBasicSprite.Flip.Nothing;
        }

        private void SetSlotType(ItemEquipmentSlotType type)
        {
            toggleSlot.Value = false;
            if (slotType == type)
                return;
            slotType = type;
            presenter.ClearAllDisassemble();
            SortItem();
            UpdateView();
        }

        private void OnClickBtnSlotAll()
        {
            SetSlotType(ItemEquipmentSlotType.None);
        }

        private void OnClickBtnSlotHeadGear()
        {
            SetSlotType(ItemEquipmentSlotType.HeadGear);
        }

        private void OnClickBtnSlotGarment()
        {
            SetSlotType(ItemEquipmentSlotType.Garment);
        }

        private void OnClickBtnSlotArmor()
        {
            SetSlotType(ItemEquipmentSlotType.Armor);
        }

        private void OnClickBtnSlotWeapon()
        {
            SetSlotType(ItemEquipmentSlotType.Weapon);
        }

        private void OnClickBtnSlotAccessory1()
        {
            SetSlotType(ItemEquipmentSlotType.Accessory1);
        }

        private void OnClickBtnSlotAccessory2()
        {
            SetSlotType(ItemEquipmentSlotType.Accessory2);
        }

        /// <summary>
        /// 등급 토클
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

        private int SortByCustom(ItemInfo x, ItemInfo y)
        {
            // 장착, 신규, 초월, 등급, 레벨, 아이템ID
            int sortType = isAscending ? -1 : 1;
            int result0 = y.IsEquipped.CompareTo(x.IsEquipped);
            int result1 = result0 == 0 ? y.IsNew.CompareTo(x.IsNew) : result0;
            int result2 = result1 == 0 ? sortType * y.ItemTranscend.CompareTo(x.ItemTranscend) : result1;
            int result3 = result2 == 0 ? sortType * y.Rating.CompareTo(x.Rating) : result2;
            int result4 = result3 == 0 ? sortType * y.Smelt.CompareTo(x.Smelt) : result3;
            int result5 = result4 == 0 ? x.ItemId.CompareTo(y.ItemId) : result4; // 오른차순
            return result5;
        }

        public void HideAllNew()
        {
            presenter.HideNew(arrayInfo);
        }

        void OnClickBtnAutoEquip()
        {
            presenter.AutoEquip();
        }
    }
}
