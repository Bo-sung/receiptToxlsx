using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class ConvertJenkinsDetailLogWizard : ScriptableWizard
    {
        public const string TITLE = "젠킨스씨 Detail 로그 정리";

        [SerializeField]
        [TextArea(4, 100)]
        [Rename(displayName = "Detail 로그")]
        string text;

        void OnWizardUpdate()
        {
            isValid = !string.IsNullOrEmpty(text);
        }

        void OnWizardCreate()
        {
            var sb = StringBuilderPool.Get();
            using (StringReader reader = new StringReader(text))
            {
                while (true)
                {
                    string line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        break;

                    if (line.StartsWith("Commit"))
                        continue;

                    if (line.StartsWith("The file was"))
                        continue;

                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.Append(line);
                }
            }

            Debug.LogError(sb.Release());
        }
    }
}