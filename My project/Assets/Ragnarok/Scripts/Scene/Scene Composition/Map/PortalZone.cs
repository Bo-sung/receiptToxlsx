using UnityEngine;

namespace Ragnarok
{
    public class PortalZone : MonoBehaviour, IAutoInspectorFinder
    {       
        [SerializeField] GameObject warpActive;
        [SerializeField] GameObject warpDeActive;

        public void SetWarpActive(bool isActive)
        {
            warpActive.SetActive(isActive);
            warpDeActive.SetActive(!isActive);
        }
    }
}
