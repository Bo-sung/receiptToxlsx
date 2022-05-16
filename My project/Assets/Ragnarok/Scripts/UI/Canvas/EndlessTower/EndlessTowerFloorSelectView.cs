using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class EndlessTowerFloorSelectView : UIView
    {
        public interface IInput
        {
            int GetFloor();
            RewardData GetSkipItem();
        }

        [SerializeField] UILabelHelper labelFloorLevel;
        [SerializeField] UIPressButton btnLeft, btnRight;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] GameObject rewardBase;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UILabelValue freeTime;
        [SerializeField] UIButtonHelper btnFree;
        [SerializeField] UIItemCostButtonHelper btnEnter;
        [SerializeField] UIButtonHelper btnHelp;

        private int curIndex;
        private IInput[] inputs;
        private int freeEntryCount, freeMaxEntryCount;

        public event System.Action<int> OnSelectFloor;
        public event System.Action OnSelectHelp;
        public event System.Action<int> OnSelectEnter;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnLeft.onClick, OnClickedBtnLeft);
            EventDelegate.Add(btnRight.onClick, OnClickedBtnRight);
            EventDelegate.Add(btnFree.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnLeft.onClick, OnClickedBtnLeft);
            EventDelegate.Remove(btnRight.onClick, OnClickedBtnRight);
            EventDelegate.Remove(btnFree.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnLocalize()
        {
            freeTime.TitleKey = LocalizeKey._39502; // 무료 지급까지 남은 시간
            btnEnter.LocalKey = LocalizeKey._39504; // 입장
            labelDescription.LocalKey = LocalizeKey._39505; // 추가 입장 재료가 필요하지 않습니다.
            labelNotice.LocalKey = LocalizeKey._39506; // 해당 층을 입장하기 위해서는 추가 입장 재료가 필요합니다.

            UpdateBtnFreeText();
        }

        void OnClickedBtnLeft()
        {
            Select(curIndex - 1);
        }

        void OnClickedBtnRight()
        {
            Select(curIndex + 1);
        }

        void OnClickedBtnEnter()
        {
            if (inputs == null)
                return;

            IInput input = inputs[curIndex];
            RewardData skipItem = input.GetSkipItem();
            int skipItemCount = skipItem == null ? 0 : skipItem.Count;
            OnSelectEnter?.Invoke(skipItemCount);
        }

        void OnClickedBtnHelp()
        {
            OnSelectHelp?.Invoke();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(IInput[] inputs, string needItemIcon)
        {
            this.inputs = inputs;
            btnEnter.SetItemIcon(needItemIcon);
            Select(0); // 첫번째 index 세팅
        }

        /// <summary>
        /// 무료입장 수 업데이트
        /// </summary>
        public void SetFreeEntryCount(int freeEntryCount, int freeMaxEntryCount)
        {
            this.freeEntryCount = freeEntryCount;
            this.freeMaxEntryCount = freeMaxEntryCount;

            bool canFreeEntry = this.freeEntryCount > 0; // 무료 입장 가능
            btnFree.SetActive(canFreeEntry);
            btnEnter.SetActive(!canFreeEntry);
            UpdateBtnFreeText();
        }

        /// <summary>
        /// 무료입장까지 남은 쿨타임 세팅
        /// </summary>
        public void SetFreeEntryCooldownTime(RemainTime cooldownTime)
        {
            Timing.RunCoroutine(YieldCoolTime(cooldownTime).CancelWith(gameObject));
        }

        /// <summary>
        /// 입장권 수 업데이트
        /// </summary>
        public void SetTicketCount(int ticketCount)
        {
            btnEnter.SetItemCount(ticketCount);
        }

        private void Select(int index)
        {
            if (inputs == null)
                return;

            int maxIndex = inputs.Length - 1;

            curIndex = MathUtils.Clamp(index, 0, maxIndex);
            btnLeft.isEnabled = curIndex > 0;
            btnRight.isEnabled = curIndex < maxIndex;

            IInput input = inputs[curIndex];
            int floor = input.GetFloor();
            labelFloorLevel.Text = LocalizeKey._39500.ToText() // {INDEX}층
                .Replace(ReplaceKey.INDEX, input.GetFloor());

            RewardData skipItem = input.GetSkipItem();
            bool isNeedSkipItem = skipItem != null;
            labelDescription.SetActive(!isNeedSkipItem);
            rewardBase.SetActive(isNeedSkipItem);
            reward.SetData(skipItem);
            labelNotice.SetActive(isNeedSkipItem);

            OnSelectFloor?.Invoke(floor);
        }
        
        private void UpdateBtnFreeText()
        {
            if (freeMaxEntryCount > 1)
            {
                btnFree.Text = StringBuilderPool.Get()
                    .Append(LocalizeKey._39503.ToText()) // 무료 입장
                    .Append(freeEntryCount).Append("/").Append(freeMaxEntryCount)
                    .Release();
            }
            else
            {
                btnFree.LocalKey = LocalizeKey._39503; // 무료 입장
            }
        }

        IEnumerator<float> YieldCoolTime(RemainTime cooldownTime)
        {
            // 무료 입장이 없을 경우
            while (freeEntryCount == 0)
            {
                freeTime.Value = cooldownTime.ToRemainTime().ToStringTimeConatinsDay();
                yield return Timing.WaitForSeconds(0.1f);
            }

            freeTime.Value = "00:00:00";
        }
    }
}