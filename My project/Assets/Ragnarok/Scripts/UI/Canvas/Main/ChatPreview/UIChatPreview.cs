using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIChatPreview : UICanvas
    {
        [SerializeField] GameObject viewRoot;
        [SerializeField] GameObject prefab;
        [SerializeField] GameObject wrapper;
        [SerializeField] UIButton toggleButton;

        [SerializeField] GameObject toggleOn;
        [SerializeField] GameObject toggleOff;

        [SerializeField] GameObject widgetSpawnArea;
        [SerializeField] GameObject widgetTopArea;
        [SerializeField] Vector3 spawnPosInterval;

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        private Queue<(ChatInfo, ChatMode)> chatQueue = new Queue<(ChatInfo, ChatMode)>();
        private LinkedList<ChatPreviewSlot> slotList = new LinkedList<ChatPreviewSlot>();

        private ChatPreviewPresenter presenter;

        public bool IsShowing => viewRoot.activeSelf;

        protected override void OnInit()
        {
            presenter = new ChatPreviewPresenter(this);
            presenter.AddEvent();

            for (int i = 0; i < 3; ++i)
            {
                GameObject go = NGUITools.AddChild(wrapper, prefab);
                var slot = go.GetComponent<ChatPreviewSlot>();
                slot.Init();
                slotList.AddLast(slot);
            }

            EventDelegate.Add(toggleButton.onClick, Toggle);
            SetActive(true);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(toggleButton.onClick, Toggle);
        }

        private void Toggle()
        {
            SetActive(!IsShowing);
        }

        protected override void OnLocalize()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            chatQueue.Clear();
            foreach (var each in slotList)
                each.Init();
            presenter.SetActive(IsShowing);
        }

        protected override void OnHide()
        {
            presenter.SetActive(false);
        }

        public void SetActive(bool value)
        {
            viewRoot.gameObject.SetActive(value);
            toggleOn.SetActive(value);
            toggleOff.SetActive(!value);
            presenter.SetActive(value);

            if (value)
            {
                chatQueue.Clear();
                foreach (var each in slotList)
                    each.Init();
            }
        }

        public void EnqueueChat(ChatInfo chat, ChatMode chatMode)
        {
            chatQueue.Enqueue((chat, chatMode));
        }

        private void Update()
        {
            if (chatQueue.Count > 0)
            {
                foreach (var each in slotList)
                {
                    if (each.CurState == ChatPreviewSlot.State.None)
                    {
                        slotList.Remove(each);
                        slotList.AddLast(each);
                        each.SetData(chatQueue.Dequeue(), widgetSpawnArea.transform.position);
                        break;
                    }
                }
            }

            int index = 0;
            foreach (var each in slotList)
                if (each.CurState != ChatPreviewSlot.State.None)
                    each.SetPos(GetTopPositionByIndex(index++));
        }

        private Vector3 GetTopPositionByIndex(int index)
        {
            return widgetTopArea.transform.position + (spawnPosInterval * index);
        }
    }
}
