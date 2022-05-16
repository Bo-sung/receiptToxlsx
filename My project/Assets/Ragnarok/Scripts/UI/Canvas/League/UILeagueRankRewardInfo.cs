using UnityEngine;

namespace Ragnarok.View.League
{
    /// <summary>
    /// <see cref="LeagueModelView"/>
    /// </summary>
    public class UILeagueRankRewardInfo : UIView, IAutoInspectorFinder
    {
        private const int TAB_SINGLE = 0; // 싱글 보상
        private const int TAB_AGNET = 1; // 협동 보상       

        [SerializeField] UITabHelper tabReward;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UILeagueRankRewardBar element;

        private SuperWrapContent<UILeagueRankRewardBar, UILeagueRankRewardBar.IInput> wrapContent;

        public event System.Action<int> OnSelectTab;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UILeagueRankRewardBar, UILeagueRankRewardBar.IInput>(element);

            tabReward.OnSelect += InvokeOnSelectTab;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            tabReward.OnSelect -= InvokeOnSelectTab;
        }

        protected override void OnLocalize()
        {
            tabReward[TAB_SINGLE].LocalKey = LocalizeKey._47040; // 싱글 대전 보상
            tabReward[TAB_AGNET].LocalKey = LocalizeKey._47041; // 협동 대전 보상
        }

        public void SetData(UILeagueRankRewardBar.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
            wrapContent.SetProgress(0);
        }

        void InvokeOnSelectTab(int index)
        {
            OnSelectTab?.Invoke(index);
        }
    }
}