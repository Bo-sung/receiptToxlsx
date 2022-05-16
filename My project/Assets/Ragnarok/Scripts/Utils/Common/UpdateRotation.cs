using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UpdateRotation : MonoBehaviour
    {
        [SerializeField, Rename(displayName = "회전 속도")]
        float speed = 200f;

        private void Update()
        {
            transform.localRotation = Quaternion.Euler(0f, Time.deltaTime * speed, 0f) * transform.localRotation;
        }
    } 
}
