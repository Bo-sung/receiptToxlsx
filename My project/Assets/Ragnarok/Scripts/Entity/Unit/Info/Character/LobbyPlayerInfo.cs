using UnityEngine;

namespace Ragnarok
{
    public interface ILobbyPlayerInfo
    {
        int CID { get; }
        string NickName { get; }
        string GuildName { get; }
        Vector3 Pos { get; }
        long WeaponId { get; }
        string PrivateStoreComment { get; }
        Job Job { get; }
        Gender Gender { get; }
        int Level { get; }
        PrivateStoreSellingState SellingState { get; }
        string CostumeName { get; }
        int UID { get; }
    }
}