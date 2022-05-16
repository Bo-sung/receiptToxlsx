using System;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// http://haseungbong.blogspot.com/2018/06/ngui-particle-clipping_21.html
    /// </summary>
    [RequireComponent(typeof(UIPanel))]   
    public class UIParticleClipper : MonoBehaviour
    {
        private const string ShaderName = "ParticlesAdditiveAreaClip";
        private const float ClipInterval = 0.5f;

        private UIPanel m_targetPanel;
        private Shader m_shader;

        private void Start()
        {
            // find panel
            m_targetPanel = GetComponent<UIPanel>();

            if (m_targetPanel == null)
                throw new ArgumentNullException("Cann't find the right UIPanel");
            if (m_targetPanel.clipping != UIDrawCall.Clipping.SoftClip)
                throw new InvalidOperationException("Don't need to clip");

            m_shader = Shader.Find(ShaderName);

            //if (!IsInvoking("Clip"))
            //    InvokeRepeating("Clip", 0, ClipInterval);

            //Clip();
            Invoke("Clip", 0.1f); // 오브젝트가 활성화 되는 동안 딜레이가 필요한듯..
        }

        private Vector4 CalcClipArea()
        {
            var clipRegion = m_targetPanel.finalClipRegion;
            var nguiArea = new Vector4()
            {
                x = clipRegion.x - clipRegion.z / 2,
                y = clipRegion.y - clipRegion.w / 2,
                z = clipRegion.x + clipRegion.z / 2,
                w = clipRegion.y + clipRegion.w / 2
            };

            var uiRoot = m_targetPanel.root;
            var pos = m_targetPanel.transform.position;
            var rate1 = (float)Screen.width / (float)Screen.height;
            var rate2 = (float)uiRoot.manualWidth / (float)uiRoot.manualHeight;
            const float h = 2f;
            var w = h * rate1;
            var tempH = h / uiRoot.manualHeight;
            var tempW = w / uiRoot.manualWidth;
            var tempRate = Mathf.Max(tempW, tempH);
            if (rate1 < rate2)
            {
                tempRate = Mathf.Min(tempW, tempH);
            }

            var result = new Vector4()
            {
                x = pos.x + nguiArea.x * tempRate,
                y = pos.y + nguiArea.y * tempRate,
                z = pos.x + nguiArea.z * tempRate,
                w = pos.y + nguiArea.w * tempRate
            };

            return result;
        }

        private void Clip()
        {
            var clipArea = CalcClipArea();
            var renderers = GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; ++i)
            {
                var mat = renderers[i].material;

                if (mat.name.Contains("Black"))
                    continue;

                if (mat.shader.name != ShaderName)
                    mat.shader = m_shader;

                mat.SetVector("_Area", clipArea);
            }
        }
    }
}