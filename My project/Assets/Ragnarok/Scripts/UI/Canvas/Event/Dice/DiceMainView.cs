using UnityEngine;

namespace Ragnarok.View
{
    public sealed class DiceMainView : UIView, IInspectorFinder
    {
        private const string SFX_PORING_JUMP = "poring_damage";

        private const float TWEEN_MAX_DURATION = 0.5f; // 최대 시간
        private const float TWEEN_MIN_DURATION = 0.2f; // 최소 시간
        private const int TWEEN_DECREASE_PER_COUNT = 4; // 4칸마다 속도가 줄어듦
        private const float TWEEN_DECREASE_SPEED = 0.1f; // 0.1초로 속도가 줄어듦

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UIButtonHelper btnRoll;
        [SerializeField] UIButtonWithIcon btnCost;
        [SerializeField] UITextureHelper iconItem;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIMonopolyTile[] tiles;
        [SerializeField] UIPlayTween tween;
        [SerializeField] TweenPosition tweenPos;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UILabelHelper labelComplete;
        [SerializeField] UIDiceCompleteElement[] completeElements;
        [SerializeField] GameObject blind;
        [SerializeField] UIPlayTween tweenPoringIdle;
        [SerializeField] UILabelHelper labelNotice2;

        public event System.Action OnSelectExit;
        public event System.Action OnSelectRoll;
        public event System.Action OnSelectCost;
        public event System.Action OnPassByHome; // 완주지점 통과 (완주 횟수 Refresh 하기 위해 필요)
        public event System.Action OnFinishedTweenMove; // 트윈 종료
        public event System.Action OnSelectCompleteReceive; // 완료 보상 받기

        SoundManager soundManager;
        UITweener[] tweens;

        private int needCoinCount; // 필요 코인
        private int eventCoinMaxCount; // 최대 코인
        private int currentPoint; // 현재 위치한 point
        private bool isDoubleState; // 더블 여부
        private int completeCount; // 완주 횟수

        private bool isForwardStep; // 주사위 이동 방향
        private int stepPlayCount; // 주사위 이동 연출 수
        private bool isPlay = true; // 플레이 여부

        public bool IsPlay
        {
            get { return isPlay; }
            set
            {
                if (isPlay == value)
                    return;

                isPlay = value;
                NGUITools.SetActive(blind, isPlay); // 연출 중에는 다른 곳을 클릭할 수 없도록 처리
                PlayPoringIdle(!isPlay); // 포링 기본자세 연출
            }
        }

        protected override void Awake()
        {
            base.Awake();

            soundManager = SoundManager.Instance;
            tweens = GetComponentsInChildren<UITweener>();

            EventDelegate.Add(btnExit.onClick, OnClickedBtnExit);
            EventDelegate.Add(btnRoll.OnClick, OnClickedBtnRoll);
            EventDelegate.Add(btnCost.OnClick, OnClickedBtnCost);
            EventDelegate.Add(tween.onFinished, OnFinishedTween);

            foreach (var item in completeElements)
            {
                item.OnSelectReceive += OnSelectReceive;
            }

            IsPlay = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, OnClickedBtnExit);
            EventDelegate.Remove(btnRoll.OnClick, OnClickedBtnRoll);
            EventDelegate.Remove(btnCost.OnClick, OnClickedBtnCost);
            EventDelegate.Remove(tween.onFinished, OnFinishedTween);

            foreach (var item in completeElements)
            {
                item.OnSelectReceive -= OnSelectReceive;
            }
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._11300; // 포링의 기묘한 모험!
            labelNotice.LocalKey = LocalizeKey._11303; // 포링의 집에서 멈추면 특별한 보상을 받을 수 있습니다.

            UpdateNotice2Text();
            UpdateDescription();
            UpdateCompleteText();
        }

        void OnClickedBtnExit()
        {
            OnSelectExit?.Invoke();
        }

        void OnClickedBtnRoll()
        {
            OnSelectRoll?.Invoke();
        }

        void OnClickedBtnCost()
        {
            OnSelectCost?.Invoke();
        }

        void OnFinishedTween()
        {
            if (tiles[currentPoint].TileType == UIMonopolyTile.MonopolyTileType.Home)
            {
                OnPassByHome?.Invoke(); // 완주지점 통과
            }

            Tween();
        }

