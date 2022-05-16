using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UILoudSpeakerView : UICanvas<LoudSpeakerViewPresenter>, LoudSpeakerViewPresenter.IView
    {
        private readonly string TAG = nameof(UILoudSpeakerView);
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        public enum Mode
        {
            Default,
            Maze,
            GM,
        }

        private const string DEFAULT_SPEAKER_ICON_NAME = "Ui_Common_BattleAgent";
        private const string GM_SPEAKER_ICON_NAME = "Ui_Common_Icon_Cupet";

        [SerializeField] UIButtonHelper btnNickname;
        [SerializeField] UILabelHelper labelMessage;
        [SerializeField] UILabelHelper labelLeftTime;
        [SerializeField] UIButtonHelper btnClose;

        [SerializeField] GameObject goInfoBase;
        [SerializeField] UIButtonHelper btnShowInfo;
        [SerializeField] UIButtonHelper btnWhisper;

        [SerializeField] GameObject goActive;

        [SerializeField] UIWidget widgetBackground;
        [SerializeField] GameObject goMode_Default;
        [SerializeField] GameObject goMode_Maze;
        [SerializeField] UIButtonHelper btnMaze;
        [SerializeField] UISprite iconSpeaker;

        private RelativeRemainTime remainTime;
        private int uid, cid;
        private int stage;
        private Mode mode;

        protected override void OnInit()
        {
            presenter = new LoudSpeakerViewPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnNickname.OnClick, OnClickedBtnNickname);
            EventDelegate.Add(btnClose.OnClick, OnClickedBtnClose);
            EventDelegate.Add(btnShowInfo.OnClick, OnClickedBtnInfo);
            EventDelegate.Add(btnWhisper.OnClick, OnClickedBtnWhisper);
            EventDelegate.Add(btnMaze.OnClick, OnClickedBtnMaze);
        }

        protected override void OnClose()
        {
            presenter.Stop();

            presenter.RemoveEvent();

            EventDelegate.Remove(btnNickname.OnClick, OnClickedBtnNickname);
            EventDelegate.Remove(btnClose.OnClick, OnClickedBtnClose);
            EventDelegate.Remove(btnShowInfo.OnClick, OnClickedBtnInfo);
            EventDelegate.Remove(btnWhisper.OnClick, OnClickedBtnWhisper);
            EventDelegate.Remove(btnMaze.OnClick, OnClickedBtnMaze);
        }

        protected override void OnShow(IUIData data = null)
        {
            remainTime = 0f;
            goActive.SetActive(false);

            presenter.Begin();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnClose.LocalKey = LocalizeKey._21200; // CLOSE ▶
        }

        public void SetMode(Mode mode)
        {
            this.mode = mode;
            switch (mode)
            {
                case Mode.Default:
                    widgetBackground.SetAnchor(goMode_Default, 0, 0, 0, 0);
                    btnMaze.SetActive(false);
                    iconSpeaker.spriteName = DEFAULT_SPEAKER_ICON_NAME;
                    break;

                case Mode.GM:
                    widgetBackground.SetAnchor(goMode_Default, 0, 0, 0, 0);
                    btnMaze.SetActive(false);
                    iconSpeaker.spriteName = GM_SPEAKER_ICON_NAME;
                    break;

                case Mode.Maze:
                    widgetBackground.SetAnchor(goMode_Maze, 0, 0, 0, 0);
                    btnMaze.SetActive(true);
                    iconSpeaker.spriteName = DEFAULT_SPEAKER_ICON_NAME;
                    break;
            }
        }       

        void LoudSpeakerViewPresenter.IView.Show(int stage, string nickname, string message, int cid, int uid, float remainTime)
        {
            this.stage = stage;
            goActive.SetActive(true);
            goInfoBase.SetActive(false);
            labelMessage.Text = message;
            this.remainTime = remainTime;
            this.cid = cid;
            this.uid = uid;

            switch (mode)
            {
                case Mode.Default:
                case Mode.Maze:
                    btnNickname.Text = $"<{nickname}>";
                    break;

                case Mode.GM:
                    btnNickname.Text = LocalizeKey._29010.ToText();
                    break;
            }

            Timing.RunCoroutineSingleton(YieldPlay().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldPlay()
        {
            const float UPDATE_INTERVAL = 0.1f;

            while (true)
            {
                labelLeftTime.Text = $"{remainTime.GetRemainTime().ToString("N0")}s";
                yield return Timing.WaitForSeconds(UPDATE_INTERVAL);
            }
        }

        void LoudSpeakerViewPresenter.IView.Hide()
        {
            goActive.SetActive(false);
        }

        private void OnClickedBtnNickname()
        {
            if (mode == Mode.GM)
                return;

            goInfoBase.SetActive(!goInfoBase.activeSelf);
        }

        private void OnClickedBtnClose()
        {
            presenter.SkipMessage();
        }

        private void OnClickedBtnInfo()
        {
            presenter.ShowOtherCharacterInfo();
        }

        private void OnClickedBtnWhisper()
        {
            presenter.OpenWhipserUI();
        }

        private void OnClickedBtnMaze()
        {
            presenter.EnterMultiMaze(stage);
        }
    }
}