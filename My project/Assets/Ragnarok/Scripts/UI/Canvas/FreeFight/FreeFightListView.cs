using UnityEngine;

namespace Ragnarok.View
{
    public sealed class FreeFightListView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIFreeFightElement element;

        private SuperWrapContent<UIFreeFightElement, UIFreeFightElement.IInput> wrapContent;

        public event System.Action<string, string[], UIFreeFightReward.IInput[], UIEventMazeSkill.IInput[]> OnHelp;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIFreeFightElement, UIFreeFightElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelectHelp += OnSelectHelp;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelectHelp -= OnSelectHelp;
            }
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(UIFreeFightElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        void OnSelectHelp(string title, string[] times, UIFreeFightReward.IInput[] rewards, UIEventMazeSkill.IInput[] skills)
        {
            OnHelp?.Invoke(title, times, rewards, skills);
        }
    }
}