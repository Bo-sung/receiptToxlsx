using UnityEngine;

namespace Ragnarok.View.League
{
    /// <summary>
    /// <see cref="LeagueModelView"/>
    /// </summary>
    public class UILeagueGradeRewardInfo : UIView, IAutoInspectorFinder
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UILeagueGradeRewardBar element;

        private SuperWrapContent<UILeagueGradeRewardBar, UILeagueGradeRewardBar.IInput> wrapContent;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UILeagueGradeRewardBar, UILeagueGradeRewardBar.IInput>(element);
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(UILeagueGradeRewardBar.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }
    }
}