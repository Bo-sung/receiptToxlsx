namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICouponPopup"/>
    /// </summary>
    public class CouponPopupPresenter : ViewPresenter
    {
        private readonly IGamePotImpl gamePotImpl;

        // <!-- Models --!>
        private readonly CharacterModel characterModel;
        private readonly UserModel userModel;
        
        // <!-- Repositories --!>
        private readonly ConnectionManager connectionManager;

        public CouponPopupPresenter()
        {
            gamePotImpl = GamePotSystem.Instance;

            characterModel = Entity.player.Character;
            userModel = Entity.player.User;

            connectionManager = ConnectionManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void RewardNormalCoupon(string code)
        {
            int serverIndex = connectionManager.GetSelectServerGroupId();
            int channelIndex = userModel.GetChannelIndex();
            int uid = userModel.UID;
            int cid = characterModel.Cid;
            string serverPosition = connectionManager.GetServerPosition();
            string userData = $"{serverIndex}_{channelIndex}_{uid}_{cid}_{serverPosition}_{code}";
            gamePotImpl.Coupon(code, userData);
        }
    }
}