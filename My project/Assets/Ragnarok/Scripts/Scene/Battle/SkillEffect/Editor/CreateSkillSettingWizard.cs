using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public class CreateSkillSettingWizard : ScriptableWizard
    {
        private const string TITLE = "Create New SkillSetting";

        private enum InvalidType
        {
            None,

            BlankFileName,
            ExistFileName,
            BlankID,
            ExistID,
        }

        [SerializeField]
        [Rename(displayName = "이름")]
        string fileName;

        [SerializeField]
        [Rename(displayName = "아이디")]
        int id;

        [SerializeField]
        [Rename(displayName = "애니메이션 이름")]
        string aniName;

        private SkillSettingContainer skillSettings;
        private InvalidType invalidType;
        private string assetPath;

        void OnEnable()
        {
            skillSettings = AssetDatabase.LoadAssetAtPath<SkillSettingContainer>(SkillPreviewWindow.SKILL_SETTING_ASSET_PATH);
            id = GetUniqueID();
        }

        void OnWizardUpdate()
        {
            invalidType = GetInvalidType();
            isValid = invalidType == InvalidType.None;
        }

        protected override bool DrawWizardGUI()
        {
            bool isDirty = base.DrawWizardGUI();

            switch (invalidType)
            {
                case InvalidType.None:
                    EditorGUILayout.HelpBox("생성 가능", MessageType.Info, true);
                    break;

                case InvalidType.BlankFileName:
                    EditorGUILayout.HelpBox("생성할 이름이 비어있습니다", MessageType.Warning, true);
                    break;

                case InvalidType.ExistFileName:
                    EditorGUILayout.HelpBox("이미 있는 이름입니다", MessageType.Warning, true);
                    break;

                case InvalidType.BlankID:
                    EditorGUILayout.HelpBox("생성할 id가 비어있습니다", MessageType.Warning, true);
                    break;

                case InvalidType.ExistID:
                    EditorGUILayout.HelpBox("이미 있는 id입니다", MessageType.Warning, true);
                    break;
            }

            return isDirty;
        }

        private InvalidType GetInvalidType()
        {
            if (string.IsNullOrEmpty(fileName))
                return InvalidType.BlankFileName;

            assetPath = $"Assets/Ragnarok/Data/SkillSetting/{fileName}.asset";

            if (System.IO.File.Exists(assetPath))
                return InvalidType.ExistFileName;

            if (id <= 0)
                return InvalidType.BlankID;

            foreach (var item in skillSettings.GetArray())
            {
                if (item.id.Equals(id))
                    return InvalidType.ExistID;
            }

            return InvalidType.None;
        }

        private int GetUniqueID()
        {
            int id = 1;

            foreach (var item in skillSettings.GetArray())
            {
                if (!item.id.Equals(id))
                    break;

                ++id;
            }

            return id;
        }

        void OnWizardCreate()
        {
            if (!EditorUtility.DisplayDialog(TITLE, "파일을 생성하시겠습니까?", "생성", "취소"))
                return;

            // SkillSetting 생성
            SkillSetting setting = CreateInstance<SkillSetting>();
            setting.id = id;
            setting.aniName = aniName;
            AssetDatabase.CreateAsset(setting, assetPath);
            
            // Container 에 적용시키기
            SerializedObject so = new SerializedObject(skillSettings);
            SerializedProperty sp = so.FindProperty("array");
            int index = sp.arraySize;
            sp.InsertArrayElementAtIndex(sp.arraySize);
            sp.GetArrayElementAtIndex(index).objectReferenceValue = setting;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(setting);
            EditorUtility.SetDirty(skillSettings);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(setting);
        }
    }
}