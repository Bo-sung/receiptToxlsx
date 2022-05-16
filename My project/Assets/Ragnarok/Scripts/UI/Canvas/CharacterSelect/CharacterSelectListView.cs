using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UICharacterSelect"/>
    /// </summary>
    public class CharacterSelectListView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UICharacterSelectElement element;

        private SuperWrapContent<UICharacterSelectElement, UICharacterSelectElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;
        public event System.Action OnCreate;
        public event System.Action OnFinishRemainTime;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UICharacterSelectElement, UICharacterSelectElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += InvokeSelect;
                item.OnCreate += InvokeCreate;
                item.OnFinishRemainTime += InvokeFinishRemainTime;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in wrapContent)
            {
                item.OnSelect -= InvokeSelect;
                item.OnCreate -= InvokeCreate;
                item.OnFinishRemainTime -= InvokeFinishRemainTime;
            }
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._1006; // 나의 캐릭터
        }

        public void SetData(UICharacterSelectElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        public void MoveTo(int index)
        {
            wrapContent.Move(index);
        }

        private void InvokeSelect(int cid)
        {
            OnSelect?.Invoke(cid);
        }

        private void InvokeCreate()
        {
            OnCreate?.Invoke();
        }

        private void InvokeFinishRemainTime()
        {
            OnFinishRemainTime?.Invoke();
        }
    }
}
