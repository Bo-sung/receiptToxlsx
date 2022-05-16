using UnityEngine;

namespace Ragnarok
{
    public sealed class UIContentsUnlock : UICanvas
    {
        public static float attackPowerInfoDelay = 0f;

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UITextureHelper content;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButton btnClose;

        [Rename(displayName = "이 시간동안은 클릭 안 됨")]
        [SerializeField] float delaySkipTime = 2f;

        RelativeRemainTime remainTime;
        ContentType contentType;

        ContentsUnlockPresenter presenter;

        protected override void OnInit()
        {
            presenter = new ContentsUnlockPresenter();

            presenter.AddEvent();
            EventDelegate.Add(btnClose.onClick, CloseUI);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(btnClose.onClick, CloseUI);

            Launch();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._10500; // 컨텐츠 오픈
        }

        public void Set(ContentType type)
        {
            presenter.AddNewContent(type); // 알림 추가

            PlaySfx("[SYSTEM]_CLEAR_QUEST");

            contentType = type;
            remainTime = delaySkipTime;

            labelDescription.Text = LocalizeKey._10501.ToText() // {NAME} 컨텐츠가 오픈되었습니다.
                .Replace(ReplaceKey.NAME, contentType.ToText());

            const string SPRITE_NAME = "ContentsUnlock_{VALUE}";
            content.Set(SPRITE_NAME.Replace(ReplaceKey.VALUE, (int)contentType), isAsync: false); // 애니메이션이 Active를 제어하기 때문에 isAsync 를 false 처리
        }

        private void CloseUI()
        {
            if (remainTime.GetRemainTime() > 0f)
                return;

            UI.Close<UIContentsUnlock>();
        }

        private void Launch()
        {
            if (!IsLaunchContents())
                return;

            UIContentsLauncher uiLauncher = UI.Show<UIContentsLauncher>();
            uiLauncher.Launch(content, contentType);
        }

        private bool IsLaunchContents()
        {
            if (contentType == default)
                return false;

            // 셰어 컨트롤의 경우에는 Launch 금지
            if (contentType == ContentType.ShareControl)
                return false;

            return true;
        }

        protected override void OnBack()
        {
            CloseUI();
        }
    }
}