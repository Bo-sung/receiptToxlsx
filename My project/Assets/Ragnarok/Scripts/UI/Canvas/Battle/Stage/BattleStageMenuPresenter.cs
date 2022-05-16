namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleStageMenu"/>
    /// </summary>
    public sealed class BattleStageMenuPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;
        private readonly QuestModel questModel;

        public event System.Action OnUpdateSummonMvpTicket
        {
            add { dungeonModel.OnUpdateSummonMvpTicket += value; }
            remove { dungeonModel.OnUpdateSummonMvpTicket -= value; }
        }

        public event System.Action OnUpdateNewOpenContent
        {
            add { questModel.OnUpdateNewOpenContent += value; }
            remove { questModel.OnUpdateNewOpenContent -= value; }
        }

        public BattleStageMenuPresenter()
        {
            dungeonModel = Entity.player.Dungeon;
            questModel = Entity.player.Quest;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public int GetMvpSummonTicketCount()
        {
            return dungeonModel.SummonMvpTicketCount;
        }

        /// <summary>
        /// 컨텐츠 오픈 여부
        /// </summary>
        public bool IsOpenContent(UIBattleStageMenu.MenuContent content, bool isShowPopup)
        {
            switch (content)
            {
                case UIBattleStageMenu.MenuContent.MvpSummon:
                    return true;

                case UIBattleStageMenu.MenuContent.BossSummon:
                    return questModel.IsOpenContent(ContentType.Boss, isShowPopup);

                case UIBattleStageMenu.MenuContent.Assemble:
                    return true;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattleStageMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 알림 표시 여부
        /// </summary>
        public bool GetHasNotice(UIBattleStageMenu.MenuContent content)
        {
            switch (content)
            {
                case UIBattleStageMenu.MenuContent.MvpSummon:
                    return false;

                case UIBattleStageMenu.MenuContent.BossSummon:
                    return false;

                case UIBattleStageMenu.MenuContent.Assemble:
                    return true;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattleStageMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 신규 컨텐츠 여부
        /// </summary>
        public bool GetHasNewIcon(UIBattleStageMenu.MenuContent content)
        {
            switch (content)
            {
                case UIBattleStageMenu.MenuContent.MvpSummon:
                    return false;

                case UIBattleStageMenu.MenuContent.BossSummon:
                    return questModel.HasNewOpenContent(ContentType.Boss);

                case UIBattleStageMenu.MenuContent.Assemble:
                    return false;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattleStageMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }
    }
}