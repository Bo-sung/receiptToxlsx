using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public static class ResourceMenuItem
    {        
        private const string BASE_PATH = "라그나로크/테이블/";
        private const string OPEN_DATA_FOLDER_PATH = BASE_PATH + "테이블 폴더 열기";
        private const string DELETE_DATA_PATH = BASE_PATH + "테이블 삭제";

        [MenuItem(OPEN_DATA_FOLDER_PATH)]
        private static void OpenDataFolder()
        {
            string dataFolderPath = $"{Application.persistentDataPath}/Data";

            if (!Directory.Exists(dataFolderPath))
                Directory.CreateDirectory(dataFolderPath);

            System.Diagnostics.Process.Start(dataFolderPath); // 폴더 열기
        }

        [MenuItem(DELETE_DATA_PATH)]
        private static void DeleteData()
        {
            string dataFolderPath = $"{Application.persistentDataPath}/Data";

            if (!Directory.Exists(dataFolderPath))
                return;           

            FileUtil.DeleteFileOrDirectory(dataFolderPath); // 제거           
        }
    }
}