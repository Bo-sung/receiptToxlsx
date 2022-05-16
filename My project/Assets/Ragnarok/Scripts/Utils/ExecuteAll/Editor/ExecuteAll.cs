using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public static class ExecuteAll
    {
        private const string CTRL = "%";
        private const string ALT = "&";
        private const string SHIFT = "#";

        [MenuItem("라그나로크/일괄 처리/애니메이션 이벤트 제거")]
        private static void RemoveAnimationEvent()
        {
            Object[] objects = Selection.objects;
            foreach (Object obj in objects)
            {
                if (obj is AnimationClip)
                {
                    AnimationClip clip = obj as AnimationClip;
                    AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
                }
            }
        }

        [MenuItem("라그나로크/일괄 처리/빠진 스크립트 확인")]
        private static void CleanUpMissingComponent()
        {
            List<Object> list = new List<Object>();

            GameObject[] objs = Selection.gameObjects;
            foreach (GameObject obj in objs)
            {
                GetMissingObjs(list, obj.transform);
            }

            Selection.objects = list.ToArray();

            if (list.Count == 0)
            {
                Debug.LogError("빠진 스크립트 음슴");
            }
        }

        private static void GetMissingObjs(List<Object> list, Transform tf)
        {
            Component[] components = tf.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    list.Add(tf.gameObject);
                }
            }

            for (int i = 0; i < tf.childCount; i++)
            {
                GetMissingObjs(list, tf.GetChild(i));
            }
        }

        /// <summary>
        /// 유효하지 않은 컬라이더 체크
        /// </summary>
        [MenuItem("라그나로크/일괄 처리/유효하지 않은 컬라이더 체크")]
        private static void CheckInvalidCollider()
        {
            List<string> list = new List<string>();

            GameObject[] objects = Selection.gameObjects;
            foreach (GameObject obj in objects)
            {
                BoxCollider2D[] colliders = obj.GetComponentsInChildren<BoxCollider2D>(includeInactive: true);
                foreach (var collider in colliders)
                {
                    if (collider.size.x >= 720f && collider.size.y >= 1280f)
                    {
                        UIWidget widget = collider.GetComponent<UIWidget>();
                        if (widget == null)
                        {
                            list.Add(NGUITools.GetHierarchy(collider.gameObject)); // 음슴
                        }
                        else if (!widget.autoResizeBoxCollider)
                        {
                            list.Add(NGUITools.GetHierarchy(collider.gameObject)); // auto 풀려있음
                        }
                        else if (!widget.isAnchored)
                        {
                            list.Add(NGUITools.GetHierarchy(collider.gameObject)); // Anchor 형태가 아님
                        }
                    }
                }
            }

            if (list.Count > 0)
            {
                list.Sort((a, b) => a.CompareTo(b));

                var sb = StringBuilderPool.Get();
                foreach (var item in list)
                {
                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.Append(item);
                }

                Debug.LogError("클립보드에 저장");
                GUIUtility.systemCopyBuffer = sb.Release();
            }
            else
            {
                Debug.LogError("그런거 음슴");
            }
        }

        [MenuItem("라그나로크/일괄 처리/임시 " + ALT + SHIFT + "m")]
        private static void TempExecute()
        {
            List<Object> list = new List<Object>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            GameObject[] objs = Selection.gameObjects;
            foreach (GameObject obj in objs)
            {
                GetTempObjects(list, obj.transform, sb);
            }

            if (list.Count == 0)
            {
                Debug.LogError("그런거 음슴");
            }
            else
            {
                MarkSceneDirty();
                //EditorUtility.DisplayDialog(string.Concat("확인 필요: ", list.Count), sb.ToString(), "확인");
                GUIUtility.systemCopyBuffer = sb.ToString();
                Selection.objects = list.ToArray();

                AssetDatabase.SaveAssets();
                //AssetDatabase.Refresh();
            }
        }

        private static void GetTempObjects(List<Object> list, Transform tf, System.Text.StringBuilder sb)
        {
            Component[] components = tf.GetComponents<Component>();

            foreach (var item in components)
            {
                if (CheckTempObject(item, sb))
                {
                    list.Add(tf.gameObject);
                    break;
                }
            }

            for (int i = 0; i < tf.childCount; i++)
            {
                GetTempObjects(list, tf.GetChild(i), sb);
            }
        }

        private static bool CheckWidgetDepth(Transform transform, int depth, System.Text.StringBuilder sb)
        {
            Transform parent = transform.parent;
            if (parent == null)
                return false;

            int childCount = parent.childCount;
            int siblingIndex = transform.GetSiblingIndex();
            int count = Mathf.Min(childCount, siblingIndex);
            for (int i = 0; i < count; i++)
            {
                Transform child = parent.GetChild(i);
                UIWidget[] widgets = child.GetComponentsInChildren<UIWidget>(includeInactive: true);
                foreach (var item in widgets)
                {
                    if (item is UILabel)
                        continue;

                    if (item.depth >= depth)
                    {
                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.Append(NGUITools.GetHierarchy(transform.gameObject));
                        sb.Append(":\t");
                        sb.Append(NGUITools.GetHierarchy(item.gameObject));
                        return true;
                    }
                }
            }

            return CheckWidgetDepth(parent, depth, sb);
        }

        private static bool CheckTempObject(Component component, System.Text.StringBuilder sb)
        {
            if (component is UIWidget widget)
            {
                if (widget.GetComponent<UIIgnoreSafeArea>() != null)
                    return false;

                if (widget.width >= 720 || widget.height >= 1280)
                {
                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.Append(NGUITools.GetHierarchy(component.gameObject));
                    return true;
                }
            }

            return false;


            // Atlas 정보가 없는 Sprite
            if (component is UISprite sprite)
            {
                if (sprite.GetAtlasSprite() == null)
                {
                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.Append(NGUITools.GetHierarchy(component.gameObject));
                    return true;
                }
            }

            // 할당된 값이 없는 UITextureHelper
            foreach (System.Reflection.FieldInfo field in component.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
            {
                if (field.FieldType.Equals(typeof(UITextureHelper)))
                {
                    if (field.GetValue(component) == null)
                    {
                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.Append(NGUITools.GetHierarchy(component.gameObject)).Append(":").Append(field.Name);
                        return true;
                    }
                }
            }

            return false;
        }

        //[MenuItem("라그나로크/일괄 처리/임시 " + ALT + SHIFT + "m")]
        private static void Temp()
        {
            List<Object> list = new List<Object>();

            GameObject[] objs = Selection.gameObjects;
            foreach (GameObject obj in objs)
            {
                GetTempObjs(list, obj.transform);
            }

            Selection.objects = list.ToArray();
        }

        private static void GetTempObjs(List<Object> list, Transform tf)
        {
            UIWidget[] widgets = tf.GetComponents<UIWidget>();

            for (int i = 0; i < widgets.Length; i++)
            {
                if (widgets[i] == null)
                    continue;

                if (widgets[i].height > 1200)
                    list.Add(tf.gameObject);
            }

            for (int i = 0; i < tf.childCount; i++)
            {
                GetTempObjs(list, tf.GetChild(i));
            }
        }

        private static void MarkSceneDirty()
        {
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
            else
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().scene);
            }
        }

        [MenuItem("라그나로크/일괄 처리/스킬 HitTime Export")]
        private static void ExportSkillHitTime()
        {
            SkillSettingContainer container = AssetDatabase.LoadAssetAtPath<SkillSettingContainer>(SkillPreviewWindow.SKILL_SETTING_ASSET_PATH);
            if (container == null)
            {
                EditorUtility.DisplayDialog("Skill Settings", "스킬 음슴", "확인");
                return;
            }

            System.Text.StringBuilder sbLog = new System.Text.StringBuilder();
            SkillSetting[] array = container.GetArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (sbLog.Length > 0)
                    sbLog.AppendLine();

                sbLog.Append(array[i].id).Append("\t").Append(array[i].hitTime);
            }

            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "SkillSettings");
            System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(path);
            if (!info.Exists)
            {
                System.IO.Directory.CreateDirectory(path);
            }

            System.IO.File.WriteAllText(System.IO.Path.Combine(path, "log.txt"), sbLog.ToString());

            EditorUtility.DisplayDialog("Skill Settings 저장", path, "확인");
            System.Diagnostics.Process.Start(path); // 폴더 열기
        }

        [MenuItem("라그나로크/일괄 처리/파일 이름 변경", priority = 10001)]
        private static void RenameFile()
        {
            FileRenameWizard wizard = ScriptableWizard.DisplayWizard<FileRenameWizard>(FileRenameWizard.TITLE);
            wizard.minSize = wizard.maxSize = new Vector2(480f, 240f);
            wizard.Focus();
            wizard.Repaint();
            wizard.Show();
        }
    }
}