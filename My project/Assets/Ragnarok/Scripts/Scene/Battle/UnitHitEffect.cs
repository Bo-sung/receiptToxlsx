using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UnitHitEffect : MonoBehaviour
    {
        private const string BRIGHTNESS_PROPERTY_NAME = "_Brightness";
        private const string COLOR_PROPERTY_NAME = "_Color";

        [SerializeField] Renderer[] renderers;

        private MaterialPropertyBlock mpb;

        private int propertyIndex_Brightness;
        private int propertyIndex_Color;

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            propertyIndex_Brightness = Shader.PropertyToID(BRIGHTNESS_PROPERTY_NAME);
            propertyIndex_Color = Shader.PropertyToID(COLOR_PROPERTY_NAME);
        }

        void OnDisable()
        {
            StopAllCoroutines();
            ClearEffectes();
        }

        public void PlayEffect()
        {
            StopEffect();
            StartCoroutine(EffectRoutine());
        }

        public void StopEffect()
        {
            StopAllCoroutines();

            mpb.SetFloat(propertyIndex_Brightness, 1f);
            RefreshEffects();
        }

        public void SetDark()
        {
            mpb.SetColor(propertyIndex_Color, Color.gray);
            RefreshEffects();
        }

        public void ResetColor()
        {
            mpb.SetColor(propertyIndex_Color, Color.white);
            RefreshEffects();
        }

        private void RefreshEffects()
        {
            int length = renderers == null ? 0 : renderers.Length;
            for (int i = 0; i < length; ++i)
            {
                if (renderers[i] == null)
                    continue;

                renderers[i].SetPropertyBlock(mpb);
            }
        }

        private void ClearEffectes()
        {
            mpb.Clear();

            int length = renderers == null ? 0 : renderers.Length;
            for (int i = 0; i < length; ++i)
            {
                if (renderers[i] == null)
                    continue;

                renderers[i].SetPropertyBlock(null);
            }
        }

        private IEnumerator EffectRoutine()
        {
            if (renderers == null)
                yield break;

            float stopwatch = 0;
            float prog = 0;
            float oneThreshold = 0.15f;

            while (prog < 1)
            {
                stopwatch += Time.deltaTime;
                prog = Mathf.Clamp01(stopwatch / 0.15f);

                float curveVal = 1;
                if (prog > oneThreshold)
                {
                    float p2 = prog * prog;
                    float p3 = prog * p2;
                    curveVal = 2.23751f * p3 + -3.611289f * p2 + 0.3700489f * prog + 1.00373f;
                }

                float brightness = Mathf.Lerp(1, 2.5f, curveVal);
                mpb.SetFloat(propertyIndex_Brightness, brightness);
                RefreshEffects();
                yield return null;
            }

            mpb.SetFloat(propertyIndex_Brightness, 1f);
            RefreshEffects();
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(UnitHitEffect))]
        [UnityEditor.CanEditMultipleObjects]
        public class E : UnityEditor.Editor
        {
            UnitHitEffect[] uhes;

            private void OnEnable()
            {
                uhes = new UnitHitEffect[targets.Length];
                for (int i = 0; i < uhes.Length; ++i)
                    uhes[i] = targets[i] as UnitHitEffect;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("자동으로 랜더러 찾기"))
                {
                    for (int i = 0; i < uhes.Length; ++i)
                    {
                        var each = uhes[i];

                        var rendererList = new List<Renderer>(each.gameObject.GetComponentsInChildren<Renderer>(true));

                        rendererList.RemoveAll(v =>
                        {
                            return v.sharedMaterial.shader.FindPropertyIndex(BRIGHTNESS_PROPERTY_NAME) == -1 && v.sharedMaterial.shader.FindPropertyIndex(COLOR_PROPERTY_NAME) == -1;
                        });

                        each.renderers = rendererList.ToArray();
                        UnityEditor.EditorUtility.SetDirty(each);
                    }
                }
            }
        }
#endif
    }
}