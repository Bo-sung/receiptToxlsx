using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TextureSettingsCollection : ScriptableObject
    {
        public const string TITLE = "TextureSettings";

        private const string BASE_PATH = "Assets/Editor Default Resources/";
        private const string PATH = "TextureSettings/TextureSettingsCollectionData.asset";
        private const string PRESET_PATH = BASE_PATH + "TextureSettings/Presets/{0:D3}.preset";

        [System.Serializable]
        public class Settings
        {
            [RelativePath] public string guid;
            public Preset preset;
        }

        public List<Settings> settingsList = new List<Settings>();

        /// <summary>
        /// 가져오기
        /// </summary>
        public static TextureSettingsCollection Get()
        {
            Object obj = EditorGUIUtility.Load(PATH);
            if (obj == null)
                return null;

            return obj as TextureSettingsCollection;
        }

        /// <summary>
        /// 생성
        /// </summary>
        public static TextureSettingsCollection Create()
        {
            const string CREATE_PATH = BASE_PATH + PATH;

            TextureSettingsCollection textureSettingsCollection = CreateInstance<TextureSettingsCollection>();
            CreateAsset(textureSettingsCollection, CREATE_PATH);
            return textureSettingsCollection;
        }

        /// <summary>
        /// 프리셋 찾기
        /// </summary>
        public static Preset FindPreset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return null;

            TextureSettingsCollection collection = Get();
            if (collection == null)
                return null;

            FileInfo fileInfo = new FileInfo(assetPath);
            if (!fileInfo.Exists)
                return null;

            string guid = PathUtils.GetGUID(fileInfo.Directory.FullName, PathUtils.PathType.Absolute);
            for (int i = 0; i < collection.settingsList.Count; i++)
            {
                if (string.Equals(guid, collection.settingsList[i].guid))
                    return collection.settingsList[i].preset;
            }

            return null;
        }

        /// <summary>
        /// 추가
        /// </summary>
        public static void Add(object obj)
        {
            if (!EditorUtility.DisplayDialog(TITLE, "세팅을 추가하시겠습니까?", "추가", "취소"))
                return;

            string guid = obj as string;
            if (string.IsNullOrEmpty(guid))
                return;

            TextureSettingsCollection collection = Get();

            if (collection == null)
                collection = Create();

            // Texture 타입이 아님
            string assetPath = PathUtils.GetPath(guid, PathUtils.PathType.Relative);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null)
                return;

            FileInfo fileInfo = new FileInfo(assetPath);
            if (!fileInfo.Exists)
                return;

            string directoryGuid = PathUtils.GetGUID(fileInfo.Directory.FullName, PathUtils.PathType.Absolute);
            if (string.IsNullOrEmpty(directoryGuid))
                return;

            Settings settings = collection.settingsList.Find(a => string.Equals(a.guid, directoryGuid));
            if (settings == null)
            {
                Preset preset = new Preset(textureImporter);
                string presetName = string.Concat(fileInfo.Directory.Parent.Parent.Name, "_", fileInfo.Directory.Parent.Name, "_", fileInfo.Directory.Name);
                string path = string.Format(PRESET_PATH, presetName);
                CreateAsset(preset, path);

                collection.settingsList.Add(new Settings { guid = directoryGuid, preset = preset, });
            }
            else
            {
                settings.preset.UpdateProperties(textureImporter);
            }

            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("세팅 추가", collection);
        }

        /// <summary>
        /// 제거
        /// </summary>
        public static void Remove(object obj)
        {
            if (!EditorUtility.DisplayDialog(TITLE, "세팅을 제거하시겠습니까?", "제거", "취소"))
                return;

            string guid = obj as string;
            if (string.IsNullOrEmpty(guid))
                return;

            TextureSettingsCollection collection = Get();
            if (collection == null)
                return;

            string assetPath = PathUtils.GetPath(guid, PathUtils.PathType.Relative);
            FileInfo fileInfo = new FileInfo(assetPath);
            if (!fileInfo.Exists)
                return;

            string directoryGuid = PathUtils.GetGUID(fileInfo.Directory.FullName, PathUtils.PathType.Absolute);
            Settings find = collection.settingsList.Find(a => string.Equals(a.guid, directoryGuid));
            if (find == null)
                return;

            if (!collection.settingsList.Remove(find))
                return;

            if (find.preset != null)
            {
                string presetPath = PathUtils.GetPath(find.preset, PathUtils.PathType.Relative);
                AssetDatabase.DeleteAsset(presetPath); // 프리셋 제거
            }

            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("세팅 제거", collection);
        }

        /// <summary>
        /// 세팅 적용
        /// </summary>
        public static void Apply(object obj)
        {
            if (!EditorUtility.DisplayDialog(TITLE, "이미지의 세팅을 적용하시겠습니까?", "적용", "취소"))
                return;

            string guid = obj as string;
            if (string.IsNullOrEmpty(guid))
                return;

            TextureSettingsCollection collection = Get();
            if (collection == null)
                return;

            string assetPath = PathUtils.GetPath(guid, PathUtils.PathType.Relative);
            Preset preset = FindPreset(assetPath);
            if (preset == null)
                return;

            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null)
                return;

            if (preset.DataEquals(textureImporter))
                return;

            if (!preset.ApplyTo(textureImporter))
                return;

            textureImporter.SaveAndReimport();
            Debug.Log("세팅 적용", collection);
        }

        /// <summary>
        /// 세팅 적용 (해당 폴더전체)
        /// </summary>
        public static void ApplyFolder(object obj)
        {
            if (!EditorUtility.DisplayDialog(TITLE, "해당 폴더의 모든 이미지의 세팅을 적용하시겠습니까?", "일괄적용", "취소"))
                return;

            string guid = obj as string;
            if (string.IsNullOrEmpty(guid))
                return;

            TextureSettingsCollection collection = Get();
            if (collection == null)
                return;

            string assetPath = PathUtils.GetPath(guid, PathUtils.PathType.Relative);
            Preset preset = FindPreset(assetPath);
            if (preset == null)
                return;

            FileInfo fileInfo = new FileInfo(assetPath);
            if (!fileInfo.Exists)
                return;

            foreach (FileInfo childFileInfo in fileInfo.Directory.EnumerateFiles())
            {
                string childAssetPath = PathUtils.ConvertToRelativePath(childFileInfo.FullName);
                TextureImporter textureImporter = AssetImporter.GetAtPath(childAssetPath) as TextureImporter;
                if (textureImporter == null)
                    continue;

                if (preset.DataEquals(textureImporter))
                    continue;

                if (!preset.ApplyTo(textureImporter))
                    continue;

                textureImporter.SaveAndReimport();
            }

            Debug.Log("세팅 적용 (해당 폴더전체)", collection);
        }

        /// <summary>
        /// 프리셋 위치 확인
        /// </summary>
        public static void SelectPreset(object obj)
        {
            string guid = obj as string;
            if (string.IsNullOrEmpty(guid))
                return;

            string assetPath = PathUtils.GetPath(guid, PathUtils.PathType.Relative);
            Preset preset = FindPreset(assetPath);
            if (preset == null)
                return;

            Selection.activeObject = preset;
            EditorGUIUtility.PingObject(preset);
        }

        /// <summary>
        /// 관리자 데이터 위치 확인
        /// </summary>
        public static void SelectCollectionData()
        {
            TextureSettingsCollection collection = Get();
            if (collection == null)
                return;

            Selection.activeObject = collection;
            EditorGUIUtility.PingObject(collection);
        }

        /// <summary>
        /// 모두 적용
        /// </summary>
        public static void AllApply()
        {
            if (!EditorUtility.DisplayDialog(TITLE, "모든 이미지의 세팅을 적용하시겠습니까?", "일괄적용", "취소"))
                return;

            TextureSettingsCollection collection = Get();
            if (collection == null)
                return;

            for (int i = 0; i < collection.settingsList.Count; i++)
            {
                if (collection.settingsList[i] == null || collection.settingsList[i].preset == null)
                    continue;

                string path = PathUtils.GetPath(collection.settingsList[i].guid, PathUtils.PathType.Relative);
                DirectoryInfo info = new DirectoryInfo(path);
                if (!info.Exists)
                    continue;

                foreach (FileInfo childFileInfo in info.EnumerateFiles())
                {
                    string childAssetPath = PathUtils.ConvertToRelativePath(childFileInfo.FullName);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(childAssetPath) as TextureImporter;
                    if (textureImporter == null)
                        continue;

                    if (collection.settingsList[i].preset.DataEquals(textureImporter))
                        continue;

                    if (!collection.settingsList[i].preset.ApplyTo(textureImporter))
                        continue;

                    textureImporter.SaveAndReimport();
                }
            }

            Debug.Log("세팅 모두 적용 (모든 세팅전체)", collection);
        }

        private static void CreateAsset(Object obj, string path)
        {
            // Create Directory
            string directoryPath = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directoryPath);

            // Create Asset
            AssetDatabase.CreateAsset(obj, path);

            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [ContextMenu("정리")]
        private void Organize()
        {
            if (settingsList.Count == 0)
                return;

            settingsList.RemoveAll(a => a.preset == null); // 비어있는 preset 제거
            settingsList.Sort((x, y) => x.preset.name.CompareTo(y.preset.name)); // 이름순으로 정렬
        }
    }
}