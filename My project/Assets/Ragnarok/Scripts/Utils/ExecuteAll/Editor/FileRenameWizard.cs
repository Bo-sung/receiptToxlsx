using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class FileRenameWizard : ScriptableWizard
    {
        public const string TITLE = "이름 일괄 변경";

        [SerializeField]
        string namePrefix;

        [SerializeField]
        int startNum = 1;

        [SerializeField]
        int zeroPadCount = 0;

        void OnWizardUpdate()
        {
            isValid = Selection.objects != null && Selection.objects.Length > 0;
        }

        protected override bool DrawWizardGUI()
        {
            EditorGUILayout.HelpBox("meta가 초기화 되므로 주의하세요 TT^TT", MessageType.Warning, true);
            return base.DrawWizardGUI();
        }

        void OnWizardCreate()
        {
            List<Object> list = new List<Object>(Selection.objects);
            list.Sort((a, b) => a.name.CompareTo(b.name));

            for (int i = 0; i < list.Count; i++)
            {
                string indexName = (startNum + i).ToString();

                if (zeroPadCount > 0)
                    indexName = indexName.PadLeft(zeroPadCount, '0');

                string path = PathUtils.GetPath(list[i], PathUtils.PathType.Absolute);
                FileInfo info = new FileInfo(path);
                string destiny = string.Concat(Path.Combine(info.DirectoryName, string.Concat(namePrefix, indexName)), info.Extension);

                FileUtil.MoveFileOrDirectory(path, destiny);
                EditorUtility.SetDirty(list[i]);
            }

            AssetDatabase.Refresh();
        }
    }
}