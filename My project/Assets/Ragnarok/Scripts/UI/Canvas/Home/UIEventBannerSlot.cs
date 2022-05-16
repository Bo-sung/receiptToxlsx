using UnityEngine;

namespace Ragnarok.View.Home
{
    public class UIEventBannerSlot : UIView
    {
        [SerializeField] UITextureHelper texture;
        [SerializeField] UIButtonHelper button;

        IEventBanner eventBanner;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(button.OnClick, OnClickedButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(button.OnClick, OnClickedButton);
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(IEventBanner eventBanner)
        {
            this.eventBanner = eventBanner;
            Refresh();
        }

        private void Refresh()
        {
            if (eventBanner == null)
                return;

            texture.SetFromUrl(eventBanner.TextureUrl);
        }

        void OnClickedButton()
        {
            if (eventBanner.ShortcutType == ShortCutType.None)
            {
                Application.OpenURL(eventBanner.Url);
            }
            else
            {
                eventBanner.ShortcutType.GoShortCut(eventBanner.ShortcutValue);
            }
        }

        public string GetNotice()
        {
            if (eventBanner == null)
                return string.Empty;

            var time = eventBanner.RemainTime.ToRemainTime().ToTimeSpan();

            if (time.Days >= 1000)
            {
                return LocalizeKey._2519.ToText() // {NAME}\n(이벤트 기간 : {START_DATE} ~ 별도 공지 시)
                    .Replace(ReplaceKey.NAME, eventBanner.Description)
                    .Replace(ReplaceKey.START_DATE, eventBanner.StartTime.ToString("yyyy-MM-dd"));
            }

            return LocalizeKey._2514.ToText() // {NAME}\n(이벤트 기간 : {START_DATE} ~ {END_DATE} 점검 전)
                .Replace(ReplaceKey.NAME, eventBanner.Description)
                .Replace(ReplaceKey.START_DATE, eventBanner.StartTime.ToString("yyyy-MM-dd"))
                .Replace(ReplaceKey.END_DATE, eventBanner.EndTime.ToString("yyyy-MM-dd"));
        }
    }
}