using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIPass"/>
    /// </summary>
    public class PassQuestView : UIView
    {
        [SerializeField] PassDailyQuestView dailyQuestView;
        [SerializeField] PassSeasonQuestView seasonQuestView;

        public event System.Action<int> OnSelectDailyQuest;
        public event System.Action<int> OnSelectSeasonQuest;

        protected override void Awake()
        {
            base.Awake();
            dailyQuestView.OnSelect += OnSelectDailyQuestElement;
            seasonQuestView.OnSelect += OnSelectSeasonQuestElement;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            dailyQuestView.OnSelect -= OnSelectDailyQuestElement;
            seasonQuestView.OnSelect -= OnSelectSeasonQuestElement;
        }

        protected override void OnLocalize()
        {

        }

        void OnSelectDailyQuestElement(int id)
        {
            OnSelectDailyQuest?.Invoke(id);
        }

        void OnSelectSeasonQuestElement(int id)
        {
            OnSelectSeasonQuest?.Invoke(id);
        }

        public void SetDailyQuestData(UIPassQuestElement.IInput[] inputs)
        {
            dailyQuestView.SetData(inputs);
        }

        public void SetSeasonQuestsData(UIPassQuestElement.IInput[] inputs)
        {
            seasonQuestView.SetData(inputs);
        }
    }
}