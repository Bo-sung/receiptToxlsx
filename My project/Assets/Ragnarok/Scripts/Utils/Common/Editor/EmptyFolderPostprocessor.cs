using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    /**
     * 어셋이 수정되었을 때
     * 빈 폴더일 경우에는 .keep 파일을 자동 생성한다.
     * 비어있지 않은 폴더의 경우에는 .keep 파일을 자동 제거한다.
     * 
     * Unity에서는 모든 파일을 (빈 폴더 포함) .meta 파일로 관리하지만
     * Git에서는 빈 폴더의 버전 관리는 하지 않으므로
     * 코드를 통해 임시파일(.keep)을 경로마다 추가한다.
     * 
     * git 터미널을 이용하여 .keep 파일 생성하는 방법
     * touch .keep
     * 
     * git 터미널을 이용하여 모든 빈 폴더를 검사해서 .keep 파일 생성
     * git clean -nd | sed s/'^Would remove '// | xargs -I{} touch "{}.keep"
     */
    [UnityEditor.Callbacks.PostProcessBuild(1)]
    class EmptyFolderPostprocessor : AssetPostprocessor
    {
        private const string EMPTY_FILE_NAME = ".keep";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            System.Array.ForEach(importedAssets, CheckPath);
            System.Array.ForEach(deletedAssets, CheckPath);
            System.Array.ForEach(movedAssets, CheckPath);
            System.Array.ForEach(movedFromAssetPaths, CheckPath);
        }

        /// <summary>
        /// 파일 체크
        /// </summary>
        private static void CheckPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            DirectoryInfo info = new DirectoryInfo(path);

            // 폴더의 경우
            if (info.Exists)
                CheckFolder(info.FullName); // 현재 폴더 체크

            CheckFolder(info.Parent.FullName); // 부모 폴더 체크
        }

        /// <summary>
        /// 폴더를 체크하여
        /// 빈 폴더의 경우에는 빈 파일을 생성하고
        /// 빈 폴더가 아닐 경우에는 빈 파일을 제거한다.
        /// </summary>
        private static void CheckFolder(string folderPath)
        {
            string path = Path.Combine(folderPath, EMPTY_FILE_NAME);

            if (IsEmptyFolder(folderPath))
            {
                using (FileStream stream = File.Create(path))
                {
                    Debug.Log($"[빈 파일 생성] {nameof(path)} = {path}");
                }
            }
            else
            {
                if (FileUtil.DeleteFileOrDirectory(path))
                {
                    Debug.Log($"[빈 파일 제거] {nameof(path)} = {path}");
                }
            }
        }

        /// <summary>
        /// 비어있는 폴더 확인
        /// </summary>
        private static bool IsEmptyFolder(string folderPath)
        {
            // 폴더가 존재하지 않을 경우
            if (!Directory.Exists(folderPath))
                return false;

            return Directory.GetFiles(folderPath).Length == 0 && Directory.GetDirectories(folderPath).Length == 0;
        }
    }
}