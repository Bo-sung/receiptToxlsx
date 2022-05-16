using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIUserInfo"/>
    /// </summary>
    public sealed class UserInfoPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void CloseUI();
            void UpdateJobLevel(int level);
            void UpdateBaseLevel(int level);
            void ShowUserInfoView();
            void ShowNameChangeView();
        }

        IView view;
        InventoryModel InventoryModel;
        CharacterModel characterModel;
        DungeonModel DungeonModel;
        StatusModel statusModel;
        CameraController cameraController;
        UIManager UIMgr;
        private readonly int freeNameChangeCount, nameChangeCatCoin;

        public UserInfoPresenter(IView view)
        {
            this.view = view;
            InventoryModel = Entity.player.Inventory;
            characterModel = Entity.player.Character;
            DungeonModel = Entity.player.Dungeon;
            statusModel = Entity.player.Status;
            cameraController = CameraController.Instance;
            UIMgr = UIManager.Instance;
            freeNameChangeCount = BasisType.FREE_NAME_CHANGE_CNT.GetInt();
            nameChangeCatCoin = BasisType.NAME_CHANE_CAT_COIN.GetInt();
        }

        public override void AddEvent()
        {
            characterModel.OnUpdateJobLevel += view.UpdateJobLevel;
            characterModel.OnUpdateLevel += view.UpdateBaseLevel;
            statusModel.OnUpdateBasicStatus += view.Refresh;

            view.UpdateJobLevel(characterModel.JobLevel);
            view.UpdateBaseLevel(characterModel.Level);
        }

        public override void RemoveEvent()
        {
            characterModel.OnUpdateJobLevel -= view.UpdateJobLevel;
            characterModel.OnUpdateLevel -= view.UpdateBaseLevel;
            statusModel.OnUpdateBasicStatus -= view.Refresh;
        }

        public Job Job => characterModel.Job;
        public Gender Gender => characterModel.Gender;
        public string CharName => characterModel.Name;
        public string HexName => characterModel.CidHex;
        public int BaseLevel => characterModel.Level;
        public int JobLevel => characterModel.JobLevel;
        public string PowerText => SetPower();

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

        private async Task RequestChangeNameAsync(string name)
        {
            await characterModel.RequestChangeName(name);
            view.ShowUserInfoView();
            view.Refresh();
        }

        string SetPower()
        {
            var sb = StringBuilderPool.Get();
            sb.Append(LocalizeKey._4047.ToText()); // 전투력 :
            sb.Append(" ");
            sb.Append(Entity.player.GetTotalAttackPower());
            return sb.Release();
        }
    }
}