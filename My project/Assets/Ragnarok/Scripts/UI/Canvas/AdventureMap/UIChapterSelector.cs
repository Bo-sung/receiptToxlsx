using UnityEngine;

namespace Ragnarok.View
{
    public class UIChapterSelector : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIChapterElement element;

        private SuperWrapContent<UIChapterElement, UIChapterElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIChapterElement, UIChapterElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectChapter;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectChapter;
            }
        }

        protected override void OnLocalize()
        {
        }

        void OnSelectChapter(int chapter)
        {
            OnSelect?.Invoke(chapter);
        }

        public void SetData(UIChapterElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        public void InitProgress()
        {
            wrapContent.SetProgress(0f);
        }

        public void Move(int index)
        {
            wrapContent.Move(index);
        }
    }
}