using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Ragnarok
{
    public class GuildBoardInfo : IInfo, IInitializable<GuildBoardPacket>
    {
        bool IInfo.IsInvalidData => false;

        event Action IInfo.OnUpdateEvent
        {
            add { }
            remove { }
        }

        private ObscuredString name;
        private ObscuredString message;
        private DateTime insert_dt;
        private ObscuredInt seq;
        private ObscuredInt message_id; // 0이 아니면 시스템 메시지
        private ObscuredInt cid;
        private ObscuredInt uid;
        private ObscuredString hex_cid;

        public int Seq => seq;

        public int CID => cid;

        public string InsertTime => $"{insert_dt:yyyy-MM-dd hh:mm:ss tt}";

        /// <summary>
        /// 게시판 닉네임
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 게시판 메시지
        /// </summary>
        public string Message => GetMessage();

        /// <summary>
        /// 시스템 메시지 여부
        /// </summary>
        public bool IsSystem => message_id != 0;

        private string GetMessage()
        {
            if (message_id != 0)
            {
                string msg = message;
                if (!msg.Contains("<;>"))
                    return message_id.ToText().Replace("{NAME}", msg);

                // LocalizeKey._90060.ToText(); // 길드스킬 {NAME}이 {LEVEL}레벨로 변경 되었습니다.
                var args = msg.Replace("<;>", ";").Split(';');
                if (args.Length < 2)
                    return default;

                if (!int.TryParse(args[0], out int result))
                    return default;

                return message_id.ToText()
                    .Replace("{NAME}", result.ToText())
                    .Replace("{LEVEL}", args[1]);
            }
            return message;
        }


        public void Initialize(GuildBoardPacket packet)
        {
            name = packet.name;
            message = packet.message;
            insert_dt = packet.insert_dt.ToDateTime();
            seq = packet.seq;
            message_id = packet.message_id;
            cid = packet.cid;
            uid = packet.uid;
            hex_cid = packet.hex_cid;
        }
    }
}
