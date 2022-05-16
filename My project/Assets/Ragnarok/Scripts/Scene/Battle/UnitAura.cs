using UnityEngine;

namespace Ragnarok
{
    public class UnitAura : PoolObject
    {
        public enum AuraType
        {
            MonsterRed = 1,
            MonsterBlue = 2,
        }

        [SerializeField] GameObject monsterRed, monsterBlue;

        public override void OnSpawn()
        {
            base.OnSpawn();

            Clear();
        }

        public void Initialize(params AuraType[] auraTypes)
        {
            foreach (var item in auraTypes)
            {
                Show(item);
            }
        }

        public void Show(AuraType auraType)
        {
            GameObject goEffect = GetEffect(auraType);
            NGUITools.SetActive(goEffect, true);
        }

        public void Hide(AuraType auraType)
        {
            GameObject goEffect = GetEffect(auraType);
            NGUITools.SetActive(goEffect, false);
        }

        public void Clear()
        {
            foreach (AuraType item in System.Enum.GetValues(typeof(AuraType)))
            {
                Hide(item);
            }
        }
        
        private GameObject GetEffect(AuraType auraType)
        {
            switch (auraType)
            {
                case AuraType.MonsterRed:
                    return monsterRed;

                case AuraType.MonsterBlue:
                    return monsterBlue;
            }

            return null;
        }
    }
}