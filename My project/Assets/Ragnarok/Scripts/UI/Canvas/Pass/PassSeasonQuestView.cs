using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="PassQuestView"/>
    /// </summary>
    public class PassSeasonQuestView : UIView
    {
        [SerializeField] UILabelHelper labelQuestTitle;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIPassQuestElement element;

        private SuperWrapContent<UIPassQuestElement, UIPassQuestElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UIPassQuestElement, UIPassQuestElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectElement;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectElement;
            }
        }

        protected override void OnLocalize()
        {
            labelQuestTitle.LocalKey = LocalizeKey._39810; // 패스 시즌 퀘스트
        }

        void OnSelectElement(int id)
        {
            OnSelect?.Invoke(id);
        }

        public void SetData(UIPassQuestElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }
    }
}