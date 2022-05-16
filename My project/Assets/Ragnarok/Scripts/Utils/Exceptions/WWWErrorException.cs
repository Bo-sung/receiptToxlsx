using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Networking;

namespace Ragnarok
{
    public class WWWErrorException : Exception
    {
        public readonly string url;
        public readonly string rawErrorMessage;
        public readonly string text;
        public readonly bool hasResponse;
        public readonly int timeout;
        public readonly HttpStatusCode statusCode;
        public readonly Dictionary<string, string> responseHeaders;

        public override string Message
        {
            get
            {
                if (string.IsNullOrEmpty(text))
                    return rawErrorMessage;

                return string.Concat(text, " : ", rawErrorMessage);
            }
        }

        public WWWErrorException(UnityWebRequest request)
            : this(request, string.Empty)
        {
        }

        public WWWErrorException(UnityWebRequest request, string text)
        {
            this.url = request.url;
            this.rawErrorMessage = request.error;
            this.text = text;
            this.timeout = request.timeout;

            // responseCode 가 -1일 경우에는 응답을 처리하지 않은 상태
            // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest-responseCode.html
            if (request.responseCode == -1L)
            {
                hasResponse = false;
            }
            else
            {
                hasResponse = true;
                statusCode = (HttpStatusCode)request.responseCode;
                responseHeaders = request.GetResponseHeaders();
            }
        }
    }
}