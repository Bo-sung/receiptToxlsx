#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{

    public sealed class TestSharing : TestCode
    {
        private int uid;
        private int cid;

        private int useUid;
        private int useCid;

        private SharingModel.CloneCharacterType cloneType;
        private int cloneUid;
        private int cloneCid;

        private SharingModel.CloneCharacterType releaseCloneType;

        private int itemId;
        private int itemCount;

        private int charCid;
        private int charCid1;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void ShowCharacterInfo()
        {
            uid = Entity.player.User.UID;
            cid = Entity.player.Character.Cid;
        }

        private void RequestUseShare()
        {
            Entity.player.Sharing.RequestShareCharacterUseSetting(SharingModel.CharacterShareFlag.Use, useCid, useUid, SharingModel.SharingCharacterType.Normal).WrapNetworkErrors();
        }

        private void RequestShareCloneCharacter()
        {
            Entity.player.Sharing.RequestShareCloneCharacter(cloneType, cloneUid, cloneCid).WrapNetworkErrors();
        }

        private void RequestReleaseCloneCharacter()
        {
            Entity.player.Sharing.RequestReleaseCloneCharacter(releaseCloneType).WrapNetworkErrors();
        }

        private void Disconnect()
        {
            ConnectionManager.Instance.Disconnect();
        }

        private void RequestShareviceLevelUp()
        {
            var sfs = Protocol.NewInstance();
            var sfsArray = Protocol.NewArrayInstance();

            var element = Protocol.NewInstance();
            element.PutInt("1", itemId);
            element.PutInt("2", itemCount);
            sfsArray.AddSFSObject(element);

            sfs.PutSFSArray("1", sfsArray);
            Protocol.REQUEST_SHARE_VICE_LEVELUP.SendAsync(sfs).WrapErrors();
        }

        private void RequestShareCharRewardInfo()
        {
            Entity.player.Sharing.RequestShareCharacterRewardInfo().WrapNetworkErrors();
        }

        private async void ReqeustChracterRetry()
        {
            await Protocol.CHARACTER_LIST.SendAsync();
            await Protocol.USER_INFO.SendAsync();

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", charCid);
            await Protocol.JOIN_GAME_MAP.SendAsync(sfs);

            await Protocol.CHARACTER_LIST.SendAsync();
            await Protocol.USER_INFO.SendAsync();

            var sfs1 = Protocol.NewInstance();
            sfs1.PutInt("1", charCid1);
            await Protocol.JOIN_GAME_MAP.SendAsync(sfs1);
        }

        private void RequestGuildShareCharList()
        {
            Protocol.REQUEST_GUILD_SHARE_CHAR_LIST.SendAsync().WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestSharing))]
        class TestSharingEditor : Editor<TestSharing>
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
            }

            private void Draw1()
            {
                if (DrawMiniHeader("CHARACTER INFO"))
                {
                    BeginContents();
                    {
                        DrawTitle("나의 캐릭터 정보");
                        DrawField("uid", ref test.uid);
                        DrawField("cid", ref test.cid);
                        DrawButton("Show", test.ShowCharacterInfo);
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("REQUEST_SHARE_CHAR_USE_SETTING (213)"))
                {
                    BeginContents();
                    {
                        DrawTitle("특정 캐릭터 공유 사용");
                        DrawField("uid", ref test.useUid);
                        DrawField("cid", ref test.useCid);
                        DrawButton("Request", test.RequestUseShare);
                    }
                    EndContents();
                }
            }

            private void Draw3()
            {
                if (DrawMiniHeader("DISCONNECT"))
                {
                    BeginContents();
                    {
                        DrawTitle("강제 연결 끊기");
                        DrawButton("Disconnect", test.Disconnect);
                    }
                    EndContents();
                }
            }

            private void Draw4()
            {
                if (DrawMiniHeader("REQUEST_CLONE_SHARE (285)"))
                {
                    BeginContents();
                    {
                        DrawTitle("클론 캐릭터 셰어");
                        DrawField("type", ref test.cloneType);
                        DrawField("uid", ref test.cloneUid);
                        DrawField("cid", ref test.cloneCid);
                        DrawButton("Request", test.RequestShareCloneCharacter);
                    }
                    EndContents();
                }
            }

            private void Draw5()
            {
                if (DrawMiniHeader("REQUEST_CLONE_SHARE_RELEASE (286)"))
                {
                    BeginContents();
                    {
                        DrawTitle("클론 캐릭터 셰어 해제");
                        DrawField("type", ref test.releaseCloneType);
                        DrawButton("Request", test.RequestReleaseCloneCharacter);
                    }
                    EndContents();
                }
            }

            private void Draw6()
            {
                if (DrawMiniHeader("REQUEST_SHARE_VICE_LEVELUP (254)"))
                {
                    BeginContents();
                    {
                        DrawTitle("셰어바이스 레벨업");
                        DrawField("ItemId", ref test.itemId);
                        DrawField("Count", ref test.itemCount);
                        DrawButton("요청", test.RequestShareviceLevelUp);
                    }
                    EndContents();
                }
            }

            private void Draw7()
            {
                if (DrawMiniHeader("REQUEST_SHARE_CHAR_REWARD_INFO (215)"))
                {
                    BeginContents();
                    {
                        DrawTitle("셰어공유 보상 정보");
                        DrawButton("요청", test.RequestShareCharRewardInfo);
                    }
                    EndContents();
                }
            }

            private void Draw8()
            {
                if (DrawMiniHeader("동일 캐릭터 재접속"))
                {
                    BeginContents();
                    {
                        DrawField("cid", ref test.charCid);
                        DrawField("cid1", ref test.charCid1);
                        DrawButton("요청", test.ReqeustChracterRetry);
                    }
                    EndContents();
                }
            }

            private void Draw9()
            {
                if (DrawMiniHeader("REQUEST_GUILD_SHARE_CHAR_LIST(340)"))
                {
                    BeginContents();
                    {
                        DrawButton("요청", test.RequestGuildShareCharList);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif