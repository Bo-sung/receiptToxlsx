namespace Ragnarok
{
    public interface IStageInfoPopupData
    {
        #region MvpMonster

        /// <summary>
        /// mvpMonster 그룹 아이디
        /// </summary>
        int MvpMonsterGroupId { get; }

        /// <summary>
        /// 소환 레벨 - mvpMonster
        /// </summary>
        int MvpMonsterLevel { get; }

        /// <summary>
        /// 보상 최대 인덱스 - mvpMonster
        /// </summary>
        int MaxMvpMonsterRewardIndex { get; }
        
        /// <summary>
        /// 인덱스에 해당하는 보상 Id
        /// </summary>
        int GetMvpMonsterRewardId(int index);

        #endregion

        #region NormalMonster

        /// <summary>
        /// 소환 가능한 최대 인덱스 - normalMonster
        /// </summary>
        int MaxNormalMonsterIndex { get; }

        /// <summary>
        /// 인덱스에 해당하는 몬스터 Id - normalMonster
        /// </summary>
        int GetNormalMonsterId(int index);

        /// <summary>
        /// 소환 레벨 - normalMonster
        /// </summary>
        int NormalMonsterLevel { get; }

        /// <summary>
        /// 보상 최대 인덱스 - normalMonster
        /// </summary>
        int MaxNormalMonsterRewardIndex { get; }

        /// <summary>
        /// 인덱스에 해당하는 보상 Id - normalMonster
        /// </summary>
        int GetNormalMonsterRewardId(int index);

        #endregion
    }
}