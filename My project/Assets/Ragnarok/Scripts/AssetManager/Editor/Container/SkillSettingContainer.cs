using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/SkillSetting")]
    public sealed class SkillSettingContainer : IntAssetContainer<SkillSetting>
    {
        protected override int ConvertKey(SkillSetting t)
        {
            return t.id;
        }

#if UNITY_EDITOR
        [ContextMenu("정리 (null 제거)")]
        private void Organize()
        {
            bool isDirty = false;
            Buffer<SkillSetting> buffer = new Buffer<SkillSetting>();
            for (int i = 0; i < array.Length; i++)
            {
                SkillSetting skillSetting = array[i];
                if (skillSetting == null)
                {
                    isDirty = true;
                    continue;
                }

                buffer.Add(skillSetting);
            }

            if (isDirty)
            {
                array = buffer.GetBuffer(isAutoRelease: true);
                UnityEditor.EditorUtility.SetDirty(this);

                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("완벽 그 자체");
            }
        }

        [ContextMenu("유효성 검사")]
        private void CheckValidData()
        {
            var effects = UnityEditor.AssetDatabase.LoadAssetAtPath<PrefabContainer>("Assets/Ragnarok/AssetBundles/BattleEffectContainer.asset").GetArray();
            var effectDirtyDic = new System.Collections.Generic.Dictionary<string, bool>(effects.Length, System.StringComparer.Ordinal);
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] == null)
                {
                    Debug.LogError($"[BattleEffectContainer]  i = {i}");
                    continue;
                }

                string effectName = effects[i].name;
                if (effectDirtyDic.ContainsKey(effectName))
                {
                    Debug.LogError($"이미 있는 Effect i = {i}, {nameof(effectName)} = {effectName}");
                    continue;
                }

                effectDirtyDic.Add(effectName, false);
            }

            var projectileListDic = new System.Collections.Generic.Dictionary<(int, int, string), System.Collections.Generic.List<string>>();

            var sb = StringBuilderPool.Get();
            var array = GetArray();
            for (int index = 0; index < array.Length; index++)
            {
                var item = array[index];
                if (item.arrVfx != null)
                {
                    for (int i = 0; i < item.arrVfx.Length; i++)
                    {
                        string vfxName = item.arrVfx[i].name;
                        if (effectDirtyDic.ContainsKey(vfxName))
                        {
                            effectDirtyDic[vfxName] = true; // isDirty
                            continue;
                        }

                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.Append("[index] ").Append(index).Append(" [id] ").Append(item.id).Append(" [name] ").Append(item.name);
                        sb.AppendLine();
                        sb.Append("vfx 음슴  [").Append(i).Append("]").Append(vfxName);
                    }
                }

                if (item.arrProjectile != null)
                {
                    projectileListDic.Add((index, item.id, item.name), new System.Collections.Generic.List<string>());
                    foreach (var projectile in item.arrProjectile)
                    {
                        projectileListDic[(index, item.id, item.name)].Add(projectile.name);
                    }
                }
            }

            var projectileSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileSettingContainer>("Assets/Ragnarok/AssetBundles/ProjectileSettingContainer.asset").GetArray();
            var projectileSettingDic = new System.Collections.Generic.Dictionary<string, ProjectileSetting>(projectileSettings.Length, System.StringComparer.Ordinal);
            var projectileSettingDirtyDic = new System.Collections.Generic.Dictionary<string, bool>(projectileSettings.Length, System.StringComparer.Ordinal);
            for (int i = 0; i < projectileSettings.Length; i++)
            {
                if (projectileSettings[i] == null)
                {
                    Debug.LogError($"[ProjectileSettingContainer]  i = {i}");
                    continue;
                }

                string projectileName = projectileSettings[i].name;
                if (effectDirtyDic.ContainsKey(projectileName))
                {
                    Debug.LogError($"이미 있는 projectile i = {i}, {nameof(projectileName)} = {projectileName}");
                    continue;
                }

                projectileSettingDic.Add(projectileName, projectileSettings[i]);
                projectileSettingDirtyDic.Add(projectileName, false);
            }

            foreach (var item in projectileListDic)
            {
                foreach (var projectileName in item.Value)
                {
                    if (projectileSettingDirtyDic.ContainsKey(projectileName))
                    {
                        projectileSettingDirtyDic[projectileName] = true; // isDirty
                    }

                    ProjectileSetting projectileSetting = projectileSettingDic[projectileName];
                    if (projectileSetting.start != null)
                    {
                        string vfxName = projectileSetting.start.name;
                        if (effectDirtyDic.ContainsKey(vfxName))
                        {
                            effectDirtyDic[vfxName] = true; // isDirty
                            continue;
                        }

                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.Append("[index] ").Append(item.Key.Item1).Append(" [id] ").Append(item.Key.Item2).Append(" [name] ").Append(item.Key.Item3);
                        sb.AppendLine();
                        sb.Append("start 음슴 ").Append(vfxName);
                    }

                    if (projectileSetting.loop != null)
                    {
                        string vfxName = projectileSetting.loop.name;
                        if (effectDirtyDic.ContainsKey(vfxName))
                        {
                            effectDirtyDic[vfxName] = true; // isDirty
                            continue;
                        }

                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.Append("[id] ").Append(item.Key);
                        sb.AppendLine();
                        sb.Append("loop 음슴 ").Append(vfxName);
                    }

                    if (projectileSetting.end != null)
                    {
                        string vfxName = projectileSetting.end.name;
                        if (effectDirtyDic.ContainsKey(vfxName))
                        {
                            effectDirtyDic[vfxName] = true; // isDirty
                            continue;
                        }

                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.Append("[id] ").Append(item.Key);
                        sb.AppendLine();
                        sb.Append("end 음슴 ").Append(vfxName);
                    }
                }
            }

            foreach (var item in effectDirtyDic)
            {
                if (item.Value)
                    continue;

                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append("불필요 effect ").Append(item.Key);
            }

            foreach (var item in projectileSettingDirtyDic)
            {
                if (item.Value)
                    continue;

                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append("불필요 projectile ").Append(item.Key);
            }

            Debug.LogError(sb.Release());
        }
#endif
    }
}