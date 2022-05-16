using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public class SkillTimeControl
    {
        SkillSettingEditor editor;

        private bool isPlaying;
        private float currentTime;
        private float aniTime;
        private float maxTime;
        private double lastFrameEditorTime;
        private float mouseDrag;

        public System.Action<SerializedProperty> onSelect;
        public System.Action onTimeOver;

        public bool IsInvalid
        {
            get { return maxTime == 0f; }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (isPlaying == value)
                    return;

                isPlaying = value;
            }
        }

        public float CurTime
        {
            get
            {
                double timeSinceStartup = EditorApplication.timeSinceStartup;
                float deltaTime = (float)(timeSinceStartup - lastFrameEditorTime);
                lastFrameEditorTime = timeSinceStartup;

                if (isPlaying)
                {
                    currentTime += deltaTime;

                    // Loop
                    if (currentTime > maxTime)
                    {
                        currentTime = 0f;
                        deltaTime = 0f;

                        onTimeOver?.Invoke();
                    }
                }

                return currentTime;
            }
        }

        private int extraTime = 200; // 기본값 2초

        public void Draw()
        {
            using (new GUILayout.VerticalScope(EditorStyles.objectFieldThumb))
            {
                using (new EditorGUI.DisabledGroupScope(editor == null))
                {
                    //EditorStyles.objectFieldThumb
                    using (new GUILayout.VerticalScope())
                    {
                        using (new EditorGUI.DisabledGroupScope(IsInvalid))
                        {
                            Rect rect = EditorGUILayout.GetControlRect();
                            Rect rectBar = rect;
                            rectBar.xMin += 33f;

                            if (!IsInvalid)
                            {
                                Event e = Event.current;
                                int controlID = nameof(SkillTimeControl).GetHashCode();

                                switch (e.type)
                                {
                                    case EventType.MouseDown:
                                        if (rectBar.Contains(e.mousePosition))
                                        {
                                            EditorGUIUtility.SetWantsMouseJumping(1);
                                            GUIUtility.hotControl = controlID;

                                            IsPlaying = false;
                                            mouseDrag = e.mousePosition.x - rectBar.xMin;
                                            currentTime = maxTime * Mathf.Clamp(mouseDrag, 0f, rectBar.width) / rectBar.width;
                                            e.Use();
                                        }
                                        break;

                                    case EventType.MouseUp:
                                        if (GUIUtility.hotControl == controlID)
                                        {
                                            EditorGUIUtility.SetWantsMouseJumping(0);
                                            GUIUtility.hotControl = 0;
                                            e.Use();
                                        }
                                        break;

                                    case EventType.MouseDrag:
                                        if (GUIUtility.hotControl == controlID)
                                        {
                                            mouseDrag += e.delta.x;
                                            currentTime = maxTime * Mathf.Clamp(mouseDrag, 0f, rectBar.width) / rectBar.width;
                                            e.Use();
                                        }
                                        break;
                                }
                            }


                            GUI.Box(rect, GUIContent.none, "TimeScrubber");

                            GUIContent btnPlay = EditorGUIUtility.IconContent(IsPlaying ? "PauseButton" : "PlayButton");
                            Rect rectPlay = rect;
                            rectPlay.width = 33f;

                            IsPlaying = GUI.Toggle(rectPlay, IsPlaying, btnPlay, "TimeScrubberButton");

                            float THICKNESS = 2f;
                            float x = Mathf.Lerp(rectBar.x, rectBar.xMax, CurTime / maxTime);
                            float halfThickness = THICKNESS * 0.5f;
                            EditorGUI.DrawRect(Rect.MinMaxRect(x - halfThickness, rectBar.yMin, x + halfThickness, rectBar.yMax), Color.white);

                            //GUILayout.Label($"{CurTime}/{maxTime}", "PreLabel");
                            GUILayout.Label($"{CurTime}/{maxTime}", EditorStyles.centeredGreyMiniLabel);
                        }

                        EditorGUI.BeginChangeCheck();
                        extraTime = EditorGUILayout.IntField("재생 추가시간", extraTime);
                        if (EditorGUI.EndChangeCheck())
                            ApplyMaxTime();
                    }

                    if (editor)
                        editor.OnInspectorGUI();
                }
            }
        }

        public void SetSkillSetting(SkillSetting skillSetting)
        {
            editor = skillSetting == null ? null : Editor.CreateEditor(skillSetting) as SkillSettingEditor;

            if (editor)
                editor.onSelect = OnSelect;
        }

        public void SetMaxTime(float aniTime)
        {
            currentTime = 0f;
            this.aniTime = aniTime;

            ApplyMaxTime();
        }

        private void ApplyMaxTime()
        {
            if (extraTime > 0)
            {
                maxTime = aniTime + (extraTime * 0.01f);
            }
            else
            {
                maxTime = aniTime;
            }

            if (editor)
                editor.maxTime = maxTime;
        }

        private void OnSelect(SerializedProperty sp)
        {
            onSelect?.Invoke(sp);
        }
    }
}