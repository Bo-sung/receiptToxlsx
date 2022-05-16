using UnityEngine;

namespace Ragnarok.View
{
    public class DiceRollPlayView : UIView, IInspectorFinder
    {
        private const string ANI_DICE_PARAMETER = "DiceNum";
        private const string SFX_ROLL_NAME = "[SYSTEM] BOX_runout";

        [SerializeField] UIPlayTween tweenResult;
        [SerializeField] GameObject result;
        [SerializeField] UILabelHelper labelDouble;
        [SerializeField] UILabelHelper labelDiceNum;
        [SerializeField] Animator dice1, dice2;
        [SerializeField] UIPlayTween tweenDice;
        [SerializeField] TweenPosition[] tweenPositions;
        [SerializeField] float finishedTime = 2.4f;
        [SerializeField] float posStartRange = 180f;

        SoundManager soundManager;

        public event System.Action OnFinished;

        protected override void Awake()
        {
            base.Awake();

            soundManager = SoundManager.Instance;

            EventDelegate.Add(tweenDice.onFinished, OnFinishedTween);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(tweenDice.onFinished, OnFinishedTween);
        }

        protected override void OnLocalize()
        {
            labelDouble.LocalKey = LocalizeKey._11308; // Double!!
        }

        void OnFinishedTween()
        {
            SetActiveResult(true); // 주사위 결과 On
        }

        public void Play(byte diceNum1, byte diceNum2, bool isDouble)
        {
            Show();

            // 결과 세팅
            labelDouble.SetActive(isDouble);
            labelDiceNum.Text = (diceNum1 + diceNum2).ToString();

            // 애니메이터 세팅
            dice1.SetInteger(ANI_DICE_PARAMETER, diceNum1);
            dice2.SetInteger(ANI_DICE_PARAMETER, diceNum2);
        }

        public override void Show()
        {
            base.Show();

            Invoke(nameof(AutoViewHide), finishedTime); // 결과 자동 Hide 처리
            SetActiveResult(false); // 주사위 결과 Off

            // Tween
            for (int i = 0; i < tweenPositions.Length; i++)
            {
                tweenPositions[i].from = GetRandomPos(posStartRange);
            }
            tweenDice.Play();

            PlaySound(SFX_ROLL_NAME); // 효과음 재생
        }

        public override void Hide()
        {
            base.Hide();

            // 애니메이터 초기화
            dice1.SetInteger(ANI_DICE_PARAMETER, 0);
            dice2.SetInteger(ANI_DICE_PARAMETER, 0);
        }

        private void AutoViewHide()
        {
            Hide();

            OnFinished?.Invoke();
        }

        private void SetActiveResult(bool isResult)
        {
            NGUITools.SetActive(result, isResult);

            if (isResult)
            {
                tweenResult.Play();
            }
        }

        private Vector3 GetRandomPos(float value)
        {
            return new Vector3(Random.Range(0, value * 2) - value, 0f, 0f);
        }

        private void PlaySound(string sfx)
        {
            if (string.IsNullOrEmpty(sfx))
                return;

            soundManager.PlaySfx(sfx);
        }

        bool IInspectorFinder.Find()
        {
            if (tweenDice)
            {
                tweenPositions = tweenDice.GetComponentsInChildren<TweenPosition>();
            }

            return true;
        }
    }
}