using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.SceneComposition
{
    public sealed class EndlessTowerMap : Map
    {
        public enum LightType
        {
            Normal = 1,
            Boss,
            Clear,
        }

        [SerializeField] GameObject goNormalFloor;
        [SerializeField] GameObject goBossFloor;
        [SerializeField] GameObject goClear;

        private List<LightInfo> lightInfoList;
        private LightType lightType;

        void Awake()
        {
            lightInfoList = new List<LightInfo>
            {
                new LightInfo(LightType.Normal, goNormalFloor),
                new LightInfo(LightType.Boss, goBossFloor),
                new LightInfo(LightType.Clear, goClear),
            };
        }

        public override void SetEndlessTowerLight(LightType type)
        {
            if (lightType == type)
                return;

            lightType = type;

            for (int i = 0; i < lightInfoList.Count; i++)
            {
                lightInfoList[i].Set(type == lightInfoList[i].lightType);
            }
        }

        public override void TweenEndlessTowerLight(LightType type)
        {
            if (lightType == type)
                return;

            TweenLightInfo(lightType, isLightOn: false); // 이전 Light TweenOff

            lightType = type;
            TweenLightInfo(lightType, isLightOn: true); // 현재 Light TweenOn
        }

        private void TweenLightInfo(LightType type, bool isLightOn)
        {
            for (int i = 0; i < lightInfoList.Count; i++)
            {
                if (lightInfoList[i].lightType == type)
                {
                    lightInfoList[i].Tween(duration: 0.4f, isLightOn);
                    break;
                }
            }
        }

        private class LightInfo
        {
            public readonly LightType lightType;
            public readonly GameObject parent;

            public readonly Light[] lights;
            public readonly float[] intensities;

            public LightInfo(LightType type, GameObject go)
            {
                lightType = type;
                parent = go;

                lights = parent.GetComponentsInChildren<Light>();
                intensities = new float[lights.Length];
                for (int i = 0; i < intensities.Length; i++)
                {
                    intensities[i] = lights[i].intensity;
                }
            }

            public void Set(bool isLightOn)
            {
                NGUITools.SetActive(parent, isLightOn);

                if (isLightOn)
                {
                    for (int i = 0; i < lights.Length; i++)
                    {
                        lights[i].intensity = intensities[i]; // 자식의 Light 값을 최초 값으로 설정
                    }
                }
            }

            public void Tween(float duration, bool isLightOn)
            {
                NGUITools.SetActive(parent, true);
                Timing.RunCoroutine(YieldTween(duration, isLightOn).CancelWith(parent));
            }

            IEnumerator<float> YieldTween(float duration, bool isLightOn)
            {
                float normalizedTime = 0;
                float time = 0f;
                while (normalizedTime < 1f)
                {
                    normalizedTime = Mathf.Clamp01(time / duration);

                    for (int i = 0; i < lights.Length; i++)
                    {
                        float start = isLightOn ? 0 : intensities[i];
                        float end = isLightOn ? intensities[i] : 0;
                        lights[i].intensity = Mathf.Lerp(start, end, normalizedTime);
                    }

                    time += Time.deltaTime;
                    yield return Timing.WaitForOneFrame;
                }

                NGUITools.SetActive(parent, isLightOn);
            }
        }
    }
}