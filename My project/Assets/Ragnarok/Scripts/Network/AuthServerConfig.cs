using UnityEngine;

namespace Ragnarok
{
    [System.Serializable]
    public sealed class AuthServerConfig
    {
        [Tooltip("인증 서버 접속 시 사용하는 ip")]
        public string connectIP;

        [Tooltip("인증 서버 접속 시 사용하는 Port")]
        public int connectPort;       

        [Tooltip("테이블 받아올 때 사용하는 주소")]
        public string resourceUrl;
    }
}