using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class PartsItemView : UISubCanvas
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper emptyListNotice;

        InvenPresenter presenter;
        ItemInfo[] arrayInfo;

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

        public void Initialize(InvenPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void SortItem()
        {
            arrayInfo = presenter.GetPartsItemInfos();
            System.Array.Sort(arrayInfo, SortByCustom);
        }

        private void UpdateView()
        {
            // 재료 정보           
            var array = presenter.GetPartsItemInfos();
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
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIPartsInfoSlot ui = go.GetComponent<UIPartsInfoSlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }
        private int SortByCustom(ItemInfo x, ItemInfo y)
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
