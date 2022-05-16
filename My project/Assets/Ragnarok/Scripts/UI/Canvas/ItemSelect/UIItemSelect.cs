using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class UIItemSelect : UICanvas
    {
        public class Input : IUIData
        {
            public Func<ItemInfo, bool> filter;
            public Func<ItemInfo, ItemInfo, int> sort;
            public Action<List<ItemInfo>> onSelectionFinished;
            public Func<ItemInfo, Task<bool>> preSelection;
            public int targetSelectionCount;
        }

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper emptyNotice;
        [SerializeField] UICostButtonHelper okButton;
        [SerializeField] UIButtonHelper closeButton;

        private Input input;
        private List<UIItemSelectSlot.Input> slotInputList;
        private List<UIItemSelectSlot.Input> selected;

        protected override void OnInit()
        {
            EventDelegate.Add(okButton.OnClick, OnClickOK);
            EventDelegate.Add(closeButton.OnClick, OnClickClose);

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(okButton.OnClick, OnClickOK);
            EventDelegate.Remove(closeButton.OnClick, OnClickClose);
        }

        protected override void OnShow(IUIData data = null)
        {
            input = data as Input;

            slotInputList = new List<UIItemSelectSlot.Input>();

            var itemList = Entity.player.Inventory.itemList.FindAll(Filter);
            foreach (var each in itemList)
                slotInputList.Add(new UIItemSelectSlot.Input()
                {
                    info = each,
                    isSelected = false
                });


            if (input != null && input.sort != null)
                slotInputList.Sort(Sort);

            wrapper.Resize(slotInputList.Count);
            emptyNotice.gameObject.SetActive(slotInputList.Count == 0);

            okButton.IsEnabled = false;
            okButton.CostText = $"{0}/{input.targetSelectionCount}";
            selected = new List<UIItemSelectSlot.Input>();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            emptyNotice.LocalKey = LocalizeKey._22300;
            titleLabel.LocalKey = LocalizeKey._22302;
            okButton.LocalKey = LocalizeKey._22303;
        }

        private bool Filter(ItemInfo item)
        {
            if (input == null || input.filter == null)
                return true;
            return input.filter(item);
        }

        private int Sort(UIItemSelectSlot.Input a, UIItemSelectSlot.Input b)
        {
            return input.sort(a.info, b.info);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIItemSelectSlot slot = go.GetComponent<UIItemSelectSlot>();
            slot.SetData(slotInputList[index], OnSelectItem);
        }

        private async void OnSelectItem(UIItemSelectSlot slot, UIItemSelectSlot.Input slotInput)
        {
            bool result = slotInput.isSelected ? true : await input.preSelection(slotInput.info);

            if (!slotInput.isSelected && input.preSelection != null && !result)
                return;

            slotInput.isSelected = !slotInput.isSelected;
            slotInput.onUpdate();

            if (slotInput.isSelected)
            {
                selected.Add(slotInput);

                if (selected.Count > input.targetSelectionCount)
                {
                    selected[0].isSelected = false;
                    selected[0].onUpdate();
                    selected.RemoveAt(0);
                }
            }
            else
            {
                int index = selected.FindIndex(v => v == slotInput);
                selected.RemoveAt(index);
            }

            okButton.IsEnabled = selected.Count == input.targetSelectionCount;
            okButton.CostText = $"{selected.Count}/{input.targetSelectionCount}";
        }

        private void OnClickOK()
        {
            if (selected.Count < input.targetSelectionCount)
            {
                UI.ShowToastPopup(LocalizeKey._22301.ToText());
                return;
            }
            else
            {
                List<ItemInfo> result = new List<ItemInfo>(selected.Count);
                for (int i = 0; i < selected.Count; ++i)
                    result.Add(selected[i].info);
                input.onSelectionFinished?.Invoke(result);
                UI.Close<UIItemSelect>();
            }
        }

        private void OnClickClose()
        {
            UI.Close<UIItemSelect>();
        }
    }
}