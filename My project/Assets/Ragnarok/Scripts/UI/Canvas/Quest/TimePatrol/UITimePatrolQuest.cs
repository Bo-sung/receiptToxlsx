using UnityEngine;

namespace Ragnarok
{
    public sealed class UITimePatrolQuest : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;
        public override int layer => Layer.UI_ExceptForCharZoom;

        // 타이틀
        [SerializeField] UILabelHelper labelQuestTitle;
        [SerializeField] TweenColor twColorQuestTitle;

        // 퀘스트 내용
        [SerializeField] UILabelHelper labelQuestDesc;

        // 버튼
        [SerializeField] UIButtonHelper btnQuest;

        // 퀘스트 클리어 베이스
        [SerializeField] GameObject goCompleteBase;
        [SerializeField] GameObject goProgressBase;

        [SerializeField] TweenScale labelQuestDescTween;

        TimePatrolQuestPresenter presenter;

        protected override void OnInit()
        {
            presenter = new TimePatrolQuestPresenter();
            presenter.AddEvent();

            presenter.OnReqeustReward += OnReqeustReward;
            presenter.OnUpdateTimePatrolQuest += UpdateQuest;

            EventDelegate.Add(btnQuest.OnClick, OnClickedBtnQuest);
        }

        protected override void OnClose()
        {
            presenter.OnReqeustReward -= OnReqeustReward;
            presenter.OnUpdateTimePatrolQuest -= UpdateQuest;

            presenter.RemoveEvent();

            EventDelegate.Remove(btnQuest.OnClick, OnClickedBtnQuest);
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateQuest();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void UpdateQuest()
        {
            QuestInfo curQuest = presenter.GetQuest();
            if (curQuest.IsInvalidData)
            {
                string text = LocalizeKey._90045.ToText(); // 업데이트 예정입니다.
                SetQuestCompleteState(isComplete: false);
                SetQuestText(text, text);
                return;
            }

            bool isReward = curQuest.CompleteType == QuestInfo.QuestCompleteType.StandByReward;

            // 이번에 퀘스트가 클리어 된 경우 SFX 발생
            if (isReward)
                presenter.PlayUISfx();

            SetQuestCompleteState(isComplete: isReward);
            SetQuestText($"({curQuest.Group}) {curQuest.Name}", $"{curQuest.ConditionText} ({curQuest.CurrentValue}/{curQuest.MaxValue})");
        }

        void SetQuestText(string title, string description)
        {
            labelQuestTitle.Text = title;
            labelQuestDesc.Text = description;
        }

        void SetQuestCompleteState(bool isComplete)
        {
            if (isComplete)
            {
                goCompleteBase.SetActive(true);
                goProgressBase.SetActive(false);
                labelQuestTitle.Outline = twColorQuestTitle.to;
                labelQuestDescTween.enabled = false;
                labelQuestDescTween.tweenFactor = 0;
            }
            else
            {
                goCompleteBase.SetActive(false);
                goProgressBase.SetActive(true);
                labelQuestTitle.Outline = twColorQuestTitle.from;
                labelQuestDescTween.enabled = true;
                labelQuestDescTween.tweenFactor = 0;
            }
        }

        void OnClickedBtnQuest()
        {
            presenter.OnClickedBtnQuest();
        }

        void OnReqeustReward(bool isRequest)
        {
            btnQuest.IsEnabled = !isRequest;
        }
    }
}