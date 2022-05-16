using UnityEngine;

namespace Ragnarok.View
{
    public class BuffInfoListView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        ConsumableInfoSlot.Info[] infos;

        protected override void Awake()
        {
            base.Awake();
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnLocalize()
        {

        }

        public void Set(ConsumableInfoSlot.Info[] infos)
        {
            this.infos = infos;
            wrapper.Resize(infos.Length);
        }

        void OnItemRefresh(GameObject go, int dataIndex)
        {
            ConsumableInfoSlot ui = go.GetComponent<ConsumableInfoSlot>();
            ui.Set(infos[dataIndex]);
        }
    }
}