namespace Ragnarok
{
    public sealed class OnBuffPassPacket : IPacket<Response>, IPassPacket
    {
        public static readonly OnBuffPassPacket EMPTY = new OnBuffPassPacket();

        private long pay_pass_end_time; // 패스 구매 여부, 0 이상 구매
        private string pass_free_step; // 무료 보상 수령한 회차 목록
        private string pass_pay_step; // 유료 보상 수령한 회차 목록
        private int pass_exp; // 패스 경험치

        public long PayPassEndTime => pay_pass_end_time;
        public string PassFreeStep => pass_free_step;
        public string PassPayStep => pass_pay_step;
        public int PassExp => pass_exp;

        void IInitializable<Response>.Initialize(Response response)
        {
            pay_pass_end_time = response.GetLong("1");
            pass_free_step = response.GetUtfString("2");
            pass_pay_step = response.GetUtfString("3");
            pass_exp = response.GetInt("4");
        }
    }
}