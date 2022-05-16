using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIOnBuffPass"/>
    /// </summary>
    public class OnBuffPassQuestView : UIView
    {
        [SerializeField] UILabelHelper labelQuestTitle;
        [SerializeField] UILabelHelper labelDailyQuestNotice;
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
            labelQuestTitle.LocalKey = LocalizeKey._39809; // 패스 퀘스트
            labelDailyQuestNotice.LocalKey = LocalizeKey._39815; // 매일 패스 일일 퀘스트가 초기화됩니다.
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