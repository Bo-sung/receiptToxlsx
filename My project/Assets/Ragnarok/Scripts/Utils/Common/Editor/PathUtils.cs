using System.IO;
using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public class PathUtils
    {
        public enum PathType
        {
            Relative,
            Absolute,
        }

        public static T GetObject<T>(string guid)
            where T : Object
        {
            string relativePath = GetPath(guid, PathType.Relative);
            return AssetDatabase.LoadAssetAtPath<T>(relativePath);
        }

        public static Object GetObject(string guid, System.Type type)
        {
            string relativePath = GetPath(guid, PathType.Relative);
            return AssetDatabase.LoadAssetAtPath(relativePath, type);
        }

        public static string GetGUID(Object obj)
        {
            string relativePath = GetPath(obj, PathType.Relative);
            return GetGUID(relativePath, PathType.Relative);
        }

        public static string GetGUID(string path, PathType pathType)
        {
            string relativePath = pathType == PathType.Relative ? path : ConvertToRelativePath(path);
            return AssetDatabase.AssetPathToGUID(relativePath);
        }

        public static string GetPath(Object obj, PathType pathType)
        {
            string relativePath = AssetDatabase.GetAssetPath(obj);
            return pathType == PathType.Relative ? relativePath : ConvertToAbsolutePath(relativePath);
        }

        public static string GetPath(string guid, PathType pathType)
        {
            string relativePath = AssetDatabase.GUIDToAssetPath(guid);
            return pathType == PathType.Relative ? relativePath : ConvertToAbsolutePath(relativePath);
        }

        public static string ConvertToRelativePath(string absolutePath)
        {
            string absoluteBasePath = GetAbsoluteBasePath();
            return absolutePath.Substring(absoluteBasePath.Length).Replace('\\', '/');
        }

        public static string ConvertToAbsolutePath(string relativePath)
        {
            string absoluteBasePath = GetAbsoluteBasePath();
            return string.Concat(absoluteBasePath, relativePath).Replace('/', '\\');
        }

        private static string GetAbsoluteBasePath()
        {
            return string.Concat(Directory.GetCurrentDirectory(), @"\");
        }

        [MenuItem("Assets/PathUtils/선택한 오브젝트의 Path 복사", false)]
        private static void CopyPath()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            GUIUtility.systemCopyBuffer = assetPath;
            Debug.Log($"Path 복사: {nameof(assetPath)} = {assetPath}");
        }

        [MenuItem("Assets/PathUtils/선택한 오브젝트의 Path 복사", true)]
        private static bool GetCopyPathValidate()
        {
            Object activeObject = Selection.activeObject;
            return activeObject != null && AssetDatabase.Contains(activeObject) && !Directory.Exists(AssetDatabase.GetAssetPath(activeObject));
        }

        [MenuItem("Assets/PathUtils/선택한 오브젝트의 Guid 복사", false)]
        private static void CopyGuid()
        {
            string guid = GetGUID(Selection.activeObject);
            GUIUtility.systemCopyBuffer = guid;
            Debug.Log($"Guid 복사: {nameof(guid)} = {guid}");
        }

        [MenuItem("Assets/PathUtils/선택한 오브젝트의 Guid 복사", true)]
        private static bool GetCopyGuidValidate()
        {
            Object activeObject = Selection.activeObject;
            return activeObject != null && AssetDatabase.Contains(activeObject) && !Directory.Exists(AssetDatabase.GetAssetPath(activeObject));
        }

        //public static string RemoveEnd(string text, string suffix)
        //{
        //    if (text.EndsWith(suffix))
        //        return text.Substring(0, text.Length - suffix.Length);

        //    return text;
        //}
    }
}