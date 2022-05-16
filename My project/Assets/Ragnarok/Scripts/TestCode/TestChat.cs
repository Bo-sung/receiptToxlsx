#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestChat : TestCode
    {
        private int targetCid;
        private string comment;
        private byte reasonType;

        protected override void Awake()
        {
            base.Awake();
            Protocol.RESULT_CHAT_LIMIT.AddEvent(ResultChatLimit);
        }

        private void OnDestroy()
        {
            Protocol.RESULT_CHAT_LIMIT.RemoveEvent(ResultChatLimit);
        }

        private void ResultChatLimit(Response response)
        {
            var remainTime = response.GetLong("1");
            Debug.LogError("(Protocol.RESULT_CHAT_LIMIT) Remain Time : " + remainTime);
        }

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestRepotChar()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", targetCid);
            sfs.PutByte("3", reasonType);
            Protocol.REQUEST_REPORT_CHAR.SendAsync(sfs).WrapNetworkErrors();

            // resultcode 39 --> 신고 후 5분이 지나야 다시 신고가능(팝업 표시)
        }

        [UnityEditor.CustomEditor(typeof(TestChat))]
        class TestChatEditor : Editor<TestChat>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("REPORT CHARACTER INFO"))
                {
                    BeginContents();
                    {
                        DrawTitle("신고할 캐릭터 정보");
                        DrawField("target_cid (int)", ref test.targetCid);
                        DrawField("reason_type (byte)", ref test.reasonType);
                        DrawButton("Send", test.RequestRepotChar);
                    }
                    EndContents();
                }
            }

        }
    }
}
#endif