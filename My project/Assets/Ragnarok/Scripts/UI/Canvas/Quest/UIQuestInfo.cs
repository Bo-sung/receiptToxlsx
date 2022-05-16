using UnityEngine;

namespace Ragnarok
{
    public abstract class UIQuestInfo : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField]
        protected UILabelHelper labelTitle, labelCondition;

        [SerializeField]
        public UIButtonHelper btnComplete;

        [SerializeField]
        protected UIRewardHelper[] rewardHelpers;

        [SerializeField]
        private GameObject fxComplete;

        protected QuestPresenter presenter;
        protected QuestInfo questInfo;

        protected virtual void Awake()
        {
            EventDelegate.Add(btnComplete.OnClick, OnClickedBtnComplete);
        }

        protected virtual void Start()
        {
            OnLocalize();
        }

        protected virtual void OnDestroy()
        {
            EventDelegate.Remove(btnComplete.OnClick, OnClickedBtnComplete);
            RemoveEvent();
        }

        protected virtual void OnDisable()
        {
            RemoveEvent();

            NGUITools.SetActive(fxComplete, false);
        }

        public void SetData(QuestPresenter presenter, QuestInfo questInfo)
        {
            RemoveEvent();

            this.presenter = presenter;
            this.questInfo = questInfo;

            AddEvent();

            UpdateView();
        }

        protected virtual void OnLocalize()
        {
            UpdateView();

            btnComplete.LocalKey = LocalizeKey._10013; // 보상획득
        }

        protected abstract void UpdateView();

        /// <summary>
        /// 퀘스트 완료 (보상 받기)
        /// </summary>
        protected virtual void OnClickedBtnComplete()
        {
            if (IsInvalid())
                return;

            // 길드퀘스트의 경우, 일일 완료 횟수를 넘기는지 체크한다.
            if (questInfo.Category == QuestCategory.Guild && !presenter.HasGuildQuestReward())
                return;
            presenter.RequestQuestReward(questInfo);

            NGUITools.SetActive(fxComplete, false);
            NGUITools.SetActive(fxComplete, true);
        }

        /// <summary>
        /// 무효한 데이터
        /// </summary>
        protected bool IsInvalid()
        {
            return questInfo == null || questInfo.IsInvalidData;
        }

        private void AddEvent()
        {
            if (IsInvalid())
                return;

            questInfo.OnUpdateEvent += UpdateView;
        }

        private void RemoveEvent()
        {
            if (IsInvalid())
                return;

            questInfo.OnUpdateEvent -= UpdateView;
            questInfo = null;
        }
    }
}