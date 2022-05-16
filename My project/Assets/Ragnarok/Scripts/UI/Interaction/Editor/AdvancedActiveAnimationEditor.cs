//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2018 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
	[CustomEditor(typeof(AdvancedActiveAnimation))]
	public class AdvancedActiveAnimationEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			NGUIEditorTools.SetLabelWidth(80f);
			AdvancedActiveAnimation aaa = target as AdvancedActiveAnimation;
			GUILayout.Space(3f);
			NGUIEditorTools.DrawEvents("On Finished", aaa, aaa.onFinished);
		}
	}
}