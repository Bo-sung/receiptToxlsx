using UnityEngine;

namespace Ragnarok
{
    public sealed class UITutorialGuide : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        public enum GuideType
        {
            CardSmelt = 0,
        }

        [SerializeField] UIButton btnClose;
        [SerializeField] UILabelHelper labelTitle, labelMessage;
        [SerializeField] UITextureHelper texture;
        [SerializeField] UITextureHelper content;

        ContentType contentType;

        protected override void OnInit()
        {
            EventDelegate.Add(btnClose.onClick, OnBack);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClose.onClick, OnBack);

            if (contentType != default)
            {
                Launch();
            }
        }

        protected override void OnShow(IUIData data = null)
        {
            contentType = default;
            texture.SetNPC(Npc.HOLORUCHI.imageName);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void SetGuideType(GuideType type)
        {
            switch (type)
            {
                case GuideType.CardSmelt:
                    labelTitle.LocalKey = LocalizeKey._27200; // 카드 강화
                    labelMessage.LocalKey = LocalizeKey._27201; // 카드를 터치하여 강화해봐!\n강화한 카드를 장착하면 더 강해질 거라구\n쿡쿡쿡!
                    break;
            }
        }

        public void SetContentType(ContentType type)
        {
            contentType = type;

            labelTitle.LocalKey = LocalizeKey._27202; // 퀘스트 알림
            labelMessage.Text = LocalizeKey._27203.ToText() // {NAME} 퀘스트를 진행할 수 있습니다.
                .Replace(ReplaceKey.NAME, contentType.ToText());
        }

        private void Launch()
        {
            const string SPRITE_NAME = "ContentsUnlock_{VALUE}";
            content.SetContentsUnlock(SPRITE_NAME.Replace(ReplaceKey.VALUE, (int)contentType), isAsync: false);

            UIContentsLauncher uiLauncher = UI.Show<UIContentsLauncher>();
            uiLauncher.Launch(content, contentType);
        }
    }
}