#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestJobChange : TestCode
    {
        Job job;

        private void RequestJobChangeTicket()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)job);
            Protocol.REQUEST_JOB_CHANGE_TICKET.SendAsync(sfs).WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestJobChange))]
        class TestJobChangeEditor : Editor<TestJobChange>
        {
            public override void OnInspectorGUI()
            {
                //base.OnInspectorGUI();
                DrawBaseGUI();

                Draw1();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("REQUEST_JOB_CHANGE_TICKET (334)"))
                {
                    BeginContents();
                    {
                        DrawTitle("직업 변경");
                        DrawField("[1] job", ref test.job);
                        DrawButton("Request", test.RequestJobChangeTicket);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif