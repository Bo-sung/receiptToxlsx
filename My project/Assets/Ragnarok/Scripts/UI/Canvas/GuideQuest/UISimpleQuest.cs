using UnityEngine;

namespace Ragnarok
{
    public class UISimpleQuest : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;
        public override int layer => Layer.UI_ExceptForCharZoom;

        public enum Mode
        {
            Main,
            Kafra,
        }

        public interface ISimpleQuest
        {
            Mode CurrentMode { get; }

            string GetTitle();
            string GetDesc();

            bool IsFirst();
            bool IsComplete();
        }

        // 타이틀
        [SerializeField] UILabelHelper labelQuestTitle;
        [SerializeField] TweenColor twColorQuestTitle;

        // 퀘스트 내용
        [SerializeField] UILabelHelper labelQuestDesc;

        // 버튼
        [SerializeField] UIButtonHelper btnQuest;
        [SerializeField] UIButtonHelper btnQuest_UI; // 누르면 바로가기 대신 UI창 띄우기

        // 퀘스트 클리어 베이스
        [SerializeField] UIWidget progressBase;
        [SerializeField] UIWidget completeBase;
        [SerializeField] UIWidget kafraProgressBase;
        [SerializeField] UIWidget kafraCompleteBase;
        [SerializeField] GameObject goQuestNotice;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UILabelHelper labelClick;

        [SerializeField] TweenScale labelQuestDescTween;

        SimpleQuestPresenter presenter;

        protected override void OnInit()
        {
            presenter = new SimpleQuestPresenter();
            presenter.AddEvent();

            presenter.OnRefreshSimpleQuest += Refresh;
            presenter.OnReqeustReward += OnReqeustReward;

            EventDelegate.Add(btnQuest.OnClick, OnClickedBtnQuest);
            EventDelegate.Add(btnQuest_UI.OnClick, OnClickedBtnQuest_UI);

            presenter.Initialize();
        }

        protected override void OnClose()
        {
            presenter.OnRefreshSimpleQuest -= Refresh;
            presenter.OnReqeustReward -= OnReqeustReward;

            presenter.RemoveEvent();

            EventDelegate.Remove(btnQuest.OnClick, OnClickedBtnQuest);
            EventDelegate.Remove(btnQuest_UI.OnClick, OnClickedBtnQuest_UI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._19509; // 퀘스트 보상
            labelClick.LocalKey = LocalizeKey._19510; // Click!

            Refresh();
        }

        void OnClickedBtnQuest()
        {
            presenter.OnClickedBtnQuest();
        }

        void OnClickedBtnQuest_UI()
        {
            presenter.OnClickedBtnQuest_UI();
        }

        private void Refresh()
        {
            ISimpleQuest simpleQuest = presenter.GetSimpleQuest();
            labelQuestTitle.Text = simpleQuest.GetTitle();
            labelQuestDesc.Text = simpleQuest.GetDesc();

            if (simpleQuest.IsComplete())
            {
                progressBase.cachedGameObject.SetActive(false);
                completeBase.cachedGameObject.SetActive(true);
                kafraProgressBase.cachedGameObject.SetActive(false);
                kafraCompleteBase.cachedGameObject.SetActive(true);
                labelQuestDescTween.enabled = false;
                labelQuestDescTween.tweenFactor = 0;
            }
            else
            {
                progressBase.cachedGameObject.SetActive(true);
                completeBase.cachedGameObject.SetActive(false);
                kafraProgressBase.cachedGameObject.SetActive(true);
                kafraCompleteBase.cachedGameObject.SetActive(false);
                labelQuestDescTween.enabled = true;
                labelQuestDescTween.tweenFactor = 0;
            }

            if (simpleQuest.CurrentMode == Mode.Kafra)
            {
                progressBase.alpha = 0f;
                completeBase.alpha = 0f;
                kafraProgressBase.alpha = 1f;
                kafraCompleteBase.alpha = 1f;

                labelQuestTitle.Outline = twColorQuestTitle.to;
            }
            else
            {
                progressBase.alpha = 1f;
                completeBase.alpha = 1f;
                kafraProgressBase.alpha = 0f;
                kafraCompleteBase.alpha = 0f;

                labelQuestTitle.Outline = twColorQuestTitle.from;
            }

            NGUITools.SetActive(goQuestNotice, simpleQuest.IsComplete() && simpleQuest.IsFirst());
        }

        private void OnReqeustReward(bool isRequest)
        {
            btnQuest.IsEnabled = !isRequest;
            btnQuest_UI.IsEnabled = !isRequest;
        }
    }
}