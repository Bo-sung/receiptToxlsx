using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public abstract class BaseSkillInfoDrawer : PropertyDrawer
    {
        PrefabContainer battleEffectContainer;
        SoundContainer soundContainer;
        ProjectileSettingContainer container;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SkillInfoAttribute myAttribute = attribute as SkillInfoAttribute;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(myAttribute.title, "TL Selection H2", GUILayout.ExpandWidth(false));

                switch (myAttribute.skillInfoType)
                {
                    case SkillInfoAttribute.SkillInfoType.Damage:
                        DrawDamage(property);
                        break;

                    case SkillInfoAttribute.SkillInfoType.Vfx:
                        DrawEffect(property);
                        break;

                    case SkillInfoAttribute.SkillInfoType.Sound:
                        DrawSound(property);
                        break;

                    case SkillInfoAttribute.SkillInfoType.Projectile:
                        DrawProjectile(property);
                        break;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0f;
        }

        protected abstract void DrawDamage(SerializedProperty property);
        protected abstract void DrawEffect(SerializedProperty property);
        protected abstract void DrawSound(SerializedProperty property);
        protected abstract void DrawProjectile(SerializedProperty property);

        protected Object GetBattleEffect(string name)
        {
            if (battleEffectContainer == null)
            {
                EditorUtility.DisplayProgressBar("Loading SkillInfoDrawer", "BattleEffect", 1f);
                battleEffectContainer = AssetDatabase.LoadAssetAtPath<PrefabContainer>(SkillPreviewWindow.BATTLE_EFFECT_ASSET_PATH);
                EditorUtility.ClearProgressBar();
            }

            foreach (var item in battleEffectContainer.GetArray())
            {
                if (item.name.Equals(name))
                    return item;
            }

            return null;
        }

        protected Object GetSound(string name)
        {
            if (soundContainer == null)
            {
                EditorUtility.DisplayProgressBar("Loading SoundInfoDrawer", "Sound", 1f);
                soundContainer = AssetDatabase.LoadAssetAtPath<SoundContainer>(SkillPreviewWindow.EFFECT_SOUND_ASSET_PATH);
                EditorUtility.ClearProgressBar();
            }

            foreach (var item in soundContainer.GetArray())
            {
                if (item.name.Equals(name))
                    return item;
            }

            return null;
        }

        protected Object GetProjectile(string name)
        {
            if (container == null)
            {
                EditorUtility.DisplayProgressBar("Loading ProjectileInfoDrawer", "Projectile", 1f);
                container = AssetDatabase.LoadAssetAtPath<ProjectileSettingContainer>(SkillPreviewWindow.PROJECTILE_SETTING_ASSET_PATH);
                EditorUtility.ClearProgressBar();
            }

            foreach (var item in container.GetArray())
            {
                if (item.name.Equals(name))
                    return item;
            }

            return null;
        }

        protected void ShowBattleEffectSelector(System.Action<string> action)
        {
            ShowSelectorWindow<BattleEffectSelectorWindow>(action);
        }

        protected void ShowSoundSelector(System.Action<string> action)
        {
            ShowSelectorWindow<SoundSelectorWindow>(action);
        }

        protected void ShowProjectileSelector(System.Action<string> action)
        {
            ShowSelectorWindow<ProjectileSelectorWindow>(action);
        }

        private void ShowSelectorWindow<T_Window>(System.Action<string> action)
            where T_Window : SelectorWindow
        {
            T_Window window = EditorWindow.GetWindow<T_Window>(title: "Selector", utility: true, focus: true);
            window.minSize = new Vector2(480f, 480f);
            window.onSelect = action;
            window.Focus();
            window.Repaint();
            window.Show();
        }

        private abstract class SelectorWindow : EditorWindow
        {
            public System.Action<string> onSelect;
        }

        private abstract class SelectorWindowWithTreeView<T> : SelectorWindow
            where T : UnityEditor.IMGUI.Controls.TreeView
        {
            T treeView;

            void OnEnable()
            {
                if (treeView == null)
                {
                    EditorUtility.DisplayProgressBar("Loading SelectWindow", "SkillInfo", 1f);
                    treeView = GetTreeView();
                    EditorUtility.ClearProgressBar();
                }
            }

            void OnGUI()
            {
                treeView.OnGUI(Screen.safeArea);
            }

            protected abstract T GetTreeView();
        }

        private sealed class BattleEffectSelectorWindow : SelectorWindowWithTreeView<PrefabTreeView>
        {
            protected override PrefabTreeView GetTreeView()
            {
                return new PrefabTreeView(SkillPreviewWindow.BATTLE_EFFECT_ASSET_PATH) { onSelect = Select };
            }

            private void Select(GameObject obj)
            {
                Close();
                onSelect?.Invoke(obj.name);
            }
        }

        private sealed class SoundSelectorWindow : SelectorWindowWithTreeView<SoundTreeView>
        {
            protected override SoundTreeView GetTreeView()
            {
                return new SoundTreeView { onSelect = Select };
            }

            private void Select(AudioClip obj)
            {
                Close();
                onSelect?.Invoke(obj.name);
            }
        }

        private sealed class ProjectileSelectorWindow : SelectorWindowWithTreeView<ProjectileTreeView>
        {
            protected override ProjectileTreeView GetTreeView()
            {
                return new ProjectileTreeView { onSelect = Select };
            }

            private void Select(ProjectileSetting obj)
            {
                Close();
                onSelect?.Invoke(obj.name);
            }
        }
    }
}