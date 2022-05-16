using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    public class UIRoundTexture : UITexture
    {
        public enum RoundMode
        {
            /// <summary>
            /// 일반
            /// </summary>
            None,

            /// <summary>
            /// 라운드
            /// </summary>
            Round,
        }

        [HideInInspector, SerializeField]
        RoundMode roundMode;

        public RoundMode Round
        {
            get { return roundMode; }
            set
            {
                if (roundMode == value)
                    return;

                roundMode = value;

                if (panel != null)
                {
                    panel.RemoveWidget(this);
                    panel = null;
                }

                CreatePanel();
            }
        }

        [HideInInspector, SerializeField]
        float roundValue = 0;

        public float RoundValue
        {
            get
            {
                return roundValue;
            }
            set
            {
                roundValue = Mathf.Max(0, value);
                MarkAsChanged();
            }
        }

        public override void OnFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
        {
            if (Round == RoundMode.Round && RoundValue > 0)
            {
                OnFillRound(verts, uvs, cols);
                return;
            }

            base.OnFill(verts, uvs, cols);
        }

        void OnFillRound(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
        {
            Texture tex = mainTexture;
            if (tex == null) return;

            Rect outer = new Rect(uvRect.x * tex.width, uvRect.y * tex.height, tex.width * uvRect.width, tex.height * uvRect.height);

            float w = 1f / tex.width;
            float h = 1f / tex.height;

            outer.xMin *= w;
            outer.xMax *= w;
            outer.yMin *= h;
            outer.yMax *= h;
            mOuterUV = outer;

            Vector4 v = drawingDimensions;
            Vector4 u = drawingUVs;
            Color32 c = drawingColor;

            float width = v.z - v.x;
            float uvWidth = u.z - u.x;
            float uvRound = RoundValue * (uvWidth / width);

            int slice = Mathf.CeilToInt(RoundValue / 2);
            slice = Mathf.Min(20, slice);
            float[] xList = new float[slice + 1];
            float[] yList = new float[slice + 1];

            for (int i = 0; i < slice; i++)
            {
                float deg = -180f + (90f / slice) * i;
                float x = Mathf.Cos(deg * Mathf.Deg2Rad);
                float y = Mathf.Sqrt(1 - (x * x));
                xList[i] = 1 + x;
                yList[i] = y;
            }
            xList[slice] = 1;
            yList[slice] = 1;

            // left
            for (int i = 0; i < slice; i++)
            {
                float x1 = xList[i] * RoundValue;
                float x2 = xList[i + 1] * RoundValue;
                float uvX1 = xList[i] * uvRound;
                float uvX2 = xList[i + 1] * uvRound;

                float y1 = RoundValue - RoundValue * (yList[i]);
                float y2 = RoundValue - RoundValue * (yList[i + 1]);
                float uvY1 = uvRound - uvRound * (yList[i]);
                float uvY2 = uvRound - uvRound * (yList[i + 1]);

                verts.Add(new Vector3(v.x + x1, v.y + y1));
                verts.Add(new Vector3(v.x + x1, v.w - y1));
                verts.Add(new Vector3(v.x + x2, v.w - y2));
                verts.Add(new Vector3(v.x + x2, v.y + y2));
                uvs.Add(new Vector2(u.x + uvX1, u.y + uvY1));
                uvs.Add(new Vector2(u.x + uvX1, u.w - uvY1));
                uvs.Add(new Vector2(u.x + uvX2, u.w - uvY2));
                uvs.Add(new Vector2(u.x + uvX2, u.y + uvY2));
                cols.Add(c);
                cols.Add(c);
                cols.Add(c);
                cols.Add(c);
            }

            // center
            verts.Add(new Vector3(v.x + RoundValue, v.y));
            verts.Add(new Vector3(v.x + RoundValue, v.w));
            verts.Add(new Vector3(v.z - RoundValue, v.w));
            verts.Add(new Vector3(v.z - RoundValue, v.y));
            uvs.Add(new Vector2(u.x + uvRound, u.y));
            uvs.Add(new Vector2(u.x + uvRound, u.w));
            uvs.Add(new Vector2(u.z - uvRound, u.w));
            uvs.Add(new Vector2(u.z - uvRound, u.y));
            cols.Add(c);
            cols.Add(c);
            cols.Add(c);
            cols.Add(c);

            // right
            for (int i = 0; i < slice; i++)
            {
                float x1 = xList[i] * RoundValue;
                float x2 = xList[i + 1] * RoundValue;
                float uvX1 = xList[i] * uvRound;
                float uvX2 = xList[i + 1] * uvRound;

                float y1 = RoundValue - RoundValue * (yList[i]);
                float y2 = RoundValue - RoundValue * (yList[i + 1]);
                float uvY1 = uvRound - uvRound * (yList[i]);
                float uvY2 = uvRound - uvRound * (yList[i + 1]);

                verts.Add(new Vector3(v.z - x1, v.y + y1));
                verts.Add(new Vector3(v.z - x1, v.w - y1));
                verts.Add(new Vector3(v.z - x2, v.w - y2));
                verts.Add(new Vector3(v.z - x2, v.y + y2));
                uvs.Add(new Vector2(u.z - uvX1, u.y + uvY1));
                uvs.Add(new Vector2(u.z - uvX1, u.w - uvY1));
                uvs.Add(new Vector2(u.z - uvX2, u.w - uvY2));
                uvs.Add(new Vector2(u.z - uvX2, u.y + uvY2));
                cols.Add(c);
                cols.Add(c);
                cols.Add(c);
                cols.Add(c);
            }
        }
    }
}