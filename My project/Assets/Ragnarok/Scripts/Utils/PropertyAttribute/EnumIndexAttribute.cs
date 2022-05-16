namespace Ragnarok
{
    public class EnumIndexAttribute : RenameAttribute
    {
        public readonly System.Type type;

        public EnumIndexAttribute(System.Type type)
        {
            if (!type.IsEnum)
                throw new System.ArgumentException("t is not Enum");

            this.type = type;
        }
    }
}