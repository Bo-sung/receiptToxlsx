using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="PassQuestView"/>
    /// </summary>
    public class PassDailyQuestView : UIView
    {
        [SerializeField] UILabelHelper labelQuestTitle;
        [SerializeField] UILabelHelper labelDailyQuestNotice;
        [SerializeField] UIPassQuestElement[] elements;

        public event System.Action<int> OnSelect;

        protected override void OnLocalize()
        {
            labelQuestTitle.LocalKey = LocalizeKey._39809; // 패스 퀘스트
            labelDailyQuestNotice.LocalKey = LocalizeKey._39815; // 매일 패스 일일 퀘스트가 초기화됩니다.
        }

        protected override void Awake()
        {
            base.Awake();
            foreach (var item in elements)
            {
                item.OnSelect += OnSelectElement;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in elements)
            {
                item.OnSelect -= OnSelectElement;
            }
        }

        void OnSelectElement(int id)
        {
            OnSelect?.Invoke(id);
        }

        public void SetData(UIPassQuestElement.IInput[] inputs)
        {
            Debug.Assert(elements.Length == inputs.Length, "패스 일일 퀘스트 목록 불일치");
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].SetData(inputs[i]);
            }
        }
    }
}