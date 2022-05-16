using UnityEngine;

namespace Ragnarok
{
    public class ItemPoolManager : PoolManager<ItemPoolManager>, IItemPool
    {
        IItemContainer itemContainer;

        protected override void Awake()
        {
            base.Awake();

            itemContainer = AssetManager.Instance;
        }

        protected override Transform GetOriginal(string key)
        {
            GameObject go = itemContainer.Get(key);

            if (go == null)
            {
                Debug.LogError($"해당 프리팹이 존재하지 않습니다: {nameof(key)} = {key}");
                go = new GameObject(key);
            }

            return go.transform;
        }

        protected override PoolObject Create(string key)
        {
            GameObject go = itemContainer.Get(key);

            if (go == null)
            {
                Debug.LogError($"해당 프리팹이 존재하지 않습니다: {nameof(key)} = {key}");
                go = new GameObject(key);
            }

            return Instantiate(go).AddMissingComponent<PoolObject>();
        }

        PoolObject IItemPool.SpawnWeapon(string name)
        {
            return Spawn(name);
        }

        Transform IItemPool.GetParent(Transform body, EquipmentClassType weaponType)
        {
            string nodeName = GetNodeName(weaponType);
            return body.transform.Find(nodeName);
        }

        private string GetNodeName(EquipmentClassType weaponType)
        {
            switch (weaponType)
            {
                case EquipmentClassType.OneHandedSword: // 한손검
                case EquipmentClassType.OneHandedStaff: // 지팡이
                case EquipmentClassType.Dagger: // 단검
                case EquipmentClassType.TwoHandedSword: // 양손검
                case EquipmentClassType.TwoHandedSpear: // 양손창
                    return Constants.AvatarNode.HAND_RIGHT;

                case EquipmentClassType.Bow: // 활
                    return Constants.AvatarNode.HAND_LEFT;

                case EquipmentClassType.HeadGear:
                case EquipmentClassType.Accessory1:
                    return Constants.AvatarNode.HEAD;

                case EquipmentClassType.Garment:
                    return Constants.AvatarNode.CAPE;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(weaponType)}] {nameof(weaponType)} = {weaponType}");
        }


        PoolObject IItemPool.SpawnCostume(string name)
        {
            return Spawn(name);
        }

        Transform IItemPool.GetParent(Transform body, CostumeType type)
        {
            if(type == CostumeType.Pet)
            {
                return body;
            }

            string nodeName = GetNodeName(type);
            return body.transform.Find(nodeName);
        }

        private string GetNodeName(CostumeType type)
        {
            switch (type)
            {
                case CostumeType.OneHandedSword: // 한손검
                case CostumeType.OneHandedStaff: // 지팡이
                case CostumeType.Dagger: // 단검
                case CostumeType.TwoHandedSword: // 양손검
                case CostumeType.TwoHandedSpear: // 양손창
                    return Constants.AvatarNode.HAND_RIGHT;

                case CostumeType.Bow: // 활
                    return Constants.AvatarNode.HAND_LEFT;

                case CostumeType.Hat:
                case CostumeType.Face:
                    return Constants.AvatarNode.HEAD;

                case CostumeType.Garment:
                    return Constants.AvatarNode.CAPE;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(type)}] {nameof(type)} = {type}");
        }

        FollowTarget IItemPool.SpawnCostumePet(string name, Vector3 position)
        {
            return Spawn(name, position, Quaternion.identity) as FollowTarget;
        }
    }
}
