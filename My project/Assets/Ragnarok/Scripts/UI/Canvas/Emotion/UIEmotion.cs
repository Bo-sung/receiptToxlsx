using Ragnarok.View;
using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEmotion : UICanvas, IInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIButtonHelper btnIcon;
        [SerializeField] GameObject popup;
        [SerializeField] UIEmotionSlot[] slots;
        [SerializeField] GameObject mask;

        public delegate void EmotionClickEvent(EmotionType type);
        public event EmotionClickEvent OnEmotion;

        Action<EmotionType, float> OnEmotionSlot;

        int listLength;
        bool isActive = false;

        protected override void OnInit()
        {
            OnEmotionSlot += OnClickedEmotion;

            listLength = Enum.GetNames(typeof(EmotionType)).Length;

            EventDelegate.Add(btnIcon.OnClick, ButtonClicked);

            // 슬롯 초기화
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].InitEmotion(i, true, OnEmotionSlot);
            }

            // 마스크 셋팅
            mask.SetActive(false);
        }

        protected override void OnClose()
        {
            OnEmotionSlot -= OnClickedEmotion;

            EventDelegate.Remove(btnIcon.OnClick, ButtonClicked);
        }

        protected override void OnShow(IUIData data = null)
        {
            isActive = false;
            ButtonClicked();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnClickedEmotion(EmotionType type, float remainTime)
        {
            OnEmotion?.Invoke(type);

            // 버튼 비활성화
            ActiveSlotButtons(false);

            // 이모션 연출 후 활성화
            Invoke("ActiveSlots", remainTime);
        }

        void ActiveSlots()
        {
            ActiveSlotButtons(true);
        }

        void ActiveSlotButtons(bool isActive)
        {
            // 버튼 활성화 셋팅
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].ActiveButton(isActive);
            }

            // 마스크 셋팅
            mask.SetActive(!isActive);
        }

        void ButtonClicked()
        {
            popup.SetActive(isActive);
            isActive = !isActive;
        }

        bool IInspectorFinder.Find()
        {
            slots = GetComponentsInChildren<UIEmotionSlot>();
            return true;
        }
    }
}