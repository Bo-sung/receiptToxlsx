using Sfs2X.Util;
using Sfs2X.Entities.Data;

namespace Ragnarok
{
    public class Response : ISFSObject
    {
        private const string RESULT_KEY = "0";

        private readonly ISFSObject received;

        public readonly ISFSObject sended;
        public readonly ResultCode resultCode;
        public readonly bool isSuccess;

        public Response(ISFSObject received)
            : this(received, null)
        {
        }

        public Response(ISFSObject received, ISFSObject sended)
        {
            this.received = received;
            this.sended = sended;

            int resultKey = ResultCode.SUCCESS.Key;
            if (received.ContainsKey(RESULT_KEY))
            {
                const int BYTE_TYPE = (int)SFSDataType.BYTE;
                const int INT_TYPE = (int)SFSDataType.INT;

                SFSDataWrapper wrapper = received.GetData(RESULT_KEY);
                switch (wrapper.Type)
                {
                    case BYTE_TYPE:
                        resultKey = received.GetByte(RESULT_KEY);
                        break;

                    case INT_TYPE:
                        resultKey = received.GetInt(RESULT_KEY);
                        break;
                }
            }

            resultCode = ResultCode.GetByKey(resultKey);
            isSuccess = resultCode == ResultCode.SUCCESS;
        }

        /// <summary>
        /// 내부 생성자 (ResultCode의 Execute 호출하지 않습니다)
        /// </summary>
        private Response(ISFSObject sfsObject, bool isSuccess, ResultCode resultCode)
        {
            this.received = sfsObject;
            this.isSuccess = isSuccess;
            this.resultCode = resultCode;
        }

        /// <summary>
        /// 에러 코드에 해당하는 메시지 보여주기
        /// </summary>
        public void ShowResultCode()
        {
            if (resultCode.Execute())
                return;

            // 서버에서 보내준 메세지로 세팅
            if (resultCode == ResultCode.RCODE_MSG)
            {
                if (received.ContainsKey("1"))
                {
                    string serverMessage = received.GetUtfString("1");
                    resultCode.SetSeverMessage(serverMessage);
                }
            }

            // 온버프 점검 중
            if(resultCode == ResultCode.INNO_MAINTENANCE)
            {
                if(ContainsKey("1"))
                {
                    RemainTime remainTime = GetLong("1"); // 남은 시간 (밀리초)
                    string message = StringBuilderPool.Get()
                        .Append(resultCode.GetDescription())
                        .AppendLine()
                        .Append('(').Append(remainTime.ToRemainTime().ToStringTimeConatinsDayLocal()).Append(')')
                        .Release();
                    UI.ConfirmPopup(message);
                    return;
                }
            }

            resultCode.ShowResultCode();
        }

        public T GetPacket<T>()
            where T : IPacket<Response>, new()
        {
            T packet = new T();
            packet.Initialize(this);
            return packet;
        }

        public T GetPacket<T>(string key)
            where T : IPacket<Response>, new()
        {
            T packet = new T();
            ISFSObject sfs = GetSFSObject(key);
            packet.Initialize(new Response(sfs, isSuccess, resultCode));
            return packet;
        }

        public T[] GetPacketArray<T>(string key)
            where T : IPacket<Response>, new()
        {
            ISFSArray sfsArray = GetSFSArray(key);

            T[] arrPacket = new T[sfsArray.Size()];
            for (int i = 0; i < arrPacket.Length; i++)
            {
                arrPacket[i] = new T();
                ISFSObject sfs = sfsArray.GetSFSObject(i);
                arrPacket[i].Initialize(new Response(sfs, isSuccess, resultCode));
            }

            return arrPacket;
        }

        public T GetBinaryPacket<T>(string key)
            where T : IPacket<ByteArray>, new()
        {
            T binaryPacket = new T();
            binaryPacket.Initialize(GetByteArray(key));
            return binaryPacket;
        }

        public bool IsNull(string key)
        {
            return received.IsNull(key);
        }

        public bool ContainsKey(string key)
        {
            return received.ContainsKey(key);
        }

        public void RemoveElement(string key)
        {
            received.RemoveElement(key);
        }

        public string[] GetKeys()
        {
            return received.GetKeys();
        }

        public int Size()
        {
            return received.Size();
        }

