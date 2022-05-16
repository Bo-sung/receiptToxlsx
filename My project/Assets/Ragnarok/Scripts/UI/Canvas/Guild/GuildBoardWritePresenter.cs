namespace Ragnarok
{
    public class GuildBoardWritePresenter : ViewPresenter
    {
        public interface IView
        {
            void CloseUI();
        }

        private readonly IView view;
        private readonly GuildModel guild;

        public GuildBoardWritePresenter(IView view)
        {
            this.view = view;
            guild = Entity.player.Guild;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 게시판 글쓰기
        /// </summary>
        public async void WriteGuildBoard(string message)
        {           
            bool isSuccess = await guild.RequestWriteGuildBoard(message);
            if(isSuccess)
            {
                view.CloseUI();
            }
        }
    }
}
