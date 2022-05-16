using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 제니, 경험치 획득 뷰
    /// </summary>
    public class UIGoodsView : UICanvas<GoodsViewPresenter>, GoodsViewPresenter.IView
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] GameObject prefab;
        [SerializeField] GameObject wrapper;

        [SerializeField] UIWidget widgetSpawnArea;
        [SerializeField] UIWidget widgetTopArea;
        [SerializeField] Vector3 spawnPosInterval;

        BetterList<GameObject> slots;
        BetterList<GameObject> recyclebin;

        protected override void OnInit()
        {
            presenter = new GoodsViewPresenter(this);
            presenter.AddEvent();

            recyclebin = new BetterList<GameObject>();
            slots = new BetterList<GameObject>();

            presenter.SpawnTransform = widgetSpawnArea.cachedTransform;
            presenter.TopTransform = widgetTopArea.cachedTransform;
            presenter.SpawnPosInterval = spawnPosInterval;
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        /// <summary>
        /// 획득한 재화 프리팹 추가
        /// </summary>
        void GoodsViewPresenter.IView.AddSlot(GoodsViewData data)
        {
            GameObject go;
            if (recyclebin.size != 0)
            {
                go = recyclebin.Pop();
            }
            else
            {
                go = NGUITools.AddChild(wrapper, prefab);
            }
            var slot = go.GetComponent<GoodsViewSlot>();
            go.SetActive(true);
            slot.SetData(presenter, data);
            slot.SetIndex(presenter.Slots.size);

            presenter.Slots.Add(slot);
        }

        void GoodsViewPresenter.IView.ReleaseSlot(GameObject go)
        {
            go.SetActive(false);
            recyclebin.Add(go);
        }
    }
}