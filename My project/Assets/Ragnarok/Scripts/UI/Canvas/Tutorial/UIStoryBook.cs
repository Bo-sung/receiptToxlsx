using UnityEngine;

namespace Ragnarok
{
    public sealed class UIStoryBook : UICanvas
    {
        private const string ACTIVE_ANI_CLIP = "UIStoryBook_Active";
        private const string DEACTIVE_ANI_CLIP = "UIStoryBook_Deactive";

        private enum State
        {
            None = 1,
            PlayingActive,
            Active,
            PlayingDeactive,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        [SerializeField] UIButton btnUnselect;
        [SerializeField] UIButton btnClose;
        [SerializeField] UILabelHelper labelChapter;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelStory;
        [SerializeField] Animation popupBase;

        private State state;

        protected override void OnInit()
        {
            EventDelegate.Add(btnUnselect.onClick, PlayDeactiveAni);
            EventDelegate.Add(btnClose.onClick, PlayDeactiveAni);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnUnselect.onClick, PlayDeactiveAni);
            EventDelegate.Remove(btnClose.onClick, PlayDeactiveAni);

            state = State.None;
        }

        protected override void OnShow(IUIData data = null)
        {
            state = State.None;
        }

        protected override void OnHide()
        {
            state = State.None;
        }

        protected override void OnLocalize()
        {
        }

        public void SetChapter(int chapter)
        {
            labelChapter.Text = LocalizeKey._27100.ToText()
                .Replace(ReplaceKey.INDEX, chapter);

            labelTitle.LocalKey = BasisType.STORY_BOOK_TITLE_LANGUAGE_ID.GetInt(chapter);
            labelStory.LocalKey = BasisType.STORY_BOOK_STORY_LANGUAGE_ID.GetInt(chapter);

            PlayActiveAni();
        }

        private void PlayActiveAni()
        {
            // 이미 Active 중
            if (state == State.PlayingActive)
                return;

            // None 일 때에만 Active 가능
            if (state != State.None)
                return;

            state = State.PlayingActive;
            ActiveAnimation aa = ActiveAnimation.Play(popupBase, ACTIVE_ANI_CLIP, AnimationOrTween.Direction.Forward);
            EventDelegate.Add(aa.onFinished, OnFinishedAniActive, oneShot: true);
        }

        private void PlayDeactiveAni()
        {
            // 이미 Deactive 중
            if (state == State.PlayingDeactive)
                return;

            // Active 일 때에만 Deactive 가능
            if (state != State.Active)
                return;

            state = State.PlayingDeactive;
            ActiveAnimation aa = ActiveAnimation.Play(popupBase, DEACTIVE_ANI_CLIP, AnimationOrTween.Direction.Forward);
            EventDelegate.Add(aa.onFinished, OnFinishedAniDeactive, oneShot: true);
        }

        void OnFinishedAniActive()
        {
            state = State.Active;
        }

        void OnFinishedAniDeactive()
        {
            state = State.None;
            CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIStoryBook>();
        }
    }
}