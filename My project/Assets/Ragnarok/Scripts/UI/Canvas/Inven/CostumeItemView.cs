using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class CostumeItemView : UISubCanvas<InvenPresenter>
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        [SerializeField] UIButtonHelper btnCrash;
        [SerializeField] UIButtonHelper btnAllSelect;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnCrashConfirm;

        [SerializeField] public TweenAlpha publicTweenAlpha;
        [SerializeField] UILabelHelper emptyListNotice;

        InvenPresenter presenter;
        ItemInfo[] arrayInfo;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnCrash.OnClick, OnClickedBtnDisassembleMode);
            EventDelegate.Add(btnCrashConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnAllSelect.OnClick, OnClickedBtnAllDisassemble);          
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnCrash.OnClick, OnClickedBtnDisassembleMode);
            EventDelegate.Remove(btnCrashConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnAllSelect.OnClick, OnClickedBtnAllDisassemble);           
        }

        protected override void OnShow()
        {
            UpdateView();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnCrash.LocalKey = LocalizeKey._6018; // 코스튬 분해
            btnCrashConfirm.LocalKey = LocalizeKey._6006; // 분해
            btnCancel.LocalKey = LocalizeKey._6007; // 취소      
            btnAllSelect.LocalKey = LocalizeKey._6013; // 전체 선택
            emptyListNotice.LocalKey = LocalizeKey._22300;
        }

        public void Initialize(InvenPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void SortItem()
        {
            arrayInfo = presenter.GetCostumeItemInfos();
            System.Array.Sort(arrayInfo, SortByCustom);
        }

        private void UpdateView()
        {
            // 코스튬 정보            
            var array = presenter.GetCostumeItemInfos();
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

            btnCrash.IsEnabled = arrayInfo.FirstOrDefault(a => !a.IsEquipped) != default(ItemInfo);

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
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UICostumeInfoInventorySlot ui = go.GetComponent<UICostumeInfoInventorySlot>();
            ui.Set(new UICostumeInfoSlot.Info(arrayInfo[index], presenter));
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
            presenter.SelectAllDisassemble(ItemGroupType.Costume);
        }

        /// <summary>
        /// 분해 버튼
        /// </summary>
        private void OnClickedBtnConfirm()
        {
            presenter.RequestItemDisassemble(ItemGroupType.Costume);
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
            int result0 = y.IsEquipped.CompareTo(x.IsEquipped);
            int result1 = result0 == 0 ? y.IsNew.CompareTo(x.IsNew) : result0;
            int result2 = result1 == 0 ? y.ItemId.CompareTo(x.ItemId) : result1;
            return result2;
        }

        public void HideAllNew()
        {
            presenter.HideNew(arrayInfo);
        }
    }
}