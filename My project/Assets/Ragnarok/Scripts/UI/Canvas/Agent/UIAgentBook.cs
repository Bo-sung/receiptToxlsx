using UnityEngine;
using System.Linq;
using System;

namespace Ragnarok
{
    public class AgentBookSlotViewInfo : IInfo
    {
        public bool IsInvalidData => false;

        public event Action OnUpdateEvent;

        public AgentBookState bookState;
        public AgentBookData bookData;

        public event Action<AgentBookData> OnClickReceiveReward;

        public void Update() { OnUpdateEvent?.Invoke(); }

        public void InvokeReceiveReward()
        {
            if (bookState.IsRewarded)
                return;
            OnClickReceiveReward?.Invoke(bookData);
        }
    }

    public class UIAgentBook : MonoBehaviour
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        private AgentBookPresenter presenter;
        private AgentBookSlotViewInfo[] bookSlotViewInfos;
        
        public void OnInit(AgentBookPresenter presenter)
        {
            this.presenter = presenter;
            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
        }

        public void OnClose()
        {

        }

        private void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<UIAgentBookSlot>();
            slot.SetData(bookSlotViewInfos[index]);
        }

        public void OnLocalize()
        {
        }

        public void ShowBookDatas(AgentBookSlotViewInfo[] agents)
        {
            bookSlotViewInfos = agents;

            for (int i = 0; i < bookSlotViewInfos.Length; ++i)
                bookSlotViewInfos[i].OnClickReceiveReward += OnClickReceiveReward;
            wrapper.Resize(bookSlotViewInfos.Length);
        }

        public void UpdateSlot(int bookID)
        {
            var viewInfo = bookSlotViewInfos.FirstOrDefault(v => v.bookData.id == bookID);
            if (viewInfo != null)
                viewInfo.Update();
        }

        private void OnClickReceiveReward(AgentBookData agentBookData)
        {
            presenter.RequestReceiveBookReward(agentBookData.id);
        }
    }
}