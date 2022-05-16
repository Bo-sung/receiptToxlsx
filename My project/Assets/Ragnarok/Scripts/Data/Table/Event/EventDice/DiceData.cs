using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DiceDataManager"/>
    /// </summary>
    public sealed class DiceData : IData, UIDiceEventResult.IInput
    {
        public int EventId { get; }
        public DiceEventType DiceEventType { get; }
        public int DiceEventValue { get; }
        public string ImageName { get; }
        public int NameId { get; }
        public int DescrptionId { get; }

        public DiceData(IList<MessagePackObject> data)
        {
            int index = 0;
            EventId = data[index++].AsInt32();
            int rate = data[index++].AsInt32();
            DiceEventType = data[index++].AsInt32().ToEnum<DiceEventType>();
            DiceEventValue = data[index++].AsInt32();
            ImageName = data[index++].AsString();
            NameId = data[index++].AsInt32();
            DescrptionId = data[index++].AsInt32();
        }
    }
}