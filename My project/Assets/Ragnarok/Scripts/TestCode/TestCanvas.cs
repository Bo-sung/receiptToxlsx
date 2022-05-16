#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestCanvas : TestCode
    {
        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private int day;
        private int step;

        private int uid;
        private int cid;

        private void TestMvpResult()
        {
            RewardData[] rewards =
            {
                new RewardData(RewardType.Zeny, 10, 100, 0),
                new RewardData(RewardType.CatCoin, 100, 100, 0),
                new RewardData(RewardType.Item, 80001, 1, 0),
                new RewardData(RewardType.Item, 80002, 1, 0),
            };

            RewardData[] wasted =
            {
                new RewardData(RewardType.Item, 80003, 1, 0),
                new RewardData(RewardType.Item, 80004, 1, 0),
            };

            UI.Show<UIMvpResult>().SetData(rewards, wasted);
        }

        private void ClearRewardLauncher()
        {
            UIRewardLauncher ui = UI.GetUI<UIRewardLauncher>();
            if (ui == null)
                return;

            ui.Clear();
        }

        private void ShowAttendEvent()
        {
            Entity.player.Event.TempAttendEvent(day, step);
            UI.Show<UIAttendEvent>();
        }

        private void ShowChacterInfo()
        {
            uid = Entity.player.User.UID;
            cid = Entity.player.Character.Cid;
            Entity.player.User.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }

        private void ShowOtherChacterInfo()
        {
            Entity.player.User.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }

        private void ShowTanscendenceDisassemble()
        {
            UI.Show<UITranscendenceDisassemble>();
        }

        private void ShowWordCollectionEvent()
        {
            UI.Show<UIWordCollectionEvent>();
        }

        [UnityEditor.CustomEditor(typeof(TestCanvas))]
        class TestCanvasEditor : Editor<TestCanvas>
        {
            [SerializeField] ContentType contentType;

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
                Draw8();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("캐릭터 셰어링"))
                {
                    BeginContents();
                    {
                        DrawCanvas<UICharacterShare>();
                        DrawCanvas<UICharacterShareWaiting>();
                        DrawCanvas<UICharacterShareReward>();
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("컨텐츠 오픈"))
                {
                    BeginContents();
                    {
                        DrawField(nameof(ContentType), ref contentType);

                        BeginHorizontal(hasSpace: false);
                        {
                            DrawTitle(nameof(UIContentsUnlock));

                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("Show", GUILayout.ExpandWidth(expand: false)))
                                UI.Show<UIContentsUnlock>().Set(contentType);

                            if (GUILayout.Button("Close", GUILayout.ExpandWidth(expand: false)))
                                UI.Close<UIContentsUnlock>();
                        }
                        EndHorizontal();
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("Mvp 보상 테스트"))
                {
                    BeginContents();
                    {
                        DrawButton("Show", test.TestMvpResult);
                        DrawButton("Clear", test.ClearRewardLauncher);
                    }
                    EndContents();
                }
            }

            private void Draw4()
            {
                if (DrawMiniHeader("이벤트 출석 테스트"))
                {
                    BeginContents();
                    {
                        DrawField("day", ref test.day);
                        DrawField("step", ref test.step);
                        DrawButton("Show", test.ShowAttendEvent);
                    }
                    EndContents();
                }
            }

            private void Draw5()
            {
                if (DrawMiniHeader("내 정보 보기"))
                {
                    BeginContents();
                    {
                        DrawButton("Show", test.ShowChacterInfo);
                    }
                    EndContents();
                }
            }

            private void Draw6()
            {
                if (DrawMiniHeader("다른 유저 정보 보기"))
                {
                    BeginContents();
                    {
                        DrawField("uid", ref test.uid);
                        DrawField("cid", ref test.cid);
                        DrawButton("Show", test.ShowOtherChacterInfo);
                    }
                    EndContents();
                }
            }

            private void Draw7()
            {
                if (DrawMiniHeader("초월 분해"))
                {
                    BeginContents();
                    {
                        DrawButton("Show", test.ShowTanscendenceDisassemble);
                    }
                    EndContents();
                }
            }

            private void Draw8()
            {
                if (DrawMiniHeader("단어 수집"))
                {
                    BeginContents();
                    {
                        DrawButton("Show", test.ShowWordCollectionEvent);
                    }
                    EndContents();
                }
            }

            private void DrawCanvas<T>()
                where T : UICanvas
            {
                BeginHorizontal(hasSpace: false);
                {
                    DrawTitle(typeof(T).Name);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Show", GUILayout.ExpandWidth(expand: false)))
                        UI.Show<T>();

                    if (GUILayout.Button("Close", GUILayout.ExpandWidth(expand: false)))
                        UI.Close<T>();
                }
                EndHorizontal();
            }
        }
    }
}
#endif