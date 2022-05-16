using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/Prefab")]
    public sealed class PrefabContainer : StringAssetContainer<GameObject>
    {
        protected override string ConvertKey(GameObject t)
        {
            return t.name;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(PrefabContainer))]
        public class PrefabContainerEditor : UnityEditor.Editor
        {
            PrefabContainer container;

            void OnEnable()
            {
                container = target as PrefabContainer;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("비어있는 아이템 제거"))
                {
                    container.RemoveEmptyItem();
                }

                if (GUILayout.Button("세분화한 데이터 생성"))
                {
                    SplitData(10);
                }

                if (GUILayout.Button("중복체크"))
                {
                    var array =  container.GetArray();
                    var dic = new System.Collections.Generic.Dictionary<string, BetterList<int>>(System.StringComparer.Ordinal);
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] == null)
                            continue;

                        string name = array[i].name;
                        if (!dic.ContainsKey(name))
                            dic.Add(name, new BetterList<int>());

                        dic[name].Add(i);
                    }

                    var sb = StringBuilderPool.Get();
                    foreach (var item in dic)
                    {
                        // 중복
                        if (item.Value.size > 1)
                        {
                            if (sb.Length > 0)
                                sb.AppendLine();

                            sb.Append("[name] ").Append(item.Key);

                            foreach (var index in item.Value)
                            {
                                sb.Append(" [").Append(index).Append("] ");
                            }
                        }
                    }

                    Debug.LogError(sb.Release());
                }
            }

            private void SplitData(int splitCount)
            {
                if (!UnityEditor.EditorUtility.DisplayDialog("데이터 세분화", "세분화 콜?", "ㄱㄱ", "ㄴㄴ"))
                    return;

                int count = 0;
                int index = 0;

                string path = UnityEditor.AssetDatabase.GetAssetPath(target).Replace(".asset", string.Empty);
                string assetPath = string.Empty;
                foreach (var item in container.array)
                {
                    if (count == 0)
                    {
                        ++index;

                        assetPath = string.Concat(path, index.ToString("00"), ".asset");
                        PrefabContainer isntance = CreateInstance<PrefabContainer>();
                        
                        if (isntance.array == null)
                            isntance.array = System.Array.Empty<GameObject>();

                        UnityEditor.AssetDatabase.CreateAsset(isntance, assetPath);
                    }

                    PrefabContainer container = UnityEditor.AssetDatabase.LoadAssetAtPath<PrefabContainer>(assetPath);

                    UnityEditor.ArrayUtility.Add(ref container.array, item);
                    UnityEditor.EditorUtility.SetDirty(container);

                    if (++count == splitCount)
                        count = 0;
                }

                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }

            private void Test()
            {
                var sb = StringBuilderPool.Get();
                foreach (var item in container.array)
                {
                    ParticleSystemRenderer[] renderers = item.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);

                    if (renderers.Length > 0)
                        sb.AppendLine(item.name);

                    //foreach (var renderer in renderers)
                    //{
                    //    sb.Append("[").Append(renderer.name).Append("] ").Append(renderer.sortingOrder).AppendLine();
                    //}
                }
                Debug.LogError(sb.Release());

                //foreach (var item in container.array)
                //{
                //    UIPanel[] panels = item.GetComponentsInChildren<UIPanel>(includeInactive: true);

                //    foreach (var panel in panels)
                //    {
                //        panel.useSortingOrder = true; // Sorting Order 사용
                //        panel.sortingOrder = panel.depth * 10;
                //    }
                //}
            }
        }
#endif
    }
}