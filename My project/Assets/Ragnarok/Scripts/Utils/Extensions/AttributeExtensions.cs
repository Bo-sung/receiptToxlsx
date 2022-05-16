namespace Ragnarok
{
    public static class AttributeExtensions
    {
        public static T GetAttribute<T>(this System.Type type)
                where T : System.Attribute
        {
            object[] attributes = type.GetCustomAttributes(true);

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is T)
                    return attributes[i] as T;
            }

            return null;
        }

        public static T GetAttribute<T>(this System.IConvertible enumField)
            where T : System.Attribute
        {
#if UNITY_EDITOR
            if (!enumField.GetType().IsEnum)
                return null;
#endif
            System.Reflection.MemberInfo[] infos = enumField.GetType().GetMember(enumField.ToString());

            for (int i = 0; i < infos.Length; i++)
            {
                object[] attributes = infos[i].GetCustomAttributes(typeof(T), false);

                foreach (var item in attributes)
                {
                    if (item is T)
                        return item as T;
                }
            }

            return null;
        }
    }
}