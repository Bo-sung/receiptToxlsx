namespace Ragnarok
{
    public sealed class AdventureGroup : AdventureGroupElement.IInput
    {
        public static readonly AdventureGroup GROUP_1 = new AdventureGroup(1, "Ui_Texture_Adventure_Group_1", LocalizeKey._48284, LocalizeKey._48285);
        public static readonly AdventureGroup GROUP_2 = new AdventureGroup(2, "Ui_Texture_Adventure_Group_2", LocalizeKey._48286, LocalizeKey._48287);

        public static readonly AdventureGroup[] DATA_LIST = new AdventureGroup[] { GROUP_1, GROUP_2, };

        public int GroupId { get; }
        public string TextureName { get; }
        public int LocalKey { get; }
        public int DescLocalKey { get; }
        public bool IsSelected { get; private set; }

        private AdventureGroup(int groupId, string textureName, int localKey, int descLocalKey)
        {
            GroupId = groupId;
            TextureName = textureName;
            LocalKey = localKey;
            DescLocalKey = descLocalKey;
        }

        public void SetCurrentGroup(int currentGroup)
        {
            IsSelected = GroupId == currentGroup;
        }
    }
}