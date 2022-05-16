using UnityEngine;

namespace Ragnarok.SceneComposition
{
    public class MazeSpawnZone : SpawnZone, IInspectorFinder
    {
        [SerializeField] Transform[] patrols;

        public Vector3[] GetPatrolPosition()
        {
            Vector3[] vectors = new Vector3[patrols.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i] = patrols[i].position;
            }
            return vectors;
        }

        bool IInspectorFinder.Find()
        {
            patrols = transform.GetComponentsInChildren<Transform>();
            return true;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            {
                for (int i = 0; i < patrols.Length; i++)
                {
                    if (i == 0)
                    {
                        Gizmos.DrawWireSphere(patrols[i].position, radius);
                    }
                    else
                    {
                        Gizmos.DrawWireSphere(patrols[i].position, radius * 0.5f);
                    }
                }
            }
            Gizmos.color = Color.white;
        }
#endif
    }
}
