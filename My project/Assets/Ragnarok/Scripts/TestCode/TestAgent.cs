#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestAgent : TestCode
    {
        private int stageId;        

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }       

        private void RequestAgentResetTradeCount()
        {
            Entity.player.Agent?.RequestAgentResetTradeCount(stageId).WrapNetworkErrors();
        }        

        [UnityEditor.CustomEditor(typeof(TestAgent))]
        class TestAgentEditor : Editor<TestAgent>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();               
            }

            private void Draw1()
            {
                if (DrawMiniHeader("REQUEST_AGENT_INIT_TRADE_COUNT (208)"))
                {
                    BeginContents();
                    {
                        DrawField("stageId", ref test.stageId);
                        DrawButton("초기화", test.RequestAgentResetTradeCount);
                    }
                    EndContents();
                }               
            }            
        }
    }
}
#endif