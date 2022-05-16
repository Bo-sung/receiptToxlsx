#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestShortCut : TestCode
    {
        private ShortCutType shortCutType;
        private int shortCutValue;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void GoToShortCut()
        {
            shortCutType.GoShortCut(shortCutValue);
        }

        [UnityEditor.CustomEditor(typeof(TestShortCut))]
        class TestSharingEditor : Editor<TestShortCut>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();               
            }

            private void Draw1()
            {
                if (DrawMiniHeader("숏컷"))
                {
                    BeginContents();
                    {
                        DrawField("ShortCutType", ref test.shortCutType);
                        DrawField("ShortCutValue", ref test.shortCutValue);
                        DrawButton("이동", test.GoToShortCut);
                    }
                    EndContents();
                }
            }
        }
    }    
}

#endif