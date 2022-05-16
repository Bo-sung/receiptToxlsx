//#define USE_MENU_ITEM

using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Ragnarok
{
    public sealed class DataImport
    {
        private const string TAG = "Data Import";

#if USE_MENU_ITEM
        [MenuItem("라그나로크/데이터/Import CharacterSkill")]
#endif
        private static void ImportCharacterSkill()
        {
            string path = EditorUtility.OpenFilePanel("Import CharacterSkill", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "bin");

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog(TAG, "사용자가 작업을 취소하였습니다", "확인");
                return;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                using (CustomReader reader = new CustomReader(stream))
                {
                    SkillFxDescription[] descriptions = reader.ReadSkillFxDescriptions(); // read
                    foreach (var item in descriptions)
                    {
                        Create(item.skillName, item, isOverwrite: true);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ShowSuccessMessage();
        }

#if USE_MENU_ITEM
        [MenuItem("라그나로크/데이터/Import MonsterSkill")]
#endif
        private static void ImportMonsterSkill()
        {
            string path = EditorUtility.OpenFilePanel("Import MonsterSkill", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "bin");

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog(TAG, "사용자가 작업을 취소하였습니다", "확인");
                return;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                using (CustomReader reader = new CustomReader(stream))
                {
                    MonsterFxDescription[] descriptions = reader.ReadMonsterFxDescriptions(); // read

                    int index = 1000;

                    foreach (var item in descriptions)
                    {
                        Create(string.Concat(item.monsterName, "Attack"), item.normalAttack, ++index, "Attack", isOverwrite: true);
                        Create(string.Concat(item.monsterName, "Skill01"), item.skill1, ++index, "Skill01", isOverwrite: true);
                        Create(string.Concat(item.monsterName, "Skill02"), item.skill2, ++index, "Skill02", isOverwrite: true);
                        //Create(string.Concat(item.monsterName, "Die"), item.die, ++index, "Die", isOverwrite: true);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ShowSuccessMessage();
        }

#if USE_MENU_ITEM
        [MenuItem("라그나로크/데이터/Import Projectile")]
#endif
        private static void ImportProjectile()
        {
            string path = EditorUtility.OpenFilePanel("Import Projectile", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "bin");

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog(TAG, "사용자가 작업을 취소하였습니다", "확인");
                return;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                using (CustomReader reader = new CustomReader(stream))
                {
                    ProjectileFxDescription[] descriptions = reader.ReadProjectileFxDescriptions(); // read
                    foreach (var item in descriptions)
                    {
                        Create(item.projectileName, item, isOverwrite: true);
                    }
                }
            }

            AssetDatabase.Refresh();
            ShowSuccessMessage();
        }

        private static void Create(string name, SkillFxDescription description, int fixedIndex = -1, string fixedAniName = null, bool isOverwrite = false)
        {
            if (name.Length < 2)
                return;

            const string REMOVE_TEXT = "Attack";
            int index = name.IndexOf(REMOVE_TEXT);

            if (index >= 0)
            {
                index += REMOVE_TEXT.Length;

                if (index < name.Length)
                    name = name.Remove(index);
            }

            string path = $"Assets/Ragnarok/Data/SkillSetting/{name}.asset";

            // 이미 존재할 경우
            if (File.Exists(path))
            {
                if (isOverwrite)
                {
                    AssetDatabase.DeleteAsset(path);
                }
                else
                {
                    Debug.LogError($"이미 존재하는 데이터가 있습니다: name = {name}");
                    return;
                }
            }

            SkillSetting setting = ScriptableObject.CreateInstance<SkillSetting>();
            AssetDatabase.CreateAsset(setting, path);

            setting.id = fixedIndex > 0 ? fixedIndex : description.skillId;
            setting.aniName = !string.IsNullOrEmpty(fixedAniName) ? fixedAniName : description.animationName;

            int hitCount = 0;
            foreach (var item in description.animationEvents)
            {
                switch (item.type)
                {
                    case AnimationEventDescription.AnimationEventType.Vfx:
                        if (setting.arrVfx == null)
                            setting.arrVfx = new SkillSetting.Vfx[0];

                        ArrayUtility.Add(ref setting.arrVfx, item.ToVfx());
                        break;

                    case AnimationEventDescription.AnimationEventType.Sfx:
                        if (setting.arrSound == null)
                            setting.arrSound = new SkillSetting.Sound[0];

                        ArrayUtility.Add(ref setting.arrSound, item.ToSound());
                        break;

                    case AnimationEventDescription.AnimationEventType.Hit:
                        ++hitCount;

                        if (hitCount == 1)
                            setting.hitTime = item.frame;
                        break;

                    case AnimationEventDescription.AnimationEventType.LaunchProjectile:
                        if (setting.arrProjectile == null)
                            setting.arrProjectile = new SkillSetting.Projectile[0];

                        ArrayUtility.Add(ref setting.arrProjectile, item.ToProjectile());
                        break;
                }
            }

            if (hitCount != 1)
                Debug.LogError($"Hit Count 체크: name = {name}, hitCount = {hitCount}");

            setting.SortByFrame(); // 프레임 수로 정렬

            EditorUtility.SetDirty(setting);
        }

        private static void Create(string name, ProjectileFxDescription description, bool isOverwrite)
        {
            string path = $"Assets/Ragnarok/Data/ProjectileSetting/{name}.asset";

            // 이미 존재할 경우
            if (File.Exists(path))
            {
                if (isOverwrite)
                {
                    AssetDatabase.DeleteAsset(path);
                }
                else
                {
                    Debug.LogError($"이미 존재하는 데이터가 있습니다: name = {name}");
                    return;
                }
            }

            ProjectileSetting setting = ScriptableObject.CreateInstance<ProjectileSetting>();
            AssetDatabase.CreateAsset(setting, path);

            //description.pathType;

            foreach (var item in description.fxElements)
            {
                switch (item.fxType)
                {
                    case ProjectileFxElement.ProjectileFxType.Vfx:
                        switch (item.phaseType)
                        {
                            case ProjectileFxElement.ProjectilePhaseType.Start:
                                setting.start = item.ToStart();
                                break;

                            case ProjectileFxElement.ProjectilePhaseType.Loop:
                                setting.loop = item.ToLoop();
                                break;

                            case ProjectileFxElement.ProjectilePhaseType.End:
                                setting.end = item.ToEnd();
                                break;
                        }
                        break;

                    case ProjectileFxElement.ProjectileFxType.Sfx:
                        setting.sound = item.ToSound();
                        break;
                }
            }

            EditorUtility.SetDirty(setting);
        }

        private static void ShowSuccessMessage()
        {
            if (EditorUtility.DisplayDialog(TAG, "Import 성공하였습니다", "확인"))
                EditorUtility.FocusProjectWindow();
        }

        private class SkillFxDescription
        {
            public int skillId;
            public string skillName;
            public string animationName;
            public AnimationEventDescription[] animationEvents;
        }

        private class MonsterFxDescription
        {
            public string monsterName;
            public SkillFxDescription normalAttack;
            public SkillFxDescription skill1;
            public SkillFxDescription skill2;
            public SkillFxDescription die;
        }

        private class ProjectileFxDescription
        {
            public string projectileName;
            public string pathType;
            public ProjectileFxElement[] fxElements;
        }

        private class AnimationEventDescription
        {
            public enum AnimationEventType
            {
                Vfx,
                Sfx,
                Hit,
                LaunchProjectile,
            }

            public AnimationEventType type;
            public int id; // unique id
            public int frame;
            public string vfxName;
            public string sfxName;
            public string projectileName;
            public bool toTarget;
            public bool isLoop;
            public float duration; // frames (IsLoop == true 일때만)
            public bool isAttach; //
            public string node; // ToTarget 여부에 따라서 자신 또는 상대의 노드가 된다.
            public Vector3 offset; // Node로 부터 offset
            public Vector3 offsetRot; // Node로 부터 rotation offset

            public SkillSetting.Vfx ToVfx()
            {
                return new SkillSetting.Vfx
                {
                    time = frame,
                    name = vfxName,
                    duration = (int)(duration),
                    toTarget = toTarget,
                    node = node,
                    isAttach = isAttach,
                    offset = offset,
                    rotate = offsetRot,
                };
            }

            public SkillSetting.Sound ToSound()
            {
                return new SkillSetting.Sound
                {
                    time = frame,
                    name = sfxName,
                    duration = (int)(duration),
                };
            }

            public SkillSetting.Projectile ToProjectile()
            {
                return new SkillSetting.Projectile
                {
                    time = frame,
                    name = projectileName,
                    duration = (int)(duration),
                    node = node,
                    offset = offset,
                    rotate = offsetRot,
                };
            }
        }

        private class ProjectileFxElement
        {
            public enum ProjectileFxType
            {
                Vfx,
                Sfx,
            }

            public enum ProjectilePhaseType
            {
                Start, // 발사체 생성
                Loop, // 발사체 이동
                End, // 발사체 소멸
            }

            public ProjectileFxType fxType;
            public ProjectilePhaseType phaseType;
            public int overlapFrame; // 선행 PhaseType의 종료와 겹치는 Frame.
            public int destroyDelayFrame; // ProjectilePhaseType.Loop일 경우 destroy 지연시간(대부분 0이다)
            public string vfxName;
            public string sfxName;
            public string assetBundleName; // VfxName, SfxName이 포함된 AssetBundle 이름
            public string pathClass; // 
            public AnimationCurve heightCurve;
            public AnimationCurve moveCurve;
            public AnimationCurve sideDirCurve; // Left(-)/Right(+) side direction
            public bool isAttach; // 
            public string node; // ToTarget 여부에 따라서 자신 또는 상대의 노드가 된다.
            public Vector3 offset; // Node로 부터 offset
            public Vector3 offsetRot; // Node로 부터 rotation offset

            public ProjectileSetting.Start ToStart()
            {
                return new ProjectileSetting.Start
                {
                    name = vfxName,
                };
            }

            public ProjectileSetting.Loop ToLoop()
            {
                return new ProjectileSetting.Loop
                {
                    name = vfxName,
                    delayDestory = destroyDelayFrame,
                    moveCurve = moveCurve,
                    heightCurve = heightCurve,
                    sideDirCurve = sideDirCurve,
                };
            }

            public ProjectileSetting.End ToEnd()
            {
                return new ProjectileSetting.End
                {
                    overlapTime = overlapFrame,
                    name = vfxName,
                };
            }

            public ProjectileSetting.Sound ToSound()
            {
                return new ProjectileSetting.Sound
                {
                    time = overlapFrame,
                    name = sfxName,
                    duration = destroyDelayFrame,
                };
            }
        }

        private class CustomReader : BinaryReader
        {
            public CustomReader(Stream input) : base(input)
            {
            }

            public SkillFxDescription[] ReadSkillFxDescriptions()
            {
                int descriptionCount = ReadInt32();
                SkillFxDescription[] descriptions = new SkillFxDescription[descriptionCount];

                for (int i = 0; i < descriptionCount; i++)
                {
                    descriptions[i] = ReadSkillFxDescription();
                }

                return descriptions;
            }

            public MonsterFxDescription[] ReadMonsterFxDescriptions()
            {
                int descriptionCount = ReadInt32();
                MonsterFxDescription[] descriptions = new MonsterFxDescription[descriptionCount];

                for (int i = 0; i < descriptionCount; i++)
                {
                    descriptions[i] = ReadMonsterFxDescription();
                }

                return descriptions;
            }

            public ProjectileFxDescription[] ReadProjectileFxDescriptions()
            {
                int descriptionCount = ReadInt32();
                ProjectileFxDescription[] descriptions = new ProjectileFxDescription[descriptionCount];

                for (int i = 0; i < descriptionCount; i++)
                {
                    descriptions[i] = ReadProjectileFxDescription();
                }

                return descriptions;
            }

            private SkillFxDescription ReadSkillFxDescription()
            {
                int skillID = ReadInt32(); // read
                string skillName = ReadString(); // read
                string animationName = ReadString(); // read
                AnimationEventDescription[] animationEvents = ReadAnimationEvents(); // read

                return new SkillFxDescription
                {
                    skillId = skillID,
                    skillName = skillName,
                    animationName = animationName,
                    animationEvents = animationEvents,
                };
            }

            private MonsterFxDescription ReadMonsterFxDescription()
            {
                string monsterName = ReadString(); // read
                SkillFxDescription normalAttack = ReadSkillFxDescription(); // read
                SkillFxDescription skill1 = ReadSkillFxDescription(); // read
                SkillFxDescription skill2 = ReadSkillFxDescription(); // read
                SkillFxDescription die = ReadSkillFxDescription(); // read

                return new MonsterFxDescription
                {
                    monsterName = monsterName,
                    normalAttack = normalAttack,
                    skill1 = skill1,
                    skill2 = skill2,
                    die = die,
                };
            }

            public ProjectileFxDescription ReadProjectileFxDescription()
            {
                string projectileName = ReadString(); // read
                string pathType = ReadString(); // read
                ProjectileFxElement[] elements = ReadProjectileElements(); // read

                return new ProjectileFxDescription
                {
                    projectileName = projectileName,
                    pathType = pathType,
                    fxElements = elements,
                };
            }

            private AnimationEventDescription[] ReadAnimationEvents()
            {
                int animationEventCount = ReadInt32(); // read

                AnimationEventDescription[] animationEvents = new AnimationEventDescription[animationEventCount];
                for (int i = 0; i < animationEventCount; i++)
                {
                    AnimationEventDescription animationEvent = new AnimationEventDescription();

                    byte type = ReadByte(); // read
                    int id = ReadInt32(); // read
                    int frame = ReadInt32(); // read
                    string vfxName = ReadString(); // read
                    string sfxName = ReadString(); // read
                    string projectileName = ReadString(); // read
                    bool toTarget = ReadBoolean(); // read
                    bool isLoop = ReadBoolean(); // read
                    float duration = ReadSingle(); // read
                    bool isAttach = ReadBoolean(); // read
                    string node = ReadString(); // read
                    Vector3 offset = ReadVector3(); // read
                    Vector3 offsetRot = ReadVector3(); // read

                    animationEvents[i] = new AnimationEventDescription
                    {
                        type = (AnimationEventDescription.AnimationEventType)type,
                        id = id,
                        frame = frame,
                        vfxName = vfxName,
                        sfxName = sfxName,
                        projectileName = projectileName,
                        toTarget = toTarget,
                        isLoop = isLoop,
                        duration = duration,
                        isAttach = isAttach,
                        node = node,
                        offset = offset,
                        offsetRot = offsetRot,
                    };
                }

                return animationEvents;
            }

            private ProjectileFxElement[] ReadProjectileElements()
            {
                int elementCount = ReadInt32(); // read

                ProjectileFxElement[] projectileElements = new ProjectileFxElement[elementCount];
                for (int i = 0; i < elementCount; i++)
                {
                    byte fxType = ReadByte(); // read
                    byte phaseType = ReadByte(); // read
                    int overlapFrame = ReadInt32(); // read
                    int destroyDelayFrame = ReadInt32(); // read
                    string vfxName = ReadString(); // read
                    string sfxName = ReadString(); // read
                    string pathClass = ReadString(); // read
                    AnimationCurve heightCurve = ReadAnimationCurve(); // read
                    AnimationCurve moveCurve = ReadAnimationCurve(); // read
                    AnimationCurve sideDirCurve = ReadAnimationCurve(); // read
                    bool isAttach = ReadBoolean(); // read
                    string node = ReadString(); // read
                    Vector3 offset = ReadVector3(); // read
                    Vector3 offsetRot = ReadVector3(); // read

                    projectileElements[i] = new ProjectileFxElement
                    {
                        fxType = (ProjectileFxElement.ProjectileFxType)fxType,
                        phaseType = (ProjectileFxElement.ProjectilePhaseType)phaseType,
                        overlapFrame = overlapFrame,
                        destroyDelayFrame = destroyDelayFrame,
                        vfxName = vfxName,
                        sfxName = sfxName,
                        pathClass = pathClass,
                        heightCurve = heightCurve,
                        moveCurve = moveCurve,
                        sideDirCurve = sideDirCurve,
                        isAttach = isAttach,
                        node = node,
                        offset = offset,
                        offsetRot = offsetRot,
                    };
                }

                return projectileElements;
            }

            private Vector3 ReadVector3()
            {
                float x = ReadSingle();
                float y = ReadSingle();
                float z = ReadSingle();

                return new Vector3(x, y, z);
            }

            private AnimationCurve ReadAnimationCurve()
            {
                byte preWrapMode = ReadByte(); // read
                byte postWrapMode = ReadByte(); // read
                Keyframe[] keys = ReadKeyFrames(); // read

                return new AnimationCurve
                {
                    preWrapMode = (WrapMode)preWrapMode,
                    postWrapMode = (WrapMode)postWrapMode,
                    keys = keys,
                };
            }

            private Keyframe[] ReadKeyFrames()
            {
                int keyLength = ReadInt32(); // read

                Keyframe[] keyframes = new Keyframe[keyLength];
                for (int i = 0; i < keyLength; i++)
                {
                    float time = ReadSingle();
                    float value = ReadSingle();
                    float inTangent = ReadSingle();
                    float outTangent = ReadSingle();
                    int tangentMode = ReadInt32();

                    keyframes[i] = new Keyframe
                    {
                        time = time,
                        value = value,
                        inTangent = inTangent,
                        outTangent = outTangent,
                        tangentMode = tangentMode,
                    };
                }

                return keyframes;
            }
        }
    }
}