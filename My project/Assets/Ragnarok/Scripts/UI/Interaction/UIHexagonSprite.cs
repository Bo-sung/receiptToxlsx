using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UISprite))]
    public class UIHexagonSprite : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UISprite sprite;

        [SerializeField, Range(0f, 1f)]
        float v1 = 1f, v2 = 1f, v3 = 1f, v4 = 1f, v5 = 1f, v6 = 1f;

        [SerializeField]
        float duration;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (sprite == null)
                sprite = GetComponent<UISprite>();

            sprite.MarkAsChanged();
        }
#endif

        void Start()
        {
            if (sprite == null)
                sprite = GetComponent<UISprite>();

            sprite.onPostFill = OnPostFill;
        }

        void OnDestroy()
        {
            Timing.KillCoroutines(gameObject);
        }

        void OnDisable()
        {
            Set(0f, 0f, 0f, 0f, 0f, 0f);
        }

        private void OnPostFill(UIWidget widget, int bufferOffset, List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
        {
            Vector4 v = widget.drawingDimensions;

            List<Vector2> u = new List<Vector2>(uvs);
            List<Color> gc = new List<Color>(cols);

            Vector3[] ver = new Vector3[7];
            ver[0] = Vector3.zero;
            ver[1] = new Vector3(0, v.w) * v1;
            ver[2] = new Vector3(v.z, v.w * 0.5f) * v2;
            ver[3] = new Vector3(v.z, v.y * 0.5f) * v3;
            ver[4] = new Vector3(0, v.y) * v4;
            ver[5] = new Vector3(v.x, v.y * 0.5f) * v5;
            ver[6] = new Vector3(v.x, v.w * 0.5f) * v6;

            verts.Clear();
            uvs.Clear();
            cols.Clear();
            for (int i = 0; i < 6; i++)
            {
                verts.Add(ver[0]);
                verts.Add(ver[i + 1]);
                verts.Add(ver[i == 5 ? 1 : i + 2]);
                verts.Add(ver[0]);

                uvs.AddRange(u);
                cols.AddRange(gc);
            }
        }

        public void UpdateVertext(float v1, float v2, float v3, float v4, float v5, float v6)
        {
            Timing.KillCoroutines(gameObject);

            if (duration > 0f)
            {
                Timing.RunCoroutine(TimeProcess(v1, v2, v3, v4, v5, v6));
                return;
            }

            Set(v1, v2, v3, v4, v5, v6);
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        IEnumerator<float> TimeProcess(params float[] endValues)
        {
            float lastTime = Time.time;
            float runningTime = 0f;
            float percentage = 0f;
            float timeRate = 1f / duration;

            float[] startValues = { v1, v2, v3, v4, v5, v6 };

            while (percentage < 1f)
            {
                runningTime = Time.time - lastTime;
                percentage = runningTime * timeRate;

                float v1 = Mathf.Lerp(startValues[0], endValues[0], percentage);
                float v2 = Mathf.Lerp(startValues[1], endValues[1], percentage);
                float v3 = Mathf.Lerp(startValues[2], endValues[2], percentage);
                float v4 = Mathf.Lerp(startValues[3], endValues[3], percentage);
                float v5 = Mathf.Lerp(startValues[4], endValues[4], percentage);
                float v6 = Mathf.Lerp(startValues[5], endValues[5], percentage);

                Set(v1, v2, v3, v4, v5, v6);

                yield return Timing.WaitForOneFrame;
            }

            Set(endValues[0], endValues[1], endValues[2], endValues[3], endValues[4], endValues[5]);
        }

        private void Set(float v1, float v2, float v3, float v4, float v5, float v6)
        {
            if (Mathf.Approximately(this.v1, v1) && Mathf.Approximately(this.v2, v2) && Mathf.Approximately(this.v3, v3) && Mathf.Approximately(this.v4, v4) && Mathf.Approximately(this.v5, v5) && Mathf.Approximately(this.v6, v6))
                return;

            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.v4 = v4;
            this.v5 = v5;
            this.v6 = v6;

            sprite.MarkAsChanged();
        }

        bool IInspectorFinder.Find()
        {
            sprite = GetComponent<UISprite>();
            return true;
        }
    }
}