#if UNITY_EDITOR

namespace Ragnarok.Test
{
    public sealed class TestShop : TestCode
    {
        private int shopId;
        private int jobStep;
        private int scenarioStep;
        private byte dungeonType;
        private int dungeonId;
        private int purchaseShopId;
        private byte adType;
        private byte isFreeAd; // 0: 광고보고 획득, 1 : 무료 획득
        private int exchangeId;
        private int exchangeCount;
        private byte passFlag;
        private int passLevel;
        private int totalPassExp;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestShopPurchase()
        {
            Entity.player.ShopModel?.RequestCashShopPurchase(purchaseShopId, false);
        }

        private void RequestEveryDayGoodsGet()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", shopId);
            Protocol.EVERYDAY_GOODS_GET.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestEveryDayGoodsMyInfo()
        {
            Protocol.EVERYDAY_GOODS_MY_INFO.SendAsync().WrapNetworkErrors();
        }

        private void RequestPayJobLevelReward()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", jobStep);
            Protocol.REQUEST_PAY_JOB_LEVEL_REWARD.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestPayScenarioReward()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", scenarioStep);
            Protocol.REQUEST_PAY_SCENARIO_REWARD.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestMileageReward()
        {
            Entity.player.ShopModel?.RequestMileageReward().WrapNetworkErrors();
        }

        private void RequestDungeonFastClear()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", dungeonType);
            sfs.PutInt("2", dungeonId);
            Protocol.REQUEST_DUNGEON_FAST_CLEAR.SendAsync(sfs).WrapNetworkErrors();
        }

        private void ShowUIMileage()
        {
            UI.Show<UIMileage>();
        }

        private void RequestAdReward()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", adType);
            sfs.PutByte("2", isFreeAd);
            Protocol.REQUEST_AD_REWARD.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestExchange()
        {
            Entity.player.ShopModel.RequestExcnage(exchangeId, exchangeCount).WrapNetworkErrors();
        }

        private void ShowUIPass()
        {
            UI.Show<UIPass>();
        }

        private void RequestPassReward()
        {
            Entity.player.ShopModel.RequestPassReward(PassType.Labyrinth, passFlag, passLevel).WrapErrors();
        }

        private void RequestPassBuyExp()
        {
            Entity.player.ShopModel.RequestBuyPassExp().WrapErrors();
        }

        private void ShowPassExpInfo()
        {
            var output = PassDataManager.Instance.GetLevel(totalPassExp);
            UnityEngine.Debug.Log($"Level = {output.Level}");
            UnityEngine.Debug.Log($"curExp = {output.CurExp}");
            UnityEngine.Debug.Log($"maxExp = {output.MaxExp}");
        }

        [UnityEditor.CustomEditor(typeof(TestShop))]
        class TestSharingEditor : Editor<TestShop>
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
                Draw8();
                Draw9();
                Draw10();
                Draw11();
                Draw12();
                Draw13();
                Draw14();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("상점 구매"))
                {
                    BeginContents();
                    {
                        DrawField("상점ID", ref test.purchaseShopId);
                        DrawButton("구매", test.RequestShopPurchase);
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("EVERYDAY_GOOODS_GET (261)"))
                {
                    BeginContents();
                    {
                        DrawTitle("매일매일 보상 패키지 받기 요청");
                        DrawField("상점ID", ref test.shopId);
                        DrawButton("요청", test.RequestEveryDayGoodsGet);
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("EVERYDAY_GOOODS_MY_INFO (262)"))
                {
                    BeginContents();
                    {
                        DrawTitle("매일매일 보상 패키지 내 정보");
                        DrawButton("요청", test.RequestEveryDayGoodsMyInfo);
                    }
                    EndContents();
                }
            }

            private void Draw4()
            {
                if (DrawMiniHeader("REQUEST_PAY_JOB_LEVEL_REWARD (263)"))
                {
                    BeginContents();
                    {
                        DrawTitle("레벨업(직업) 패키지 보상 받기");
                        DrawField("레벨 스탭", ref test.jobStep);
                        DrawButton("요청", test.RequestPayJobLevelReward);
                    }
                    EndContents();
                }
            }

            private void Draw5()
            {
                if (DrawMiniHeader("REQUEST_PAY_SCENARIO_REWARD (264)"))
                {
                    BeginContents();
                    {
                        DrawTitle("시나리오 패키지 보상 받기");
                        DrawField("시나리오 스탭", ref test.scenarioStep);
                        DrawButton("요청", test.RequestPayScenarioReward);
                    }
                    EndContents();
                }
            }

            private void Draw6()
            {
                if (DrawMiniHeader("REQUEST_MILEAGE_REWARD_GET (267)"))
                {
                    BeginContents();
                    {
                        DrawTitle("마일리지 보상 받기");
                        DrawButton("요청", test.RequestMileageReward);
                    }
                    EndContents();
                }
            }

            private void Draw7()
            {
                if (DrawMiniHeader("REQUEST_DUNGEON_FAST_CLEAR (268)"))
                {
                    BeginContents();
                    {
                        DrawTitle("던전 소탕하기");
                        DrawField("던전타입", ref test.dungeonType);
                        DrawField("던전ID", ref test.dungeonId);
                        DrawButton("요청", test.RequestDungeonFastClear);
                    }
                    EndContents();
                }
            }

            private void Draw8()
            {
                if (DrawMiniHeader("마일리지"))
                {
                    BeginContents();
                    {
                        DrawButton("Show", test.ShowUIMileage);
                    }
                    EndContents();
                }
            }

            private void Draw9()
            {
                if (DrawMiniHeader("REQUEST_AD_REWARD (290)"))
                {
                    BeginContents();
                    {
                        DrawField("광고타입", ref test.adType);
                        DrawField("isFreeAd", ref test.isFreeAd);
                        DrawButton("Show", test.RequestAdReward);
                    }
                    EndContents();
                }
            }

            private void Draw10()
            {
                if (DrawMiniHeader("REQUEST_KAF_EXCHANGE (312)"))
                {
                    BeginContents();
                    {
                        DrawField("id", ref test.exchangeId);
                        DrawField("count", ref test.exchangeCount);
                        DrawButton("요청", test.RequestExchange);
                    }
                    EndContents();
                }
            }

            private void Draw11()
            {
                if (DrawMiniHeader("패스 UI 보기"))
                {
                    BeginContents();
                    {
                        DrawButton("Show", test.ShowUIPass);
                    }
                    EndContents();
                }
            }

            private void Draw12()
            {
                if (DrawMiniHeader("REQUEST_PASS_REWARD (342)"))
                {
                    BeginContents();
                    {
                        DrawField("passType", ref test.passFlag);
                        DrawField("passLevel", ref test.passLevel);
                        DrawButton("요청", test.RequestPassReward);
                    }
                    EndContents();
                }
            }

            private void Draw13()
            {
                if (DrawMiniHeader("REQUEST_PASS_BUY_POINT (343)"))
                {
                    BeginContents();
                    {
                        DrawButton("요청", test.RequestPassBuyExp);
                    }
                    EndContents();
                }
            }

            private void Draw14()
            {
                if (DrawMiniHeader("패스 경험치 정보"))
                {
                    BeginContents();
                    {
                        DrawField("TotlaPassExp", ref test.totalPassExp);
                        DrawButton("요청", test.ShowPassExpInfo);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif