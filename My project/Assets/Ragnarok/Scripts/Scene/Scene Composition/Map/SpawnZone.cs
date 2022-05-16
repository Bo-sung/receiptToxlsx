using UnityEngine;

namespace Ragnarok.SceneComposition
{
    /// <summary>
    /// 소환 지역
    /// </summary>
    public class SpawnZone : MonoBehaviour
    {
        public float radius = 3f;

        Vector3 center;
        Quaternion rotation;

        void Awake()
        {
            Transform tf = transform;
            center = tf.position;
            rotation = tf.localRotation;
        }

        public Vector3 GetCenter()
        {
            return center;
        }

        public Quaternion GetRotation()
        {
            return rotation;
        }

        public Vector3[] GetPatrolZone()
        {
            float size = 1.8f;
            Vector3[] vectors = new Vector3[4];
            vectors[0] = center + new Vector3(size, 0, size);
            vectors[1] = center + new Vector3(size, 0, -size);
            vectors[2] = center + new Vector3(-size, 0, -size);
            vectors[3] = center + new Vector3(-size, 0, size);
            return vectors;
        }

        ///// <summary>
        ///// 위치 세팅
        ///// </summary>
        //public void SetPosition(UnitActor actor)
        //{
        //    if (actor == null)
        //        return;

        //    actor.AI.SetHomePosition(center); // 가운데

        //    //SetPosition(unitActors); // 가운데를 중심으로 위치 세팅

        //    //// 바인딩 타겟 설정
        //    //foreach (var item in unitActors)
        //    //{
        //    //    item.AI.SetBindingActor(actor);
        //    //}
        //}

        ///// <summary>
        ///// 위치 세팅 (가운데를 중심으로 위치 세팅)
        ///// </summary>
        //public void SetPosition(UnitActor[] unitActors)
        //{
        //    if (unitActors == null)
        //        return;

        //    int length = unitActors.Length;

        //    float rad360 = Mathf.PI * 2;
        //    float startRad = Random.Range(0, rad360); // 시작 각도
        //    float addRad = rad360 / length; // 추가 각도
        //    for (int i = 0; i < length; i++)
        //    {
        //        float x = Mathf.Sin(startRad + (addRad * i)) * radius;
        //        float z = Mathf.Cos(startRad + (addRad * i)) * radius;

        //        unitActors[i].AI.SetHomePosition(center + new Vector3(x, 0, z)); // 위치 세팅
        //    }
        //}

        /// <summary>
        /// 랜덤 위치
        /// </summary>
        public Vector3 GetRandomPosition()
        {
            float rad360 = Mathf.PI * 2;
            float startRad = Random.Range(0, rad360); // 시작 각도
            float x = Mathf.Sin(startRad);
            float z = Mathf.Cos(startRad);
            return center + new Vector3(x, 0, z);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            {
                Gizmos.DrawWireSphere(transform.position, radius);
            }
            Gizmos.color = Color.white;
        }
#endif
    }
}