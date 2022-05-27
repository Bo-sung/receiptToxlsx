using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class MoveAboveKeyboard : MonoBehaviour
    {
        [SerializeField] DebugTable debugTable;
        [SerializeField] UIInputExtension uiInput;
        [SerializeField] UIPanel panel;
        [SerializeField] UISprite uiFrame;
        [Range(1,100)]
        [SerializeField] float lerpTime = 2;
        [SerializeField] int fixedPixel;
        [SerializeField] float Debug_KeyboardHeight = 0;

        UIRoot uiRoot;
        TouchScreenKeyboard touchScreenKeyboard;
        Vector3 originalPosition;
        int fixedblankarea;
        bool isKeyboardused = false;

        private void Awake()
        {
            uiRoot = panel.root;

            float pixelSizeAdjustment = uiRoot.pixelSizeAdjustment; 
            float activeHeight = uiRoot.activeHeight;
            debugTable.Add("Screen Height", $"{Screen.height}");
            debugTable.Add("Screen Width", $"{Screen.width}");
            //렌더링 상의 화면길이와 UI상의 화면 길이의 오차 수정용.
            originalPosition = transform.localPosition;
            //키보드 using 여부 초기화
            isKeyboardused = false;
            //키보드 인풋 시작 이벤트 추가
            uiInput.submitOnUnselect = true;
            EventDelegate.Add(uiInput.onShow, UiInputShow);
            EventDelegate.Add(uiInput.onSubmit, UiInputSubmit);
            //UI 배경 최 하단 좌표
            float backgroundBottom = 0;
            //배경 피벗에 따라 변환.
            switch (uiFrame.pivot)
            {
                case UIWidget.Pivot.Top:
                case UIWidget.Pivot.TopLeft:
                case UIWidget.Pivot.TopRight:
                    {
                        backgroundBottom = uiFrame.transform.localPosition.y - uiFrame.height;
                    }
                    break;
                case UIWidget.Pivot.Bottom:
                case UIWidget.Pivot.BottomLeft:
                case UIWidget.Pivot.BottomRight:
                    {
                        backgroundBottom = uiFrame.transform.localPosition.y;
                    }
                    break;
                default:
                    {
                        backgroundBottom = uiFrame.transform.localPosition.y - (uiFrame.height / 2);
                    }
                    break;
            }
            //기존 위치와 화면 최 하단간의 갭.
            //(activeHeight / -2) = 화면상의 하단 좌표.
            //backgroundBottomy = UI 배경으로 사용하는 UISprite의 최 하단 좌표.
            fixedblankarea = (int)Mathf.Abs((activeHeight / -2) - backgroundBottom - fixedPixel);

            debugTable.Add("KeyboardHeight", "");
            debugTable.Add("fixedSize", $"{fixedblankarea}");
            //transform.localPosition = new Vector3(originalPosition.x, originalPosition.y + fixedSize, originalPosition.z);
            originalPosition = panel.transform.localPosition;
            debugTable.Add("GetHeight(true)", "");
        }
        private void OnDestroy()
        {
            //키보드 인풋 시작 이벤트 제거
            EventDelegate.Remove(uiInput.onShow, UiInputShow);
            EventDelegate.Remove(uiInput.onSubmit, UiInputSubmit);
        }
        bool istest = false;
        private void Update()
        {
            debugTable.UpdateValue("fixedSize", $"{fixedblankarea}");
            //키보드 using 여부 확인
            if (!isKeyboardused)
                return;

            if (Debug_KeyboardHeight != 0)
            {
                int _KeyboardHeight = (int)((Debug_KeyboardHeight * uiRoot.pixelSizeAdjustment));
                debugTable.UpdateValue("GetHeight(true)", $"{Debug_KeyboardHeight}");
                debugTable.UpdateValue("KeyboardHeight", $"{_KeyboardHeight}");
                Vector3 _TargetPosition = new Vector3(originalPosition.x, _KeyboardHeight - fixedblankarea, originalPosition.z);
                panel.transform.localPosition = _TargetPosition;//= Vector3.Lerp(panel.transform.localPosition, _TargetPosition, Time.deltaTime * lerpTime);
            }

            //키보드 출력중인 경우
            else if (touchScreenKeyboard.status == TouchScreenKeyboard.Status.Visible)
            {
                
                int KeyboardHeight = (int)((SoftwareKeyboardArea.GetHeight(true) * uiRoot.pixelSizeAdjustment));
                debugTable.UpdateValue("GetHeight(true)",$"{SoftwareKeyboardArea.GetHeight(true)}");
                debugTable.UpdateValue("KeyboardHeight", $"{KeyboardHeight}");
                Vector3 TargetPosition = new Vector3(originalPosition.x, KeyboardHeight - fixedblankarea, originalPosition.z);
                panel.transform.localPosition = Vector3.Lerp(panel.transform.localPosition, TargetPosition, Time.deltaTime * lerpTime);

            }
            //키보드 상태가 완료 혹은 취소 된경우
            else if (touchScreenKeyboard.status == TouchScreenKeyboard.Status.Done || touchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                panel.transform.localPosition = originalPosition;
            }
        }
        void UiInputShow()
        {
            string str = "";
            isKeyboardused = true;
            touchScreenKeyboard = TouchScreenKeyboard.Open(str, TouchScreenKeyboardType.NumberPad);
        }
        void UiInputSubmit()
        {
            panel.transform.localPosition = originalPosition;
        }
    }
}
