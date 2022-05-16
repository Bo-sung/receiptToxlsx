using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Ragnarok
{
    [UnityEditor.Callbacks.PostProcessBuild(1)]
    class TexturePostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            Preset preset = TextureSettingsCollection.FindPreset(assetPath);
            if (preset == null)
                return;

            if (preset.DataEquals(assetImporter))
                return;

            if (preset.ApplyTo(assetImporter))
            {
                Debug.Log("적용완료", preset);
            }
        }
    }
}