#if UNITY_EDITOR

namespace Ragnarok.Test
{
    public class TestProtocol : TestCode
    {
        DungeonType dungeonType;
        int profileId;
        int leftCount, rightCount;
        int[] leftCupetIds, rightCupetIds;
        int guildId;
        int defGuildId, supportMemberCid;
        int attackCupetCount;
        int[] attackCupetIds;
        int itemCount;
        long[] itemNoArray;
        int type;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestDungeonDailyFreeReward()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", (int)dungeonType);
            Protocol.REQUEST_DUNGEON_DAILY_FREE_REWARD.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestExchange()
        {
            Entity.player.Character.RequestChangeProfile(profileId).WrapNetworkErrors();
        }

        private void RequestGuildBattleSeasonInfo()
        {
            Protocol.REQUEST_GUILD_BATTLE_SEASON_INFO.SendAsync().WrapNetworkErrors();
        }

        private void RequestGuildBattlEntry()
        {
            var sfs = Protocol.NewInstance();
            if (leftCupetIds != null && leftCupetIds.Length > 0)
                sfs.PutIntArray("1", leftCupetIds);

            if (rightCupetIds != null && rightCupetIds.Length > 0)
                sfs.PutIntArray("2", rightCupetIds);

            Protocol.REQUEST_GUILD_BATTLE_ENTRY.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestGuildBattleTargetDetailInfo()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", guildId);
            Protocol.REQUEST_GUILD_BATTLE_TARGET_DETAIL_INFO.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestGuildBattleStartDheck()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", defGuildId);
            sfs.PutInt("2", supportMemberCid);
            Protocol.REQUEST_GUILD_BATTLE_START_CHECK.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestGuildBattlStartLoaindComplete()
        {
            Protocol.REQUEST_GUILD_BATTLE_START_LOADING_COMP.SendAsync().WrapNetworkErrors();
        }

        private void RequestGuildBattleSupportCupetSetting()
        {
            var sfs = Protocol.NewInstance();
            if (attackCupetIds != null && attackCupetIds.Length > 0)
                sfs.PutIntArray("1", attackCupetIds);

            Protocol.REQUEST_GUILD_BATTLE_SUPPORT_CUPET_SETTING.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestGuildBattlDefInfo()
        {
            Protocol.REQUEST_GUILD_BATTLE_DEF_INFO.SendAsync().WrapNetworkErrors();
        }

        private void RequestGuildBattlCupetLevelUp()
        {
            Protocol.REQUEST_GUILD_CUPET_EXP_UP.SendAsync().WrapNetworkErrors();
        }

        private void RequestGuildBattlBuffInfo()
        {
            Protocol.REQUEST_GUILD_BATTLE_BUFF_INFO.SendAsync().WrapNetworkErrors();
        }

        private void RequestGuildBattlBuffLevelUp()
        {
            Protocol.REQUEST_GUILD_BATTLE_BUFF_EXP_UP.SendAsync().WrapNetworkErrors();
        }

        private void RequestGuildBattlAttackDamage()
        {
            Protocol.REQUEST_GUILD_BATTLE_ATTACK_DAMAGE.SendAsync().WrapNetworkErrors();
        }

        private void RequestGuildBattlRank()
        {
            Protocol.REQUEST_GUILD_BATTLE_RANK.SendAsync().WrapNetworkErrors();
        }

        private void RequestGuildBattlEnd()
        {
            Protocol.REQUEST_GUILD_BATTLE_END.SendAsync().WrapNetworkErrors();
        }

        private void RequestItemDisassemble()
        {
            Entity.player.Inventory.RequestItemDisassemble(itemNoArray, type, ItemGroupType.Equipment).WrapNetworkErrors();
        }

        private void RequestFindAlphabetReward()
        {
            Entity.player.Event.RequestWordCollectionReward().WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestProtocol))]
        class TestProtocolEditor : Editor<TestProtocol>
        {
            public override void OnInspectorGUI()
            {
                DrawBaseGUI();

                Draw1();
                Draw2();
                Draw3();
                Draw4();
                Draw5();
                Draw6();
                Draw7();
                Draw8();
                Draw9();
                Draw10();
                Draw11();
                Draw12();
                Draw13();
                Draw14();
                Draw15();
                Draw16();
                Draw17();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("REQUEST_DUNGEON_DAILY_FREE_REWARD (279)"))
                {
                    BeginContents();
                    {
                        DrawTitle("특정 던전 무료 보상");
                        DrawField("[1] dungeon_type", ref test.dungeonType);
                        DrawButton("Request", test.RequestDungeonDailyFreeReward);
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("REQUEST_PROFILE_CHANGE (313)"))
                {
                    BeginContents();
                    {
                        DrawField("[1] profile_id", ref test.profileId);
                        DrawButton("Request", test.RequestExchange);
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_SEASON_INFO (319)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattleSeasonInfo);
                    }
                    EndContents();
                }
            }

