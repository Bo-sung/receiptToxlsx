using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class ModeSelectView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;

        [SerializeField] UIButtonHelper btnMulti;
        [SerializeField] UILabelHelper labelTicketCount;
        [SerializeField] UIButtonHelper btnFreeFight;
        [SerializeField] UILabelValue labelRemainTime;
        [SerializeField] UILabelValue labelEnterTime;

        [SerializeField] UIButtonHelper btnHelp;
        [SerializeField] UILabelHelper labelDescription;

        public event System.Action OnSelectExit;
        public event System.Action OnSelectMaze;
        public event System.Action OnSelectFreeFight;
        public event System.Action OnSelectHelp;
        public event System.Action OnRequestFreeFightInfo;

        private bool isRequestFreeFightInfo;
        private bool canEnterFreeFight = true;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, OnClickedBtnExit);
            EventDelegate.Add(btnMulti.OnClick, OnClickedBtnMulti);
            EventDelegate.Add(btnFreeFight.OnClick, OnClickedBtnFreeFight);
            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, OnClickedBtnExit);
            EventDelegate.Remove(btnMulti.OnClick, OnClickedBtnMulti);
            EventDelegate.Remove(btnFreeFight.OnClick, OnClickedBtnFreeFight);
            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._49500; // 미궁 선택
            btnMulti.LocalKey = LocalizeKey._49501; // 멀티
            btnFreeFight.LocalKey = LocalizeKey._49502; // 난전
            labelRemainTime.TitleKey = LocalizeKey._49503; // 남은 시간
            labelEnterTime.TitleKey = LocalizeKey._49504; // 입장 가능
            labelDescription.LocalKey = LocalizeKey._49505; // 이벤트 멀티미궁 티켓은 자정(GMT+8)에 초기화 됩니다.
        }

        public override void Show()
        {
            base.Show();

            SetCanEnterFreeFight(false);
            RequestFreeFightInfo();
        }

        void OnClickedBtnExit()
        {
            OnSelectExit?.Invoke();
        }

        void OnClickedBtnMulti()
        {
            OnSelectMaze?.Invoke();
        }

        void OnClickedBtnFreeFight()
        {
            if (!canEnterFreeFight)
            {
                string message = LocalizeKey._49506.ToText(); // 입장 가능 시간이 아닙니다.
                UI.ShowToastPopup(message);
                return;
            }

            OnSelectFreeFight?.Invoke();
        }

        void OnClickedBtnHelp()
        {
            OnSelectHelp?.Invoke();
        }

        public void SetMultiMazeData(int eventFreeCount, int eventMaxCount)
        {
            labelTicketCount.Text = string.Concat(eventFreeCount, "/", eventMaxCount);
        }

        public void SetFreeFightData(System.DateTime startTime, System.DateTime endTime)
        {
            Timing.RunCoroutineSingleton(YieldRefreshTime(startTime, endTime).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void SetCanEnterFreeFight(bool value)
        {
            if (canEnterFreeFight == value)
                return;

            canEnterFreeFight = value;
            labelRemainTime.SetActive(!canEnterFreeFight);
            labelEnterTime.SetActive(canEnterFreeFight);

            UpdateFreeFightTimeText(System.TimeSpan.Zero);
        }

        private void UpdateFreeFightTimeText(System.TimeSpan timeSpan)
        {
            if (canEnterFreeFight)
            {
                labelEnterTime.Value = timeSpan.ToString(@"hh\:mm\:ss");
            }
            else
            {
                labelRemainTime.Value = timeSpan.ToString(@"hh\:mm\:ss");
            }
        }

        /// <summary>
        /// 입장 남은 시간 업데이트
        /// </summary>
        IEnumerator<float> YieldRefreshTime(System.DateTime startTime, System.DateTime endTime)
        {
            // 서버 시간이 존재하지 않을 경우
            if (!ServerTime.IsInitialize)
                yield break;

            while (true)
            {
                System.DateTime now = ServerTime.Now;

                // 이미 시즌이 지났을 경우
                if (now > endTime)
                {
                    SetCanEnterFreeFight(false);
                    break;
                }

                isRequestFreeFightInfo = false; // 시간 업데이트

                System.TimeSpan timeSpan;
                if (now < startTime)
                {
                    timeSpan = startTime - now;
                    SetCanEnterFreeFight(false);
                }
                else
                {
                    timeSpan = endTime - now;
                    SetCanEnterFreeFight(true);
                }

                UpdateFreeFightTimeText(timeSpan);
                yield return Timing.WaitForSeconds(1f);
            }

            RequestFreeFightInfo(); // 서버 시간 다시 요청
        }

        private void RequestFreeFightInfo()
        {
            if (isRequestFreeFightInfo)
                return;

            isRequestFreeFightInfo = true;
            OnRequestFreeFightInfo?.Invoke();
        }
    }
}