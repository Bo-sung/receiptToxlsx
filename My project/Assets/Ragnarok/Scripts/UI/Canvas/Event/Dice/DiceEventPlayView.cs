using UnityEngine;

namespace Ragnarok.View
{
    public class DiceEventPlayView : UIView, IInspectorFinder
    {
        /// <summary>
        /// 연출 최소 개수
        /// </summary>
        private const int PLAY_MIN_COUNT = 10;

        private const string SFX_SCROLL_NAME = "[SYSTEM]_GM_ALARM04_Half";
        private const string SFX_FINISH_NAME = "[SYSTEM] Charge_Alarm";

        [SerializeField] UILabelHelper labelEventTitle;
        [SerializeField] UIScrollWrapContent scrollWrapContent;
        [SerializeField] UIDiceEventElement[] elements;
        [SerializeField] UIDiceEventResult result;

        SoundManager soundManager;

        public event System.Action<DiceEventType, int> OnHide;

        private string[] images;
        private UIDiceEventResult.IInput currentEvent;

        protected override void Awake()
        {
            base.Awake();

            soundManager = SoundManager.Instance;

            scrollWrapContent.onInitializeItem += OnInitializeItem;
            scrollWrapContent.OnFinishedPlay += OnFinishedPlay;
            result.OnConfirm += Hide;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            scrollWrapContent.onInitializeItem -= OnInitializeItem;
            scrollWrapContent.OnFinishedPlay -= OnFinishedPlay;
            result.OnConfirm -= Hide;
        }

        protected override void OnLocalize()
        {
            labelEventTitle.LocalKey = LocalizeKey._11309; // 기묘한 사건
        }

        public override void Hide()
        {
            base.Hide();

            result.Hide();

            if (currentEvent != null)
            {
                DiceEventType type = currentEvent.DiceEventType;
                int value = currentEvent.DiceEventValue;

                currentEvent = null; // 초기화

                OnHide?.Invoke(type, value);
            }
        }

        void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
        {
            int imageIndex = GetImageIndex(realIndex); // 현재 무한스크롤 해당하는 이미지 Index 반환
            elements[wrapIndex].SetData(imageIndex == -1 ? string.Empty : images[imageIndex]); // 이미지 세팅

            // 스크롤 2부터 효과음 재생 (연출로 인한 스크롤은 2부터 처리)
            if (realIndex > 1)
            {
                PlaySound(SFX_SCROLL_NAME);
            }
        }

        void OnFinishedPlay()
        {
            PlaySound(SFX_FINISH_NAME);
            result.Show(currentEvent); // 결과 팝업
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(string[] images)
        {
            this.images = images;
        }

        /// <summary>
        /// 이벤트 결과 세팅
        /// </summary>
        public void SetEvent(UIDiceEventResult.IInput value)
        {
            currentEvent = value;
        }

        /// <summary>
        /// 연출
        /// </summary>
        public bool Play()
        {
            if (currentEvent == null)
                return false;

            Show();

            int scrollCount = GetPlayScrollCount(currentEvent.ImageName); // 결과 이미지에 해당하는 스크롤 연출 수
            scrollWrapContent.PlayScroll(scrollCount); // 스크롤 연출
            return true;
        }

        /// <summary>
        /// 강제 닫기
        /// </summary>
        public void TryHide()
        {
            if (!result.IsShow)
                return;

            Hide();
        }

        private int GetImageIndex(int index)
        {
            int imageMax = images == null ? 0 : images.Length;
            if (imageMax == 0)
                return -1;

            if (index < 0)
            {
                index += imageMax;
            }
            else if (index >= imageMax)
            {
                index -= imageMax;
            }
            else
            {
                return index;
            }

            return GetImageIndex(index);
        }

        private int GetPlayScrollCount(string imageName)
        {
            int index = GetFindImageIndex(imageName);
            if (index == -1)
                return -1;

            // 최소 연출보다는 커야함
            while (index < PLAY_MIN_COUNT)
            {
                index += images.Length;
            }

            return index;
        }

        private int GetFindImageIndex(string imageName)
        {
            for (int i = 0, imageMax = images == null ? 0 : images.Length; i < imageMax; i++)
            {
                if (string.Equals(images[i], imageName))
                    return i;
            }

            return -1;
        }

        private void PlaySound(string sfx)
        {
            if (string.IsNullOrEmpty(sfx))
                return;

            soundManager.PlaySfx(sfx);
        }

        bool IInspectorFinder.Find()
        {
            elements = GetComponentsInChildren<UIDiceEventElement>();
            return true;
        }
    }
}