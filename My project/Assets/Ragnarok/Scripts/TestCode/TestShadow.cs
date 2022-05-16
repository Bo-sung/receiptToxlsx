#if UNITY_EDITOR

namespace Ragnarok.Test
{
    public sealed class TestShadow : TestCode
    {
        private long itemNo;
        private byte slot;
        private long levelUpItemNo;
        private long openSlotItemNo;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestItemEquip()
        {
            Entity.player.Inventory.RequestItemEquip(itemNo, slot);
        }

        private void RequestEquipmentLevelUp()
        {
            Entity.player.Inventory.RequestEquipmentLevelUp(levelUpItemNo);
        }

        private void RequestShadowItemOpenCardSlot()
        {
            Entity.player.Inventory.RequestShadowItemOpenCardSlot(openSlotItemNo);
        }


        [UnityEditor.CustomEditor(typeof(TestShadow))]
        class TestShadowEditor : Editor<TestShadow>
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
                if (DrawMiniHeader("장비 장착"))
                {
                    BeginContents();
                    {
                        DrawTitle("ITEM_EQUIP(25)");
                        DrawField("itemNo", ref test.itemNo);
                        DrawField("slot", ref test.slot);
                        DrawButton("요청", test.RequestItemEquip);
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("장비 강화"))
                {
                    BeginContents();
                    {
                        DrawTitle("REQUEST_EQUIP_ITEM_LEVELUP(238)");
                        DrawField("itemNo", ref test.levelUpItemNo);
                        DrawButton("요청", test.RequestEquipmentLevelUp);
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("장비 카드 슬롯 오픈"))
                {
                    BeginContents();
                    {
                        DrawTitle("REQUEST_SHADOW_ITEM_OPEN_CARD_SLOT(300)");
                        DrawField("itemNo", ref test.openSlotItemNo);
                        DrawButton("요청", test.RequestShadowItemOpenCardSlot);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif