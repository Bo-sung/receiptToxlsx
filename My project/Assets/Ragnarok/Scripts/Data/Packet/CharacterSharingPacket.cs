namespace Ragnarok
{
    public sealed class CharacterSharingPacket : IPacket<Response>
    {
        public int shareFlag;
        public SharingRewardPacket sharingRewardPacket;
        public SharingEmployerPacket sharingEmployerPacket;
        public int[] jobFilterAry;

        void IInitializable<Response>.Initialize(Response response)
        {
            shareFlag = response.GetInt("1");
            sharingRewardPacket = response.ContainsKey("2") ? response.GetPacket<SharingRewardPacket>("2") : null;
            sharingEmployerPacket = response.ContainsKey("3") ? response.GetPacket<SharingEmployerPacket>("3") : null;

            if(response.ContainsKey("4"))
            {
                var filterAry = response.GetUtfString("4").Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
                jobFilterAry = System.Array.ConvertAll(filterAry, int.Parse);
            }
            else
            {
                jobFilterAry = new int[0];
            }
        }
    }
}