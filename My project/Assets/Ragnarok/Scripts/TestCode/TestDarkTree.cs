#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.Test
{    
    public sealed class TestDarkTree : TestCode
    {
        private int rewardId;
        private int arrayCount;       
        private (int id, int count)[] itemArray;
        private int uid;
        private int cid;
        private int charCid;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestDarkTreeSelectReward()
        {
            Entity.player.Inventory.RequestDarkTreeSelectReward(rewardId);
        }

        private void RequestDarkTreeRegPoint()
        {
            Entity.player.Inventory.RequestDarkTreeRegPoint(itemArray);
        }

        private void RequestDarkTreeStart()
        {
            Entity.player.Inventory.RequestDarkTreeStart();
        }

        private void RequestDarkTreeGetReward()
        {
            Entity.player.Inventory.RequestDarkTreeGetReward();
        }

        private void ShowCharacterInfo()
        {
            uid = Entity.player.User.UID;
            cid = Entity.player.Character.Cid;
        }

        private async void RequestJoinGameMap()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", charCid);
            await Protocol.JOIN_GAME_MAP.SendAsync(sfs);
        }


        [UnityEditor.CustomEditor(typeof(TestDarkTree))]
        class TestDardTreeEditor : Editor<TestDarkTree>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (DrawMiniHeader("어둠의 나무"))
                {
                    Draw1();
                    Draw2();
                    Draw3();
                    Draw4();
                    Draw5();
                    Draw6();
                }
            }

            private void Draw1()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_DARK_TREE_SELECT_REWARD(302)");
                    DrawField("rewardId", ref test.rewardId);
                    DrawButton("요청", test.RequestDarkTreeSelectReward);
                }
                EndContents();
            }

            private void Draw2()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_DARK_TREE_REG_POINT(303)");
                    DrawField("arrayCount", ref test.arrayCount);
                    if (test.arrayCount > 0)
                    {
                        if (test.itemArray == null || test.itemArray.Length != test.arrayCount)
                            test.itemArray = new (int, int)[test.arrayCount];

                        for (int i = 0; i < test.arrayCount; i++)
                        {
                            DrawField($"itemId[{i}]", ref test.itemArray[i].id);
                            DrawField($"count[{i}]", ref test.itemArray[i].count);
                        }
                    }                    
                    DrawButton("요청", test.RequestDarkTreeRegPoint);
                }
                EndContents();
            }

            private void Draw3()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_DARK_TREE_START(304)");
                    DrawButton("요청", test.RequestDarkTreeStart);
                }
                EndContents();
            }

            private void Draw4()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_DARK_TREE_GET_REWARD(305)");
                    DrawButton("요청", test.RequestDarkTreeGetReward);
                }
                EndContents();
            }

            private void Draw5()
            {
                BeginContents();
                {
                    DrawTitle("나의 캐릭터 정보");
                    DrawField("uid", ref test.uid);
                    DrawField("cid", ref test.cid);
                    DrawButton("Show", test.ShowCharacterInfo);
                }
                EndContents();
            }

            private void Draw6()
            {
                BeginContents();
                {
                    DrawTitle("JOIN_GAME_MAP(10)");
                    DrawField("cid", ref test.charCid);
                    DrawButton("요청", test.RequestJoinGameMap);
                }
                EndContents();
            }
        }
    }
}
#endif