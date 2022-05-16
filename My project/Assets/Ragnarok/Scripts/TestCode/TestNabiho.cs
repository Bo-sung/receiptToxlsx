#if UNITY_EDITOR

namespace Ragnarok.Test
{
    public sealed class TestNabiho : TestCode
    {
        private int nabihoId;
        private int count;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestNabihoInfo()
        {
            Entity.player.Inventory.RequestNabihoInfo().WrapNetworkErrors();
        }

        private void RequestNabihoItemSelect()
        {
            Entity.player.Inventory.RequestNabihoItemSelect(nabihoId).WrapNetworkErrors();
        }

        private void RequestNabihoItemSelectCancel()
        {
            Entity.player.Inventory.RequestNabihoItemSelectCancel(nabihoId).WrapNetworkErrors();
        }

        private void RequestNabihoItemADTimeReduction()
        {
            Entity.player.Inventory.RequestNabihoItemAdTimeReduction(nabihoId).WrapNetworkErrors();
        }

        private void RequestNabihoItemSelectGet()
        {
            Entity.player.Inventory.RequestNabihoItemSelectGet(nabihoId).WrapNetworkErrors();
        }

        private void RequestNabihoSendPresent()
        {
            Entity.player.Inventory.RequestNabihoSendPresent(count).WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestNabiho))]
        class TestSharingEditor : Editor<TestNabiho>
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
            }

            private void Draw1()
            {
                if (DrawMiniHeader("REQUEST_NABIHO_INFO(351)"))
                {
                    BeginContents();
                    {
                        DrawTitle("나비호 정보");
                        DrawButton("Request", test.RequestNabihoInfo);
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("REQUEST_NABIHO_ITEM_SELECT(352)"))
                {
                    BeginContents();
                    {
                        DrawTitle("나비호 의뢰 아이템 선택");
                        DrawField("[1]nabihoId", ref test.nabihoId);
                        DrawButton("Request", test.RequestNabihoItemSelect);
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("REQUEST_NABIHO_ITEM_SELECT_CANCEL(353)"))
                {
                    BeginContents();
                    {
                        DrawTitle("나비호 의뢰 아이템 선택 취소");
                        DrawField("[1]nabihoId", ref test.nabihoId);
                        DrawButton("Request", test.RequestNabihoItemSelectCancel);
                    }
                    EndContents();
                }
            }

            private void Draw4()
            {
                if (DrawMiniHeader("REQUEST_NABIHO_ITEM_AD_TIME_REDUCTION(354)"))
                {
                    BeginContents();
                    {
                        DrawTitle("나비호 의뢰 아이템 광고 시청 후 시간 단축");
                        DrawField("[1]nabihoId", ref test.nabihoId);
                        DrawButton("Request", test.RequestNabihoItemADTimeReduction);
                    }
                    EndContents();
                }
            }

            private void Draw5()
            {
                if (DrawMiniHeader("REQUEST_NABIHO_ITEM_SELECT_GET(355)"))
                {
                    BeginContents();
                    {
                        DrawTitle("나비호 의뢰 아이템 수령");
                        DrawField("[1]nabihoId", ref test.nabihoId);
                        DrawButton("Request", test.RequestNabihoItemSelectGet);
                    }
                    EndContents();
                }
            }

            private void Draw6()
            {
                if (DrawMiniHeader("REQUEST_NABIHO_SEND_PRESENT(356)"))
                {
                    BeginContents();
                    {
                        DrawTitle("나비호 선물 - 친밀도 증가");
                        DrawField("[1]count", ref test.count);
                        DrawButton("Request", test.RequestNabihoSendPresent);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif