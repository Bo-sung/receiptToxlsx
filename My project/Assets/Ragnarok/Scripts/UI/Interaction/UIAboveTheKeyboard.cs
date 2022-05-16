using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 모바일 키보드 사용시 UI를 키보드 상단에 위치하도록 하는 스크립트.
    /// </summary>
    public sealed class UIAboveTheKeyboard : MonoBehaviour
    {
        [SerializeField] UIPanel targetPanel;
        [SerializeField] UIInputExtension input;

        [Header("Background Setting")]
        [SerializeField] GameObject backgroundDark;
        [SerializeField] bool isBackgroundDark;

        [Range(1, 100)]
        [SerializeField] float lerpTime = 2;  //UI 부드러운 이동 관련.

        /// <summary>
        /// 원래 위치
        /// </summary>
        Vector3 originalPosition;

        float pixelSizeAdjustment;

        float halfActiveHeight;
        /// <summary>
        /// 키보드 사용 여부
        /// </summary>
        bool isOpendKeyboard = false;

        private void Awake()
        {
            //패널 뒤 검은 배경 설정.
            backgroundDark.SetActive(false);
            //렌더링 화면과 출력중인 화면사이의 비율.
            pixelSizeAdjustment = targetPanel.root.pixelSizeAdjustment;
            halfActiveHeight = targetPanel.root.activeHeight / 2;
            originalPosition = transform.localPosition;

            //키보드 using 여부 초기화
            isOpendKeyboard = false;
            input.OnSelection += OnSelectionInput;
            EventDelegate.Add(input.onSubmit, OnSubmitInput);
        }

        private void OnDestroy()
        {
            //키보드 인풋 시작 이벤트 제거
            input.OnSelection -= OnSelectionInput;
            EventDelegate.Remove(input.onSubmit, OnSubmitInput);
        }

        private void Update()
        {
#if UNITY_EDITOR
            return;
#endif

            if (!isOpendKeyboard)
                return;

            //키보드 높이 = 렌더링 상 키보드 높이 * 화면 보정 비율 
            int KeyboardHeight = (int)((SoftwareKeyboardArea.GetHeight(true) * pixelSizeAdjustment));
            transform.localPosition = new Vector3(0, KeyboardHeight - halfActiveHeight, 0);
        }

        void OnSelectionInput()
        {
            isOpendKeyboard = true;
            backgroundDark.SetActive(isBackgroundDark);
        }

        void OnSubmitInput()
        {
            isOpendKeyboard = false;
            transform.localPosition = originalPosition;
            backgroundDark.SetActive(false);
        }
    }
}
