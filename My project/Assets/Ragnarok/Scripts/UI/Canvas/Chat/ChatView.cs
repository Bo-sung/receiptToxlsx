using UnityEngine;

namespace Ragnarok.View
{
    public class ChatView : UIView, IAutoInspectorFinder
    {
        [SerializeField] SuperScrollTableWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIChatUserInfoPopup userInfoPopup;

        public SuperScrollTableWrapper Wrapper => wrapper;

        private ChatPresenter presenter;

        public void AddEvent()
        {
            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
        }

        public void RemoveEvent()
        {

        }

        public override void Show()
        {
            base.Show();


        }

        public void Initialize(ChatPresenter presenter)
        {
            this.presenter = presenter;
        }

        private void OnElementRefresh(GameObject go, int dataIndex)
        {
            UIChatMessage uiChatMsg = go.GetComponent<UIChatMessage>();
            presenter.RefreshChatSlot(uiChatMsg, dataIndex);
            userInfoPopup.IsShow(uiChatMsg);
        }

        protected override void OnLocalize()
        {

        }

        public void ResetPosition()
        {
            wrapper.ScrollView.ResetPosition();
        }

        public void Resize(int dataSize, int progress)
        {
            wrapper.Resize(dataSize, progress);
        }
    }
}