        void OnSelectReceive()
        {
            OnSelectCompleteReceive?.Invoke();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(int needCoinCount, int eventCoinMaxCount, string iconName, UIDiceCompleteElement.IInput[] inputs)
        {
            this.needCoinCount = needCoinCount;
            this.eventCoinMaxCount = eventCoinMaxCount;
            iconItem.SetItem(iconName);

            for (int i = 0, dataMax = inputs == null ? 0 : inputs.Length; i < completeElements.Length; i++)
            {
                completeElements[i].Initialize(i);
                completeElements[i].SetData(i < dataMax ? inputs[i] : null);
            }

            UpdateNotice2Text();
        }

        /// <summary>
        /// 재화 코인 수 세팅
        /// </summary>
        public void SetItemCostCount(int count)
        {
            btnCost.Text = count.ToString("N0");
            btnCost.SetActiveIcon(count >= eventCoinMaxCount);
        }

        /// <summary>
        /// 타일 세팅
        /// </summary>
        public void SetTileData(UIMonopolyTile.IInput[] data)
        {
            for (int i = 0, index = 0; i < tiles.Length; i++)
            {
                // 이벤트 타입의 타일은 세팅하지 않음
                if (tiles[i].TileType == UIMonopolyTile.MonopolyTileType.Event)
                    continue;

                tiles[i].SetData(index < tiles.Length ? data[index] : null);
                ++index;
            }
        }

        /// <summary>
        /// 더블 여부
        /// </summary>
        public void SetDoubleState(bool isDoubleState)
        {
            this.isDoubleState = isDoubleState;
            UpdateDescription();
        }

        /// <summary>
        /// 포인트 세팅과 동시에 위치 이동
        /// </summary>
        public void SetPoint(int point)
        {
            currentPoint = point - 1;
            tweenPos.value = GetCurrentTweenPosition();
        }

        /// <summary>
        /// 완주 정보 세팅
        /// </summary>
        public void SetCompleteInfo(int completeCount, int completeRewardStep)
        {
            this.completeCount = completeCount;

            foreach (var item in completeElements)
            {
                item.SetRewardStep(completeCount, completeRewardStep);
            }

            UpdateCompleteText();
        }

        /// <summary>
        /// 변화 Step 세팅 (Play 필요)
        /// </summary>
        public void SetPlusStep(int plusStep)
        {
            isForwardStep = plusStep > 0;
            stepPlayCount = MathUtils.Abs(plusStep);
        }

        /// <summary>
        /// 시작 Step 세팅 (Play 필요)
        /// </summary>
        public void SetHomeStep(bool isForward)
        {
            SetPlusStep(isForward ? tiles.Length - currentPoint : -currentPoint);
        }

        /// <summary>
        /// 현재 세팅되어있는 Step 만큼 연출
        /// </summary>
        public void Play()
        {
            SetSpeed(stepPlayCount);
            Tween();
        }

        /// <summary>
        /// 보드 변경 이펙트 보여주기
        /// </summary>
        public void ShowChangeEffect()
        {
            foreach (var item in tiles)
            {
                item.ShowChangeEffect();
            }
        }

        private void Tween()
        {
            if (stepPlayCount == 0)
            {
                OnFinishedTweenMove?.Invoke();
                IsPlay = false;
                return;
            }

            IsPlay = true;

            --stepPlayCount;
            currentPoint = GetNextStep();

            tweenPos.from = tweenPos.value; // 현재 위치
            tweenPos.to = GetCurrentTweenPosition();
            tween.Play();

            PlaySound(SFX_PORING_JUMP);
        }

        private int GetNextStep()
        {
            if (isForwardStep)
            {
                int nextStep = currentPoint + 1;
                return nextStep < tiles.Length ? nextStep : 0;
            }
            else
            {
                int nextStep = currentPoint - 1;
                return nextStep >= 0 ? nextStep : tiles.Length - 1;
            }
        }

        private Vector3 GetCurrentTweenPosition()
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (i == currentPoint)
                {
                    //return tweenPos.cachedTransform.parent.InverseTransformPoint(tiles[i].GetPosition());
                    return tweenPos.cachedTransform.parent.worldToLocalMatrix.MultiplyPoint3x4(tiles[i].GetPosition());
                }
            }

            return tweenPos.value;
        }

        private void UpdateDescription()
        {
            if (isDoubleState)
            {
                labelDescription.Text = LocalizeKey._11302.ToText(); // 더블! 한 번 더~
            }
            else
            {
                labelDescription.Text = LocalizeKey._11301.ToText() // 필요 코인 : {VALUE}
                    .Replace(ReplaceKey.VALUE, needCoinCount);
            }
        }

        private void UpdateCompleteText()
        {
            labelComplete.Text = LocalizeKey._11304.ToText() // 오늘 완주 횟수 : {COUNT}회
                .Replace(ReplaceKey.COUNT, completeCount);
        }

        private void UpdateNotice2Text()
        {
            labelNotice2.Text = LocalizeKey._11318.ToText() // 이벤트 주화는 최대 {COUNT}개까지 획득할 수 있습니다.
                .Replace(ReplaceKey.COUNT, eventCoinMaxCount);
        }

        private void SetSpeed(int playCount)
        {
            float duration = Mathf.Max(TWEEN_MIN_DURATION, TWEEN_MAX_DURATION - TWEEN_DECREASE_SPEED * (playCount / TWEEN_DECREASE_PER_COUNT));

            foreach (var item in tweens)
            {
                if (item.tweenGroup == tween.tweenGroup)
                {
                    item.duration = duration;
                }
            }
        }

        private void PlayPoringIdle(bool isPlay)
        {
            if (isPlay)
            {
                tweenPoringIdle.Play();
            }
            else
            {
                foreach (var item in tweens)
                {
                    if (item.tweenGroup == tweenPoringIdle.tweenGroup)
                    {
                        item.Finish();
                    }
                }
            }
        }

        private void PlaySound(string sfx)
        {
            if (string.IsNullOrEmpty(sfx))
                return;

            soundManager.PlaySfx(sfx);
        }

        bool IInspectorFinder.Find()
        {
            tiles = GetComponentsInChildren<UIMonopolyTile>();

            if (tween)
            {
                tweenPos = tween.GetComponent<TweenPosition>();
            }

            UIDiceCompleteElement[] elements = GetComponentsInChildren<UIDiceCompleteElement>();
            completeElements = new UIDiceCompleteElement[elements.Length];
            for (int i = 0, index = elements.Length - 1; i < completeElements.Length; i++, index--)
            {
                completeElements[i] = elements[index];
            }

            return true;
        }
    }
}