namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterShareReward"/>
    /// </summary>
    public sealed class CharacterShareRewardPresenter : ViewPresenter
    {
        /******************** Models ********************/
        private readonly SharingModel sharingModel;
        private readonly CharacterModel characterModel;

        public event System.Action OnUpdateSharingState
        {
            add { sharingModel.OnUpdateSharingState += value; }
            remove { sharingModel.OnUpdateSharingState -= value; }
        }

        public event System.Action<bool,bool,bool> OnUpdateShareAddReward
        {
            add { sharingModel.OnUpdateShareAddReward += value; }
            remove { sharingModel.OnUpdateShareAddReward -= value; }
        }

        public CharacterShareRewardPresenter()
        {
            sharingModel = Entity.player.Sharing;
            characterModel = Entity.player.Character;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 플레이어 썸네일
        /// </summary>
        public string GetThumbnail()
        {
            return characterModel.GetProfileName();
        }

        /// <summary>
        /// 내 캐릭터 셰어상태 반환
        /// </summary>
        public SharingModel.SharingState GetSharingState()
        {
            return sharingModel.GetSharingState();
        }

        /// <summary>
        /// 내 캐릭터 셰어보상 반환
        /// </summary>
        public SharingRewardPacket GetSharingRewardData()
        {
            return sharingModel.GetSharingRewardPacket();
        }

        /// <summary>
        /// 보상 받기
        /// </summary>
        public void RequestShareCharacterRewardGet()
        {
            sharingModel.RequestShareCharacterRewardGet().WrapNetworkErrors();
        }
    }
}