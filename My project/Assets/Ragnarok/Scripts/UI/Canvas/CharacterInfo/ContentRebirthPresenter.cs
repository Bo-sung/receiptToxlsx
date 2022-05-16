namespace Ragnarok
{
    public sealed class ContentRebirthPresenter : ViewPresenter
    {
        public interface IView
        {
            void ShowBaseLevel(int level);
            void ShowNotice(int level);
            void ShowAccrueSP(int AP);
            void ShowAddSP(int AP, bool firstBonus);
            void ShowRemainSP(int AP);
            void SetActiveFirstRebirthNoti(bool value);
        }

        private readonly IView view;
        private readonly CharacterModel characterModel;
        private readonly InventoryModel inventory;
        private int[] rebirthMat;

        public ContentRebirthPresenter(IView view)
        {
            this.view = view;
            characterModel = Entity.player.Character;
            inventory = Entity.player.Inventory;
            rebirthMat = new int[2] { BasisItem.RebirthMaterial.GetID(), BasisItem.RebirthMaterial_CanTrade.GetID() };
        }

        public override void AddEvent()
        {
            characterModel.OnUpdateLevel += OnUpdateLevel;
            characterModel.OnRebirth += OnRebirth;
            characterModel.OnRebirth += SetActiveFirstRebirthNoti;

            view.ShowNotice(characterModel.PossibleRebirthLv);

            OnUpdateLevel(characterModel.Level);
            OnRebirth();
        }

        public override void RemoveEvent()
        {
            characterModel.OnUpdateLevel -= OnUpdateLevel;
            characterModel.OnRebirth -= OnRebirth;
            characterModel.OnRebirth -= SetActiveFirstRebirthNoti;
        }

        public void OnShow()
        {
            view.SetActiveFirstRebirthNoti(characterModel.RebirthCount == 0);
        }

        void OnUpdateLevel(int level)
        {
            view.ShowBaseLevel(level);
            view.ShowAddSP(characterModel.AddRibirthPoint, characterModel.RebirthCount == 0);
        }

        void OnRebirth()
        {
            view.ShowAccrueSP(characterModel.RebirthAccrueCount);
            view.ShowRemainSP(characterModel.RemainAccruePoint);
            OnUpdateLevel(characterModel.Level);
        }

        /// <summary>전승</summary>
        public void OnClickedBtnCharacterRebirth()
        {
            RewardData material = new RewardData(6, rebirthMat[1], 1);
            int myCount = inventory.GetItemCount(rebirthMat[0]) + inventory.GetItemCount(rebirthMat[1]);
            int needCount = 1;

            string description = string.Empty;
            bool isAllow = true;

            if (!characterModel.HaveAddPoint)
            {
                isAllow = false;
                description = LocalizeKey._90015.ToText(); // 더 이상 획득 가능한 능력치 포인트가 없습니다.
            }
            else if (!characterModel.CanRebirthLv)
            {
                isAllow = false;
                description = LocalizeKey._90014.ToText() // 전승은 {LEVEL}레벨부터 가능합니다.
                    .Replace("{LEVEL}", characterModel.PossibleRebirthLv.ToString());
            }
            else if (myCount < needCount)
            {
                isAllow = false;
                // [62AEE4][C]{ITEM}[/c][-] 재료가 부족합니다.
                description = LocalizeKey._90285.ToText().Replace(ReplaceKey.ITEM, material.ItemName);
            }

            if (isAllow)
            {
                description = LocalizeKey._90043.ToText(); // 전승시 레벨1로 초기화되고 추가 능력치 포인트를 획득합니다.\n전승하시겠습니까?
            }

            UI.Show<UISelectMaterialPopup>().Set(material, myCount, needCount, description, LocalizeKey._4003.ToText(), isAllow, RequestCharacterRebirth);
        }

        void RequestCharacterRebirth()
        {
            characterModel.RequestCharacterRebirth().WrapNetworkErrors();
        }

        void SetActiveFirstRebirthNoti()
        {
            view.SetActiveFirstRebirthNoti(false);
        }
    }
}
