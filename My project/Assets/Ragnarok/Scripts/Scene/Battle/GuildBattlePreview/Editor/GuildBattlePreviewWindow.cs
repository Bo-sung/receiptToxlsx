using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GuildBattlePreviewWindow : EditorWindow, GuildBattleEntry.IEditorGuildBattleInput
    {
        [MenuItem("라그나로크/시뮬레이터/길드전 테스트", priority = 9000)]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<GuildBattlePreviewWindow>(title: "길드전", focus: true);
            window.minSize = new Vector2(720f, 620f);
            window.Focus();
            window.Repaint();
            window.Show();
        }

        private const int MAX_TURRET_CUPET_COUNT = 3;
        private const int MAX_BUFF_COUNT = 5;
        private const int MAX_SKILL_COUNT = 4;

        private bool isInitialize;
        private Vector2 scrollPosition;

        // <!-- EnemeyGuild --!>
        private CupetSettings[] enemyLeftCupets; // Enemey Turret (L)
        private CupetSettings[] enemyRightCupets; // Enemey Turret (R)
        private BuffSkillSettings[] enemyBuffs; // Enemey Buffs

        // <!-- MyGuild --!>
        private AgentSettings agent; // Agent
        private CupetSettings[] cupets; // Turret
        private BuffSkillSettings[] buffs; // Buffs

        private bool IsReady => EditorApplication.isPlaying && AssetManager.IsAllAssetReady;

        void OnEnable()
        {
            Initialize();
        }

        void OnDisable()
        {
            Dispose();
        }

        private void Initialize()
        {
            if (isInitialize)
                return;

            isInitialize = true;

            enemyLeftCupets = new CupetSettings[MAX_TURRET_CUPET_COUNT];
            for (int i = 0; i < enemyLeftCupets.Length; i++)
            {
                enemyLeftCupets[i] = new CupetSettings();
            }

            enemyRightCupets = new CupetSettings[MAX_TURRET_CUPET_COUNT];
            for (int i = 0; i < enemyRightCupets.Length; i++)
            {
                enemyRightCupets[i] = new CupetSettings();
            }

            enemyBuffs = new BuffSkillSettings[MAX_BUFF_COUNT];
            enemyBuffs[0] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill1);
            enemyBuffs[1] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill2);
            enemyBuffs[2] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill3);
            enemyBuffs[3] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill4);
            enemyBuffs[4] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill5);

            agent = new AgentSettings();
            cupets = new CupetSettings[MAX_TURRET_CUPET_COUNT];
            for (int i = 0; i < cupets.Length; i++)
            {
                cupets[i] = new CupetSettings();
            }

            buffs = new BuffSkillSettings[MAX_BUFF_COUNT];
            buffs[0] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill1);
            buffs[1] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill2);
            buffs[2] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill3);
            buffs[3] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill4);
            buffs[4] = new BuffSkillSettings(BasisGuildWarInfo.BuffSkill5);

            AssetManager.OnAllAssetReady += OnAllAssetReady;

            if (IsReady)
            {
                OnAllAssetReady();
            }
        }

        private void Dispose()
        {
            if (!isInitialize)
                return;

            isInitialize = false;

            enemyLeftCupets = null;
            enemyRightCupets = null;
            enemyBuffs = null;
            agent = null;
            cupets = null;
            buffs = null;

            AssetManager.OnAllAssetReady -= OnAllAssetReady;
        }

        void OnGUI()
        {
            using (new EditorGUI.DisabledGroupScope(!IsReady))
            {
                using (var gui = new GUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = gui.scrollPosition;

                    GUILayout.Label("[상대 길드]", EditorStyles.boldLabel);

                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        DrawTitle("큐펫 L", enemyLeftCupets);
                        foreach (var item in enemyLeftCupets)
                        {
                            item.Draw();
                        }
                    }

                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        DrawTitle("큐펫 R", enemyRightCupets);
                        foreach (var item in enemyRightCupets)
                        {
                            item.Draw();
                        }
                    }

                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        DrawTitle("버프", enemyBuffs);
                        foreach (var item in enemyBuffs)
                        {
                            item.Draw();
                        }
                    }

                    GUILayout.Space(20f);
                    GUILayout.Label("[내 길드]", EditorStyles.boldLabel);

                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        DrawTitle("동행동료", agent);
                        agent.Draw();
                    }

                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        DrawTitle("큐펫", cupets);
                        foreach (var item in cupets)
                        {
                            item.Draw();
                        }
                    }

                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        DrawTitle("버프", buffs);
                        foreach (var item in buffs)
                        {
                            item.Draw();
                        }
                    }

                    GUILayout.Space(20f);

                    bool canTest = IsValid(enemyLeftCupets) && IsValid(enemyRightCupets);
                    using (new EditorGUI.DisabledGroupScope(!canTest))
                    {
                        if (GUILayout.Button("테스트 시작", GUILayout.Height(32f)))
                        {
                            StartGuildBattle();
                        }
                    }
                }
            }
        }

        private void DrawTitle(string text, params ISettings[] settings)
        {
            if (IsValid(settings))
            {
                GUILayout.Label(text);
                return;
            }

            Color savedColor = GUI.contentColor;
            GUI.contentColor = Color.yellow;
            GUILayout.Label(text);
            GUI.contentColor = savedColor;
        }

        private bool IsValid(ISettings[] settings)
        {
            foreach (var item in settings)
            {
                if (item.IsValid())
                    return true;
            }

            return false;
        }

        private void StartGuildBattle()
        {
            BattleManager.Instance.StartBattle(BattleMode.GuildBattle, this, true);
        }

        void OnAllAssetReady()
        {
            foreach (var item in enemyLeftCupets)
            {
                item.Initialize();
            }

            foreach (var item in enemyRightCupets)
            {
                item.Initialize();
            }

            foreach (var item in enemyBuffs)
            {
                item.Initialize();
            }

            agent.Initialize();

            foreach (var item in cupets)
            {
                item.Initialize();
            }

            foreach (var item in buffs)
            {
                item.Initialize();
            }
        }

        #region Interface
        IMultiPlayerInput GuildBattleEntry.IEditorGuildBattleInput.GetAgent()
        {
            return agent.ToInput();
        }

        GuildBattleEntry.IEdiitorCupetInput[] GuildBattleEntry.IEditorGuildBattleInput.GetCupets()
        {
            BetterList<GuildBattleEntry.IEdiitorCupetInput> list = new BetterList<GuildBattleEntry.IEdiitorCupetInput>();
            foreach (var item in cupets)
            {
                if (!item.IsValid())
                    continue;

                list.Add(item.ToInput());
            }

            return list.ToArray();
        }

        GuildBattleEntry.IEditorBuffSkillInput[] GuildBattleEntry.IEditorGuildBattleInput.GetBuffs()
        {
            BetterList<GuildBattleEntry.IEditorBuffSkillInput> list = new BetterList<GuildBattleEntry.IEditorBuffSkillInput>();
            foreach (var item in buffs)
            {
                if (!item.IsValid())
                    continue;

                list.Add(item.ToInput());
            }

            return list.ToArray();
        }

        GuildBattleEntry.IEdiitorCupetInput[] GuildBattleEntry.IEditorGuildBattleInput.GetEnemyLeftCupets()
        {
            BetterList<GuildBattleEntry.IEdiitorCupetInput> list = new BetterList<GuildBattleEntry.IEdiitorCupetInput>();
            foreach (var item in enemyLeftCupets)
            {
                if (!item.IsValid())
                    continue;

                list.Add(item.ToInput());
            }

            return list.ToArray();
        }

        GuildBattleEntry.IEdiitorCupetInput[] GuildBattleEntry.IEditorGuildBattleInput.GetEnemyRightCupets()
        {
            BetterList<GuildBattleEntry.IEdiitorCupetInput> list = new BetterList<GuildBattleEntry.IEdiitorCupetInput>();
            foreach (var item in enemyRightCupets)
            {
                if (!item.IsValid())
                    continue;

                list.Add(item.ToInput());
            }

            return list.ToArray();
        }

        GuildBattleEntry.IEditorBuffSkillInput[] GuildBattleEntry.IEditorGuildBattleInput.GetEnemyBuffs()
        {
            BetterList<GuildBattleEntry.IEditorBuffSkillInput> list = new BetterList<GuildBattleEntry.IEditorBuffSkillInput>();
            foreach (var item in enemyBuffs)
            {
                if (!item.IsValid())
                    continue;

                list.Add(item.ToInput());
            }

            return list.ToArray();
        }
        #endregion

        #region Settings
        private interface ISettings
        {
            void Initialize();
            void Draw();
            bool IsValid();
        }

        private abstract class BaseSttings : ISettings
        {
            protected const string EMPTY = "비어있음";
            private const string ETC = "기타";

            protected class Tuple : System.IComparable<Tuple>
            {
                public int id;
                public string name;
                public string displayName;

                public Tuple(int id, string name, string displayName)
                {
                    this.id = id;
                    this.name = name;
                    this.displayName = displayName;
                }

                public int CompareTo(Tuple other)
                {
                    // "기타"로 시작할 경우
                    if (displayName.IndexOf(ETC) != -1)
                    {
                        if (other.displayName.IndexOf(ETC) != -1)
                            return name.CompareTo(other.name);

                        return 1;
                    }

                    return displayName.CompareTo(other.displayName);
                }
            }

            protected string GetInitialText(string name)
            {
                if (string.IsNullOrEmpty(name) || name.Length == 0)
                    return ETC;

                char ch = name[0];
                if (ch >= '가' && ch <= '깋') return "ㄱ";
                if (ch >= '까' && ch <= '낗') return "ㄲ";
                if (ch >= '나' && ch <= '닣') return "ㄴ";
                if (ch >= '다' && ch <= '딯') return "ㄷ";
                if (ch >= '따' && ch <= '띻') return "ㄸ";
                if (ch >= '라' && ch <= '맇') return "ㄹ";
                if (ch >= '마' && ch <= '밓') return "ㅁ";
                if (ch >= '바' && ch <= '빟') return "ㅂ";
                if (ch >= '빠' && ch <= '삫') return "ㅃ";
                if (ch >= '사' && ch <= '싷') return "ㅅ";
                if (ch >= '싸' && ch <= '앃') return "ㅆ";
                if (ch >= '아' && ch <= '잏') return "ㅇ";
                if (ch >= '자' && ch <= '짛') return "ㅈ";
                if (ch >= '짜' && ch <= '찧') return "ㅉ";
                if (ch >= '차' && ch <= '칳') return "ㅊ";
                if (ch >= '카' && ch <= '킿') return "ㅋ";
                if (ch >= '타' && ch <= '팋') return "ㅌ";
                if (ch >= '파' && ch <= '핗') return "ㅍ";
                if (ch >= '하' && ch <= '힣') return "ㅎ";

                return ETC;
            }

            public abstract void Initialize();

            public void Draw()
            {
                float savedLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 40f;
                using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(108f)))
                {
                    OnDraw();
                }
                EditorGUIUtility.labelWidth = savedLabelWidth;
            }

            public abstract bool IsValid();

            protected abstract void OnDraw();
        }

        private class CupetSettings : BaseSttings
        {
            private readonly BetterList<int> idList;
            private readonly BetterList<string> nameList;
            private readonly BetterList<string> displayList;

            public int id;
            public int rank;
            public int level;
            private int selected;

            private int minRank, maxRank;
            private int minLevel, maxLevel;

            public CupetSettings()
            {
                idList = new BetterList<int>();
                nameList = new BetterList<string>();
                displayList = new BetterList<string>();

                ResetData();
            }

            private void ResetData()
            {
                idList.Release();
                nameList.Release();
                displayList.Release();

                idList.Add(0);
                nameList.Add(EMPTY);
                displayList.Add(EMPTY);
            }

            public override void Initialize()
            {
                ResetData();

                minRank = 1;
                minLevel = 1;
                maxRank = BasisType.CUPET_MAX_RANK.GetInt();

                List<Tuple> list = new List<Tuple>();

                CupetDataManager dataRepo = CupetDataManager.Instance;
                foreach (int cupetId in dataRepo.GetCupetIDs())
                {
                    CupetData data = dataRepo.Get(cupetId);
                    string name = data.name_id.ToText(LanguageType.KOREAN);
                    string displayName = StringBuilderPool.Get().Append(GetInitialText(name)).Append('/').Append(name).Release();

                    list.Add(new Tuple(cupetId, name, displayName));
                }

                list.Sort(); // 정렬

                foreach (var item in list)
                {
                    idList.Add(item.id);
                    nameList.Add(item.name);
                    displayList.Add(item.displayName);
                }
            }

            public override bool IsValid()
            {
                return id > 0 && rank > 0 && level > 0;
            }

            protected override void OnDraw()
            {
                selected = EditorGUILayout.Popup(selected, displayList.ToArray());
                EditorGUI.Popup(GUILayoutUtility.GetLastRect(), selected, nameList.ToArray());

                int oldId = idList[selected];
                id = EditorGUILayout.IntField(oldId, EditorStyles.miniTextField);

                if (id != oldId)
                {
                    selected = Mathf.Max(0, idList.IndexOf(id));
                }

                int oldRank = rank;
                rank = MathUtils.Clamp(EditorGUILayout.IntField(nameof(rank), oldRank, EditorStyles.miniTextField), minRank, maxRank);

                if (rank != oldRank)
                {
                    maxLevel = BasisType.MAX_CUPET_LEVEL.GetInt(rank);
                }

                level = MathUtils.Clamp(EditorGUILayout.IntField(nameof(level), level, EditorStyles.miniTextField), minLevel, maxLevel);
            }

            public GuildBattleEntry.IEdiitorCupetInput ToInput()
            {
                if (IsValid())
                    return new EditorInput(id, rank, level);

                return null;
            }

            private class EditorInput : GuildBattleEntry.IEdiitorCupetInput
            {
                public int Id { get; }
                public int Rank { get; }
                public int Level { get; }

                public EditorInput(int id, int rank, int level)
                {
                    Id = id;
                    Rank = rank;
                    Level = level;
                }
            }
        }

        private class BuffSkillSettings : BaseSttings
        {
            private readonly BasisGuildWarInfo info;

            public int id;
            public int level;

            private string displayName;
            private int minLevel, maxLevel;

            public BuffSkillSettings(BasisGuildWarInfo info)
            {
                this.info = info;
                displayName = EMPTY;
            }

            public override void Initialize()
            {
                id = info.GetInt();
                minLevel = 0;
                maxLevel = BasisGuildWarInfo.BuffSkillMaxLevel.GetInt();

                SkillData data = SkillDataManager.Instance.Get(id, level: 1);
                displayName = data == null ? EMPTY : data.name_id.ToText(LanguageType.KOREAN);
            }

            protected override void OnDraw()
            {
                GUILayout.Label(displayName, EditorStyles.miniLabel);
                level = MathUtils.Clamp(EditorGUILayout.IntField(nameof(level), level, EditorStyles.miniTextField), minLevel, maxLevel);
            }

            public override bool IsValid()
            {
                return level > 0;
            }

            public GuildBattleEntry.IEditorBuffSkillInput ToInput()
            {
                if (IsValid())
                    return new EditorInput(id, level);

                return null;
            }

            private class EditorInput : GuildBattleEntry.IEditorBuffSkillInput
            {
                public int Id { get; }
                public int Level { get; }

                public EditorInput(int id, int level)
                {
                    Id = id;
                    Level = level;
                }
            }
        }

        private class AgentSettings : ISettings
        {
            private readonly JobSettings job;
            private readonly WeaponSettings weapon;
            private readonly SkillSettings[] skills;

            public AgentSettings()
            {
                job = new JobSettings();
                weapon = new WeaponSettings();
                skills = new SkillSettings[MAX_SKILL_COUNT];
                skills[0] = new SkillSettings();
                skills[1] = new SkillSettings();
                skills[2] = new SkillSettings();
                skills[3] = new SkillSettings();
            }

            public void Initialize()
            {
                job.Initialize();
                weapon.Initialize();
                foreach (var item in skills)
                {
                    item.Initialize();
                }
            }

            public void Draw()
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        job.Draw();
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        weapon.Draw();
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        foreach (var item in skills)
                        {
                            item.SetJobId(job.id); // Job세팅
                            item.Draw();
                        }
                    }
                }
            }

            public bool IsValid()
            {
                return job.IsValid();
            }

            public IMultiPlayerInput ToInput()
            {
                if (IsValid())
                {
                    int jobId = job.id;
                    int jobLevel = job.level;
                    JobData agentJobData = JobDataManager.Instance.Get(jobId);
                    int totalStatPoint = StatDataManager.Instance.GetTotalPoint(jobLevel);
                    int maxStatus = BasisType.MAX_STAT.GetInt(); // 500
                    JobData.StatValue basicStat = new JobData.StatValue(0);
                    JobData.StatValue maxStat = new JobData.StatValue(maxStatus);
                    short[] status = agentJobData.GetAutoStatGuidePoints(totalStatPoint, basicStat, maxStat);
                    return new EditorInput(job.id.ToEnum<Job>(), job.level, weapon.id, status, skills);
                }

                return null;
            }

            private class JobSettings : BaseSttings
            {
                private readonly BetterList<int> idList;
                private readonly BetterList<string> nameList;
                private readonly BetterList<string> displayList;

                public int id;
                public int level;
                private int selected;

                private int minLevel;
                private int maxLevel;

                public JobSettings()
                {
                    idList = new BetterList<int>();
                    nameList = new BetterList<string>();
                    displayList = new BetterList<string>();

                    ResetData();
                }

                private void ResetData()
                {
                    idList.Release();
                    nameList.Release();
                    displayList.Release();

                    idList.Add(0);
                    nameList.Add(EMPTY);
                    displayList.Add(EMPTY);
                }

                public override void Initialize()
                {
                    ResetData();

                    minLevel = 1;

                    List<Tuple> list = new List<Tuple>();

                    JobDataManager dataRepo = JobDataManager.Instance;
                    int maxGrade = dataRepo.GetMaxJobClass();
                    for (int i = 0; i < maxGrade; i++)
                    {
                        JobData[] arrData = dataRepo.GetJobs(i);
                        foreach (var item in arrData)
                        {
                            string name = item.name_id.ToText(LanguageType.KOREAN);
                            string initialText = StringBuilderPool.Get().Append(i + 1).Append("차").Release();
                            string displayName = StringBuilderPool.Get().Append(initialText).Append('/').Append(name).Release();
                            list.Add(new Tuple(item.id, name, displayName));
                        }
                    }

                    list.Sort(); // 정렬

                    foreach (var item in list)
                    {
                        idList.Add(item.id);
                        nameList.Add(item.name);
                        displayList.Add(item.displayName);
                    }
                }

                public override bool IsValid()
                {
                    return id > 0 && level > 0;
                }

                protected override void OnDraw()
                {
                    int oldSelected = selected;
                    selected = EditorGUILayout.Popup(oldSelected, displayList.ToArray());
                    EditorGUI.Popup(GUILayoutUtility.GetLastRect(), selected, nameList.ToArray());
                    id = idList[selected];

                    if (oldSelected != selected)
                    {
                        JobDataManager jobDataRepo = JobDataManager.Instance;
                        JobData data = jobDataRepo.Get(id);
                        int grade = data == null ? 0 : data.grade;
                        int nextGrade = Mathf.Min(grade + 1, jobDataRepo.GetMaxJobClass());
                        maxLevel = BasisType.JOB_MAX_LEVEL.GetInt(nextGrade);
                    }

                    level = MathUtils.Clamp(EditorGUILayout.IntField(nameof(level), level, EditorStyles.miniTextField), minLevel, maxLevel);
                }
            }

            private class WeaponSettings : BaseSttings
            {
                private readonly BetterList<int> idList;
                private readonly BetterList<string> nameList;
                private readonly BetterList<string> displayList;

                public int id;
                private int selected;

                public WeaponSettings()
                {
                    idList = new BetterList<int>();
                    nameList = new BetterList<string>();
                    displayList = new BetterList<string>();

                    ResetData();
                }

                private void ResetData()
                {
                    idList.Release();
                    nameList.Release();
                    displayList.Release();

                    idList.Add(0);
                    nameList.Add(EMPTY);
                    displayList.Add(EMPTY);
                }

                public override void Initialize()
                {
                    ResetData();

                    List<Tuple> list = new List<Tuple>();

                    ItemDataManager dataRepo = ItemDataManager.Instance;
                    foreach (var data in dataRepo.EntireItems)
                    {
                        if (data.ItemType != ItemType.Equipment)
                            continue;

                        if (data.GetEquipmentSlotType() != ItemEquipmentSlotType.Weapon)
                            continue;

                        string name = data.name_id.ToText(LanguageType.KOREAN);
                        string displayName = StringBuilderPool.Get().Append(GetInitialText(name)).Append('/').Append(name).Release();

                        list.Add(new Tuple(data.id, name, displayName));
                    }

                    list.Sort(); // 정렬

                    foreach (var item in list)
                    {
                        idList.Add(item.id);
                        nameList.Add(item.name);
                        displayList.Add(item.displayName);
                    }
                }

                public override bool IsValid()
                {
                    return id > 0;
                }

                protected override void OnDraw()
                {
                    int oldSelected = selected;
                    selected = EditorGUILayout.Popup(oldSelected, displayList.ToArray());
                    EditorGUI.Popup(GUILayoutUtility.GetLastRect(), selected, nameList.ToArray());

                    int oldId = idList[selected];
                    id = EditorGUILayout.IntField(oldId, EditorStyles.miniTextField);

                    if (id != oldId)
                    {
                        selected = Mathf.Max(0, idList.IndexOf(id));
                    }
                }
            }

            private class SkillSettings : BaseSttings
            {
                private readonly List<Tuple> list;
                private readonly BetterList<int> idList;
                private readonly BetterList<string> nameList;
                private readonly BetterList<string> displayList;

                public int id;
                public int level;

                private int selected;
                private int jobId;

                private int minLevel, maxLevel;

                public SkillSettings()
                {
                    list = new List<Tuple>();
                    idList = new BetterList<int>();
                    nameList = new BetterList<string>();
                    displayList = new BetterList<string>();

                    ResetData();
                }

                private void ResetData()
                {
                    list.Clear();
                    idList.Release();
                    nameList.Release();
                    displayList.Release();

                    idList.Add(0);
                    nameList.Add(EMPTY);
                    displayList.Add(EMPTY);
                }

                public override void Initialize()
                {
                    ResetData();

                    minLevel = 1;

                    Initialize(jobId);

                    list.Sort(); // 정렬

                    foreach (var item in list)
                    {
                        idList.Add(item.id);
                        nameList.Add(item.name);
                        displayList.Add(item.displayName);
                    }
                }

                public void SetJobId(int jobId)
                {
                    if (this.jobId == jobId)
                        return;

                    this.jobId = jobId;
                    selected = 0;
                    Initialize();
                }

                public override bool IsValid()
                {
                    return id > 0 && level > 0;
                }

                protected override void OnDraw()
                {
                    int oldSelected = selected;
                    selected = EditorGUILayout.Popup(oldSelected, displayList.ToArray());
                    EditorGUI.Popup(GUILayoutUtility.GetLastRect(), selected, nameList.ToArray());
                    id = idList[selected];

                    if (selected != oldSelected)
                    {
                        maxLevel = SkillDataManager.Instance.GetMaxLevel(id);
                    }

                    level = MathUtils.Clamp(EditorGUILayout.IntField(nameof(level), level, EditorStyles.miniTextField), minLevel, maxLevel);
                }

                private void Initialize(int jobId)
                {
                    JobDataManager jobDataRepo = JobDataManager.Instance;
                    JobData jobData = jobDataRepo.Get(jobId);
                    if (jobData == null)
                        return;

                    int grade = jobData.grade;
                    SkillDataManager skillDataRepo = SkillDataManager.Instance;
                    for (int i = 0; i < JobData.MAX_SKILL_COUNT; i++)
                    {
                        int skillId = jobData.GetSkillId(i);
                        if (skillId == 0)
                            continue;

                        SkillData skillData = skillDataRepo.Get(skillId, level: 1);
                        if (skillData == null)
                            continue;

                        string name = skillData.name_id.ToText(LanguageType.KOREAN);
                        string initialText = StringBuilderPool.Get().Append(grade + 1).Append("차").Release();
                        string displayName = StringBuilderPool.Get().Append(initialText).Append('/').Append(name).Release();
                        list.Add(new Tuple(skillId, name, displayName));
                    }

                    Initialize(jobData.previous_index);
                }
            }

            private class EditorInput : IMultiPlayerInput, BattleItemInfo.IValue
            {
                private readonly BetterList<SkillInfo> skillList;

                public bool IsExceptEquippedItems => true; // 장착아이템 옵션 제외 (더미)

                public int Cid => 0;
                public string Name { get; private set; }
                public byte Job { get; private set; }
                public byte Gender { get; private set; }
                public int Level { get; private set; }
                public int LevelExp => 0;
                public int JobLevel { get; private set; }
                public long JobLevelExp => 0;
                public int RebirthCount => 0;
                public int RebirthAccrueCount => 0;
                public int NameChangeCount => 0;
                public string CidHex => string.Empty;
                public int ProfileId => 0;

                public int Str { get; private set; }
                public int Agi { get; private set; }
                public int Vit { get; private set; }
                public int Int { get; private set; }
                public int Dex { get; private set; }
                public int Luk { get; private set; }
                public int StatPoint => 0;

                public int WeaponItemId { get; private set; }
                public BattleItemInfo.IValue ItemStatusValue => this;
                public int ArmorItemId => 0;
                public ElementType WeaponChangedElement => ElementType.None;
                public int WeaponElementLevel => 0;
                public ElementType ArmorChangedElement => ElementType.None;
                public int ArmorElementLevel => 0;
                public int TotalItemAtk => 0;
                public int TotalItemMatk => 0;
                public int TotalItemDef => 0;
                public int TotalItemMdef => 0;

                public SkillModel.ISkillValue[] Skills => skillList.ToArray();
                public SkillModel.ISlotValue[] Slots => skillList.ToArray();

                public CupetListModel.IInputValue[] Cupets => null;

                public IBattleOption[] BattleOptions => null;
                public IBattleOption[] GuildBattleOptions => null;

                public int GuildId => -1;
                public string GuildName => string.Empty;
                public int GuildEmblem => 0;
                public byte GuildPosition => 0;
                public int GuildCoin => 0;
                public int GuildQuestRewardCount => 0;
                public long GuildSkillBuyDateTime => 0L;
                public byte GuildSkillBuyCount => 0;
                public long GuildRejoinTime => 0L;

                public float PosX => 0f;
                public float PosY => 0f;
                public float PosZ => 0f;
                public string CostumeName => string.Empty;
                public byte State => 0;
                string TradeModel.IInputValue.PrivateStoreComment => string.Empty;
                PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => default;
                int IMultiPlayerInput.UID => 0;
                bool IMultiPlayerInput.HasMaxHp => false;
                int IMultiPlayerInput.MaxHp => 0;
                bool IMultiPlayerInput.HasCurHp => false;
                int IMultiPlayerInput.CurHp => 0;
                byte IMultiPlayerInput.TeamIndex => 0;
                public ItemInfo.IEquippedItemValue[] GetEquippedItems => null;
                public int[] EquipCostumeIds => null;

                public EditorInput(Job job, int level, int weaponId, short[] status, params SkillSettings[] skills)
                {
                    skillList = new BetterList<SkillInfo>();

                    Name = "Dummy";
                    Job = (byte)job;
                    Gender = (byte)(Random.Range(0, 2) + 1);
                    Level = level;
                    JobLevel = level;
                    WeaponItemId = weaponId;

                    Str = status[0];
                    Agi = status[1];
                    Vit = status[2];
                    Int = status[3];
                    Dex = status[4];
                    Luk = status[5];

                    for (int i = 0; i < skills.Length; i++)
                    {
                        if (!skills[i].IsValid())
                            continue;

                        skillList.Add(new SkillInfo(i, skills[i].id, skills[i].level));
                    }
                }

                DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
                {
                    return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.SharingCharacter, Cid, Level);
                }

                private class SkillInfo : SkillModel.ISkillValue, SkillModel.ISlotValue
                {
                    public bool IsInPossession => true;
                    public long SkillNo { get; private set; }
                    public int SkillId { get; private set; }
                    public int SkillLevel { get; private set; }
                    public int OrderId => 0;
                    public int ChangeSkillId => 0;

                    public long SlotNo => SkillNo;
                    public int SlotIndex => 0;
                    public bool IsAutoSkill => true;

                    public SkillInfo(long no, int id, int level)
                    {
                        SkillNo = no;
                        SkillId = id;
                        SkillLevel = level;
                    }
                }
            }
        }
        #endregion
    }
}