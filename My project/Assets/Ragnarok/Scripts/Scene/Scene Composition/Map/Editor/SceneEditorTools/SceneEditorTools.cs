using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public class SceneEditorTools
    {
        private const string TAG = nameof(SceneEditorTools);

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorUtility.ClearProgressBar();
        }

        public static void BuildNavMesh()
        {
            Run(new NavMeshBaker());
        }

        private static void Run(params ISceneEditable[] args)
        {
            List<string> sceneList = new List<string>();

            foreach (Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (!System.IO.Path.GetExtension(path).Equals(".unity"))
                    continue;

                sceneList.Add(path);
            }

            int maxCount = sceneList.Count;
            if (maxCount == 0)
            {
                EditorUtility.DisplayDialog(TAG, "선택된 씬이 없습니다", "확인");
                return;
            }

            sceneList.Sort((a, b) => a.CompareTo(b));

            float index = 0;
            for (int i = 0; i < maxCount; i++)
            {
                index = i + 1;

                if (EditorUtility.DisplayCancelableProgressBar(TAG, $"작업중... ({index}／{maxCount})", index / maxCount))
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(TAG, "작업 취소", "확인");
                    return;
                }

                Scene scene = EditorSceneManager.OpenScene(sceneList[i]);

                if (scene.IsValid())
                {
                    foreach (var item in args)
                    {
                        item.Execute(scene);
                    }
                }

                EditorSceneManager.SaveScene(scene);
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(TAG, "작업 완료", "확인");
        }
    }
}