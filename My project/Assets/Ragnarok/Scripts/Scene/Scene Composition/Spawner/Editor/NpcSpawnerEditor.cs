using UnityEditor;
using UnityEngine;

namespace Ragnarok.SceneComposition
{
    [CustomEditor(typeof(NpcSpawner))]
    public class NpcSpawnerEditor : Editor
    {
        NpcSpawner npcSpawner;
        Transform tf;

        void OnEnable()
        {
            npcSpawner = target as NpcSpawner;
            tf = npcSpawner.transform;
        }

        void OnSceneGUI()
        {
            SerializedProperty npcType = serializedObject.FindProperty("npcType");
            int npcValue = npcType.intValue;
            NpcType type = npcValue.ToEnum<NpcType>();
            Handles.Label(tf.position, type.ToString());

            SerializedProperty avoidanceRadius = serializedObject.FindProperty("avoidanceRadius");
            float radius = avoidanceRadius.floatValue;

            if (radius == 0f)
                return;

            DrawWireSphere(tf, radius);
        }

        private void DrawWireSphere(Transform transform, float radius)
        {
            Handles.matrix = transform.localToWorldMatrix;
            Handles.color = Color.green;
            Vector3 position = Vector3.zero;

            Handles.DrawWireDisc(position, Vector3.right, radius);
            Handles.DrawWireDisc(position, Vector3.up, radius);
            Handles.DrawWireDisc(position, Vector3.forward, radius);

            if (Camera.current.orthographic)
            {
                Vector3 normal = position - Handles.inverseMatrix.MultiplyVector(Camera.current.transform.forward);
                float sqrMagnitude = normal.sqrMagnitude;
                float num0 = radius * radius;
                Handles.DrawWireDisc(position - num0 * normal / sqrMagnitude, normal, radius);
            }
            else
            {
                Vector3 normal = position - Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position);
                float sqrMagnitude = normal.sqrMagnitude;
                float num0 = radius * radius;
                float num1 = num0 * num0 / sqrMagnitude;
                float num2 = Mathf.Sqrt(num0 - num1);
                Handles.DrawWireDisc(position - num0 * normal / sqrMagnitude, normal, num2);
            }
        }
    }
}