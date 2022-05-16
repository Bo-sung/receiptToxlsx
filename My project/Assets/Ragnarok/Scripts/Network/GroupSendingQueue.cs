using Sfs2X.Entities.Data;
using System.Collections.Generic;

namespace Ragnarok
{
    public class GroupSendingQueue
    {
        /// <summary>
        /// SendingQueueGroupId
        /// </summary>
        private readonly HashSet<int> hashSet;

        /// <summary>
        /// key: SendingQueueGroupId
        /// </summary>
        private readonly Dictionary<int, Queue<ProtocolTuple>> dic;

        public GroupSendingQueue()
        {
            hashSet = new HashSet<int>(IntEqualityComparer.Default);
            dic = new Dictionary<int, Queue<ProtocolTuple>>(IntEqualityComparer.Default);
        }

        /// <summary>
        /// 데이터 삭제
        /// </summary>
        public void Clear()
        {
            hashSet.Clear();
            dic.Clear();
        }

        /// <summary>
        /// 보낸 그룹 아이디 존재 유무
        /// </summary>
        public bool HasSendingGroup(int queueGroupId)
        {
            return hashSet.Contains(queueGroupId);
        }

        /// <summary>
        /// 보낸 그룹 아이디 추가
        /// </summary>
        public void AddSendingGroupId(int queueGroupId)
        {
            hashSet.Add(queueGroupId);
        }

        /// <summary>
        /// 보낸 그룹 아이디 제거
        /// </summary>
        public void RemoveSendingGroupId(int queueGroupId)
        {
            hashSet.Remove(queueGroupId);
        }

        /// <summary>
        /// 대기 중인 패킷 유무
        /// </summary>
        public bool HasPacketQueue(int queueGroupId)
        {
            if (dic.ContainsKey(queueGroupId))
                return dic[queueGroupId].Count > 0;

            return false;
        }

        /// <summary>
        /// 대기 목록에 패킷 저장
        /// </summary>
        public void EnqueuePacket(int queueGroupId, Protocol protocol, ISFSObject param)
        {
            if (!dic.ContainsKey(queueGroupId))
                dic.Add(queueGroupId, new Queue<ProtocolTuple>());

            dic[queueGroupId].Enqueue(new ProtocolTuple(protocol, param));
        }

        /// <summary>
        /// 대기 목록의 패킷 꺼내기
        /// </summary>
        public ProtocolTuple DequeuePacket(int sendingQueueGroupId)
        {
            return dic[sendingQueueGroupId].Dequeue();
        }
    }
}