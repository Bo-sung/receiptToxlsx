using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Ragnarok
{
    public class UIBookListPanel : MonoBehaviour
    {
        public struct TabInfo
        {
            public string name;
            public object data;

            public TabInfo(string name, object data)
            {
                this.name = name;
                this.data = data;
            }
        }

        [SerializeField] GameObject[] selectionHighlights;
        [SerializeField] UITabHelper sideTab;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] Color toggleOnLabelColor;
        [SerializeField] Color toggleOffLabelColor;

        private bool invokeEvent = true;
        private TabInfo[] tabInfos;
        private List<BookStateDecoratedData> items;
        
        public event Action<object> OnClickSideTab;
        public event Action<BookStateDecoratedData> OnClickSlot;

        public void Init()
        {
            if (sideTab != null)
            {
                for (int i = 0; i < sideTab.Count; ++i)
                {
                    int index = i;
                    EventDelegate.Add(sideTab[index].OnChange, () =>
                    {
                        if (!invokeEvent)
                            return;

                        if (!sideTab[index].Value)
                            return;

                        for (int j = 0; j < sideTab.Count; ++j)
                        {
                            sideTab[j].SetLabelOutline(index == j);
                            selectionHighlights[j].SetActive(index == j);
                            sideTab[j].Label.uiLabel.color = index == j ? toggleOnLabelColor : toggleOffLabelColor;
                        }

                        if (tabInfos != null)
                            OnClickSideTab?.Invoke(tabInfos[index].data);
                    });
                }
            }

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
        }

        public void SetSideTabInfo(params TabInfo[] tabInfos)
        {
            this.tabInfos = tabInfos;
            for (int i = 0; i < sideTab.Count; ++i)
            {
                if (i < tabInfos.Length)
                {
                    sideTab[i].SetActive(true);
                    sideTab[i].Text = tabInfos[i].name;
                }
                else
                {
                    sideTab[i].SetActive(false);
                }
            }
        }

        public void SetSideTab(int index)
        {
            invokeEvent = false;
            for (int i = 0; i < sideTab.Count; ++i)
            {
                sideTab[i].Value = i == index;
                sideTab[i].SetLabelOutline(i == index);
                sideTab[i].Label.uiLabel.color = i == index ? toggleOnLabelColor : toggleOffLabelColor;
                selectionHighlights[i].SetActive(i == index);
            }
            invokeEvent = true;
        }

        public void ShowList(List<BookStateDecoratedData> items)
        {
            this.items = items;
            wrapper.Resize(items.Count);
            wrapper.SetProgress(0);
        }

        public void RefreshList()
        {
            var slots = wrapper.gameObject.GetComponentsInChildren<UIBookSlot>();
            foreach (var each in slots)
                each.Refresh();
        }

        private void OnElementRefresh(GameObject go, int index)
        {
            UIBookSlot slot = go.GetComponent<UIBookSlot>();

            if (items[index] != null)
                slot.SetData(index, items[index], InvokeOnClickSlot);
            else
                slot.SetEmpty();
        }

        private void InvokeOnClickSlot(int index)
        {
            OnClickSlot?.Invoke(items[index]);
        }
    }
}