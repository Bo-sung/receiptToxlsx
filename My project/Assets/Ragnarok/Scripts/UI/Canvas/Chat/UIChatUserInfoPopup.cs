using UnityEngine;

namespace Ragnarok
{
    public class UIChatUserInfoPopup : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIButtonHelper btnInfo;
        [SerializeField] UIButtonHelper btnChat;
        [SerializeField] UIButtonHelper btnReport;

        public delegate void SelectEvent(int uid, int cid);
        public delegate void ChatEvent(int uid, int cid, string nickname);
        public delegate void ReportEvent(int uid, int cid, string nickname);

        public event SelectEvent OnSelect;
        public event ChatEvent OnChat;
        public event ReportEvent OnReport;
        public event System.Action OnDisabled;

        private UIChatMessage info;

        private Transform cachedTransform;
        private Transform CachedTransform
        {
            get
            {
                if (cachedTransform == null)
                    cachedTransform = transform;
                return cachedTransform;
            }
        }

        void Start()
        {
            EventDelegate.Add(btnInfo.OnClick, OnClickedBtnInfo);
            EventDelegate.Add(btnChat.OnClick, OnClickedBtnChat);
            EventDelegate.Add(btnReport.OnClick, OnClickedBtnReport);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(btnInfo.OnClick, OnClickedBtnInfo);
            EventDelegate.Remove(btnChat.OnClick, OnClickedBtnChat);
            EventDelegate.Remove(btnReport.OnClick, OnClickedBtnReport);
        }

        public void SetData(UIChatMessage uiChatMessage)
        {
            this.info = uiChatMessage;

            UILabel labelName = uiChatMessage.GetLabelName();
            float textWidth = labelName.printedSize.x;
            CachedTransform.SetParent(labelName.cachedTransform);
            NGUITools.MarkParentAsChanged(gameObject);
            CachedTransform.localPosition = Vector3.right * textWidth;
            gameObject.SetActive(true);
        }

        public void IsShow(UIChatMessage uiChatMessage)
        {
            if (this.info == null)
                return;

            if (this.info.Info == uiChatMessage.Info)
            {
                SetData(uiChatMessage);
            }
        }

        private void OnDisable()
        {
            info = null;
            OnDisabled?.Invoke();
        }

        void OnClickedBtnInfo()
        {
            OnSelect?.Invoke(this.info.Info.uid, this.info.Info.cid);
        }

        void OnClickedBtnChat()
        {
            OnChat?.Invoke(this.info.Info.uid, this.info.Info.cid, this.info.Info.name);
        }

        void OnClickedBtnReport()
        {
            OnReport?.Invoke(this.info.Info.uid, this.info.Info.cid, this.info.Info.name);
        }
    }
}