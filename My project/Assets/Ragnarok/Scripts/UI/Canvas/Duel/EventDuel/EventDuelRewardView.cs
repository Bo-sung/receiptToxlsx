using UnityEngine;

namespace Ragnarok.View
{
    public class EventDuelRewardView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UILabelHelper labelNotice, labelDescription;

        private UIDuelReward.IInput[] arrData;

        protected override void Awake()
        {
            base.Awake();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnLocalize()
        {
            labelNoData.LocalKey = LocalizeKey._47918; // 보상 정보가 없습니다.
            labelNotice.LocalKey = LocalizeKey._47916; // 전체 순위, 서버 순위에 따라 각각 보상을 받을 수 있습니다.
            labelDescription.LocalKey = LocalizeKey._47917; // 기간 종료 시에 우편으로 일괄 지급됩니다.
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIDuelReward ui = go.GetComponent<UIDuelReward>();
            ui.SetData(arrData[index]);
        }

        public void SetData(UIDuelReward.IInput[] rewards)
        {
            arrData = rewards;

            int dataSize = arrData == null ? 0 : arrData.Length;
            wrapper.Resize(dataSize);

            labelNoData.SetActive(dataSize == 0);
        }
    }
}