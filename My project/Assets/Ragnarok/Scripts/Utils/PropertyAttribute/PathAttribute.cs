namespace Ragnarok
{
    public class PathAttribute : RenameAttribute
    {
        public readonly PathType pathType;
        public readonly string extension;

        public string browsePanelTitle;
        public bool isHideDisplayName;

        public PathAttribute()
            : this(PathType.Folder)
        {
        }

        public PathAttribute(string extension)
            : this(PathType.File)
        {
            this.extension = extension;
        }

        private PathAttribute(PathType pathType)
        {
            this.pathType = pathType;
        }
    }
}