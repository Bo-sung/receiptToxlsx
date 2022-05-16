#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestRoulette : TestCode
    {
        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }       

        private void RequestNormalRoulette()
        {
            const byte NORMAL_ROULETTE = 1; // 일반 룰렛

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", NORMAL_ROULETTE);
            Protocol.REQUEST_ROULETTE_REWARD.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestRareRoulette()
        {
            const byte RARE_ROULETTE = 2; // 레어 룰렛

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", RARE_ROULETTE);
            Protocol.REQUEST_ROULETTE_REWARD.SendAsync(sfs).WrapNetworkErrors();
        }      
        
        private void RequestSpecialRoulette()
        {
            Protocol.REQUEST_SPECIAL_ROULETTE_REWARD.SendAsync().WrapNetworkErrors();
        }

        private void RequestChangeSpecialRoulette()
        {
            Protocol.REQUEST_SPECIAL_ROULETTE_INIT_BOARD.SendAsync().WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestRoulette))]
        class TestSharingEditor : Editor<TestRoulette>
        { 
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
                Draw2();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("룰렛"))
                {
                    BeginContents();
                    {
                        DrawTitle("일반 룰렛");
                        DrawButton("뽑기", test.RequestNormalRoulette);
                    }
                    EndContents();
                    BeginContents();
                    {
                        DrawTitle("레어 룰렛");
                        DrawButton("뽑기", test.RequestRareRoulette);
                    }
                    EndContents();
                }               
            }

            private void Draw2()
            {
                if (DrawMiniHeader("할로윈 룰렛"))
                {
                    BeginContents();
                    {
                        DrawTitle("룰렛 뽑기(281)REQUEST_SPECIAL_ROULETTE_REWARD");
                        DrawButton("뽑기", test.RequestSpecialRoulette);
                    }
                    EndContents();
                    BeginContents();
                    {
                        DrawTitle("룰렛 변경(282)REQUEST_SPECIAL_ROULETTE_INIT_BOARD");
                        DrawButton("변경", test.RequestChangeSpecialRoulette);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif