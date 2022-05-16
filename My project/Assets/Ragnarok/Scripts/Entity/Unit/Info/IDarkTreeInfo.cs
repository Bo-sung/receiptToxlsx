namespace Ragnarok
{
    public interface IDarkTreeInfo
    {
        event System.Action OnUpdate;

        int GetSelectedRewardId();
        RewardData GetSelectedReward();

        int GetCurPoint();
        int GetMaxPoint();
        int GetTotalMinutes();

        int GetRemainMinitues();
        bool HasRemainTime();
        bool HasStandByReward();
    }
}