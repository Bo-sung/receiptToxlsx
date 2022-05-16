#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestOVerStatus : TestCode
    {
        private BasicStatusType statusType;
        private int str, agi, vit, @int, dex, luk;
        private BasicStatusType uiStatusType;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestOverStatus()
        {
            Entity.player.Status.RequestOverStatus(statusType).WrapNetworkErrors();
        }

        private void RequestStatPointUpdate()
        {
            Entity.player.Status.RequestCharStatPointUpdate((short)str, (short)agi, (short)vit, (short)@int, (short)dex, (short)luk, false, false).WrapErrors();
        }

        private void ShowUIOverStatus()
        {
            UI.Show<UIOverStatus>().Set(uiStatusType);
        }

        [UnityEditor.CustomEditor(typeof(TestOVerStatus))]
        class TestGuildAttackEditor : Editor<TestOVerStatus>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
                Draw2();
                Draw3();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("REQUEST_OVER_STAT(289)"))
                {
                    BeginContents();
                    {
                        DrawField("스탯", ref test.statusType);
                        DrawButton("오버스탯", test.RequestOverStatus);
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("스탯 적용"))
                {
                    BeginContents();
                    {
                        DrawField("STR", ref test.str);
                        DrawField("AGI", ref test.agi);
                        DrawField("VIT", ref test.vit);
                        DrawField("INT", ref test.@int);
                        DrawField("DEX", ref test.dex);
                        DrawField("LUK", ref test.luk);
                        DrawButton("적용", test.RequestStatPointUpdate);
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("오버스탯 UI"))
                {
                    BeginContents();
                    {
                        DrawField("스탯", ref test.uiStatusType);
                        DrawButton("오버스탯", test.ShowUIOverStatus);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif