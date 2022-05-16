using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using System.Linq;

namespace Ragnarok
{
    public static class PresetApplyer
    {
        /// <summary>
        /// 기본 Preset 적용
        /// </summary>
        /// <param name="obj"></param>
        public static void Apply(Object obj)
        {
            Apply(obj, string.Empty);
        }

        /// <summary>
        /// 특정 이름에 해당하는 Preset 적용
        /// </summary>
        public static bool Apply(Object obj, string name)
        {
            Preset preset = GetPreset(obj, name);

            if (preset)
                return preset.ApplyTo(obj);

            return false;
        }

        /// <summary>
        /// 특정 이름에 해당하는 Preset 반환
        /// </summary>
        private static Preset GetPreset(Object obj, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                var defaults = Preset.GetDefaultPresetsForObject(obj);

                if (defaults.Length != 0)
                    return defaults.Last();                
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Preset"))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(assetPath);

                // 해당 Object에 적용 불가능
                if (!preset.CanBeAppliedTo(obj))
                    continue;

                // 이름이 같음
                if (preset.name.Equals(name))
                    return preset;
            }

            return null;
        }
    }
}