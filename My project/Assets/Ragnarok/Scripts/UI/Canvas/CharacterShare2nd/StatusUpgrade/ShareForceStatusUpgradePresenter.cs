namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIShareForceStatusUpgrade"/>
    /// </summary>
    public sealed class ShareForceStatusUpgradePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly ShareStatBuildUpDataManager shareStatBuildUpDataRepo;

        // <!-- Event --!>
        public event System.Action OnUpdateShareForceStatus
        {
            add { characterModel.OnUpdateShareForceStatus += value; }
            remove { characterModel.OnUpdateShareForceStatus -= value; }
        }

        public ShareForceStatusUpgradePresenter()
        {
            characterModel = Entity.player.Character;
            shareStatBuildUpDataRepo = ShareStatBuildUpDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public DataGroup<ShareStatBuildUpData> Get(int group)
        {
            return shareStatBuildUpDataRepo.Get(group);
        }

        /// <summary>
        /// 쉐어포스 그룹 레벨 반환
        /// </summary>
        public int GetLevel(int group)
        {
            return characterModel.GetShareForceStatusLevel(group);
        }

        /// <summary>
        /// 현재 스탯 포인트
        /// </summary>
        public int GetStatPoint()
        {
            return characterModel.GetShareForce();
        }

        /// <summary>
        /// 쉐어포스 스탯 강화
        /// </summary>
        public void RequestShareStatBuildUp(int group, int level)
        {
            characterModel.RequestShareStatBuildUp(group, level).WrapNetworkErrors();
        }
    }
}