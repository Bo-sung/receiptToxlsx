using UnityEngine;

namespace Ragnarok.View
{
    public class EventDuelServerRewardView : UIView
    {
        [SerializeField] UIDuelBuffReward perfect;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UILabelHelper labelNotice, labelDescription;

        UIDuelBuffReward.IInput[] arrData;

        protected override void Awake()
        {
            base.Awake();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnLocalize()
        {
            labelNoData.LocalKey = LocalizeKey._47918; // 보상 정보가 없습니다.
            labelNotice.LocalKey = LocalizeKey._47912; // 완전 승리 보상은 1개 서버만 남은 경우의 보상입니다.
            labelDescription.LocalKey = LocalizeKey._47913; // 기간 종료 시, 빼앗을 듀얼 조각이 많은 순으로 보상을 받습니다.
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIDuelBuffReward ui = go.GetComponent<UIDuelBuffReward>();
            ui.SetData(arrData[index], isResult: false);
        }

        public void SetData(UIDuelBuffReward.IInput perfectRank, UIDuelBuffReward.IInput[] normalRanks)
        {
            perfect.SetData(perfectRank, isResult: false);
            arrData = normalRanks;

            int dataSize = arrData == null ? 0 : arrData.Length;
            wrapper.Resize(dataSize);

            labelNoData.SetActive(dataSize == 0);
        }
    }
}