#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestMail : TestCode
    {
        private enum MailType : byte
        {
            Account = 0,
            Character
        }

        private MailType mailType;
        private int mailId;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestMailDelete()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", mailId);
            sfs.PutByte("2", (byte)mailType);
            Protocol.REQUEST_MAIL_DELETE.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestFacebookLikeReward()
        {
            Protocol.REQUEST_FB_LIKE_REWARD.SendAsync().WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestMail))]
        class TestMailEditor : Editor<TestMail>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("Mail Test"))
                {
                    BeginContents();
                    {
                        DrawTitle("메일");
                        DrawField("메일ID", ref test.mailId);
                        DrawField("메일Type", ref test.mailType);
                        DrawButton("삭제", test.RequestMailDelete);
                    }
                    EndContents();
                }

                if (DrawMiniHeader("Like Test"))
                {
                    BeginContents();
                    {
                        DrawTitle("좋아요");
                        DrawButton("보상", test.RequestFacebookLikeReward);
                    }
                    EndContents();
                }
            }

        }
    }
}
#endif