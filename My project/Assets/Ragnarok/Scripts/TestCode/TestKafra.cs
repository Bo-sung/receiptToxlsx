#if UNITY_EDITOR

namespace Ragnarok.Test
{
    public sealed class TestKafra : TestCode
    {
        private int arrayCount;
        private (int id, int count)[] itemArray;
       
        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestKafraDelivery()
        {
            var sfs = Protocol.NewInstance();
            var sfsArray = Protocol.NewArrayInstance();

            for (int i = 0; i < itemArray.Length; i++)
            {
                var element = Protocol.NewInstance();
                element.PutInt("1", itemArray[i].id);
                element.PutInt("2", itemArray[i].count);
                sfsArray.AddSFSObject(element);
            }

            sfs.PutSFSArray("1", sfsArray);
            Protocol.REQUEST_KAF_DELIVERY.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestKafraDeliveryAccept()
        {
            Protocol.REQUEST_KAF_DELIVERY_ACCEPT.SendAsync().WrapNetworkErrors();
        }
        
        private void RequestKafraDeliveryReward()
        {
            Protocol.REQUEST_KAF_DELIVERY_REWARD.SendAsync().WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestKafra))]
        class TestKafraEditor : Editor<TestKafra>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (DrawMiniHeader("카프라 운송"))
                {
                    Draw1();
                    Draw2();
                    Draw3();
                }
            }

            private void Draw1()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_KAF_DELIVERY(314)");
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
                    DrawButton("요청", test.RequestKafraDelivery);
                }
                EndContents();
            }
            
            private void Draw2()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_KAF_DELIVERY_ACCEPT(315)");
                    DrawButton("요청", test.RequestKafraDeliveryAccept);
                }
                EndContents();
            }
            
            private void Draw3()
            {
                BeginContents();
                {
                    DrawTitle("REQUEST_KAF_DELIVERY_REWARD(316)");
                    DrawButton("요청", test.RequestKafraDeliveryReward);
                }
                EndContents();
            }
        }
    }
}

#endif