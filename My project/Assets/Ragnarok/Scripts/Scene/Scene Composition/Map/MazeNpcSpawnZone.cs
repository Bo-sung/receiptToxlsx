using Ragnarok.SceneComposition;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ragnarok
{
    public class MazeNpcSpawnZone : SpawnZone
    {
        [SerializeField] GameObject actor;

        public GameObject Actor => actor;
    }
}