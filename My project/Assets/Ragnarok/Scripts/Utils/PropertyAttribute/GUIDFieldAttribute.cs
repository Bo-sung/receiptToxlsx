namespace Ragnarok
{
    public class GUIDFieldAttribute : RenameAttribute
    {
        public readonly GUIDFieldType guidFieldType;

        public readonly System.Type objectType;
        public readonly float width;
        public readonly float height;

        public readonly string browsePanelTitle;
        public readonly string extension;
        
        public bool isHideDisplayName;

        /// <summary>
        /// Object Field Type
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public GUIDFieldAttribute(System.Type objectType, float width = -1, float height = -1)
            : this(GUIDFieldType.ObjectField)
        {
            if (!objectType.IsSubclassOf(typeof(UnityEngine.Object)))
                throw new System.NullReferenceException("objectType is Null");

            if (!objectType.IsSubclassOf(typeof(UnityEngine.Object)))
                throw new System.ArgumentException(string.Concat("Type is Not Object's Subclass: type = ", objectType));

            this.objectType = objectType;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Browse Type
        /// </summary>
        /// <param name="browsePanelTitle"></param>
        /// <param name="extension"></param>
        public GUIDFieldAttribute(string browsePanelTitle, string extension)
            : this(GUIDFieldType.Browse)
        {
            this.browsePanelTitle = browsePanelTitle;
            this.extension = extension;
        }

        private GUIDFieldAttribute(GUIDFieldType guidFieldType)
        {
            this.guidFieldType = guidFieldType;
        }
    }
}