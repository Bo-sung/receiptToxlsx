using System;

namespace Ragnarok.CIBuilder
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AuthServerConfigAttribute : Attribute
    {
        public static readonly AuthServerConfigAttribute DEFAULT = new AuthServerConfigAttribute(string.Empty, 0, string.Empty);

        public readonly string connectIP;
        public readonly int connectPort;
        public readonly string resourceUrl;

        /// <summary>
        /// 인증서버 설정
        /// </summary>
        /// <param name="connectIP">인증 서버 접속 시 사용하는 IP</param>
        /// <param name="connectPort">인증 서버 접속 시 사용하는 PORT</param>
        /// <param name="resourceUrl">DB를 받아올 때 사용하는 Url</param>
        public AuthServerConfigAttribute(string connectIP, int connectPort, string resourceUrl)
        {
            this.connectIP = connectIP;
            this.connectPort = connectPort;           
            this.resourceUrl = resourceUrl;
        }

        public bool Equals(string connectIP, int connectPort, string resourceUrl)
        {
            if (!string.Equals(this.connectIP, connectIP))
                return false;

            if (!this.connectPort.Equals(connectPort))
                return false;           

            if (!this.resourceUrl.Equals(resourceUrl))
                return false;

            return true;
        }
    }
}