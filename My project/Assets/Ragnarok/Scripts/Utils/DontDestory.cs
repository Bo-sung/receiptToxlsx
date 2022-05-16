using UnityEngine;

namespace Ragnarok
{
    public class DontDestory : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
