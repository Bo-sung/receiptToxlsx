using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class ConsumableItemView : UISubCanvas<InvenPresenter>
    {
        [SerializeField] protected SuperScrollListWrapper wrapper;
        [SerializeField] protected GameObject prefab;
        [SerializeField] UILabelHelper emptyListNotice;

        protected ItemInfo[] arrayInfo;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
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
            emptyListNotice.LocalKey = LocalizeKey._22300;
        }

        public virtual void SortItem()
        {
            arrayInfo = presenter.GetConsumableItemInfos();
            System.Array.Sort(arrayInfo, SortByCustom);
        }

        protected virtual void UpdateView()
        {
            var array = presenter.GetConsumableItemInfos();
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
            // 소모품 정보            
            wrapper.Resize(arrayInfo.Length);
            emptyListNotice.gameObject.SetActive(arrayInfo.Length == 0);
        }

        protected void OnItemRefresh(GameObject go, int index)
        {
            UIConsumableInfoSlot ui = go.GetComponent<UIConsumableInfoSlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        protected int SortByCustom(ItemInfo x, ItemInfo y)
        {
            int result0 = y.IsNew.CompareTo(x.IsNew);
            int result1 = result0 == 0 ? y.ItemId.CompareTo(x.ItemId) : result0;
            return result1;
        }

        public void HideAllNew()
        {
            presenter.HideNew(arrayInfo);    
        }
    }
}