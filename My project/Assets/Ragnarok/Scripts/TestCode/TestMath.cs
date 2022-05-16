#if UNITY_EDITOR

using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestMath : TestCode
    {
        int id;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void Request()
        {
            int input = id;
            for (int i = 0; i < 10; i++)
            {
                float permyriadValue = MathUtils.ToPermyriadValue(input);
                int value = MathUtils.ToInt(permyriadValue); // 소수 3번째 자리에서 반올림
                int value1 = MathUtils.ToInt(permyriadValue, 2); // 소수 2번째 자리에서 반올림
                long longValue = MathUtils.ToLong(permyriadValue); // 소수 3번째 자리에서 반올림
                int ceilValue = Mathf.CeilToInt(permyriadValue); // 올림
                Debug.LogError($"id={input}");
                Debug.LogError($"{nameof(permyriadValue)}={permyriadValue}");
                Debug.LogError($"{nameof(value)}={value}");
                Debug.LogError($"{nameof(value1)}={value1}");
                Debug.LogError($"{nameof(longValue)}={longValue}");
                Debug.LogError($"{nameof(ceilValue)}={ceilValue}");
                Debug.LogError($"===============");
                input += 10000;
            }           
        }


        [UnityEditor.CustomEditor(typeof(TestMath))]
        class TestMathEditor : Editor<TestMath>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("수식"))
                {
                    BeginContents();
                    {
                        DrawField("id", ref test.id);
                        DrawButton("요청", test.Request);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif