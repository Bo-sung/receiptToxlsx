#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public sealed class TestServerChange : TestCode
    {
        private int selectServer;

        protected override void Awake()
        {
            base.Awake();

            Protocol.REQUEST_ALL_SERVER_INFO.AddEvent(OnResponseAllServerInfo);
        }

        private void OnDestroy()
        {
            Protocol.REQUEST_ALL_SERVER_INFO.RemoveEvent(OnResponseAllServerInfo);
        }

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void OnResponseAllServerInfo(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            ServerInfoPacket[] packets = response.GetPacketArray<ServerInfoPacket>("1");

            foreach (var item in packets)
            {
                Debug.Log(item.GetDump());
            }
        }

        private void RequestAllServerInfo()
        {
            Protocol.REQUEST_ALL_SERVER_INFO.SendAsync().WrapNetworkErrors();
        }

        private void ServerChange()
        {
            ConnectionManager.Instance.Disconnect();
            ConnectionManager.Instance.SelectServer(selectServer);
            SceneLoader.LoadIntro(); // 타이틀 화면으로 이동
        }

        [UnityEditor.CustomEditor(typeof(TestServerChange))]
        class TestSharingEditor : Editor<TestServerChange>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Draw1();
                Draw2();
            }

            private void Draw1()
            {
                if (DrawMiniHeader("REQUEST_ALL_SERVER_INFO (2006)"))
                {
                    BeginContents();
                    {
                        DrawTitle("모든 서버 정보 목록");
                        DrawButton("Request", test.RequestAllServerInfo);
                    }
                    EndContents();
                }
            }

            private void Draw2()
            {
                if (DrawMiniHeader("다른서버로 접속"))
                {
                    BeginContents();
                    {
                        DrawField("[1] 서버ID", ref test.selectServer);
                        DrawButton("변경", test.ServerChange);
                    }
                    EndContents();
                }
            }
        }
    }
}
#endif