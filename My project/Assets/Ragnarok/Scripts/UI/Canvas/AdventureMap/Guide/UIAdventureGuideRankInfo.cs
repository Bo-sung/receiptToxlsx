using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIAdventureGuideRankInfo : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] EnvelopContent background;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] GameObject prefab;

        GameObject goGrid;
        Transform tfGrid;

        private UIDuelReward[] prefabs;

        void Awake()
        {
            goGrid = rewardGrid.gameObject;
            tfGrid = rewardGrid.transform;
        }

        public void SetTitle(int localKey)
        {
            labelRewardTitle.LocalKey = localKey;
        }

        public void SetData(UIDuelReward.IInput[] arrData)
        {
            int size = arrData == null ? 0 : arrData.Length;
            for (int i = tfGrid.childCount; i < size; i++)
            {
                goGrid.AddChild(prefab); // 필요한 Prefab 만큼 추가
            }

            prefabs = goGrid.GetComponentsInChildren<UIDuelReward>(includeInactive: true);
            for (int i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].SetData(i < size ? arrData[i] : null);
            }

            rewardGrid.Reposition();
            background.Execute();
        }
    }
}