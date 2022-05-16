namespace Ragnarok
{
    public class GuildSetPresenter : ViewPresenter
    {
        public interface IView
        {
            void CloseUI();
            void SetJoin(string name);
        }

        private readonly IView view;
        private readonly GuildModel guild;
        private bool isAutoJoin;

        public GuildSetPresenter(IView view)
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

        public void SetView()
        {
            isAutoJoin = guild.IsAutoJoin == 1;
            SetJoin();
        }

        private void SetJoin()
        {
            if(isAutoJoin)
            {
                view.SetJoin(LocalizeKey._33092.ToText()); // 즉시 가입
            }
            else
            {
                view.SetJoin(LocalizeKey._33093.ToText()); // 신청 가입
            }
        }

        public void OnClickedPrevious()
        {
            isAutoJoin = !isAutoJoin;
            SetJoin();
        }

        public void OnClickedNext()
        {
            isAutoJoin = !isAutoJoin;
            SetJoin();
        }
        public async void OnClickedConfirm()
        {           
            await guild.RequestGuildAutoJoinUpdate(System.Convert.ToByte(isAutoJoin));
            view.CloseUI();
        }
    }
}
