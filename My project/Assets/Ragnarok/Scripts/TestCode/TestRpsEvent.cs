#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestRpsEvent : TestCode
    {
        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestRPSGame()
        {
            Protocol.REQUEST_RPS_GAME.SendAsync().WrapNetworkErrors();
        }

        private void RequestRPSInit()
        {
            Protocol.REQUEST_RPS_INIT.SendAsync().WrapNetworkErrors();
        }

        private void RequestUserEventQuestReward()
        {
            Protocol.REQUEST_USER_EVNET_QUEST_REWARD.SendAsync().WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestRpsEvent))]
        class TestRPSGameEditor : Editor<TestRpsEvent>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("Rps Event"))
                {
                    BeginContents();
                    {
                        DrawTitle("Game");
                        DrawButton("Send", test.RequestRPSGame);
                    }
                    EndContents();
                    BeginContents();
                    {
                        DrawTitle("Init");
                        DrawButton("Send", test.RequestRPSInit);
                    }
                    EndContents();
                }

                if (DrawMiniHeader("User Event"))
                {
                    BeginContents();
                    {
                        DrawTitle("Quest Reward");
                        DrawButton("Send", test.RequestUserEventQuestReward);
                    }
                    EndContents();
                }
            }

        }
    }
}
#endif