using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class SkillPreviewWindow : EditorWindow, IHasCustomMenu
    {
        public const string CHARACTER_ANI_ASSET_PATH = "Assets/Ragnarok/Scripts/AssetManager/Editor/Container/Assets/CharacterAniContainer.asset";
        public const string SKILL_SETTING_ASSET_PATH = "Assets/Ragnarok/Scripts/AssetManager/Editor/Container/Assets/SkillSettingContainer.asset";
        public const string CHARACTER_ASSET_PATH = "Assets/Ragnarok/Scripts/AssetManager/Editor/Container/Assets/CharacterContainer.asset";
        public const string MONSTER_ASSET_PATH = "Assets/Ragnarok/Scripts/AssetManager/Editor/Container/Assets/MonsterContainer.asset";
        public const string BATTLE_EFFECT_ASSET_PATH = "Assets/Ragnarok/Scripts/AssetManager/Editor/Container/Assets/BattleEffectContainer.asset";
        public const string EFFECT_SOUND_ASSET_PATH = "Assets/Ragnarok/Scripts/AssetManager/Editor/Container/Assets/SfxContainer.asset";
        public const string PROJECTILE_SETTING_ASSET_PATH = "Assets/Ragnarok/Scripts/AssetManager/Editor/Container/Assets/ProjectileSettingContainer.asset";

        private const float FOOTER_HEIGHT = 20f;

        AnimationContainer aniContainer;
        SkillSettingContainer skillSettingContainer;
        PrefabContainer characterContainer;
        PrefabContainer monsterContainer;
        PrefabContainer battleEffectContainer;
        SoundContainer sfxContainer;
        ProjectileSettingContainer projectileSettingContainer;

        WindowSplitter splitter;
        SkillSettingTreeView treeView;
        UnitSelector unitSelector;
        SkillTimeControl timeControl;
        SkillSettingHandle settingHandle;

        private SkillSetting skillSetting;
        private GameObject unit, target;
        private float skillRange, skillArea;
        private bool isTargetPoint;
        private AnimationClip aniClip;

        private List<IEditorModePlayer> editorPlayerList;

        void OnEnable()
        {
            if (aniContainer == null)
                aniContainer = GetContainer<AnimationContainer>("Character Animation", CHARACTER_ANI_ASSET_PATH, 0.2f);

            if (skillSettingContainer == null)
                skillSettingContainer = GetContainer<SkillSettingContainer>("Skill Setting", SKILL_SETTING_ASSET_PATH, 0.3f);

            if (characterContainer == null)
                characterContainer = GetContainer<PrefabContainer>("Character", CHARACTER_ASSET_PATH, 0.4f);

            if (monsterContainer == null)
                monsterContainer = GetContainer<PrefabContainer>("Monster", MONSTER_ASSET_PATH, 0.5f);

            if (battleEffectContainer == null)
                battleEffectContainer = GetContainer<PrefabContainer>("Battle Effect", BATTLE_EFFECT_ASSET_PATH, 0.7f);

            if (sfxContainer == null)
                sfxContainer = GetContainer<SoundContainer>("Effect Sound", EFFECT_SOUND_ASSET_PATH, 0.9f);

            if (projectileSettingContainer == null)
                projectileSettingContainer = GetContainer<ProjectileSettingContainer>("Projectile Setting", PROJECTILE_SETTING_ASSET_PATH, 1f);

            if (splitter == null)
                splitter = new HorizontalSplitter(Repaint);

            if (treeView == null)
                treeView = new SkillSettingTreeView { onSelect = SelectSkillSetting };

            if (unitSelector == null)
                unitSelector = new UnitSelector { onSelectUnit = SelectUnit, onSelectTarget = SelectTarget, onSkillRangeChange = SetSkillRange, onSkillAreaChange = SetSkillArea };

            if (timeControl == null)
                timeControl = new SkillTimeControl { onSelect = SelectProperty, onTimeOver = ResetPlayer, };

            if (settingHandle == null)
                settingHandle = new SkillSettingHandle();

            if (editorPlayerList == null)
                editorPlayerList = new List<IEditorModePlayer>();

            EditorUtility.ClearProgressBar();

            SceneView.onSceneGUIDelegate += OnSceneGUI;
            EditorApplication.update += OnUpdate;

            const string TARGET_NAME = "NoviceM";
            unitSelector.SetTarget(GetCharacter(TARGET_NAME));
            unitSelector.SetSkillRange(skillRange: 800);
            unitSelector.SetSkillArea(isTargetPoint: true, skillArea: 200);
        }

        void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
            SceneView.onSceneGUIDelegate -= OnSceneGUI;

            skillSetting = null;
            skillRange = 0f;
            skillArea = 0f;
            isTargetPoint = false;
            aniClip = null;
            timeControl.IsPlaying = false;

            Destroy(unit);
            Destroy(target);

            AllRelease();

            aniContainer = null;
            skillSettingContainer = null;
            characterContainer = null;
            monsterContainer = null;
            battleEffectContainer = null;
            sfxContainer = null;
            projectileSettingContainer = null;
            splitter = null;
            treeView = null;
            unitSelector = null;
            timeControl = null;
            settingHandle = null;
            editorPlayerList = null;
        }

        void OnGUI()
        {
            splitter.OnGUI(position);

            Rect treeViewRect = splitter[0];
            treeViewRect.height -= FOOTER_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
            treeView.OnGUI(treeViewRect);

            Rect bottom = treeViewRect;
            bottom.y = treeViewRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            bottom.height = FOOTER_HEIGHT;
            using (new GUILayout.AreaScope(bottom))
            {
                using (new GUILayout.HorizontalScope())
                {
                    //if (GUILayout.Button("FixTimeFrame", GUILayout.ExpandWidth(true)))
                    //    FixTimeFrame();

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("추가", EditorStyles.miniButton))
                        Add();

                    if (GUILayout.Button("다시로드", EditorStyles.miniButton))
                        ReloadTreeView();
                }
            }

            EditorGUI.BeginChangeCheck();
            using (new GUILayout.AreaScope(splitter[1]))
            {
                unitSelector.Draw();

                using (new GUILayout.HorizontalScope())
                {
                    timeControl.Draw();
                    settingHandle.Draw();
                }
            }
            if (EditorGUI.EndChangeCheck())
                Repaint();
        }

        void Update()
        {
            if (unit == null)
                return;

            if (aniClip == null)
                return;

            if (timeControl.IsInvalid)
                return;

            aniClip.SampleAnimation(unit, timeControl.CurTime);

            if (!timeControl.IsPlaying)
                return;

            for (int i = 0; i < editorPlayerList.Count; i++)
            {
                editorPlayerList[i].Play(timeControl.CurTime);
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            GameObject pointUnit = isTargetPoint ? target : unit;

            if (pointUnit)
                Handles.DrawWireDisc(pointUnit.transform.position, Vector3.up, skillArea);
        }

        private void OnUpdate()
        {
            if (timeControl.IsInvalid)
                return;

            Repaint();
        }

        private void SelectSkillSetting(SkillSetting skillSetting)
        {
            this.skillSetting = skillSetting;
            timeControl.SetSkillSetting(this.skillSetting);

            AllRelease();

            if (skillSetting)
            {
                if (skillSetting.arrVfx != null)
                {
                    foreach (var item in skillSetting.arrVfx)
                    {
                        editorPlayerList.Add(new VfxEditorModePlayer(item));
                    }
                }

                if (skillSetting.arrProjectile != null)
                {
                    foreach (var item in skillSetting.arrProjectile)
                    {
                        editorPlayerList.Add(new ProjectileFxEditorModePlayer(item));
                    }
                }
            }

            bool isChangeUnit;
            if (skillSetting != null && skillSetting.id > 1000)
            {
                string monsterName = skillSetting.name.Replace("Attack", string.Empty).Replace("Skill01", string.Empty).Replace("Skill02", string.Empty).Replace("Skill03", string.Empty).Replace("Skill04", string.Empty);
                isChangeUnit = unitSelector.SetUnit(GetMonster(monsterName));

            }
            else
            {
                const string CHARACTER_NAME = "NoviceF";
                isChangeUnit = unitSelector.SetUnit(GetCharacter(CHARACTER_NAME));
            }

            if (!isChangeUnit)
                Initialize();
        }

        private GameObject GetMonster(string monsterName)
        {
            foreach (var item in monsterContainer.GetArray())
            {
                if (item.name.Equals(monsterName))
                    return item;
            }

            return null;
        }

        private void SelectUnit(GameObject go)
        {
            EditorUtility.DisplayProgressBar("Loading SkillPreviewWindow", nameof(SelectUnit), 1f);

            Destroy(unit);
            unit = Instantiate(go, Quaternion.identity);
            SetUnitPosition();

            Focus();

            Initialize();

            EditorUtility.ClearProgressBar();
        }

        private void SelectTarget(GameObject go)
        {
            EditorUtility.DisplayProgressBar("Loading SkillPreviewWindow", nameof(SelectTarget), 1f);

            Destroy(target);
            target = Instantiate(go, Quaternion.Euler(Vector3.up * 180f));
            SetUnitPosition();

            Focus();

            Initialize();

            EditorUtility.ClearProgressBar();
        }

        private void SetSkillRange(float skillRange)
        {
            this.skillRange = skillRange;
            SetUnitPosition();
        }

        private void SetSkillArea(bool isTargetPoint, float skillArea)
        {
            this.isTargetPoint = isTargetPoint;
            this.skillArea = skillArea;
            SceneView.RepaintAll();
        }

        private void SelectProperty(SerializedProperty property)
        {
            settingHandle.Select(property);

            GUI.FocusControl(string.Empty);
        }

        private void ResetPlayer()
        {
            for (int i = 0; i < editorPlayerList.Count; i++)
            {
                editorPlayerList[i].SetActive(false);
            }
        }

        private GameObject GetCharacter(string characterName)
        {
            foreach (var item in characterContainer.GetArray())
            {
                if (item.name.Equals(characterName))
                    return item;
            }

            return null;
        }

        private void Initialize()
        {
            timeControl.IsPlaying = false;
            settingHandle.Select(null);

            aniClip = GetAniClip();
            timeControl.SetMaxTime(aniClip == null ? 0f : aniClip.length);

            for (int i = 0; i < editorPlayerList.Count; i++)
            {
                editorPlayerList[i].SetUnit(unit);
                editorPlayerList[i].SetTarget(target);
            }

            ResetPlayer();
        }

        private AnimationClip GetAniClip()
        {
            if (unit == null)
                return null;

            if (skillSetting == null)
                return null;

            if (string.IsNullOrEmpty(skillSetting.aniName))
                return null;

            Animation animation = unit.GetComponent<Animation>();
            return animation.GetClip(skillSetting.aniName);
        }

        private void Destroy(GameObject go)
        {
            if (go == null)
                return;

            DestroyImmediate(go);
        }

        private T GetContainer<T>(string title, string assetPath, float progress) where T : Object
        {
            EditorUtility.DisplayProgressBar("Loading SkillPreviewWindow", title, progress);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private GameObject Instantiate(GameObject go, Quaternion rotation)
        {
            if (go == null)
                return null;

            GameObject clone = PrefabUtility.InstantiatePrefab(go) as GameObject;

            Transform transform = clone.transform;
            clone.transform.rotation = rotation;

            // 캐릭터
            if (clone.GetComponent<Animation>() == null)
            {
                Animation ani = clone.AddComponent<Animation>();
                foreach (var item in aniContainer.GetArray())
                {
                    ani.AddClip(item, item.name);
                }
            }

            return clone;
        }

        private void SetUnitPosition()
        {
            float halfSkillRange = skillRange * 0.5f;

            if (unit)
                unit.transform.position = Vector3.back * halfSkillRange;

            if (target)
                target.transform.position = Vector3.forward * halfSkillRange;
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("전체 다시로드"), false, Reload);
            //menu.AddItem(new GUIContent("Frame 자동 고침"), false, FixTimeFrame);
            menu.AddItem(new GUIContent("테스트 함수!! (주의)"), false, TestMethod);

            menu.ShowAsContext();
        }

        private void Reload()
        {
            OnDisable();
            OnEnable();
        }

        [System.Obsolete("클라 전용 테스트 함수")]
        private void TestMethod()
        {
            #region 정렬
            //System.Array.Sort(skillSettingContainer.GetArray(), (a, b) => a.id.CompareTo(b.id));
            #endregion

            #region 사용하지 않는 몬스터 세팅 제거
            //int id = 1000;
            //List<SkillSetting> deleteSettingList = new List<SkillSetting>();
            //foreach (var item in skillSettingContainer.array)
            //{
            //    if (item.id < id)
            //        continue;

            //    // Empty
            //    if (

            //        item.hitTime == 0
            //        && (item.arrVfx == null || item.arrVfx.Length == 0)
            //        && (item.arrSound == null || item.arrSound.Length == 0)
            //        && (item.arrProjectile == null || item.arrProjectile.Length == 0))
            //    {
            //        Debug.LogError(item.name);
            //        deleteSettingList.Add(item);
            //    }
            //}

            //for (int i = 0; i < deleteSettingList.Count; i++)
            //{
            //    ArrayUtility.Remove(ref skillSettingContainer.array, deleteSettingList[i]);
            //}

            //List<string> pathList = deleteSettingList.ConvertAll(a => AssetDatabase.GetAssetPath(a));
            //for (int i = 0; i < pathList.Count; i++)
            //{
            //    FileUtil.DeleteFileOrDirectory(pathList[i]);
            //}

            //foreach (var item in skillSettingContainer.array)
            //{
            //    if (item.id < id)
            //        continue;

            //    item.id = ++id;
            //}
            #endregion

            #region 로그
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //foreach (var item in skillSettingContainer.GetArray())
            //{
            //    foreach (var projectile in item.arrProjectile)
            //    {
            //        sb.AppendLine(projectile.ToString());
            //    }
            //}
            //Debug.LogError(sb.ToString());
            #endregion

            #region 데이터 구조 변경
            //foreach (var item in projectileSettingContainer.GetArray())
            //{
            //    if (item.arrStart != null && item.arrStart.Length > 0)
            //        item.start = item.arrStart[0];

            //    if (item.arrLoop != null && item.arrLoop.Length > 0)
            //        item.loop = item.arrLoop[0];

            //    if (item.arrEnd != null && item.arrEnd.Length > 0)
            //        item.end = item.arrEnd[0];

            //    EditorUtility.SetDirty(item);
            //}
            #endregion

            foreach (var item in monsterContainer.GetArray())
            {
                Animation ani = item.GetComponent<Animation>();
                if (ani == null)
                {
                    Debug.LogError("애니메이션이 존재하지 않음: item = " + item);
                    continue;
                }

                AnimationClip[] clips = AnimationUtility.GetAnimationClips(ani);
                foreach (var clip in clips)
                {
                    AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);

                    if (clip.name.Contains("Idle") || clip.name.Contains("Debuff") || clip.name.Contains("Run"))
                        clip.wrapMode = WrapMode.Loop;
                }

                EditorUtility.SetDirty(item);
            }

            EditorUtility.SetDirty(monsterContainer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [System.Obsolete("처음 세팅하는 경우가 아니면 사용하지 않는다")]
        private void FixTimeFrame()
        {
            if (aniClip == null)
                return;

            if (skillSetting == null)
                return;

            float aniTime = aniClip.length;
            skillSetting.hitTime = GetTime(skillSetting.hitTime, aniTime);

            if (skillSetting.arrVfx != null)
            {
                foreach (var item in skillSetting.arrVfx)
                {
                    item.time = GetTime(item.time, aniTime);
                    item.duration = GetTime(item.duration, aniTime);
                }
            }

            if (skillSetting.arrSound != null)
            {
                foreach (var item in skillSetting.arrSound)
                {
                    item.time = GetTime(item.time, aniTime);
                    item.duration = GetTime(item.duration, aniTime);
                }
            }

            if (skillSetting.arrProjectile != null)
            {
                foreach (var item in skillSetting.arrProjectile)
                {
                    item.time = GetTime(item.time, aniTime);
                    item.duration = GetTime(item.duration, aniTime);
                }
            }

            EditorUtility.SetDirty(skillSetting);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private int GetTime(float frame, float aniTime)
        {
            const float FPS = 10.0f;
            int maxFrame = Mathf.CeilToInt(aniTime * FPS);
            return (int)(aniTime * 100 * frame / maxFrame);
        }

        private bool Exist(string name)
        {
            foreach (var item in skillSettingContainer.GetArray())
            {
                if (item.name.Equals(name))
                    return true;
            }

            return false;
        }

        private void Add()
        {
            ScriptableWizard wizard = ScriptableWizard.DisplayWizard<CreateSkillSettingWizard>("Create New SkillSetting");
            wizard.minSize = wizard.maxSize = new Vector2(480f, 200f);
            wizard.Focus();
            wizard.Repaint();
            wizard.Show();
        }

        private void ReloadTreeView()
        {
            treeView = null;
            treeView = new SkillSettingTreeView { onSelect = SelectSkillSetting };
        }

        private void AllRelease()
        {
            editorPlayerList.ForEach(Release);
            editorPlayerList.Clear();
        }

        private void Release(IEditorModePlayer editorModePlayer)
        {
            editorModePlayer.Release();
        }

        [MenuItem("라그나로크/시뮬레이터/스킬 미리보기")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<SkillPreviewWindow>(title: "Skill Preview", focus: true);
            window.minSize = new Vector2(480f, 320f);
            window.Focus();
            window.Repaint();
            window.Show();
        }
    }
}