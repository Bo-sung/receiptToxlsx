#if UNITY_EDITOR
namespace Ragnarok.Test
{
    public sealed class TestEntry : TestCode
    {
        BattleManager battleManager;
        StageMode stageMode;
        int stageId;

        protected override void Awake()
        {
            base.Awake();

            battleManager = BattleManager.Instance;
        }

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void StartMultiMazeLobby(int id)
        {
            battleManager.StartBattle(BattleMode.MultiMazeLobby, id);
        }

        private void StartMultiMaze(int id)
        {
            battleManager.StartBattle(BattleMode.MultiMaze, id);
        }

        private void SummonMVP()
        {
            Entity.player.Dungeon.SummonMvpMonster().WrapNetworkErrors();
        }

        private void StartScenarioMaze(int id)
        {
            battleManager.StartBattle(BattleMode.ScenarioMaze, id);
        }

        private void StartLobby()
        {
            battleManager.StartBattle(BattleMode.Lobby);
        }

        private void StartChristmasFreeFight()
        {
            battleManager.StartBattle(BattleMode.ChristmasFreeFight, 13);
        }

        private void StartStage()
        {
            Entity.player.Dungeon.StartBattleStageMode(stageMode, stageId);
        }

        [UnityEditor.CustomEditor(typeof(TestEntry))]
        class TestEntryEditor : Editor<TestEntry>
        {
            int multiMazeLobbyId = 1;
            int multiMazeId = 1;
            int scenarioMazeId = 1;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
                Draw2();
                Draw3();
                Draw4();
                Draw5();
                //Draw6();
                Draw7();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("멀티미로-로비"))
                {
                    BeginContents();
                    {
                        DrawTitle("멀티미로-로비 입장");
                        DrawField("[1] id", ref multiMazeLobbyId);
                        DrawButton("입장", () => test.StartMultiMazeLobby(multiMazeLobbyId));
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("멀티미로"))
                {
                    BeginContents();
                    {
                        DrawTitle("멀티미로 입장");
                        DrawField("[1] id", ref multiMazeId);
                        DrawButton("입장", () => test.StartMultiMaze(multiMazeId));
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("스테이지"))
                {
                    BeginContents();
                    {
                        DrawTitle("MVP 몬스터 소환");
                        DrawButton("소환", test.SummonMVP);
                    }
                    EndContents();
                }
            }

            private void Draw4()
            {
                if (DrawMiniHeader("시나리오 미로"))
                {
                    BeginContents();
                    {
                        DrawTitle("시나리오 미로 입장");
                        DrawField("[1] id", ref scenarioMazeId);
                        DrawButton("입장", () => test.StartScenarioMaze(scenarioMazeId));
                    }
                    EndContents();
                }
            }

            private void Draw5()
            {
                if (DrawMiniHeader("마을"))
                {
                    BeginContents();
                    {
                        DrawTitle("마을 임장");
                        DrawButton("입장", test.StartLobby);
                    }
                    EndContents();
                }
            }

            private void Draw6()
            {
                if (DrawMiniHeader("크리스마스 난전"))
                {
                    BeginContents();
                    {
                        DrawTitle("난전 입장");
                        DrawButton("입장", test.StartChristmasFreeFight);
                    }
                    EndContents();
                }
            }

            private void Draw7()
            {
                if (DrawMiniHeader("스테이지"))
                {
                    BeginContents();
                    {
                        DrawTitle("스테이지 입장");
                        DrawField("[1] id", ref test.stageId);
                        DrawField("[2] stageMode", ref test.stageMode);
                        DrawButton("입장", test.StartStage);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif