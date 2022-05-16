namespace Ragnarok
{
    public static class DumpExtensions
    {
        public static string GetDump(this object obj)
        {
#if UNITY_EDITOR
            var sb = new System.Text.StringBuilder();
            sb.Append(obj.ToString());

            const System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
            foreach (var field in obj.GetType().GetFields(bindingFlags))
            {
                sb.AppendLine();
                sb.Append("[").Append(field.Name).Append("] = ").Append(field.GetValue(obj));
            }

            foreach (var property in obj.GetType().GetProperties(bindingFlags))
            {
                sb.AppendLine();
                sb.Append("[").Append(property.Name).Append("] = ").Append(property.GetValue(obj, null));
            }

            return sb.ToString();
#else
            return obj.ToString();
#endif
        }
    }
}