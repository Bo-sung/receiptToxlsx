using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CreateTextureFromRT))]
public class CreateTextureFromRTEditor : Editor {
    
    private CreateTextureFromRT me;
    private Camera cam;

    private string textureName;

    private void OnEnable()
    {
        me = (CreateTextureFromRT)target;
        cam = me.GetComponent<Camera>();
        textureName = "";
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PrefixLabel("텍스쳐 이름");
        textureName = EditorGUILayout.TextArea(textureName);

        bool oldEnable = GUI.enabled;

        if (textureName.Length == 0)
            GUI.enabled = false;

        if (GUILayout.Button("저장하기"))
        {
            cam.SaveRenderTextureAsPNG("C:/Users/Administrator/Desktop/MonsterScr/" + textureName + ".png");

            //var rt = cam.RenderToTexture(86, 86);
            //Texture2D texture = new Texture2D(rt.width, rt.height);
            //RenderTexture.active = rt;
            //texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            //texture.Apply();
            //byte[] bytes = texture.EncodeToPNG();
            //File.WriteAllBytes(Application.dataPath + "/planteam/" + textureName + ".png", bytes);
        }

        GUI.enabled = oldEnable;
    }
}
