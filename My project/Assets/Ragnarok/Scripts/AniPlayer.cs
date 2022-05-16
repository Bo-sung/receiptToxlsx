#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class AniPlayer : MonoBehaviour
    {
        Animation ani;
        ParticleSystem[] arrParticle;

        void Awake()
        {
            ani = GetComponentInChildren<Animation>(includeInactive: true);

            GameObject go = gameObject;
            ClearParticleSystemRandom(go);
            arrParticle = GetParentParticleSystem(go).ToArray();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Play();
        }

        private void Play()
        {
            Debug.LogError("Play");

            if (ani)
                ani.Play();

            if (arrParticle != null)
            {
                foreach (var item in arrParticle)
                {
                    item.Simulate(0f, true);
                    item.Play();
                }
            }
        }

        private void ClearParticleSystemRandom(GameObject go)
        {
            var psArray = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in psArray)
            {
                ps.useAutoRandomSeed = false;
            }
        }

        private List<ParticleSystem> GetParentParticleSystem(GameObject go)
        {
            List<ParticleSystem> list = new List<ParticleSystem>();
            var psArray = go.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            foreach (var ps in psArray)
            {
                if (IsRoot(ps))
                {
                    list.Add(ps);
                }
            }

            return list;
        }

        private bool IsRoot(ParticleSystem ps)
        {
            var parent = ps.transform.parent;

            if (parent == null)
                return true;

            return parent.GetComponent<ParticleSystem>() == false;
        }
    }
}

#endif