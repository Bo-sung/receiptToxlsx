using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Ragnarok
{
    public static class TemplateConverter
    {
        private const string DIRECTORY = "Assets/Ragnarok/Scripts/Utils/TemplateConverter/Templates/";

        private const string BASE = "Assets/Create/Template/";

        [MenuItem(BASE + "Script (C# Script)")]
        private static void CreateScript()
        {
            CreateScriptAsset("Script.txt", "NewScript.cs");
        }

        [MenuItem(BASE + "UI Script (C# Script)")]
        private static void CreateUiScript()
        {
            CreateScriptAsset("UiScript.txt", "NewUiScript.cs");
        }

        [MenuItem(BASE + "BattleEntry Script (C# Script)")]
        private static void CreateBattleEntryScript()
        {
            CreateScriptAsset("BattleEntry.txt", "NewBattleEntryScript.cs");
        }

        [MenuItem(BASE + "UIView Script (C# Script)")]
        private static void CreateUIViewScript()
        {
            CreateScriptAsset("UIView.txt", "NewUIViewScript.cs");
        }

        [MenuItem(BASE + "ViewPresenter Script (C# Script)")]
        private static void CreateViewPresenterScript()
        {
            CreateScriptAsset("ViewPresenter.txt", "Presenter.cs");
        }

        private static void CreateScriptAsset(string templateName, string destName)
        {
            string templatePath = string.Concat(DIRECTORY, templateName);

            if (!File.Exists(templatePath))
            {
                Debug.LogError($"템플릿이 존재하지 않습니다: path = {templatePath}");
                return;
            }
#if UNITY_2019_1_OR_NEWER
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, destName);
#else
            typeof(ProjectWindowUtil)
                .GetMethod("CreateScriptAsset", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .Invoke(null, new object[] { templatePath, destName });
#endif
        }

#region

        private const string CONVERT_TO_TEMPLATE_FORM = BASE + "Convert To Template Form";
        //private const string MAKE_C_SHARP_FROM_TEMPLATE = BASE + "Make C# From Template";

        /// <summary>
        /// Create a Form Template
        /// </summary>
        [MenuItem(CONVERT_TO_TEMPLATE_FORM, priority = -1000)]
        private static void ConvertToTemplateForm()
        {
            string[] paths = GetPaths(".cs");

            foreach (string sourcePath in paths)
            {
                string className = Path.GetFileNameWithoutExtension(sourcePath);

                string targetPath = EditorUtility.SaveFilePanel(string.Concat("Choose name for ", className), DIRECTORY, string.Concat(className, ".txt"), ".txt");

                if (string.IsNullOrEmpty(targetPath))
                    continue;

                string content = File.ReadAllText(sourcePath, System.Text.Encoding.UTF8);
                File.WriteAllText(targetPath, content.Replace(className, "#SCRIPTNAME#"), System.Text.Encoding.UTF8);

                Debug.Log(string.Concat("[Create] ", targetPath));

                string relativePath = PathUtils.ConvertToRelativePath(targetPath);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem(CONVERT_TO_TEMPLATE_FORM, validate = true, priority = -1000)]
        private static bool IsConvertToTemplateForm()
        {
            string[] paths = GetPaths(".cs");
            return paths.Length > 0;
        }

        //[MenuItem(MAKE_C_SHARP_FROM_TEMPLATE, priority = -999)]
        //private static void MakeCSharpFromTemplate()
        //{
        //    string[] paths = GetPaths(".txt");

        //    foreach (string sourcePath in paths)
        //    {
        //        string className = Path.GetFileNameWithoutExtension(sourcePath);
        //        className = className.Replace(".template", string.Empty);

        //        string targetPath = EditorUtility.SaveFilePanel(string.Concat("Choose name for ", className), sourcePath, string.Concat(className, ".cs"), "cs");

        //        if (string.IsNullOrEmpty(targetPath))
        //            continue;

        //        string scriptName = Path.GetFileNameWithoutExtension(targetPath);
        //        MakeCSharpFromTemplate(sourcePath, targetPath, scriptName);
        //    }

        //    AssetDatabase.Refresh();
        //}

        //[MenuItem(MAKE_C_SHARP_FROM_TEMPLATE, validate = true, priority = -999)]
        //private static bool IsMakeCSharpFromTemplate()
        //{
        //    string[] paths = GetPaths(".txt");
        //    return paths.Length > 0;
        //}

        //public static void MakeCSharpFromTemplate(string sourcePath, string targetPath, string scriptName, System.Func<string, string> replacer = null)
        //{
        //    if (string.IsNullOrEmpty(Path.GetExtension(targetPath)))
        //        targetPath = string.Concat(targetPath, ".cs");

        //    if (File.Exists(targetPath))
        //    {
        //        const string OVERWRITE_MESSAGE
        //        = "이미 존재하는 파일입니다. 덮어쓰겠습니까?" + "\n"
        //        + "{0}";

        //        string message = string.Format(OVERWRITE_MESSAGE, targetPath);

        //        if (!EditorUtility.DisplayDialog("Overwrite Message", message, "Yes", "No"))
        //            return;
        //    }

        //    string content = File.ReadAllText(sourcePath, System.Text.Encoding.UTF8);
        //    content = content.Replace("#SCRIPTNAME#", scriptName);

        //    if (replacer != null)
        //        content = replacer(content);

        //    File.WriteAllText(targetPath, content, System.Text.Encoding.UTF8);

        //    //string relativePath = PathUtils.ConvertToRelativePath(targetPath);
        //    //Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(relativePath);

        //    Debug.Log(string.Concat("[Create] ", targetPath));
        //}

        private static string[] GetPaths(string extension)
        {
            return Selection.objects
                .ToList() // To List
                .ConvertAll(AssetDatabase.GetAssetPath) // Convert To Path
                .Where(a => IsCheckExtension(a, extension)) // Check Extension
                .ToArray(); // To Array
        }

        private static bool IsCheckExtension(string path, string extension)
        {
            // Folder 일 경우는 제외
            if (AssetDatabase.IsValidFolder(path))
                return false;

            return Path.GetExtension(path).Equals(extension);
        }

#endregion
    }
}