namespace Ragnarok
{
    public class RelativePathAttribute : RenameAttribute
    {
        public readonly PathType pathType;
        public readonly string extension;

        public string browsePanelTitle;
        public bool isHideDisplayName;

        public RelativePathAttribute()
            : this(PathType.Folder)
        {
        }

        public RelativePathAttribute(string extension)
            : this(PathType.File)
        {
            this.extension = extension;
        }

        private RelativePathAttribute(PathType pathType)
        {
            this.pathType = pathType;
        }
    }
}