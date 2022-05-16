using System.Collections.Generic;
using System.Threading.Tasks;
using Sfs2X.Entities.Data;
using MEC;

namespace Ragnarok
{
    public class QueuedProtocol : Protocol
    {
        private readonly Queue<ISFSObject> sendingQueue;

        /// <summary>
        /// 서버 프로토콜 보내는 중
        /// </summary>
        private bool isSendingProtocol;

        /// <summary>
        /// 프로토콜 정의
        /// </summary>
        /// <param name="cmd">cmd</param>
        /// <param name="index">언어 index</param>
        /// <param name="protocolType">프로토콜 타입</param>
        /// <param name="isIndicator">중복 패킷 보내기 방지를 위한 인디케이터 창 Active 여부</param>
        /// <param name="isLog">로그 온오프</param>
        /// <param name="isExecuteDefaultResult">ResultCode에 해당하는 기본 행동 처리 유무</param>
        public QueuedProtocol(string cmd, int index, ProtocolType protocolType, bool isIndicator = true, bool isLog = true)
            : base(cmd, index, protocolType, isIndicator, isLog)
        {
            sendingQueue = new Queue<ISFSObject>();
        }

        //public override Task<Response> SendAsync(ISFSObject param)
        //{
        //    sendingQueue.Enqueue(param);

        //    if (!isSendingProtocol)
        //        Timing.RunCoroutine(YieldSendProtocol());

        //    //return Task.FromResult<Response>(null);
        //    return new TaskCompletionSource<Response>().Task;
        //}

        //private IEnumerator<float> YieldSendProtocol()
        //{
        //    isSendingProtocol = true;

        //    while (sendingQueue.Count > 0)
        //    {
        //        base.SendAsync(sendingQueue.Dequeue()).WrapNetworkErrors();
        //        yield return Timing.WaitUntilTrue(IsReceived);
        //    }

        //    isSendingProtocol = false;
        //}
    }
}