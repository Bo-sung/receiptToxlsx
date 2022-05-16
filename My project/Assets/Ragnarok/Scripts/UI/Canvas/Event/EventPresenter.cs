using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIEvent"/>
    /// </summary>
    public sealed class EventPresenter : ViewPresenter
    {
        public interface IView
        {
            void UpdateView();
        }

        private readonly IView view;
        private readonly QuestModel questModel;

        public EventPresenter(IView view)
        {
            this.view = view;
            questModel = Entity.player.Quest;
        }

        public override void AddEvent()
        {
            questModel.OnEventQuest += view.UpdateView;
        }

        public override void RemoveEvent()
        {
            questModel.OnEventQuest -= view.UpdateView;
        }

        /// <summary>
        /// 이벤트 퀘스트 반환
        /// </summary>
        /// <returns></returns>
        public QuestInfo[] GetEventQuests(int groupId)
        {
            return questModel.GetEventQuests(groupId);
        }

        /// <summary>
        /// 보상 받기
        /// </summary>
        public async Task RequestQuestRewardAsync(QuestInfo info)
        {
            await questModel.RequestQuestRewardAsync(info);
            view.UpdateView();
        }
    }
}