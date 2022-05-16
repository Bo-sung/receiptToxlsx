namespace Ragnarok
{
    public class ToggleAttribute : RenameAttribute
    {
        public readonly ToggleType toggleType;

        /// <summary>
        /// Toolbar 또는 NoNameToolbar 일 경우에만 사용
        /// </summary>
        public string yes;

        /// <summary>
        /// Toolbar 또는 NoNameToolbar 일 경우에만 사용
        /// </summary>
        public string no;
        
        public ToggleAttribute(ToggleType toggleType)
        {
            this.toggleType = toggleType;
        }
    }
}