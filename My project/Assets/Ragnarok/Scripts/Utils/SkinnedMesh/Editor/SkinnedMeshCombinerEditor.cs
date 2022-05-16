using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomEditor(typeof(SkinnedMeshCombiner))]
    public class SkinnedMeshCombinerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SkinnedMeshCombiner meshCombiner = (SkinnedMeshCombiner)target;
            if (GUILayout.Button("스킨메쉬 합치기"))
            {
                meshCombiner.CreateSkinnedMesh();
            }
        }
    } 
}
