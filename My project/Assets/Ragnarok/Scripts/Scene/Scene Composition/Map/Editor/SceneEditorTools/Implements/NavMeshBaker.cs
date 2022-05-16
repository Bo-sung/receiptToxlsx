using Ragnarok.SceneComposition;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public class NavMeshBaker : ISceneEditable
    {
        public void Execute(Scene scene)
        {
            Map map = Object.FindObjectOfType<Map>();

            if (map == null)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                foreach (var item in currentScene.GetRootGameObjects())
                {
                    if (item.name.Equals("Map"))
                    {
                        map = item.AddComponent<Map>();
                        break;
                    }
                }
            }

            if (map == null)
            {
                EditorUtility.DisplayDialog("Error", "Map이 존재하지 않습니다", "확인");
                return;
            }

            const string NAVIGATION_NAME = "Navigation";
            const string WALKABLE_NAME = "Walkable";
            const string NOT_WALKABLE_NAME = "Not Walkable";

            Transform root = map.transform;
            Transform navigation = AddChild(root, NAVIGATION_NAME);
            navigation.localPosition = new Vector3(-18f, 0f, 0f);
            navigation.localScale = Vector3.one * 3.6f;

            Transform walkable = AddChild(navigation, WALKABLE_NAME, PrimitiveType.Quad);
            walkable.localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
            walkable.localScale = new Vector3(11f, 1f, 9f);
            GameObjectUtility.SetNavMeshArea(walkable.gameObject, GameObjectUtility.GetNavMeshAreaFromName("Walkable"));
            GameObjectUtility.SetStaticEditorFlags(walkable.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);

            Transform notWalkable = AddChild(navigation, NOT_WALKABLE_NAME, PrimitiveType.Quad);
            notWalkable.localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
            notWalkable.localScale = new Vector3(19f, 1f, 17f);
            GameObjectUtility.SetNavMeshArea(notWalkable.gameObject, GameObjectUtility.GetNavMeshAreaFromName("Not Walkable"));
            GameObjectUtility.SetStaticEditorFlags(notWalkable.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);

            return;
            NavMeshBuilder.BuildNavMesh();
        }

        private Transform AddChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);

            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent, worldPositionStays: false);
            }

            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;

            return child;
        }

        private Transform AddChild(Transform parent, string name, PrimitiveType type)
        {
            Transform child = parent.Find(name);

            if (child == null)
            {
                child = GameObject.CreatePrimitive(type).transform;
                child.name = name;
                child.SetParent(parent, worldPositionStays: false);
            }

            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;

            return child;
        }
    }
}