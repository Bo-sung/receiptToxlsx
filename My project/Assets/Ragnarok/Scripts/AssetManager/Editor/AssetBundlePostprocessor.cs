using UnityEditor;
using System.Collections.Generic;
using MEC;

namespace Ragnarok
{
    class AssetBundlePostprocessor : AssetPostprocessor
    {
        private const string PATH = "Assets/ArtResources";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (EditorApplication.isPlaying)
                return;

            if (!AssetBundleMenuItems.IsAutoAssetBundleBuild)
                return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.name.Equals("PreLoad"))
                return;

            if (IsArtResourcesAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths))
                Timing.RunCoroutine(SynchronizeAssetBundle(), Segment.EditorUpdate);
        }

        private static bool IsArtResourcesAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (asset.StartsWith(PATH))
                    return true;
            }

            foreach (string asset in deletedAssets)
            {
                if (asset.StartsWith(PATH))
                    return true;
            }

            foreach (string asset in movedAssets)
            {
                if (asset.StartsWith(PATH))
                    return true;
            }

            foreach (string asset in movedFromAssetPaths)
            {
                if (asset.StartsWith(PATH))
                    return true;
            }

            return false;
        }

        private static IEnumerator<float> SynchronizeAssetBundle()
        {
            yield return Timing.WaitUntilFalse(IsCompiling); // 컴파일 대기
            yield return Timing.WaitForOneFrame; // 임포트 진행중의 에셋번들 빌드 방지

            AssetBundleMenuItems.SynchronizeAssetBundle();
        }

        private static bool IsCompiling()
        {
            return EditorApplication.isCompiling;
        }
    }
}