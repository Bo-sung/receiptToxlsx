using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UITextureHelper), true)]
    public class UITextureHelperEditor : UITextureInspector
    {
		protected override void DrawCustomProperties()
        {
            GUILayout.Space(6f);
            NGUIEditorTools.DrawProperty("Mode", serializedObject, "mode", GUILayout.MinWidth(20f));

            SerializedProperty sp = NGUIEditorTools.DrawProperty("RoundMode", serializedObject, "roundMode", GUILayout.MinWidth(20f));

            UIRoundTexture.RoundMode mode = (UIRoundTexture.RoundMode)sp.intValue;

            if (mode == UIRoundTexture.RoundMode.Round)
            {
                NGUIEditorTools.DrawProperty("RoundValue", serializedObject, "roundValue");
            }

            GUILayout.Space(-6f);
            base.DrawCustomProperties();

            var isPrefab = NGUIEditorTools.IsPrefab(mWidget.gameObject);
			if (isPrefab)
			{
				DrawDepthAndSnap();
			}
        }

		private void DrawDepthAndSnap()
		{
			GUILayout.Space(2f);
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Depth");

				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button("Back", GUILayout.MinWidth(46f)))
				{
					foreach (GameObject go in Selection.gameObjects)
					{
						UIWidget pw = go.GetComponent<UIWidget>();
						if (pw != null) pw.depth = mWidget.depth - 1;
					}
				}

				NGUIEditorTools.DrawProperty("", serializedObject, "mDepth", GUILayout.MinWidth(20f));

				if (GUILayout.Button("Forward", GUILayout.MinWidth(60f)))
				{
					foreach (GameObject go in Selection.gameObjects)
					{
						UIWidget pw = go.GetComponent<UIWidget>();
						if (pw != null) pw.depth = mWidget.depth + 1;
					}
				}

				EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);

				if (GUILayout.Button("Snap", GUILayout.Width(60f)))
				{
					foreach (GameObject go in Selection.gameObjects)
					{
						UIWidget pw = go.GetComponent<UIWidget>();

						if (pw != null)
						{
							NGUIEditorTools.RegisterUndo("Snap Dimensions", pw);
							NGUIEditorTools.RegisterUndo("Snap Dimensions", pw.transform);
							pw.MakePixelPerfect();
						}
					}
				}
				GUI.backgroundColor = Color.white;

				EditorGUI.EndDisabledGroup();
			}
			GUILayout.EndHorizontal();

			int matchingDepths = 1;

			UIPanel p = mWidget.panel;

			if (p != null)
			{
				for (int i = 0, imax = p.widgets.Count; i < imax; ++i)
				{
					UIWidget pw = p.widgets[i];
					if (pw != mWidget && pw.depth == mWidget.depth)
						++matchingDepths;
				}
			}

			if (matchingDepths > 1)
			{
				EditorGUILayout.HelpBox(matchingDepths + " widgets are sharing the depth value of " + mWidget.depth, MessageType.Info);
			}
		}
	}
}