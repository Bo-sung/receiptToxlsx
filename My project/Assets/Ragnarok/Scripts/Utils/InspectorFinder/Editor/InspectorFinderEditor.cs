using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class InspectorFinderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target is IAutoInspectorFinder)
            {
                Draw();
            }
        }

        private void Draw()
        {
            if (targets == null)
                return;

            if (targets.Length == 0)
                return;

            if (GUILayout.Button("Find"))
            {
                InspectorFinderTools.FindFields(targets);
            }
        }
    }
}