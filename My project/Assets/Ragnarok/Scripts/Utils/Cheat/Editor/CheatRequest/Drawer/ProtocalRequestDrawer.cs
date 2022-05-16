using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ProtocalRequestDrawer : CheatRequestDrawer
    {
        public override int OrderNum => 5;
        public override string Title => "프로토콜";

        private string inno_uid;

        protected override void Awake()
        {
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDraw()
        {
            if (DrawMiniHeader("온버프"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("REQUEST_ONBUFF_LOGIN(359)");
                    DrawRequest(RequestOnBuffLogin);

                    GUILayout.Label("REQUEST_ONBUFF_ACCOUNT_LINK(360)");
                    inno_uid = EditorGUILayout.TextField(nameof(inno_uid), inno_uid);
                    DrawRequest(RequestOnBuffAccountLink);

                    GUILayout.Label("REQUEST_ONBUFF_ACCOUNT_UNLINK(361)");
                    DrawRequest(RequestOnBuffAccountUnLink);
                }
            }
        }

        private void RequestOnBuffLogin()
        {
            Protocol.REQUEST_ONBUFF_LOGIN.SendAsync().WrapNetworkErrors();
        }

        private void RequestOnBuffAccountLink()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", inno_uid);
            Protocol.REQUEST_ONBUFF_ACCOUNT_LINK.SendAsync(sfs).WrapNetworkErrors();
        }

        private void RequestOnBuffAccountUnLink()
        {
            Protocol.REQUEST_ONBUFF_ACCOUNT_UNLINK.SendAsync().WrapNetworkErrors();
        }
    }
}