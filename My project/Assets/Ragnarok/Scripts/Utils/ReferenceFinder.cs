using Ragnarok;
using Ragnarok.View;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ReferenceFinder : MonoBehaviour
{
#if UNITY_EDITOR

    [CustomEditor(typeof(ReferenceFinder))]
    private class FinderEditor : Editor
    {
        private class A
        {
            public string prefabName;
            public List<string> pathes = new List<string>();
        }

        private string spriteName;

        public override void OnInspectorGUI()
        {
            spriteName = EditorGUILayout.TextField(spriteName);

            if (GUILayout.Button("검색"))
            {
                if (spriteName == null || spriteName.Length == 0)
                    return;
                
                var prefabs = GetAllUIPrefabs();

                List<A> names = new List<A>();

                List<UISprite> spriteList = new List<UISprite>();

                for (int i = 0; i < prefabs.Count; ++i)
                {
                    spriteList.Clear();

                    string prefabName = prefabs[i].name;
                    var sprite = prefabs[i].GetComponent<UISprite>();
                    var sprites = prefabs[i].GetComponentsInChildren<UISprite>(true);

                    if (sprite != null)
                        spriteList.Add(sprite);
                    spriteList.AddRange(sprites);

                    A a = null;

                    for (int j = 0; j < spriteList.Count; ++j)
                    {
                        var each = spriteList[j];

                        if (each.spriteName == spriteName)
                        {
                            if (a == null)
                            {
                                a = new A();
                                a.prefabName = prefabName;
                                names.Add(a);
                            }

                            a.pathes.Add(GetPath(prefabs[i], each.gameObject));
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"{spriteName} 검색 결과");
                sb.AppendLine();

                sb.AppendLine("------ 프리펩들 ------");
                for (int i = 0; i < names.Count; ++i)
                    sb.AppendFormat($"{names[i].prefabName}\n");

                sb.AppendLine();
                sb.AppendLine("------ 스프라이트 컴포넌트 경로 ------");
                for (int i = 0; i < names.Count; ++i)
                {
                    sb.AppendFormat($"{names[i].prefabName}\n");
                    for (int j = 0; j < names[i].pathes.Count; ++j)
                        sb.AppendFormat($"- {names[i].pathes[j]}\n");
                }

                Debug.Log(sb.ToString());
            }
        }

        private string GetPath(GameObject root, GameObject child)
        {
            if (root == child)
            {
                return "";
            }
            else
            {
                string path = GetPath(root, child.transform.parent.gameObject);
                path += $"/{child.name}";
                return path;
            }
        }
    }

    public static List<GameObject> GetAllUIPrefabs()
    {
        List<GameObject> ret = new List<GameObject>();
    
        string[] files = Directory.GetFiles("Assets/ArtResources/UI/Canvas/Prefabs/", "*.*", SearchOption.AllDirectories);
    
        for (int i = 0; i < files.Length; ++i)
        {
            if (files[i].EndsWith(".prefab"))
                ret.Add(AssetDatabase.LoadAssetAtPath<GameObject>(files[i]));
        }
    
        return ret;
    }

#endif
}
