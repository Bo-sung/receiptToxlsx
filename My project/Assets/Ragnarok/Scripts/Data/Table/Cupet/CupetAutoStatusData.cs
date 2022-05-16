namespace Ragnarok
{
    public class CupetAutoStatusData
    {
        public readonly byte str;
        public readonly byte agi;
        public readonly byte vit;
        public readonly byte @int;
        public readonly byte dex;
        public readonly byte luk;

        public CupetAutoStatusData(byte str, byte agi, byte vit, byte @int, byte dex, byte luk)
        {
            this.str = str;
            this.agi = agi;
            this.vit = vit;
            this.@int = @int;
            this.dex = dex;
            this.luk = luk;
        }
    }
}