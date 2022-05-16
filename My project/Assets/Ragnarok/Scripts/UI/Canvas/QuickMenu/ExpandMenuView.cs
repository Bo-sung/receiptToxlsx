using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// 확장/축소 버튼을 가진 확장 스크롤뷰
    /// </summary>
    public class ExpandMenuView : UIView, IInspectorFinder
    {
        public enum State
        {
            Collapsed,
            Expanded,
        }

        [SerializeField] UISprite iconArrow;
        [SerializeField] UIButtonHelper btnExpandCollapse;
        [SerializeField] UIButtonHelper btnLockShare;
        [SerializeField] UIGridHelper grid;
        [SerializeField] ExpandMenuSlot[] slots;

        public int ListSize => slots.Length;

        public event System.Action<int> OnSelect;
        public event System.Action OnUpdate;

        /// <summary>
        /// 확장/축소 상태
        /// </summary>
        public State CurState { get; private set; }

        private int jobGrade;
        private int slotCount;
        private int cloneCount;

        protected override void Start()
        {
            base.Start();

            EventDelegate.Add(btnExpandCollapse.OnClick, OnClickedBtnExpandCollapse);
            EventDelegate.Add(btnLockShare.OnClick, OnClickedBtnLockShare);

            for (int i = 0; i < slots.Length; ++i)
            {
                slots[i].OnSelect += OnSelectSlot;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExpandCollapse.OnClick, OnClickedBtnExpandCollapse);
            EventDelegate.Remove(btnLockShare.OnClick, OnClickedBtnLockShare);

            for (int i = 0; i < slots.Length; ++i)
            {
                slots[i].OnSelect -= OnSelectSlot;
            }
        }

        protected override void OnLocalize()
        {
        }

        void OnClickedBtnExpandCollapse()
        {
            if (CurState == State.Collapsed)
            {
                SetExpansionState(State.Expanded);
            }
            else
            {
                SetExpansionState(State.Collapsed);
            }
        }

        void OnClickedBtnLockShare()
        {
            if (jobGrade > 0)
            {
                UI.Show<UIJobGrowth>();
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
            }
        }

        void OnSelectSlot(int index)
        {
            OnSelect?.Invoke(index);
        }

        public void SetExpansionState(State state)
        {
            if (CurState == state)
                return;

            CurState = state;
            OnUpdate?.Invoke();

            RefreshSlotCount();
        }

        public void SetJobGrade(int jobGrade, int slotCount, int cloneCount)
        {
            this.jobGrade = jobGrade;
            this.slotCount = slotCount;
            this.cloneCount = cloneCount;

            RefreshSlotCount();
        }

        public void SetThumnailName(int index, string thumbnailName)
        {
            slots[index].SetData(thumbnailName);
        }

        public void SetNoticeMode(int index, bool isShareNoticeMode)
        {
            slots[index].SetNoticeMode(isShareNoticeMode);
        }

        /// <summary>
        /// 현재 체력 % 설정
        /// </summary>
        public void SetShareCharacterCurrentHp(int index, int cur, int max, bool skipAnim = false)
        {
            slots[index].SetHpProgress(cur, max, skipAnim);
        }

        /// <summary>
        /// 셰어 캐릭터 부활 대기시간 설정
        /// </summary>
        public void SetShareCharacterReviveTime(int index, float reviveTime)
        {
            slots[index].SetReviveTime(reviveTime);
        }

        /// <summary>
        /// 셰어 캐릭터 조종 아이콘 설정
        /// </summary>
        public void SetShareCharacterSelectState(int index, bool isSelect)
        {
            slots[index].SetSelectState(isSelect);
        }

        private void RefreshSlotCount()
        {
            iconArrow.flip = CurState == State.Expanded ? UIBasicSprite.Flip.Nothing : UIBasicSprite.Flip.Horizontally;
            btnLockShare.SetActive(CurState == State.Expanded);

            // 슬롯 셋팅
            if (CurState == State.Expanded)
            {
                // 셰어 슬롯
                grid.SetValue(slotCount);

                // 클론 슬롯
                for (int i = 0; i < cloneCount; i++)
                {
                    slots[Constants.Size.SHARE_SLOT_SIZE + i].SetActiveGO(true);
                }

                // 슬롯위치 갱신
                grid.Reposition();
            }
            else
            {
                grid.SetValue(0);
            }
        }

        public void SetActiveExpandButton(bool value)
        {
            btnExpandCollapse.SetActive(value);
        }

        bool IInspectorFinder.Find()
        {
            slots = GetComponentsInChildren<ExpandMenuSlot>();
            return true;
        }
    }
}