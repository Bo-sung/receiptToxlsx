using UnityEngine;

namespace Ragnarok
{
    public class ElementCount : MonoBehaviour
    {
        [SerializeField] UIButton btnMinus;
        [SerializeField] UIButton btnPlus;
        [SerializeField] UIInputExtension inputCount;

        /// <summary>
        /// 값 변경시 호출
        /// </summary>
        public event System.Action OnRefresh;
        /// <summary>
        /// 입력 시작시 호출
        /// </summary>
        public event System.Action OnInputStart;
        /// <summary>
        /// 입력 종료시 호출
        /// </summary>
        public event System.Action OnInputEnd;
        /// <summary>
        /// (필드) 데이터
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// (필드) 카운트 최댓값
        /// </summary>
        int maxCount;

        protected virtual void Awake()
        {
            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);

            inputCount.submitOnUnselect = true;
            inputCount.OnSelection += OnStartInput;
            EventDelegate.Add(inputCount.onChange, OnChangeInputCount);
            EventDelegate.Add(inputCount.onSubmit, OnSubmitInputCount);
        }

        protected virtual void OnDestroy()
        {
            EventDelegate.Remove(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Remove(btnPlus.onClick, OnClickedBtnPlus);

            inputCount.OnSelection -= OnStartInput;
            EventDelegate.Remove(inputCount.onChange, OnChangeInputCount);
            EventDelegate.Remove(inputCount.onSubmit, OnSubmitInputCount);
        }

        private void SetCount(int count)
        {
            Count = Mathf.Clamp(count, 1, maxCount);
            inputCount.value = Count.ToString();
        }

        /// <summary>
        /// 초기화. 카운트 최댓값 설정 필수
        /// </summary>
        public void Initiallize(int count, int maxCount)
        {
            this.maxCount = Mathf.Max(1, maxCount);
            if (!inputCount.isSelected)
                SetCount(count);
        }

        /// <summary>
        /// 새로고침. 각 이벤트 실행시 혹은 값 변경시마다 호출
        /// </summary>
        void Refresh()
        {
            //리프레시 이벤트 호출
            OnRefresh?.Invoke();
        }

        public void SetEnable(bool isEnable)
        {
            SetInputEnable(isEnable);
            btnMinus.isEnabled = isEnable;
            btnPlus.isEnabled = isEnable;
        }

        public void SetInputEnable(bool isEnable)
        {
            inputCount.IsEnabled = isEnable;
        }

        /// <summary>
        /// 마이너스 버튼 클릭 시 호출
        /// summary>
        void OnClickedBtnMinus()
        {
            if (Count - 1 <= 0)
            {
                Count = maxCount;
            }
            else
            {
                Count--;
            }
            SetCount(Count);
            Refresh();
        }

        /// <summary>
        /// 플러스 버튼 클릭 시 호출
        /// </summary>
        void OnClickedBtnPlus()
        {
            if (Count + 1 > maxCount)
            {
                Count = 1;
            }
            else
            {
                Count++;
            }
            SetCount(Count);
            Refresh();
        }

        /// <summary>
        /// 입력 시작시 호출
        /// </summary>
        void OnStartInput()
        {
            SetCount(Count);
            OnInputStart?.Invoke();
        }

        /// <summary>
        /// 입력내용 변경시 호출
        /// </summary>
        void OnChangeInputCount()
        {
            if (string.IsNullOrEmpty(inputCount.value))
                return;

            Count = GetCount(inputCount.value);
            SetCount(Count);
            Refresh();
        }

        /// <summary>
        /// 입력 완료 혹은 입력 취소시 호출
        /// </summary>
        void OnSubmitInputCount()
        {
            Count = GetCount(inputCount.value);
            SetCount(Count);
            Refresh();
        }

        /// <summary>
        /// 입력값 정수로 변경.
        /// </summary>
        private int GetCount(string inputText)
        {
            if (!int.TryParse(inputText.Trim(), out int inputValue))
                return 1;

            return Mathf.Clamp(inputValue, 1, maxCount);
        }
    }
}
