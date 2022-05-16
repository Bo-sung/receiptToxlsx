using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class DarkTreeInfo : IDarkTreeInfo
    {
        private const string TAG = nameof(DarkTreeInfo);

        private enum State
        {
            /// <summary>
            /// 진행 대기
            /// </summary>
            NONE = 1,
            /// <summary>
            /// 진행 중
            /// </summary>
            InProgress = 2,
            /// <summary>
            /// 보상 대기 중
            /// </summary>
            StandByReward = 3,
        }

        private State state;
        private ObscuredInt point;
        private DarkTreeRewardData data;
        private RemainTime remainTime;

        public event System.Action OnUpdate;

        public void ResetData()
        {
            state = State.NONE;
            remainTime = 0L;
            point = 0;
            data = null;

            Timing.KillCoroutines(TAG);

            OnUpdate?.Invoke();
        }

        public void Initialize(long time, DarkTreeRewardData data, int point)
        {
            const long NONE = 0L;
            const long STAND_BY_REWARD = -1L;

            switch (time)
            {
                case NONE:
                    state = State.NONE;
                    remainTime = 0L;
                    break;

                case STAND_BY_REWARD:
                    state = State.StandByReward;
                    remainTime = 0L;
                    break;

                default:
                    SetProgressTime(time);
                    break;
            }

            this.data = data;
            this.point = point;

            InvokeUpdateAction();
        }

        public void SelectReward(DarkTreeRewardData data)
        {
            if (state != State.NONE)
            {
                Debug.LogError($"유효하지 않은 처리: {nameof(State)} = {state}");
                return;
            }

            if (remainTime.ToRemainTime() > 0f)
            {
                Debug.LogError($"유효하지 않은 처리: {nameof(remainTime)} = {remainTime.ToStringTime()}");
                return;
            }

            if (point > 0)
            {
                Debug.LogError($"유효하지 않은 처리: {nameof(point)} = {point}");
                return;
            }

            if (data == null)
            {
                Debug.LogError("유효하지 않은 처리: data is Null");
                return;
            }

            this.data = data;

            InvokeUpdateAction();
        }

        public void PlusPoint(int plusPoint)
        {
            if (state != State.NONE)
            {
                Debug.LogError($"유효하지 않은 처리: {nameof(State)} = {state}");
                return;
            }

            if (data == null)
            {
                Debug.LogError("유효하지 않은 처리: data is Null");
                return;
            }

            if (plusPoint <= 0)
            {
                Debug.LogError($"유효하지 않은 처리: {nameof(plusPoint)} = {plusPoint}");
                return;
            }

            int curPoint = GetCurPoint();
            int maxPoint = GetMaxPoint();
            if (curPoint == maxPoint)
            {
                Debug.LogError($"유효하지 않은 처리: {nameof(curPoint)} = {curPoint}, {nameof(maxPoint)} = {maxPoint}");
                return;
            }

            point = curPoint + plusPoint;
            InvokeUpdateAction();
        }

        public void Run()
        {
            if (state != State.NONE)
            {
                Debug.LogError($"유효하지 않은 처리: {nameof(State)} = {state}");
                return;
            }

            if (data == null)
            {
                Debug.LogError("유효하지 않은 처리: data is Null");
                return;
            }

            int maxPoint = GetMaxPoint();
            int curPoint = GetCurPoint();
            if (curPoint < maxPoint)
            {
                Debug.LogError($"유효하지 않은 처리: {nameof(curPoint)} = {curPoint}, {nameof(maxPoint)} = {maxPoint}");
                return;
            }

            SetProgressTime(data.required_time);
            InvokeUpdateAction();
        }

        public int GetSelectedRewardId()
        {
            if (data == null)
                return 0;

            return data.Id;
        }

        public RewardData GetSelectedReward()
        {
            if (data == null)
                return null;

            return data.GetReward();
        }

        public int GetCurPoint()
        {
            int maxPoint = GetMaxPoint();
            return Mathf.Min(point, maxPoint);
        }

        public int GetMaxPoint()
        {
            if (data == null)
                return 0;

            return data.GetMaxPoint();
        }

        public int GetTotalMinutes()
        {
            if (data == null)
                return 0;

            return data.GetTotalMinutes();
        }

        public int GetRemainMinitues()
        {
            return (int)System.TimeSpan.FromMilliseconds(remainTime.ToRemainTime()).TotalMinutes;
        }

        private void SetProgressTime(long time)
        {
            state = State.InProgress;
            remainTime = time;

            Timing.KillCoroutines(TAG);
            Timing.RunCoroutine(YieldRemainTime().Append(InvokeUpdateAction), TAG);
        }

        public bool HasRemainTime()
        {
            return remainTime.ToRemainTime() > 0f;
        }

        /// <summary>
        /// 받을 보상이 있을 경우 true
        /// </summary>
        public bool HasStandByReward()
        {
            return state == State.StandByReward;
        }

        private void InvokeUpdateAction()
        {
            OnUpdate?.Invoke();
        }

        /// <summary>
        /// 남은시간 체크
        /// </summary>
        IEnumerator<float> YieldRemainTime()
        {
            yield return Timing.WaitUntilFalse(HasRemainTime);
            state = State.StandByReward;
        }
    }
}