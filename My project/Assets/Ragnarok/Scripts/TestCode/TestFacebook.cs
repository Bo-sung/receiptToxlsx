#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestFacebook : TestCode
    {
        enum FriendCountType
        {
            _1 = 1,
            _2,
            _3,
            _4,
            _5,
            _6,
            _7,
            _8,
            _9,
            _10,
        }

        private FriendCountType cntFriends;
        private FriendCountType cntComplete;
        private int[] aryFriend;
        private int[] aryComplete;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestInviteFriends()
        {
            var sfs = Protocol.NewInstance();

            if (aryFriend != null)
            {
                sfs.PutUtfStringArray("1", Array.ConvertAll(aryFriend, x => x.ToString()));
            }
            else
            {
                Debug.LogError("____ 초대한 친구가 없습니다.");
                return;
            }

            Protocol.REQUEST_INVITE_FB_IDS.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestAcceptFriends()
        {
            var sfs = Protocol.NewInstance();

            if (aryComplete != null)
            {
                sfs.PutUtfStringArray("1", Array.ConvertAll(aryComplete, x => x.ToString()));
            }
            else
            {
                // 완료한 친구가 없어도 보상 체크를위해 보냄
                sfs.PutUtfStringArray("1", new string[0]);
            }

            Protocol.REQUEST_ACCEPT_FB_IDS.SendAsync(sfs).WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestFacebook))]
        class TestFacebookEditor : Editor<TestFacebook>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("페이스북 친구초대"))
                {
                    BeginContents();
                    {
                        DrawTitle("초대한 친구 리스트");
                        DrawField("친구 수", ref test.cntFriends);

                        if (test.cntFriends > 0)
                        {
                            if (test.aryFriend == null || test.aryFriend.Length != test.cntFriends.ToIntValue())
                                test.aryFriend = new int[test.cntFriends.ToIntValue()];

                            for (int i = 0; i < test.cntFriends.ToIntValue(); i++)
                                DrawField("페이스북 ID", ref test.aryFriend[i]);
                        }                        

                        DrawButton("보내기", test.RequestInviteFriends);
                    }
                    EndContents();
                    BeginContents();
                    {
                        DrawTitle("보상 가능한 친구 리스트");
                        DrawField("친구 수", ref test.cntComplete);

                        if (test.cntComplete > 0)
                        {
                            if (test.aryComplete == null || test.aryComplete.Length != test.cntComplete.ToIntValue())
                                test.aryComplete = new int[test.cntComplete.ToIntValue()];

                            for (int i = 0; i < test.cntComplete.ToIntValue(); i++)
                                DrawField("페이스북 ID", ref test.aryComplete[i]);
                        }

                        DrawButton("보내기", test.RequestAcceptFriends);
                    }
                    EndContents();
                }
            }

        }
    }
}
#endif