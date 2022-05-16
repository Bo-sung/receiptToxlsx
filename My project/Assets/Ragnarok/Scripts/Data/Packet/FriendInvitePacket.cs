using System.Collections.Generic;

namespace Ragnarok
{
    public class FriendInvitePacket : IPacket<Response>
    {
        public string[] friendAry { get; private set; }
        public int InviteCount { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            if (response.ContainsKey("1"))
            {
                friendAry = response.GetUtfStringArray("1");
            }
            else
            {
                friendAry = new string[0];
            }

            InviteCount = response.GetInt("2");
        }
    }
}