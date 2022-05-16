using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIDungeonDifficulty : UIView, IInspectorFinder
    {
        [SerializeField] UIToggleWithLock[] toggles;
        [SerializeField] GameObject[] slots; // 선택중인 슬롯
        [SerializeField] UILabelHelper[] labelClears; // 클리어 슬롯

        public event System.Action<DungeonDetailElement> OnSelect;

        private DungeonDetailElement[] elements;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < toggles.Length; i++)
            {
                EventDelegate.Add(toggles[i].OnChange, OnChangedToggle);
                toggles[i].OnSelectLock += OnSelectLock;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            for (int i = 0; i < toggles.Length; i++)
            {
                EventDelegate.Remove(toggles[i].OnChange, OnChangedToggle);
                toggles[i].OnSelectLock -= OnSelectLock;
            }
        }

        protected override void OnLocalize()
        {
            foreach (var item in labelClears)
            {
                item.LocalKey = LocalizeKey._7056; // CLEAR!
            }
        }

        void OnChangedToggle()
        {
            if (!UIToggle.current.value)
                return;

            int index = GetCurrentToggleIndex();
            OnSelect?.Invoke(index < elements.Length ? elements[index] : null);
        }

        void OnSelectLock(int index)
        {
            if (elements == null)
                return;

            if (index < 0 || index >= elements.Length)
                return;

            elements[index].IsOpenedDungeon(isShowNotice: true);
        }

        private int GetCurrentToggleIndex()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i].Value)
                    return i;
            }

            return 0;
        }

        public void SetData(DungeonDetailElement[] elements)
        {
            this.elements = elements;
            Refresh();
        }

        private void Refresh()
        {
            int count = elements == null ? 0 : elements.Length;
            for (int i = 0; i < toggles.Length; i++)
            {
                if (i < count)
                {
                    toggles[i].SetActive(true);

                    bool isOpen = elements[i].IsOpenedDungeon(isShowNotice: false);
                    toggles[i].SetActiveLock(!isOpen);
                    toggles[i].IsEnable = isOpen;
                    toggles[i].Set(false, false); // Notify 해제

                    slots[i].SetActive(isOpen);
                    labelClears[i].SetActive(elements[i].IsCleardDungeon());
                }
                else
                {
                    toggles[i].SetActive(false);
                    labelClears[i].SetActive(false);
                }

                toggles[i].Text = LocalizeKey._7048.ToText() // Lv.{LEVEL}
                    .Replace(ReplaceKey.LEVEL, elements[i].Difficulty);
            }

            if (count > 0)
            {
                int clearedDifficulty = elements[0].GetClearedDifficulty();
                int index = MathUtils.Clamp(clearedDifficulty, 0, toggles.Length);

                // 배열 범위 초과 방지
                index = Mathf.Min(index, elements.Length - 1);

                // 진입 불가능할 경우에는 이전 index 로 세팅
                if (index > 0 && !elements[index].IsOpenedDungeon(isShowNotice: false))
                    --index;

                toggles[index].Value = true;
            }
        }

        bool IInspectorFinder.Find()
        {
            toggles = GetComponentsInChildren<UIToggleWithLock>();

            if (toggles.Length > 0)
            {
                slots = new GameObject[toggles.Length];
                labelClears = new UILabelHelper[toggles.Length];
                for (int i = 0; i < toggles.Length; i++)
                {
                    slots[i] = toggles[i].transform.Find("Slot").gameObject;
                    labelClears[i] = toggles[i].transform.Find("LabelClear").GetComponent<UILabelHelper>();
                }
            }
            return true;
        }
    }
}