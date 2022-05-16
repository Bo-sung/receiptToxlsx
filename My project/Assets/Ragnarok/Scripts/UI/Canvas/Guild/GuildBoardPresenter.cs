namespace Ragnarok
{
    public class GuildBoardPresenter : ViewPresenter
    {
        public interface IView
        {
            void SetBtnEdit(bool isActive);
            void SetResize(int size);
            bool IsEndDrag();
            void SetInputSelect(bool isSelect);
            string GetInputValue();
            void SetGuildNotice(string notice);
        }

        private readonly IView view;
        private readonly GuildModel guild;
        private readonly CharacterModel character;

        private GuildBoardInfo[] arrayInfo;

        public GuildBoardPresenter(IView view)
        {
            this.view = view;
            guild = Entity.player.Guild;
            character = Entity.player.Character;
        }

        public override void AddEvent()
        {
            guild.OnUpdateGuildBoard += OnUpdateGuildBoard;
        }

        public override void RemoveEvent()
        {
            guild.OnUpdateGuildBoard -= OnUpdateGuildBoard;
        }

        async void OnUpdateGuildBoard()
        {
            await guild.RequestGuildBoardList();
            SetView();
        }

        public void SetView()
        {
            view.SetBtnEdit(guild.GuildPosition == GuildPosition.Master);
            arrayInfo = guild.GetGuildBoardInfos();
            view.SetResize(arrayInfo.Length);
            view.SetGuildNotice(guild.Notice);
        }


        public async void OnDragFinished()
        {
            if (!guild.HasNextPage)
                return;

            if (view.IsEndDrag())
            {
                await guild.RequestNextBoardList();
                SetView();
            }
        }

        public GuildBoardInfo GetInfo(int index)
        {
            return arrayInfo[index];
        }

        public void OnClickedBtnEdit()
        {
            if (guild.GuildPosition != GuildPosition.Master)
                return;

            view.SetInputSelect(true);
        }

        public async void OnSubmitInput()
        {
            view.SetInputSelect(false);

            string text = view.GetInputValue();
            if (string.IsNullOrEmpty(text))
                return;

            await guild.RequestChangeGuildNotice(text);
            view.SetGuildNotice(guild.Notice);
        }

        /// <summary>
        /// 게시판 게시글 삭제
        /// </summary>
        public async void RequestRemoveGuildBoard(GuildBoardInfo info)
        {
            await guild.RequestDeleteGuildBoard(info.Seq);
            await guild.RequestGuildBoardList();
            SetView();
        }

        public bool CanDeleteBoard(GuildBoardInfo info)
        {
            if (guild.GuildPosition == GuildPosition.Master)
                return true;

            if (info.IsSystem)
                return false;

            if (info.CID == character.Cid)
                return true;

            return false;
        }


    }
}
