using UnityEngine;
using UnityEditor;

namespace Ragnarok.SceneComposition
{
    [CustomEditor(typeof(Map))]
    public sealed class MapEditor : Editor
    {
        const float CELL_SIZE = 3.6f;

        const int MIN_WALL_SIZE = 1;
        const int MAX_WALL_SIZE = 20;

        Map map;
        Transform transform;

        int wallSize = 5;

        GUIStyle wallSizeStyle;

        void OnEnable()
        {
            map = target as Map;
            transform = map.transform;
            wallSizeStyle = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //if (GUILayout.Button("Initialize"))
            //    Initialize();
        }

        void OnSceneGUI()
        {
            Handles.BeginGUI();
            {
                DrawViewSelector();
                DrawWallCreator();
                DrawGrid();
            }
            Handles.EndGUI();

            SceneView.RepaintAll();
        }

        private void DrawViewSelector()
        {
            if (GUI.Button(new Rect(10f, 10f, 100f, 20f), "Fix View"))
                FixView();

            if (GUI.Button(new Rect(10f, 34f, 100f, 20f), "Quarter View"))
                QuarterView();
        }

        private void DrawWallCreator()
        {
            if (GUI.Button(new Rect(120f, 10f, 20f, 20f), "-"))
                wallSize = Mathf.Clamp(wallSize - 1, MIN_WALL_SIZE, MAX_WALL_SIZE);

            GUI.Label(new Rect(120f, 10f, 60f, 20f), wallSize.ToString(), wallSizeStyle);

            if (GUI.Button(new Rect(160f, 10f, 20f, 20f), "+"))
                wallSize = Mathf.Clamp(wallSize + 1, MIN_WALL_SIZE, MAX_WALL_SIZE);

            if (GUI.Button(new Rect(120f, 34f, 60f, 20f), "Create"))
                CreateWall();
        }

        private void DrawGrid()
        {
            map.isShowGrid = GUI.Toggle(new Rect(10f, 70f, 100f, 10f), map.isShowGrid, "Show Grid");

            if (map.isShowGrid)
            {
                map.useHalfGrid = GUI.Toggle(new Rect(10f, 90f, 100f, 10f), map.useHalfGrid, "Use Half Grid");

                if (GUI.Button(new Rect(10f, 110f, 20f, 20f), "-"))
                    map.cellIndex.x = Mathf.Max(MIN_WALL_SIZE, map.cellIndex.x - 1);

                GUI.Label(new Rect(10f, 110f, 60f, 20f), map.cellIndex.x.ToString(), wallSizeStyle);

                if (GUI.Button(new Rect(50f, 110f, 20f, 20f), "+"))
                    map.cellIndex.x = Mathf.Max(MIN_WALL_SIZE, map.cellIndex.x + 1);

                if (GUI.Button(new Rect(10f, 134f, 20f, 20f), "-"))
                    map.cellIndex.y = Mathf.Max(MIN_WALL_SIZE, map.cellIndex.y - 1);

                GUI.Label(new Rect(10f, 134f, 60f, 20f), map.cellIndex.y.ToString(), wallSizeStyle);

                if (GUI.Button(new Rect(50f, 134f, 20f, 20f), "+"))
                    map.cellIndex.y = Mathf.Max(MIN_WALL_SIZE, map.cellIndex.y + 1);
            }
        }

        private void Initialize()
        {
            var monsterZones = serializedObject.FindProperty("monsterZones");
            monsterZones.ClearArray();

            SpawnZone[] zones = map.GetComponentsInChildren<SpawnZone>();
            foreach (var item in zones)
            {
                if (item.name.Equals("Player"))
                {
                    serializedObject.FindProperty("playerZone").objectReferenceValue = item;
                }
                else if (item.name.Equals("Boss"))
                {
                    serializedObject.FindProperty("bossZone").objectReferenceValue = item;
                }
                else
                {
                    int index = monsterZones.arraySize;
                    monsterZones.InsertArrayElementAtIndex(index);
                    monsterZones.GetArrayElementAtIndex(index).objectReferenceValue = item;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void FixView()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            sceneView.LookAt(sceneView.pivot, Quaternion.Euler(90f, 90f, 180f), sceneView.size, ortho: true);
        }

        private void QuarterView()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            sceneView.LookAt(sceneView.pivot, Quaternion.Euler(35f, -135f, 0f), sceneView.size, ortho: false);
        }

        private void CreateWall()
        {
            const string NAME = "Invisible Wall";
            NGUITools.Destroy(transform.Find(NAME)); // 제거
            Transform invisibleWall = CreateInvisibleWall(NAME); // 생성
            CreateChild(invisibleWall, "Left", Vector3.back, Vector3.right * CELL_SIZE * (wallSize - 1)); // Left
            CreateChild(invisibleWall, "Right", Vector3.forward, Vector3.right * CELL_SIZE * (wallSize - 1)); // Right
            CreateChild(invisibleWall, "Forward", Vector3.left, Vector3.forward * CELL_SIZE * (wallSize - 1)); // Forward
            CreateChild(invisibleWall, "Back", Vector3.right, Vector3.forward * CELL_SIZE * (wallSize - 1)); // Back
            EditorGUIUtility.PingObject(invisibleWall);
        }

        private Transform CreateInvisibleWall(string name)
        {
            GameObject newGameObject = new GameObject(name);
            Transform tf = newGameObject.transform;
            tf.SetParent(transform);

            tf.localPosition = Vector3.up * (CELL_SIZE * 1.5f);
            tf.localRotation = Quaternion.identity;
            tf.localScale = Vector3.one;
            return tf;
        }

        private void CreateChild(Transform parent, string text, Vector3 basePos, Vector3 plusSize)
        {
            GameObject go = new GameObject(text);
            Transform tf = go.transform;
            tf.SetParent(parent);

            tf.localPosition = basePos * (CELL_SIZE * 0.5f) * (wallSize + 1);
            tf.localRotation = Quaternion.identity;
            tf.localScale = Vector3.one;

            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.size = (Vector3.one * CELL_SIZE) + plusSize;
        }
    }
}