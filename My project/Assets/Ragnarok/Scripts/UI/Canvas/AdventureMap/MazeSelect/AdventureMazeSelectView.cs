using UnityEngine;

namespace Ragnarok.View
{
    public class AdventureMazeSelectView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIAdventureMazeElement element;

        private SuperWrapContent<UIAdventureMazeElement, UIAdventureMazeElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIAdventureMazeElement, UIAdventureMazeElement.IInput>(element);
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
        }

        void OnSelectElement(int id)
        {
            OnSelect?.Invoke(id);
        }

        public void SetData(UIAdventureMazeElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        public void InitChapterProgress()
        {
            wrapContent.SetProgress(0f);
        }

        public void MoveTo(int index)
        {
            wrapContent.Move(index);
        }

        #region Tutorial
        public void SetScrollViewEnable(bool isEnable)
        {
            wrapper.ScrollView.enabled = isEnable;
        }

        public UIWidget GetElementWidget(int id)
        {
            foreach (var item in wrapContent)
            {
                if (item.GetId() == id)
                    return item.GetComponent<UIWidget>();
            }

            return null;
        }
        #endregion
    }
}