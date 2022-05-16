using Ragnarok.SceneComposition;
using UnityEngine;

namespace Ragnarok
{
    public class TimePatrolZone : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] int index;
        [SerializeField] PlayerSpawnZone playerSpawnZone; // 플레이어 스폰존
        [SerializeField] SpawnZone[] monsterZones; // 몬스터 스폰존
        [SerializeField] DoorZone doorZone;

        public int GetIndex()
        {
            return index;
        }

        /// <summary>
        /// 플레이어 포지션
        /// </summary>
        public Vector3 GetPlayerPosition()
        {
            return playerSpawnZone.GetCenter();
        }

        /// <summary>
        /// 일반 몬스터 소환 Zone 수
        /// </summary>
        public int ZoneCount => monsterZones.Length;

        /// <summary>
        /// 일반 몬스터 포지션 (단일)
        /// </summary>
        public Vector3 GetMonsterPosition(int zoneIndex)
        {
            return monsterZones[zoneIndex].GetCenter();
        }

        /// <summary>
        /// 일반 몬스터 포지션 (복수)
        /// </summary>
        public Vector3[] GetMonsterPositions(int zoneIndex, int count)
        {
            Vector3[] output = new Vector3[count];

            Vector3 center = GetMonsterPosition(zoneIndex);
            float radius = monsterZones[zoneIndex].radius;
            float rad360 = Mathf.PI * 2;
            float startRad = Random.Range(0, rad360); // 시작 각도
            float addRad = rad360 / count; // 추가 각도
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Sin(startRad + (addRad * i)) * radius;
                float z = Mathf.Cos(startRad + (addRad * i)) * radius;

                output[i] = center + new Vector3(x, 0, z);
            }

            return output;
        }

        public Transform GetSpawn()
        {
            return doorZone.GetSpawn();
        }

        public void OpenDoor()
        {
            doorZone.OpenDoor();
        }

        public void CloseDoor()
        {
            doorZone.CloseDoor();
        }

        public bool IsDoorEffect()
        {
            return doorZone.IsZoneEffect();
        }

        bool IInspectorFinder.Find()
        {
#if UNITY_EDITOR
            index = transform.GetSiblingIndex() + 1;
            gameObject.name = index.ToString("00");

            monsterZones = transform.GetComponentsInChildren<SpawnZone>();
            for (int i = 0; i < monsterZones.Length; i++)
            {
                monsterZones[i].name = $"MonsterSpawnZone{i:00}";
            }
            doorZone = transform.GetComponentInChildren<DoorZone>();
#endif
            return true;
        }
    }
}