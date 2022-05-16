namespace Ragnarok
{
    public interface IPassData
    {
        int Id { get; }
        int PassLevel { get; }
        int NeedExp { get; }
        RewardData[] GetFreeRewards();
        RewardData[] GetPayRewards();
    }
}