using UnityEngine;

namespace Ragnarok.View
{
    public sealed class ItemUnselectView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIItemSelectElement element;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UILabelHelper labelMessage;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIToggle checkBox;
        [SerializeField] UILabelHelper labelCheckNotice;

        private SuperWrapContent<UIItemSelectElement, UIItemSelectElement.IInput> wrapContent;
        private BetterList<ItemSelectElement> list;
        private bool hasTranscend;

        public event System.Action<bool> OnCheckSelect;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIItemSelectElement, UIItemSelectElement.IInput>(element);
            list = new BetterList<ItemSelectElement>();
            EventDelegate.Add(checkBox.onChange, OnChangeCheckBox);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (list != null)
            {
                foreach (var item in list)
                {
                    item.OnClickedSelect -= OnUpdateCheckView;
                }
                list.Release();
            }

            EventDelegate.Remove(checkBox.onChange, OnChangeCheckBox);
        }

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._6025; // 가치있는 아이템이 포함되어 있습니다.
            labelMessage.LocalKey = LocalizeKey._6026; // 모두 분해 하시겠습니까?
            labelDescription.LocalKey = LocalizeKey._6027; // 카드가 장착되어있는 장비아이템은 분해 시 카드가 자동으로 회수됩니다.
            labelCheckNotice.LocalKey = LocalizeKey._6028; // 분해 시 초월 장비 포함
        }

        public void SetData(ItemInfo[] inputs)
        {
            hasTranscend = false;

            if (list != null)
            {
                foreach (var item in list)
                {
                    item.OnClickedSelect -= OnUpdateCheckView;
                }
                list.Clear();
            }

            if (inputs == null)
            {
                wrapContent.SetData(null);
                return;
            }

            for (int i = 0; i < inputs.Length; i++)
            {
                ItemSelectElement temp = new ItemSelectElement(inputs[i]);
                temp.OnClickedSelect += OnUpdateCheckView;
                list.Add(temp);
            }

            wrapContent.SetData(list.ToArray());
            OnUpdateCheckView();
        }

        /// <summary>
        /// 선택되지 않은 아이템 No 리스트 반환
        /// </summary>
        public long[] GetUnselectNos()
        {
            Buffer<long> buffer = new Buffer<long>();
            for (int i = 0; i < list.size; i++)
            {
                if (list[i].IsSelect)
                    continue;

                buffer.Add(list[i].Info.ItemNo);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        public void OnUpdateCheckView()
        {
            hasTranscend = false;

            foreach (var item in list)
            {
                if (!item.IsSelect)
                    continue;

                if (item.Info.ItemTranscend > 0)
                {
                    hasTranscend = true;
                    break;
                }
            }

            if (hasTranscend)
            {
                checkBox.gameObject.SetActive(true);
                checkBox.value = false;
            }
            else
            {
                checkBox.gameObject.SetActive(false);
                checkBox.value = true;
            }
        }

        private void OnChangeCheckBox()
        {
            OnCheckSelect?.Invoke(checkBox.value);
        }

        private class ItemSelectElement : UIItemSelectElement.IInput
        {
            public ItemInfo Info { get; private set; }
            public bool IsSelect { get; private set; }

            public event System.Action OnClickedSelect;

            public ItemSelectElement(ItemInfo info)
            {
                Info = info;
                IsSelect = true;
            }

            public void ToggleSelect()
            {
                IsSelect = !IsSelect;
                OnClickedSelect?.Invoke();
            }
        }
    }
}