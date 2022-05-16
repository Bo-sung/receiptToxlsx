namespace Ragnarok
{
    public class EnumPopupAttribute : RenameAttribute
    {
        public readonly EnumPopupType enumPopupType;

        /// <summary>
        /// Toolbar 일 경우에만 사용
        /// </summary>
        public readonly string[] names;

        public bool isHideDisplayName;

        public EnumPopupAttribute(EnumPopupType enumPopupType, params string[] names)
        {
            this.enumPopupType = enumPopupType;
            this.names = names;
        }
    }
}