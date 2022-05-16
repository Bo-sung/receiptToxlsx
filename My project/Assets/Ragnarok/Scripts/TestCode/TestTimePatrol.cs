#if UNITY_EDITOR

namespace Ragnarok.Test
{
    public sealed class TestTimePatrol : TestCode
    {
        int questId;
        int timePatrolId;

        int costumeType;
        int level;

        ShareForceType shareForceType;

        int jobId;
        int remainPoint;
        int maxStatValue;

        int group;
        int shareLevel;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestQuestRewardAsync()
        {
            QuestInfo quest = new QuestInfo();
            QuestData data = QuestDataManager.Instance.Get(questId);
            if (data == null)
                return;

            quest.SetData(data);
            Entity.player.Quest.RequestQuestRewardAsync(quest).WrapNetworkErrors();
        }

        private void StartBattle()
        {
            BattleManager.Instance.StartBattle(BattleMode.TimePatrol, timePatrolId, false);
        }

        private void RequestTPSummonBoss()
        {
            Entity.player.Dungeon.RequestSummonTimePatrolBoss().WrapNetworkErrors();
        }

        private void RequestTPCostumeLevelUp()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", costumeType);
            sfs.PutInt("2", level);
            Protocol.REQUEST_TP_COSTUME_LEVELUP.SendAsync(sfs).WrapNetworkErrors();
        }

        private void ShowShareForce()
        {
            UI.Show<UIShareForceLevelUp>().Set(shareForceType);
        }

        public void GetState()
        {
            var data = JobDataManager.Instance.Get(jobId);
            if (data != null)
            {
                JobData.StatValue basicStat = new JobData.StatValue(1);
                JobData.StatValue maxStat = new JobData.StatValue(maxStatValue);
                var points = data.GetAutoStatGuidePoints(remainPoint, basicStat, maxStat);

                UnityEngine.Debug.Log($"str={points[0]}");
                UnityEngine.Debug.Log($"agi={points[1]}");
                UnityEngine.Debug.Log($"vit={points[2]}");
                UnityEngine.Debug.Log($"int={points[3]}");
                UnityEngine.Debug.Log($"dex={points[4]}");
                UnityEngine.Debug.Log($"lux={points[5]}");
            }
        }

        private void RequestShareStatBuildUp()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", group);
            sfs.PutInt("2", shareLevel);

            Protocol.REQUEST_SHARE_STAT_BUILD_UP.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestShareStatReset()
        {
            Protocol.REQUEST_SHARE_STAT_RESET.SendAsync().WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestTimePatrol))]
        class TestTimePatrolEditor : Editor<TestTimePatrol>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (DrawMiniHeader("타임패트롤"))
                {
                    Draw1();
                    Draw2();
                    Draw3();
                    Draw4();
                    Draw5();
                    Draw6();
                    Draw7();
                    Draw8();
                }
            }

            private void Draw1()
            {
                BeginContents();
                {
                    DrawTitle("QUEST_REWARD(47)");
                    DrawField("questId", ref test.questId);
                    DrawButton("요청", test.RequestQuestRewardAsync);
                }
                EndContents();
            }

            private void Draw2()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_TP_STAGE_ENTER(306)");
                    DrawField("id", ref test.timePatrolId);
                    DrawButton("입장", test.StartBattle);
                }
                EndContents();
            }

            private void Draw3()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_TP_SUMMON_BOSS(307)");
                    DrawButton("요청", test.RequestTPSummonBoss);
                }
                EndContents();
            }

            private void Draw4()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_TP_COSTUME_LEVELUP(310)");
                    DrawField("type", ref test.costumeType);
                    DrawField("level", ref test.level);
                    DrawButton("요청", test.RequestTPCostumeLevelUp);
                }
                EndContents();
            }

            private void Draw5()
            {
                BeginContents();
                {
                    DrawTitle("쉐어포스 레벨업 UI");
                    DrawField("shareForceType", ref test.shareForceType);
                    DrawButton("열기", test.ShowShareForce);
                }
                EndContents();
            }

            private void Draw6()
            {
                BeginContents();
                {
                    DrawField("jobId", ref test.jobId);
                    DrawField("remainPoint", ref test.remainPoint);
                    DrawField("maxStatValue", ref test.maxStatValue);
                    DrawButton("요청", test.GetState);
                }
                EndContents();
            }

            private void Draw7()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_SHARE_STAT_BUILD_UP(338)");
                    DrawField("group", ref test.group);
                    DrawField("level", ref test.shareLevel);
                    DrawButton("요청", test.RequestShareStatBuildUp);
                }
                EndContents();
            }

            private void Draw8()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_SHARE_STAT_RESET(339)");
                    DrawButton("요청", test.RequestShareStatReset);
                }
                EndContents();
            }
        }
    }
}
#endif