            private void Draw4()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_ENTRY (320)"))
                {
                    BeginContents();
                    {
                        DrawField("LeftCount", ref test.leftCount);
                        if (test.leftCount > 0)
                        {
                            if (test.leftCupetIds == null || test.leftCupetIds.Length != test.leftCount)
                                test.leftCupetIds = new int[test.leftCount];

                            for (int i = 0; i < test.leftCount; i++)
                                DrawField($"[1] LeftCupetIds[{i}]", ref test.leftCupetIds[i]);
                        }

                        DrawField("RightCount", ref test.rightCount);
                        if (test.rightCount > 0)
                        {
                            if (test.rightCupetIds == null || test.rightCupetIds.Length != test.rightCount)
                                test.rightCupetIds = new int[test.rightCount];

                            for (int i = 0; i < test.rightCount; i++)
                                DrawField($"[2] RightCupetIds[{i}]", ref test.rightCupetIds[i]);
                        }

                        DrawButton("Request", test.RequestGuildBattlEntry);
                    }
                    EndContents();
                }
            }

            private void Draw5()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_LIST_DETAIL_INFO (321)"))
                {
                    BeginContents();
                    {
                        DrawField("[1] GuildId", ref test.guildId);
                        DrawButton("Request", test.RequestGuildBattleTargetDetailInfo);
                    }
                    EndContents();
                }
            }

            private void Draw6()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_START (322)"))
                {
                    BeginContents();
                    {
                        DrawField("[1] DefGuildId", ref test.defGuildId);
                        DrawField("[2] SupportMemberCid", ref test.supportMemberCid);
                        DrawButton("Request", test.RequestGuildBattleStartDheck);
                    }
                    EndContents();
                }
            }

            private void Draw7()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_START_LOADING_COMP (323)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattlStartLoaindComplete);
                    }
                    EndContents();
                }
            }

            private void Draw8()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_CUPET_SETTING (324)"))
                {
                    BeginContents();
                    {
                        DrawField("Count", ref test.attackCupetCount);
                        if (test.attackCupetCount > 0)
                        {
                            if (test.attackCupetIds == null || test.attackCupetIds.Length != test.attackCupetCount)
                                test.attackCupetIds = new int[test.attackCupetCount];

                            for (int i = 0; i < test.attackCupetCount; i++)
                                DrawField($"[1] CupetIds[{i}]", ref test.attackCupetIds[i]);
                        }

                        DrawButton("Request", test.RequestGuildBattleSupportCupetSetting);
                    }
                    EndContents();
                }
            }

            private void Draw9()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_DEF_INFO (325)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattlDefInfo);
                    }
                    EndContents();
                }
            }

            private void Draw10()
            {
                if (DrawMiniHeader("REQUEST_GUILD_CUPET_LEVEL_UP (326)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattlBuffLevelUp);
                    }
                    EndContents();
                }
            }

            private void Draw11()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_BUFF_INFO (327)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattlBuffInfo);
                    }
                    EndContents();
                }
            }

            private void Draw12()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_BUFF_LEVEL_UP (328)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattlBuffLevelUp);
                    }
                    EndContents();
                }
            }

            private void Draw13()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_ATTACK_DAMAGE (329)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattlAttackDamage);
                    }
                    EndContents();
                }
            }

            private void Draw14()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_RANK (330)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattlRank);
                    }
                    EndContents();
                }
            }

            private void Draw15()
            {
                if (DrawMiniHeader("REQUEST_GUILD_BATTLE_END (331)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestGuildBattlEnd);
                    }
                    EndContents();
                }
            }

            private void Draw16()
            {
                if (DrawMiniHeader("아이템 분해"))
                {
                    BeginContents();
                    {
                        DrawField("Count", ref test.itemCount);
                        if (test.itemCount > 0)
                        {
                            if (test.itemNoArray == null || test.itemNoArray.Length != test.itemCount)
                                test.itemNoArray = new long[test.itemCount];

                            for (int i = 0; i < test.itemCount; i++)
                                DrawField($"[1] ItemNo[{i}]", ref test.itemNoArray[i]);
                        }
                        DrawField("[2] Type", ref test.type);
                        DrawButton("Request", test.RequestItemDisassemble);
                    }
                    EndContents();
                }
            }

            private void Draw17()
            {
                if (DrawMiniHeader("REQUEST_FIND_ALPHABET_REWARD (357)"))
                {
                    BeginContents();
                    {
                        DrawButton("Request", test.RequestFindAlphabetReward);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif