namespace Ragnarok
{
    public class LoginBonusPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly UserModel userModel;

        public LoginBonusPresenter()
        {
            userModel = Entity.player.User;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public string GetImageLanguageType()
        {
            LanguageConfig config = LanguageConfig.GetBytKey(Language.Current);
            return config.type;
        }

        public bool IsNewDaily()
        {
            return userModel.IsNewDaily;
        }
    }
}