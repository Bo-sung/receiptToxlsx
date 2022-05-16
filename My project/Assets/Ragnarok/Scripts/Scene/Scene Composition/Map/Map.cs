using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok.SceneComposition
{
    public class Map : MonoBehaviour
    {
        [SerializeField] SpawnZone playerZone;
        [SerializeField] SpawnZone bossZone;
        [SerializeField] SpawnZone[] monsterZones;
        [SerializeField] Vector2 moveLimitX; // 몬스터 랜덤 생성 제한범위
        [SerializeField] Vector2 moveLimitZ; // 몬스터 랜덤 생성 제한범위        

        [SerializeField] SpawnZone guardianZone;
        [SerializeField] DefenceSpawnZone[] defenceMonsterZones;

        [SerializeField] SpawnZone opponentZone;

        [SerializeField] MazeNpcSpawnZone[] npcSpawnZones;
        [SerializeField] WayPointZone[] wayPoints;

        [SerializeField] MazeSpawnZone[] fixedSpawnZone;
        [SerializeField] MazeSpawnZone[] randomSpawnZone1;
        [SerializeField] MazeSpawnZone[] randomSpawnZone2;
        [SerializeField] SpawnZone[] boxSpawnZone;
        [SerializeField] PortalZone endPortal;

        [SerializeField] TamingSpawnZone[] tamingSpawnZones;

        /// <summary>
        /// 일반 몬스터 소환 Zone 수
        /// </summary>
        public int ZoneCount => monsterZones.Length;

        void Start()
        {
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            NGUITools.Destroy(audioListener);
        }

        /// <summary>
        /// 플레이어 포지션
        /// </summary>
        public Vector3 GetPlayerPosition()
        {
            return playerZone.GetCenter();
        }

        public Quaternion GetPlayerRotation()
        {
            return playerZone.GetRotation();
        }

        /// <summary>
        /// 특정 지점을 둘러싼 형태의 포지션 반환
        /// </summary>
        public Vector3[] GetAroundPosition(Vector3 pivot, float distance, int count)
        {
            List<Vector3> positionList = new List<Vector3>();

            float radGap = Mathf.PI * 2f / count; // 각 유닛당 각도 차이
            float startRad = Random.Range(0f, Mathf.PI * 2f); // 시작 각도를 랜덤으로 주기
            for (int i = 0; i < count; ++i)
            {
                float rad = radGap * i + startRad; // 해당 유닛이 위치하게 될 각도

                float offsetX = Mathf.Cos(rad) * distance; // pivot으로부터 떨어질 거리 (X)
                float offsetZ = Mathf.Sin(rad) * distance; // pivot으로부터 떨어질 거리 (Z)

                float retX = pivot.x + offsetX; // 최종 X값 
                float retZ = pivot.z + offsetZ; // 최종 Z값 

                positionList.Add(new Vector3(retX, pivot.y, retZ));
            }

            return positionList.ToArray();
        }

        /// <summary>
        /// 보스 몬스터 포지션
        /// </summary>
        public Vector3 GetBossPosition()
        {
            return bossZone.GetCenter();
        }

        public Quaternion GetBossRotation()
        {
            return bossZone.GetRotation();
        }

        /// <summary>
        /// 일반 몬스터 포지션 (단일)
        /// </summary>
        public Vector3 GetMonsterPosition(int zoneIndex)
        {
            return monsterZones[zoneIndex].GetCenter();
        }

        public SpawnZone GetMonsterSpawnZone(int zoneIndex)
        {
            return monsterZones[zoneIndex];
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

        public Vector3[] GetMonsterPositions(Vector3 center, float radius, int count)
        {
            Vector3[] output = new Vector3[count];

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

        /// <summary>
        /// 맵 범위 내의 랜덤 스폰지점 반환
        /// </summary>
        public Vector3 GetRandomSpawnPosition()
        {
            return new Vector3(Random.Range(moveLimitX.x, moveLimitX.y), UnitMovement.POSITION_Y, Random.Range(moveLimitZ.x, moveLimitZ.y));
        }

        public Vector4 GetLimit()
        {
            return new Vector4(moveLimitX.x, moveLimitX.y, moveLimitZ.x, moveLimitZ.y);
        }

        public Bounds GetBounds()
        {
            // Extent: 사이즈(=차), Center: 중심값(=평균)
            Vector3 size = new Vector3(Mathf.Abs(moveLimitX.x - moveLimitX.y), 0f, Mathf.Abs(moveLimitZ.x - moveLimitZ.y));
            Vector3 center = new Vector3((moveLimitX.x + moveLimitX.y) / 2f, 0f, (moveLimitZ.x + moveLimitZ.y) / 2f);

            return new Bounds(center, size);
        }

        public Vector3 ClampInMap(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, moveLimitX.x, moveLimitX.y);
            pos.z = Mathf.Clamp(pos.z, moveLimitZ.x, moveLimitZ.y);
            return pos;
        }

        /// <summary>
        /// 수호자 포지션
        /// </summary>
        public Vector3 GetGuardianPosition()
        {
            return guardianZone.GetCenter();
        }

        public Quaternion GetGuardianRotation()
        {
            return guardianZone.GetRotation();
        }

        public SpawnZone GetGuardianZone()
        {
            return guardianZone;
        }

        /// <summary>
        /// 몬스터 소환 정보 반환
        /// </summary>
        public DefenceSpawnZone GetDefenceSpawnZone(int zone)
        {
            return defenceMonsterZones[zone];
        }

        /// <summary>
        /// PVE 상대방 위치
        /// </summary>
        /// <returns></returns>
        public Vector3 GetOpponentPosition()
        {
            return opponentZone.GetCenter();
        }

        public Quaternion GetOpponentRotation()
        {
            return opponentZone.GetRotation();
        }

        /// <summary>
        /// NPC 소환 위치
        /// </summary>
        /// <returns></returns>
        public MazeNpcSpawnZone[] GetNpcSpawnZones()
        {
            return npcSpawnZones;
        }

        /// <summary>
        /// 웨이포인트 전체 반환
        /// </summary>
        public WayPointZone[] GetWayPointZones()
        {
            return wayPoints;
        }

        /// <summary>
        /// 지정한 웨이포인트 반환 (nullable)
        /// </summary>
        public WayPointZone GetWayPointZone(int id)
        {
            return wayPoints.FirstOrDefault(e => e.Id == id);
        }

        public MazeSpawnZone[] GetFixedZone()
        {
            return fixedSpawnZone;
        }

        public MazeSpawnZone[] GetRandomZone(int pathType)
        {
            if (pathType == 1)
            {
                return randomSpawnZone1;
            }
            return randomSpawnZone2;
        }

        public SpawnZone[] GetBoxZone()
        {
            return boxSpawnZone;
        }

        public void SetPortalActive(bool isActive)
        {
            endPortal.SetWarpActive(isActive);
        }

        public TamingSpawnZone[] GetTamingSpawnZones()
        {
            return tamingSpawnZones;
        }

        public bool TryGetSpawnZone(int zoneId, out TamingSpawnZone spawnZone)
        {
            for (int i = 0; i < tamingSpawnZones.Length; i++)
            {
                if (tamingSpawnZones[i].ZoneId == zoneId)
                {
                    spawnZone = tamingSpawnZones[i];
                    return true;
                }
            }
            spawnZone = null;
            return false;
        }

        public virtual void SetEndlessTowerLight(EndlessTowerMap.LightType type)
        {
        }

        public virtual void TweenEndlessTowerLight(EndlessTowerMap.LightType type)
        {
        }

#if UNITY_EDITOR
        [HideInInspector]
        public bool isShowGrid;

        [HideInInspector]
        public bool useHalfGrid;

        [HideInInspector]
        public Vector2Int cellIndex = Vector2Int.one * 12;

        const float CELL_SIZE = 3.6f;
        const float HALF_CELL_SIZE = CELL_SIZE * 0.5f;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            {
                float centerX = (moveLimitX.y + moveLimitX.x) / 2f;
                float centerZ = (moveLimitZ.y + moveLimitZ.x) / 2f;
                Gizmos.DrawWireCube(new Vector3(centerX, 0, centerZ), new Vector3((moveLimitX.y - moveLimitX.x), 5f, (moveLimitZ.y - moveLimitZ.x)));
            }
            Gizmos.color = Color.white;

            if (isShowGrid)
            {
                float cellSize = (useHalfGrid ? HALF_CELL_SIZE : CELL_SIZE) * transform.localScale.x;

                for (int y = 0; y < cellIndex.y; y++)
                {
                    for (int x = 0; x < cellIndex.x; x++)
                    {
                        if (x % 2 == 0 && y % 2 == 1)
                            continue;

                        if (x % 2 == 1 && y % 2 == 0)
                            continue;

                        Vector3 position = new Vector3(y * cellSize, 0f, x * cellSize);
                        UnityEditor.Handles.DrawWireCube(position, Vector3.one * cellSize);
                        //UnityEditor.Handles.Label(position, $"({y},{x})", UnityEditor.EditorStyles.miniLabel);
                    }
                }
            }
        }
#endif
    }
}