        public ByteArray ToBinary()
        {
            return received.ToBinary();
        }

        public string ToJson()
        {
            return received.ToJson();
        }

        public string GetDump(bool format)
        {
            return received.GetDump(format);
        }

        public string GetDump()
        {
            return received.GetDump();
        }

        public string GetHexDump()
        {
            return received.GetHexDump();
        }

        public SFSDataWrapper GetData(string key)
        {
            return received.GetData(key);
        }

        public bool GetBool(string key)
        {
            return received.GetBool(key);
        }

        public byte GetByte(string key)
        {
            return received.GetByte(key);
        }

        public short GetShort(string key)
        {
            return received.GetShort(key);
        }

        public int GetInt(string key)
        {
            return received.GetInt(key);
        }

        public long GetLong(string key)
        {
            return received.GetLong(key);
        }

        public float GetFloat(string key)
        {
            return received.GetFloat(key);
        }

        public double GetDouble(string key)
        {
            return received.GetDouble(key);
        }

        public string GetUtfString(string key)
        {
            return received.GetUtfString(key);
        }

        public string GetText(string key)
        {
            return received.GetText(key);
        }

        public bool[] GetBoolArray(string key)
        {
            return received.GetBoolArray(key);
        }

        public ByteArray GetByteArray(string key)
        {
            return received.GetByteArray(key);
        }

        public short[] GetShortArray(string key)
        {
            return received.GetShortArray(key);
        }

        public int[] GetIntArray(string key)
        {
            return received.GetIntArray(key);
        }

        public long[] GetLongArray(string key)
        {
            return received.GetLongArray(key);
        }

        public float[] GetFloatArray(string key)
        {
            return received.GetFloatArray(key);
        }

        public double[] GetDoubleArray(string key)
        {
            return received.GetDoubleArray(key);
        }

        public string[] GetUtfStringArray(string key)
        {
            return received.GetUtfStringArray(key);
        }

        public ISFSArray GetSFSArray(string key)
        {
            return received.GetSFSArray(key);
        }

        public ISFSObject GetSFSObject(string key)
        {
            return received.GetSFSObject(key);
        }

        public object GetClass(string key)
        {
            return received.GetClass(key);
        }

        public void PutNull(string key)
        {
            received.PutNull(key);
        }

        public void PutBool(string key, bool val)
        {
            received.PutBool(key, val);
        }

        public void PutByte(string key, byte val)
        {
            received.PutByte(key, val);
        }

        public void PutShort(string key, short val)
        {
            received.PutShort(key, val);
        }

        public void PutInt(string key, int val)
        {
            received.PutInt(key, val);
        }

        public void PutLong(string key, long val)
        {
            received.PutLong(key, val);
        }

        public void PutFloat(string key, float val)
        {
            received.PutFloat(key, val);
        }

        public void PutDouble(string key, double val)
        {
            received.PutDouble(key, val);
        }

        public void PutUtfString(string key, string val)
        {
            received.PutUtfString(key, val);
        }

        public void PutText(string key, string val)
        {
            received.PutText(key, val);
        }

        public void PutBoolArray(string key, bool[] val)
        {
            received.PutBoolArray(key, val);
        }

        public void PutByteArray(string key, ByteArray val)
        {
            received.PutByteArray(key, val);
        }

        public void PutShortArray(string key, short[] val)
        {
            received.PutShortArray(key, val);
        }

        public void PutIntArray(string key, int[] val)
        {
            received.PutIntArray(key, val);
        }

        public void PutLongArray(string key, long[] val)
        {
            received.PutLongArray(key, val);
        }

        public void PutFloatArray(string key, float[] val)
        {
            received.PutFloatArray(key, val);
        }

        public void PutDoubleArray(string key, double[] val)
        {
            received.PutDoubleArray(key, val);
        }

        public void PutUtfStringArray(string key, string[] val)
        {
            received.PutUtfStringArray(key, val);
        }

        public void PutSFSArray(string key, ISFSArray val)
        {
            received.PutSFSArray(key, val);
        }

        public void PutSFSObject(string key, ISFSObject val)
        {
            received.PutSFSObject(key, val);
        }

        public void PutClass(string key, object val)
        {
            received.PutClass(key, val);
        }

        public void Put(string key, SFSDataWrapper val)
        {
            received.Put(key, val);
        }
    }
}