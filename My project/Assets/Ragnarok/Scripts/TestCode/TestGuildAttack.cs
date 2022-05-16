#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestGuildAttack : TestCode
    {
        private string dateTimeString;
        DateTime dateTime;
        private int preLevel;
        private int curLevel;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void ShowUIGuildAttack()
        {
            UI.Show<UIGuildAttack>();
        }

        private void RequestGuildAttackChangeTime()
        {
            Entity.player.Guild.RequestChangeGuildAttackTime().WrapNetworkErrors();
        }

        private void RequestGuildAttckDonation()
        {
            Entity.player.Guild.RequestDonationGuildAttack().WrapNetworkErrors();
        }

        private void StringToDateTime()
        {
            if (TimeSpan.TryParse(dateTimeString, out TimeSpan span))
            {
                dateTime = ((long)span.TotalMilliseconds).ToDateTime();
            }
        }

        private void ShowGuildAttackClear()
        {
            UI.Show<UIGuildAttackClear>().Show(preLevel, curLevel, null);
        }

        private void ShowGuildAttackFail()
        {
            UI.Show<UIGuildAttackFail>().Show(preLevel, curLevel, null);
        }

        private void RequestCreateEmperium()
        {
            Entity.player.Guild.RequestCreateEmperium().WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestGuildAttack))]
        class TestGuildAttackEditor : Editor<TestGuildAttack>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
                Draw2();
                Draw3();
                Draw4();
                Draw5();
                Draw6();
                Draw7();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("길드 습격 UI"))
                {
                    BeginContents();
                    {
                        DrawButton("열기", test.ShowUIGuildAttack);
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("길드 습격 시간 변경"))
                {
                    BeginContents();
                    {
                        DrawButton("변경", test.RequestGuildAttackChangeTime);
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("길드 습격 기부"))
                {
                    BeginContents();
                    {
                        DrawButton("기부", test.RequestGuildAttckDonation);
                    }
                    EndContents();
                }
            }

            private void Draw4()
            {
                if (DrawMiniHeader("String To DateTime"))
                {
                    BeginContents();
                    {
                        DrawField("Input 시간(UTC+0)", ref test.dateTimeString);
                        DrawText("Result 시간", test.dateTime.ToString());
                        DrawButton("변경", test.StringToDateTime);
                    }
                    EndContents();
                }
            }

            private void Draw5()
            {
                if (DrawMiniHeader("길드습격 클리어"))
                {
                    BeginContents();
                    {
                        DrawField("PreLevel", ref test.preLevel);
                        DrawField("CurLevel", ref test.curLevel);
                        DrawButton("보기", test.ShowGuildAttackClear);
                    }
                    EndContents();
                }
            }

            private void Draw6()
            {
                if (DrawMiniHeader("길드습격 실패"))
                {
                    BeginContents();
                    {
                        DrawField("PreLevel", ref test.preLevel);
                        DrawField("CurLevel", ref test.curLevel);
                        DrawButton("보기", test.ShowGuildAttackFail);
                    }
                    EndContents();
                }
            }

            private void Draw7()
            {
                if (DrawMiniHeader("길드습격 엠펠리움 생성"))
                {
                    BeginContents();
                    {
                        DrawButton("생성", test.RequestCreateEmperium);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif