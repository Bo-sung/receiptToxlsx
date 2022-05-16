#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public abstract class TestCode : MonoBehaviour
    {
        protected virtual void Awake()
        {
            if (Application.isPlaying)
                DontDestroyOnLoad(this);
        }

        protected virtual void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                MainTest();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                Test1();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                Test2();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                Test3();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                Test4();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha5))
            {
                Test5();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha6))
            {
                Test6();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha7))
            {
                Test7();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha8))
            {
                Test8();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Alpha9))
            {
                Test9();
                return;
            }
        }

        [ContextMenu(nameof(MainTest))]
        private void MainTest()
        {
            Debug.LogError(nameof(MainTest), this);
            OnMainTest();
        }

        [ContextMenu(nameof(Test1))]
        private void Test1()
        {
            Debug.LogError(nameof(Test1), this);
            OnTest1();
        }

        [ContextMenu(nameof(Test2))]
        private void Test2()
        {
            Debug.LogError(nameof(Test2), this);
            OnTest2();
        }

        [ContextMenu(nameof(Test3))]
        private void Test3()
        {
            Debug.LogError(nameof(Test3), this);
            OnTest3();
        }

        [ContextMenu(nameof(Test4))]
        private void Test4()
        {
            Debug.LogError(nameof(Test4), this);
            OnTest4();
        }

        [ContextMenu(nameof(Test5))]
        private void Test5()
        {
            Debug.LogError(nameof(Test5), this);
            OnTest5();
        }

        [ContextMenu(nameof(Test6))]
        private void Test6()
        {
            Debug.LogError(nameof(Test6), this);
            OnTest6();
        }

        [ContextMenu(nameof(Test7))]
        private void Test7()
        {
            Debug.LogError(nameof(Test7), this);
            OnTest7();
        }

        [ContextMenu(nameof(Test8))]
        private void Test8()
        {
            Debug.LogError(nameof(Test8), this);
            OnTest8();
        }

        [ContextMenu(nameof(Test9))]
        private void Test9()
        {
            Debug.LogError(nameof(Test9), this);
            OnTest9();
        }

        protected virtual void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        protected virtual void OnTest1()
        {
        }

        protected virtual void OnTest2()
        {
        }

        protected virtual void OnTest3()
        {
        }

        protected virtual void OnTest4()
        {
        }

        protected virtual void OnTest5()
        {
        }

        protected virtual void OnTest6()
        {
        }

        protected virtual void OnTest7()
        {
        }

        protected virtual void OnTest8()
        {
        }

        protected virtual void OnTest9()
        {
        }

        protected abstract class Editor<T> : UnityEditor.Editor
            where T : TestCode
        {
            const bool MINIMALISTIC_LOOK = false;
            const bool DEFAULT_HEADER_STATE = false;

            protected T test;

            bool mEndHorizontal = false;

            protected virtual void OnEnable()
            {
                test = target as T;
            }

            protected void DrawBaseGUI()
            {
                UnityEditor.SerializedProperty script = serializedObject.FindProperty("m_Script");
                UnityEditor.EditorGUILayout.PropertyField(script);

                GUILayout.Space(4f);
            }

            protected void ShowDialog(string message)
            {
                UnityEditor.EditorUtility.DisplayDialog("알림", message, "확인");
            }

            protected bool DrawHeader(string header)
            {
                return DrawHeader(header, header, false, MINIMALISTIC_LOOK);
            }

            protected bool DrawMiniHeader(string header)
            {
                return DrawHeader(header, header, false, true);
            }

            protected void BeginContents()
            {
                BeginContents(MINIMALISTIC_LOOK);
            }

            protected void EndContents()
            {
                GUILayout.Space(3f);
                GUILayout.EndVertical();
                UnityEditor.EditorGUILayout.EndHorizontal();

                if (mEndHorizontal)
                {
                    GUILayout.Space(3f);
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(3f);
            }

            protected void BeginLock()
            {
                GUI.enabled = false;
            }

            protected void EndLock()
            {
                GUI.enabled = true;
            }

            protected void DrawTitle(string title)
            {
                GUILayout.Label(title);
            }

            protected void DrawTitle(string title, string buttonText, System.Action action)
            {
                BeginHorizontal(hasSpace: false);
                {
                    GUILayout.Label(title);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(expand: false)))
                        action();
                }
                EndHorizontal();
            }

            protected void DrawText(string title, bool value)
            {
                DrawText(title, value.ToString());
            }

            protected void DrawText(string title, byte value)
            {
                DrawText(title, value.ToString());
            }

            protected void DrawText(string title, int value)
            {
                DrawText(title, value.ToString());
            }

            protected void DrawText(string title, long value)
            {
                DrawText(title, value.ToString());
            }

            protected void DrawText(string title, float value)
            {
                DrawText(title, value.ToString());
            }

            protected void DrawText(string title, double value)
            {
                DrawText(title, value.ToString());
            }

            protected void DrawText(string title, string value)
            {
                BeginHorizontal(hasSpace: false);
                {
                    GUILayout.Label(title);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(value);
                }
                EndHorizontal();
            }

            protected void DrawField(string text, ref bool value)
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = UnityEditor.EditorGUILayout.Toggle(text, value);
                }
                EndHorizontal();
            }

            protected void DrawField(string text, ref byte value)
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = (byte)UnityEditor.EditorGUILayout.IntField(text, value);
                }
                EndHorizontal();
            }

            protected void DrawField(string text, ref int value)
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = UnityEditor.EditorGUILayout.IntField(text, value);
                }
                EndHorizontal();
            }

            protected void DrawField(string text, ref long value)
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = UnityEditor.EditorGUILayout.LongField(text, value);
                }
                EndHorizontal();
            }

            protected void DrawField(string text, ref float value)
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = UnityEditor.EditorGUILayout.FloatField(text, value);
                }
                EndHorizontal();
            }

            protected void DrawField(string text, ref double value)
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = UnityEditor.EditorGUILayout.DoubleField(text, value);
                }
                EndHorizontal();
            }

            protected void DrawField<TEnum>(string text, ref TEnum value)
                where TEnum : System.Enum
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = (TEnum)UnityEditor.EditorGUILayout.EnumPopup(text, value);
                }
                EndHorizontal();
            }

            protected void DrawField(string text, ref string value)
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = UnityEditor.EditorGUILayout.TextField(text, value);
                }
                EndHorizontal();
            }

            protected void DrawField(string text, UnityEditor.SerializedProperty value)
            {
                BeginHorizontal(hasSpace: true);
                {
                    UnityEditor.EditorGUILayout.PropertyField(value, new GUIContent(text));
                }
                EndHorizontal();
            }

            protected void DrawButton(string text, System.Action action)
            {
                BeginHorizontal(hasSpace: true);
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(text, GUILayout.ExpandWidth(expand: false)))
                        action();
                }
                EndHorizontal();
            }

            protected void DrawMaskEnum<TEnum>(string text, ref TEnum value)
                where TEnum : System.Enum
            {
                BeginHorizontal(hasSpace: true);
                {
                    value = (TEnum)UnityEditor.EditorGUILayout.EnumFlagsField(text, value);
                }
                EndHorizontal();
            }

            private bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
            {
                bool state = UnityEditor.EditorPrefs.GetBool(key, DEFAULT_HEADER_STATE); // state 기본 값을 false 로 둠

                if (!minimalistic) GUILayout.Space(3f);
                if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal();
                GUI.changed = false;

                if (minimalistic)
                {
                    if (state) text = "\u25BC" + (char)0x200a + text;
                    else text = "\u25BA" + (char)0x200a + text;

                    GUILayout.BeginHorizontal();
                    GUI.contentColor = UnityEditor.EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                    if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    text = "<b><size=11>" + text + "</size></b>";
                    if (state) text = "\u25BC " + text;
                    else text = "\u25BA " + text;
                    if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
                }

                if (GUI.changed) UnityEditor.EditorPrefs.SetBool(key, state);

                if (!minimalistic) GUILayout.Space(2f);
                GUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;
                if (!forceOn && !state) GUILayout.Space(3f);
                return state;
            }

            private void BeginContents(bool minimalistic)
            {
                if (!minimalistic)
                {
                    mEndHorizontal = true;
                    GUILayout.BeginHorizontal();
                    UnityEditor.EditorGUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(10f));
                }
                else
                {
                    mEndHorizontal = false;
                    UnityEditor.EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
                    GUILayout.Space(10f);
                }
                GUILayout.BeginVertical();
                GUILayout.Space(2f);
            }

            protected void BeginHorizontal(bool hasSpace)
            {
                UnityEditor.EditorGUILayout.BeginHorizontal();

                if (hasSpace)
                    GUILayout.Space(20f);
            }

            protected void EndHorizontal()
            {
                UnityEditor.EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#endif