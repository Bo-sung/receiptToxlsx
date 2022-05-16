#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestEvent : TestCode
    {
        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }       

        private void RequestDiceRoll()
        {
            Protocol.REQUEST_DICE_ROLL.SendAsync().WrapNetworkErrors();
        }

        private void RequestDiceReward()
        {
            Protocol.REQUEST_DICE_REWARD.SendAsync().WrapNetworkErrors();
        }      
        
        [UnityEditor.CustomEditor(typeof(TestEvent))]
        class TestEventEditor : Editor<TestEvent>
        { 
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("[주사위 이벤트]"))
                {
                    BeginContents();
                    {
                        DrawTitle("주사위 굴리기");
                        DrawButton("뽑기", test.RequestDiceRoll);
                    }
                    EndContents();

                    BeginContents();
                    {
                        DrawTitle("완주 보상 받기");
                        DrawButton("뽑기", test.RequestDiceReward);
                    }
                    EndContents();
                }               
            }
        }
    }
}
#endif