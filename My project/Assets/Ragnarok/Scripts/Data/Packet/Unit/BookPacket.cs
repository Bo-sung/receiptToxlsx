using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class BookPacket : IPacket<Response>
    {
        public Response response;

        public void Initialize(Response t)
        {
            response = t;
        }
    }
}
