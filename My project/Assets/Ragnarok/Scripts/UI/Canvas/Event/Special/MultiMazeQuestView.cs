using UnityEngine;

namespace Ragnarok.View
{
    public class MultiMazeQuestView : UIView, IInspectorFinder
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIMultiMazeQuestElement element;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper LabelDescription;
        [SerializeField] UILabelHelper labelNotice;

        private SuperWrapContent<UIMultiMazeQuestElement, UIMultiMazeQuestElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UIMultiMazeQuestElement, UIMultiMazeQuestElement.IInput>(element);
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
            labelTitle.LocalKey = LocalizeKey._5423; // 미궁 정복자 이벤트!
            LabelDescription.LocalKey = LocalizeKey._5424; // 미궁을 정복하고 특별한 보상을 얻을 수 있는 기회!
            labelNotice.LocalKey = LocalizeKey._5425; // 캐릭터당으로 보상을 수령할 수 있습니다.
        }

        void OnSelectElement(int id)
        {
            OnSelect?.Invoke(id);
        }

        public void SetData(UIMultiMazeQuestElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        bool IInspectorFinder.Find()
        {
            element = GetComponentInChildren<UIMultiMazeQuestElement>(true);
            return true;
        }

        #region Tutorial
        public UIWidget GetBtnCompleteWidget()
        {
            foreach (UIMultiMazeQuestElement item in wrapContent)
            {
                if (item.CanComplete)
                    return item.GetBtnComplete();
            }

            return null;
        }
        #endregion
    }
}