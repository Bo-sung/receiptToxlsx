namespace Ragnarok
{
    public struct BasicStatusOptionValue
    {
        public readonly int titleKey;
        public readonly string value;

        public BasicStatusOptionValue(int titleKey, string value)
        {
            this.titleKey = titleKey;
            this.value = value;
        }
    }
}