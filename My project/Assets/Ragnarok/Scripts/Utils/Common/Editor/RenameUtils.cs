using UnityEditor;

namespace Ragnarok
{
    public class RenameUtils
    {
        [MenuItem("라그나로크/이름 변경/큐펫 아이콘")]
        private static void Rename()
        {
            foreach (var item in Selection.objects)
            {
                string source = PathUtils.GetPath(item, PathUtils.PathType.Absolute);
                int index = source.LastIndexOf(@"\");
                string desc = source.Remove(index + 1, 3);
                //UnityEngine.Debug.LogError($"{nameof(source)} = {source}, {nameof(desc)} = {desc}");
                FileUtil.MoveFileOrDirectory(source, desc);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}