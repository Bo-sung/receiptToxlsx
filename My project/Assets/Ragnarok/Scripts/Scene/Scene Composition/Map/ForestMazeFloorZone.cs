using UnityEngine;

namespace Ragnarok
{
    public class ForestMazeFloorZone : MonoBehaviour
    {
        public enum FloorType { Up, Down, }

        [SerializeField] FloorType type;

        public FloorType GetFloorType()
        {
            return type;
        }

        //public void Hit()
        //{
        //    GetComponent<Collider>().enabled = false;
        //}
    }
}