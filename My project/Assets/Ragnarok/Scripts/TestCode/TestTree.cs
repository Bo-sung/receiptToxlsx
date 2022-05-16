#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestTree : TestCode
    {
        private int baseZeny = 400;
        private int tickSecond = 800;
        private float zenyCoefficient = 1.015f;
        private float maxTreeTime = 21600000;
        private int jobLevel = 1;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }       

        private void GetZeny()
        {
            int tickCount = (int)(maxTreeTime / (tickSecond * 1000f)); // tickSecond = 800           
            float retCoefficient1 = Mathf.Pow(zenyCoefficient, jobLevel); // zenyCoefficient = 1.015 (10150)
            float retCoefficient2 = (float)(MathUtils.RoundToInt(retCoefficient1 * 100) / 100f);
            int getZeny = MathUtils.ToInt(baseZeny * retCoefficient2) * tickCount; // baseZeny = 400
            Debug.LogError($"tickCount={tickCount}");
            Debug.LogError($"retCoefficient1={retCoefficient1}");
            Debug.LogError($"retCoefficient2={retCoefficient2}");
            Debug.LogError($"getZeny={getZeny}");                   
        }        

        [UnityEditor.CustomEditor(typeof(TestTree))]
        class TestSharingEditor : Editor<TestTree>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();              
            }

            private void Draw1()
            {
                if (DrawMiniHeader("나무"))
                {
                    BeginContents();
                    {
                        DrawField("baseZeny", ref test.baseZeny);
                        DrawField("tickSecond", ref test.tickSecond);
                        DrawField("zenyCoefficient", ref test.zenyCoefficient);
                        DrawField("maxTreeTime", ref test.maxTreeTime);
                        DrawField("jobLevel", ref test.jobLevel);
                        DrawButton("제니", test.GetZeny);
                    }
                    EndContents();
                }               
            }            
        }
    }
}
#endif