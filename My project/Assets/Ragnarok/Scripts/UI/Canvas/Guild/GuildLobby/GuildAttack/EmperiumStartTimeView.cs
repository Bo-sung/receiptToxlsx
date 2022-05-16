using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class EmperiumStartTimeView : UIView
    {
        [SerializeField] UIButtonWithIconValueHelper btnChangeStartTime;
        [SerializeField] UIButtonHelper btnHelp;
        [SerializeField] UILabelHelper labelTitleStartTime;
        [SerializeField] UILabelHelper labelStartTime;
        [SerializeField] UILabelHelper labelDescStartTime;

        public event Action OnSelect;
        public event Action OnSelectHelp;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnChangeStartTime.OnClick, OnClickedBtnChangeStartTime);
            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnChangeStartTime.OnClick, OnClickedBtnChangeStartTime);
            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnLocalize()
        {
            btnChangeStartTime.LocalKey = LocalizeKey._38408; // 변경
            labelTitleStartTime.LocalKey = LocalizeKey._38406; // 길드 습격 진행 시간
            labelDescStartTime.LocalKey = LocalizeKey._38407; // 길드 습격이 시작됩니다.
        }

        void OnClickedBtnChangeStartTime()
        {
            OnSelect?.Invoke();
        }

        void OnClickedBtnHelp()
        {
            OnSelectHelp?.Invoke();
        }

        public void SetActiveBtnChangeStartTime(bool isGuildMaster)
        {
            btnChangeStartTime.SetActive(isGuildMaster);
        }

        public void SetChangeItemCoin(int count)
        {
            btnChangeStartTime.SetValue(count.ToString());
        }

        public void SetStartTime(DateTime dateTime)
        {
            labelStartTime.Text = $"{GetDay(dateTime)} {dateTime:HH:mm}";
        }

        private string GetDay(DateTime dateTime)
        {
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return LocalizeKey._38420.ToText(); // 월요일

                case DayOfWeek.Tuesday:
                    return LocalizeKey._38421.ToText(); // 화요일

                case DayOfWeek.Wednesday:
                    return LocalizeKey._38422.ToText(); // 수요일

                case DayOfWeek.Thursday:
                    return LocalizeKey._38423.ToText(); // 목요일

                case DayOfWeek.Friday:
                    return LocalizeKey._38424.ToText(); // 금요일

                case DayOfWeek.Saturday:
                    return LocalizeKey._38425.ToText(); // 토요일

                case DayOfWeek.Sunday:
                    return LocalizeKey._38426.ToText(); // 일요일
            }
            return default;
        }
    }
}