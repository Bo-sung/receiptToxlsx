namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIContentsUnlock"/>
    /// </summary>
    public class ContentsUnlockPresenter : ViewPresenter
    {
        /******************** Models ********************/
        private readonly QuestModel questModel;

        public ContentsUnlockPresenter()
        {
            questModel = Entity.player.Quest;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void AddNewContent(ContentType contentType)
        {
            questModel.AddNewOpenContent(contentType);
        }
    }
}