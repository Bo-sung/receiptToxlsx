using UnityEngine;

namespace Ragnarok
{
    public class Billboard : MonoBehaviour
    {
        private Transform cameraTrans = null;

        private void Update()
        {
            if (cameraTrans == null)
                cameraTrans = Camera.main.transform;

            if (cameraTrans != null)
                transform.rotation = cameraTrans.rotation;
        }
    }
}