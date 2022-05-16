using UnityEngine;

namespace Ragnarok.SceneComposition
{
    /// <summary>
    /// 배틀존
    /// </summary>
    public class BattleZone : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] BattleZoneType battleZoneType;
        [SerializeField] int index;

        Vector3 center;

        void Awake()
        {
            center = transform.position;
        }

        public int GetIndex()
        {
            return index;
        }

        public Vector3 GetCenter()
        {
            return center;
        }

        public BattleZoneType GetBattleZoneType()
        {
            return battleZoneType;
        }

        bool IInspectorFinder.Find()
        {
#if UNITY_EDITOR
            transform.tag = Tag.PORTAL;

            if (battleZoneType == BattleZoneType.NonBattleZone)
            {
                transform.SetAsLastSibling();
                index = 0;
            }
#endif
            return true;
        }
    }
}