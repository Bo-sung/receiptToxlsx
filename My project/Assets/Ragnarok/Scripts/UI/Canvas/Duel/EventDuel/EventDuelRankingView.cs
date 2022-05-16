using UnityEngine;

namespace Ragnarok.View
{
    public class EventDuelRankingView : UIView
    {
        [SerializeField] UILabelHelper labelMyRank;
        [SerializeField] UIDuelRank myRank;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelNoData;

        UIDuelRank.IInput[] arrData;

        protected override void Awake()
        {
            base.Awake();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnLocalize()
        {
            labelMyRank.LocalKey = LocalizeKey._47922; // 나의 순위
            labelNoData.LocalKey = LocalizeKey._47924; // 순위 정보가 없습니다.
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIDuelRank ui = go.GetComponent<UIDuelRank>();
            ui.SetData(arrData[index]);
        }

        public void SetData(UIDuelRank.IInput myRankData, UIDuelRank.IInput[] ranks)
        {
            myRank.SetData(myRankData);
            arrData = ranks;

            int dataSize = arrData == null ? 0 : arrData.Length;
            wrapper.Resize(dataSize);

            labelNoData.SetActive(dataSize == 0);
        }
    }
}