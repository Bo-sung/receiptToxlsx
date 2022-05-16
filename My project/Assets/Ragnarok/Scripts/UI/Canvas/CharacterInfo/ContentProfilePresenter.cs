using System.Threading.Tasks;

namespace Ragnarok
{
    public sealed class ContentProfilePresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void UpdateJobLevel(int level);
            void UpdateBaseLevel(int level);
            void ShowUserInfoView();
            void ShowNameChangeView();
        }

        IView view;
        private CharacterEntity player;
        InventoryModel InventoryModel;
        CharacterModel characterModel;
        StatusModel statusModel;
        GuildModel guildModel;
        CameraController cameraController;
        UIManager UIMgr;
        private readonly int freeNameChangeCount, nameChangeCatCoin;

        private readonly BattleManager battleManager;
        private readonly ConnectionManager connectionManager;

        public ContentProfilePresenter(IView view)
        {
            this.view = view;

            cameraController = CameraController.Instance;
            UIMgr = UIManager.Instance;
            freeNameChangeCount = BasisType.FREE_NAME_CHANGE_CNT.GetInt();
            nameChangeCatCoin = BasisType.NAME_CHANE_CAT_COIN.GetInt();
            battleManager = BattleManager.Instance;
            connectionManager = ConnectionManager.Instance;
        }

        public override void AddEvent()
        {

        }

        public override void RemoveEvent()
        {
            characterModel.OnUpdateJobLevel -= view.UpdateJobLevel;
            characterModel.OnUpdateLevel -= view.UpdateBaseLevel;
            statusModel.OnUpdateBasicStatus -= view.Refresh;
            guildModel.OnUpdateGuildState -= view.Refresh;
            characterModel.OnUpdateProfile -= view.Refresh;
        }

        public Job Job => characterModel.Job;
        public Gender Gender => characterModel.Gender;
        public string CharName => characterModel.Name;
        public string HexName => characterModel.CidHex;
        public int BaseLevel => characterModel.Level;
        public string ProfileName => characterModel.GetProfileName();

        internal void SetPlayer(CharacterEntity charaEntity)
        {
            if (ReferenceEquals(this.player, charaEntity))
                return;

            if (this.player != null)
            {
                characterModel.OnUpdateJobLevel -= view.UpdateJobLevel;
                characterModel.OnUpdateLevel -= view.UpdateBaseLevel;
                statusModel.OnUpdateBasicStatus -= view.Refresh;
                guildModel.OnUpdateGuildState -= view.Refresh;
                characterModel.OnUpdateProfile -= view.Refresh;
            }

            this.player = charaEntity;
            InventoryModel = this.player.Inventory;
            characterModel = this.player.Character;
            statusModel = this.player.Status;
            guildModel = this.player.Guild;

            characterModel.OnUpdateJobLevel += view.UpdateJobLevel;
            characterModel.OnUpdateLevel += view.UpdateBaseLevel;
            statusModel.OnUpdateBasicStatus += view.Refresh;
            guildModel.OnUpdateGuildState += view.Refresh;
            characterModel.OnUpdateProfile += view.Refresh;

            view.UpdateJobLevel(characterModel.JobLevel);
            view.UpdateBaseLevel(characterModel.Level);
        }

        public int JobLevel => characterModel.JobLevel;
        public string PowerText => GetPower();

        public void ShowCamera()
        {
            cameraController.SetClearshot(CameraController.Clearshot.Inven);
            UIMgr.HideHUD();
        }

        public void HideCamera()
        {
            cameraController.SetClearshot(CameraController.Clearshot.None);
            UIMgr.ShowHUD();
        }              

        public string GetCharacterName()
        {
            return characterModel.Name;
        }

        public int GetFreeNameChangeCount()
        {
            return freeNameChangeCount;
        }

        public int GetNameChangeCatCoin()
        {
            // 무료 변경 가능할 경우
            if (characterModel.NameChangeCount < freeNameChangeCount)
                return 0;

            return nameChangeCatCoin;
        }

        public void RequestChangeName(string name)
        {
            RequestChangeNameAsync(name).WrapNetworkErrors();
        }

        public void ShowUserInfoView()
        {
            view.ShowUserInfoView();
        }

        public void ShowNameChangeView()
        {
            view.ShowNameChangeView();
        }

        public void GoToTitle()
        {
            if (battleManager.Mode == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90172.ToText()); // 미로 모드에서는 캐릭터 변경을 할 수 없습니다
                return;
            }

            UI.Show<UICharacterSelect>();
        }

        private async Task RequestChangeNameAsync(string name)
        {
            await characterModel.RequestChangeName(name);
            view.ShowUserInfoView();
            view.Refresh();
        }

        string GetPower()
        {
            var sb = StringBuilderPool.Get();
            sb.Append(LocalizeKey._4047.ToText()); // 전투력 :
            sb.Append(" ");
            sb.Append(this.player.GetTotalAttackPower());
            return sb.Release();
        }

        public bool IsMyPlayer() => ReferenceEquals(this.player, Entity.player);

        public string GetGuildText()
        {
            if (guildModel.GuildId == 0)
                return LocalizeKey._31006.ToText(); // (길드 없음)

            return LocalizeKey._31002.ToText() // 길드 : {NAME}
                .Replace(ReplaceKey.NAME, guildModel.GuildName);
        }

        public string GetServerName()
        {
            return connectionManager.GetServerNameKey().ToText();
        }
    }
}