using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CustomEditor(typeof(TweenPositionAdvanced))]
    public class TweenPositionAdvancedEditor : Editor
    {
        TweenPositionAdvanced tw;

        void OnEnable()
        {
            tw = target as TweenPositionAdvanced;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            NGUIEditorTools.SetLabelWidth(120f);

            GUI.changed = false;

            Vector3 from = EditorGUILayout.Vector3Field("From", tw.from);
            Vector3 to = EditorGUILayout.Vector3Field("To", tw.to);

            if (GUI.changed)
            {
                NGUIEditorTools.RegisterUndo("Tween Change", tw);
                tw.from = from;
                tw.to = to;
                NGUITools.SetDirty(tw);
            }

            DrawCommonProperties();
        }

        protected void DrawCommonProperties()
        {
            if (NGUIEditorTools.DrawHeader("Tweener"))
            {
                NGUIEditorTools.BeginContents();
                NGUIEditorTools.SetLabelWidth(110f);

                GUI.changed = false;

                UITweener.Style style = (UITweener.Style)EditorGUILayout.EnumPopup("Play Style", tw.style);
                GUILayout.BeginHorizontal();
                AnimationCurve curveX = EditorGUILayout.CurveField("Animation Curve", tw.animationCurve, GUILayout.Width(170f), GUILayout.Height(62f));
                AnimationCurve curveY = EditorGUILayout.CurveField(tw.animationCurveY, GUILayout.Width(58f), GUILayout.Height(62f));
                GUILayout.EndHorizontal();
                //UITweener.Method method = (UITweener.Method)EditorGUILayout.EnumPopup("Play Method", tw.method);

                GUILayout.BeginHorizontal();
                float dur = EditorGUILayout.FloatField("Duration", tw.duration, GUILayout.Width(170f));
                GUILayout.Label("seconds");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                float del = EditorGUILayout.FloatField("Start Delay", tw.delay, GUILayout.Width(170f));
                GUILayout.Label("seconds");
                GUILayout.EndHorizontal();

                int tg = EditorGUILayout.IntField("Tween Group", tw.tweenGroup, GUILayout.Width(170f));
                bool ts = EditorGUILayout.Toggle("Ignore TimeScale", tw.ignoreTimeScale);
                bool fx = EditorGUILayout.Toggle("Use Fixed Update", tw.useFixedUpdate);

                if (GUI.changed)
                {
                    NGUIEditorTools.RegisterUndo("Tween Change", tw);
                    tw.animationCurve = curveX;
                    tw.animationCurveY = curveY;
                    //tw.method = method;
                    tw.style = style;
                    tw.ignoreTimeScale = ts;
                    tw.tweenGroup = tg;
                    tw.duration = dur;
                    tw.delay = del;
                    tw.useFixedUpdate = fx;
                    NGUITools.SetDirty(tw);
                }
                NGUIEditorTools.EndContents();
            }

            NGUIEditorTools.SetLabelWidth(80f);
            NGUIEditorTools.DrawEvents("On Finished", tw, tw.onFinished);
        }
    }
}