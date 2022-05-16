using UnityEngine;

namespace Ragnarok.View
{
    public class ChatModeView : UIView, IAutoInspectorFinder
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        public SuperScrollListWrapper Wrapper => wrapper;

        public event System.Action<ChatMode> OnChatModeSelect;

        private ChatPresenter presenter;

        public void AddEvent()
        {
            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
        }

        public void RemoveEvent()
        {

        }

        protected override void OnLocalize()
        {
            
        }

        public void Initialize(ChatPresenter presenter)
        {
            this.presenter = presenter;
        }

        void OnElementRefresh(GameObject go, int dataIndex)
        {
            UIChatModeSlot uiChatModeSlot = go.GetComponent<UIChatModeSlot>();
            presenter.RefreshChatModeSlot(uiChatModeSlot, dataIndex);
        }

        public void Resize(int dataSize)
        {
            wrapper.Resize(dataSize);
            wrapper.SetProgress(0f);
        }

    }
}