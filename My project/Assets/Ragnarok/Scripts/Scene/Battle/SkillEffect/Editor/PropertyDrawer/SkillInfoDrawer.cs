using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(SkillInfoAttribute))]
    public sealed class SkillInfoDrawer : BaseSkillInfoDrawer
    {
        protected override void DrawDamage(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property, new GUIContent("대미지 시간"));
        }

        protected override void DrawEffect(SerializedProperty property)
        {
            SerializedProperty spName = property.FindPropertyRelative(nameof(SkillSetting.Vfx.name));
            SerializedProperty spTime = property.FindPropertyRelative(nameof(SkillSetting.Vfx.time));
            SerializedProperty spDuration = property.FindPropertyRelative(nameof(SkillSetting.Vfx.duration));
            SerializedProperty spToTarget = property.FindPropertyRelative(nameof(SkillSetting.Vfx.toTarget));
            SerializedProperty spNode = property.FindPropertyRelative(nameof(SkillSetting.Vfx.node));
            SerializedProperty spIsAttach = property.FindPropertyRelative(nameof(SkillSetting.Vfx.isAttach));
            SerializedProperty spOffset = property.FindPropertyRelative(nameof(SkillSetting.Vfx.offset));
            SerializedProperty spRotate = property.FindPropertyRelative(nameof(SkillSetting.Vfx.rotate));

            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                Object go = EditorGUILayout.ObjectField(new GUIContent("이펙트 이름"), GetBattleEffect(spName.stringValue), typeof(GameObject), false);
                if (EditorGUI.EndChangeCheck())
                {
                    spName.stringValue = go ? go.name : string.Empty;
                    property.serializedObject.ApplyModifiedProperties();
                }

                /**********************************************************************
                 * Unicode 2026 (Ellipsis)
                 * https://en.wikipedia.org/wiki/Ellipsis#Computer_representations
                 **********************************************************************/

                if (GUILayout.Button("\u2026", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    GUI.FocusControl(string.Empty);

                    ShowBattleEffectSelector(name =>
                    {
                        spName.stringValue = name;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            EditorGUILayout.PropertyField(spTime, new GUIContent("이펙트 시작 시간"));
            EditorGUILayout.PropertyField(spDuration, new GUIContent("이펙트 지속 시간", "0일 경우 한 번 재생"));
            EditorGUILayout.PropertyField(spToTarget, new GUIContent("타겟팅 유무"));
            EditorGUILayout.PropertyField(spNode, new GUIContent("특정 노드에서 생성"));

            bool isEmptyNode = string.IsNullOrEmpty(spNode.stringValue);
            using (new EditorGUI.DisabledGroupScope(isEmptyNode))
            {
                EditorGUILayout.PropertyField(spIsAttach, new GUIContent("특정 노드에 연결"));
            }

            bool isAttach = spIsAttach.boolValue;
            using (new EditorGUI.DisabledGroupScope(isAttach))
            {
                EditorGUILayout.PropertyField(spOffset, new GUIContent("오프셋 정보"));
                EditorGUILayout.PropertyField(spRotate, new GUIContent("회전 정보"));
            }
        }

        protected override void DrawSound(SerializedProperty property)
        {
            SerializedProperty spName = property.FindPropertyRelative(nameof(SkillSetting.Sound.name));
            SerializedProperty spTime = property.FindPropertyRelative(nameof(SkillSetting.Sound.time));
            SerializedProperty spDuration = property.FindPropertyRelative(nameof(SkillSetting.Sound.duration));

            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                Object go = EditorGUILayout.ObjectField(new GUIContent("사운드 이름"), GetSound(spName.stringValue), typeof(AudioClip), false);
                if (EditorGUI.EndChangeCheck())
                {
                    spName.stringValue = go ? go.name : string.Empty;
                    property.serializedObject.ApplyModifiedProperties();
                }

                /**********************************************************************
                 * Unicode 2026 (Ellipsis)
                 * https://en.wikipedia.org/wiki/Ellipsis#Computer_representations
                 **********************************************************************/

                if (GUILayout.Button("\u2026", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    GUI.FocusControl(string.Empty);

                    ShowSoundSelector(name =>
                    {
                        spName.stringValue = name;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            EditorGUILayout.PropertyField(spTime, new GUIContent("사운드 시작 시간"));
            EditorGUILayout.PropertyField(spDuration, new GUIContent("사운드 지속 시간", "0일 경우 한 번 재생"));
        }

        protected override void DrawProjectile(SerializedProperty property)
        {
            SerializedProperty spName = property.FindPropertyRelative(nameof(SkillSetting.Projectile.name));
            SerializedProperty spTime = property.FindPropertyRelative(nameof(SkillSetting.Projectile.time));
            SerializedProperty spDuration = property.FindPropertyRelative(nameof(SkillSetting.Projectile.duration));
            SerializedProperty spNode = property.FindPropertyRelative(nameof(SkillSetting.Projectile.node));
            SerializedProperty spOffset = property.FindPropertyRelative(nameof(SkillSetting.Projectile.offset));
            SerializedProperty spRotate = property.FindPropertyRelative(nameof(SkillSetting.Projectile.rotate));


            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                Object go = EditorGUILayout.ObjectField(new GUIContent("발사체 이름"), GetProjectile(spName.stringValue), typeof(ProjectileSetting), false);
                if (EditorGUI.EndChangeCheck())
                {
                    spName.stringValue = go ? go.name : string.Empty;
                    property.serializedObject.ApplyModifiedProperties();
                }

                /**********************************************************************
                 * Unicode 2026 (Ellipsis)
                 * https://en.wikipedia.org/wiki/Ellipsis#Computer_representations
                 **********************************************************************/

                if (GUILayout.Button("\u2026", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    GUI.FocusControl(string.Empty);

                    ShowProjectileSelector(name =>
                    {
                        spName.stringValue = name;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            EditorGUILayout.PropertyField(spTime, new GUIContent("발사체 시작 시간"));
            EditorGUILayout.PropertyField(spDuration, new GUIContent("발사체 도달 시간"));
            EditorGUILayout.PropertyField(spNode, new GUIContent("특정 노드에서 생성"));
            EditorGUILayout.PropertyField(spOffset, new GUIContent("오프셋 정보"));
            EditorGUILayout.PropertyField(spRotate, new GUIContent("회전 정보"));
        }
    }
}