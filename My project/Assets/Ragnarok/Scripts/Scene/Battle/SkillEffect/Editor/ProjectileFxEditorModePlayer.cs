using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class SfxEditorModePlayer : IEditorModePlayer
    {
        private class Setting
        {
            private readonly SkillSetting.Sound skillSound;
            private readonly ProjectileSetting.Sound projectileSound;

            public int time;
            public string name;
            public int duration;

            public Setting(SkillSetting.Sound skillSound)
            {
                this.skillSound = skillSound;
            }

            public Setting(ProjectileSetting.Sound projectileSound)
            {
                this.projectileSound = projectileSound;
            }
        }

        private readonly Setting setting;

        public SfxEditorModePlayer(SkillSetting.Sound setting)
        {
            this.setting = new Setting(setting);
        }

        public SfxEditorModePlayer(ProjectileSetting.Sound setting)
        {
            this.setting = new Setting(setting);
        }

        public void Release()
        {
        }

        public void SetActive(bool isActive)
        {

        }

        public void Play(float t)
        {
        }

        public void SetUnit(GameObject goUnit)
        {
        }

        public void SetTarget(GameObject goTarget)
        {
        }
    }

    public sealed class ProjectileFxEditorModePlayer : IEditorModePlayer
    {
        private readonly ProjectileSettingContainer container;
        private readonly SkillSetting.Projectile setting;

        private readonly List<IEditorModePlayer> players;
        ProjectileSetting projectileSetting;

        GameObject unit, target;

        double lastFrameEditorTime;
        float currentTime;
        bool isActive = true;

        public ProjectileFxEditorModePlayer(SkillSetting.Projectile setting)
        {
            EditorUtility.DisplayProgressBar("Loading ProjectileFxEditorModePlayer", "Projectile Setting", 1f);
            container = AssetDatabase.LoadAssetAtPath<ProjectileSettingContainer>(SkillPreviewWindow.PROJECTILE_SETTING_ASSET_PATH);
            EditorUtility.ClearProgressBar();

            this.setting = setting;

            players = new List<IEditorModePlayer>();

            Initialize();
        }

        private void Initialize()
        {
            string savedName = projectileSetting ? projectileSetting.name : string.Empty;

            if (!savedName.Equals(setting.name))
            {
                Release();
                Create();
            }
        }

        public void Release()
        {
            players.ForEach(Release);
            players.Clear();
        }

        public void SetActive(bool isActive)
        {
            if (this.isActive == isActive)
                return;

            this.isActive = isActive;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].SetActive(isActive);
            }

            if (isActive)
            {
                Initialize();

                lastFrameEditorTime = EditorApplication.timeSinceStartup;
                currentTime = 0f;
            }
        }

        public void Play(float t)
        {
            float activeTime = setting.time * 0.01f;

            bool isActive = t >= activeTime;
            SetActive(isActive);

            if (!isActive)
                return;

            double timeSinceStartup = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(timeSinceStartup - lastFrameEditorTime);
            lastFrameEditorTime = timeSinceStartup;
            currentTime += deltaTime;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].Play(currentTime);
            }
        }

        public void SetUnit(GameObject goUnit)
        {
            unit = goUnit;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].SetUnit(unit);
            }
        }

        public void SetTarget(GameObject goTarget)
        {
            target = goTarget;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].SetTarget(target);
            }
        }

        private void Create()
        {
            projectileSetting = GetProjectileSetting(setting.name);

            if (projectileSetting == null)
                return;

            if (projectileSetting.start != null)
                players.Add(new VfxEditorModePlayer(setting, projectileSetting.start));

            if (projectileSetting.loop != null)
                players.Add(new VfxEditorModePlayer(setting, projectileSetting.loop));

            if (projectileSetting.end != null)
                players.Add(new VfxEditorModePlayer(setting, projectileSetting.end));

            if (projectileSetting.sound != null)
                players.Add(new SfxEditorModePlayer(projectileSetting.sound));

            for (int i = 0; i < players.Count; i++)
            {
                players[i].SetUnit(unit);
                players[i].SetTarget(target);
            }
        }

        private void Release(IEditorModePlayer player)
        {
            player.Release();
        }

        private ProjectileSetting GetProjectileSetting(string name)
        {
            foreach (var item in container.GetArray())
            {
                if (item.name.Equals(name))
                    return item;
            }

            return null;
        }
    }
}