using UnityEngine;

namespace Ragnarok
{
    public interface IItemPool
    {
        PoolObject SpawnWeapon(string name);

        Transform GetParent(Transform body, EquipmentClassType weaponType);

        PoolObject SpawnCostume(string name);

        Transform GetParent(Transform body, CostumeType weaponType);

        PoolObject Spawn(string key, Transform parent, bool worldPositionStays);

        FollowTarget SpawnCostumePet(string name, Vector3 position);
    }
}