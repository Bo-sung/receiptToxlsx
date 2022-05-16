using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

[RequireComponent(typeof(Renderer))]
public class ColorModifier : MonoBehaviour
{
    private const string MapAssetPath = "Assets/ArtResources/ArtAssets/Maps";

    private new Renderer renderer;
    private Material material;
    private Texture2D originalTexture;
    private RenderTexture modifiedTexture;

    private Material modifierMat;

    [SerializeField, Range(-180, 180)] private float dh;
    [SerializeField, Range(-1, 1)] private float ds;
    [SerializeField, Range(-1, 1)] private float dv;
    [SerializeField, Range(-255, 255)] private float contrast;
    [SerializeField] private bool useHSL;
    [SerializeField] private AnimationCurve levelCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Matrix4x4[] levelCurveValues = new Matrix4x4[4];
    private string setting;

    private void OnEnable()
    {
        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
            material = renderer.sharedMaterial;
            originalTexture = material.mainTexture as Texture2D;

            modifierMat = new Material(Shader.Find("Hidden/ColorModifier"));
            modifiedTexture = new RenderTexture(originalTexture.width, originalTexture.height, 0);
            material.mainTexture = modifiedTexture;
        }

        material.mainTexture = modifiedTexture;
    }

    private void OnDisable()
    {
        material.mainTexture = originalTexture;
    }

    private void OnDestroy()
    {
        if (modifiedTexture != null)
            Object.Destroy(modifiedTexture);
        if (modifierMat != null)
            Object.Destroy(modifierMat);
    }

    private void Update()
    {
        for (int i = 0; i < 64; ++i)
            levelCurveValues[i / 16][(i / 4) % 4, i % 4] = levelCurve.Evaluate(i / 63.0f);

        modifierMat.SetFloat("_DH", dh);
        modifierMat.SetFloat("_DS", ds);
        modifierMat.SetFloat("_DV", dv);
        modifierMat.SetFloat("_Contrast", contrast);
        modifierMat.SetFloat("_UseHSL", useHSL ? 1.0f : 0.0f);
        modifierMat.SetMatrixArray("_CurveValues", levelCurveValues);

        Graphics.Blit(originalTexture, modifiedTexture, modifierMat);
    }

    private void ToSetting()
    {
        setting = "";

        var keys = levelCurve.keys;
        string[] keyStrings = new string[keys.Length];

        for (int i = 0; i < keyStrings.Length; ++i)
            keyStrings[i] = $"{keys[i].time},{keys[i].value},{keys[i].inTangent},{keys[i].outTangent},{keys[i].inWeight},{keys[i].outWeight}";
        
        setting = $"dh:{dh.ToString()}/ds:{ds.ToString()}/dv:{dv.ToString()}/co:{contrast.ToString()}/hsl:{useHSL}/lv:{string.Join(",", keyStrings)}";
    }

    private void FromSetting()
    {
        if (setting == null || setting.Length == 0)
            return;

        string[] eachFields = setting.Split('/');

        for (int i = 0; i < eachFields.Length; ++i)
        {
            string[] a = eachFields[i].Split(':');
            string key = a[0];
            string value = a[1];

            switch (key)
            {
                case "dh":
                    dh = float.Parse(value);
                    break;
                case "ds":
                    ds = float.Parse(value);
                    break;
                case "dv":
                    dv = float.Parse(value);
                    break;
                case "co":
                    contrast = float.Parse(value);
                    break;
                case "hsl":
                    useHSL = bool.Parse(value);
                    break;
                case "lv":
                    var keyFrameData = value.Split(',');
                    levelCurve = new AnimationCurve();
                    for (int j = 0; j < keyFrameData.Length; j += 6)
                    {
                        Keyframe frame = new Keyframe(
                            float.Parse(keyFrameData[j + 0]),
                            float.Parse(keyFrameData[j + 1]),
                            float.Parse(keyFrameData[j + 2]),
                            float.Parse(keyFrameData[j + 3]),
                            float.Parse(keyFrameData[j + 4]),
                            float.Parse(keyFrameData[j + 5])
                            );

                        levelCurve.AddKey(frame);
                    }
                    break;
            }
        }
    }

    [CustomEditor(typeof(ColorModifier))]
    private class ColorModifierEditor : Editor
    {
        ColorModifier modifier;

        private void OnEnable()
        {
            modifier = target as ColorModifier;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            modifier.setting = EditorGUILayout.TextField(modifier.setting);

            if (GUILayout.Button("세팅 문자열 생성"))
                modifier.ToSetting();

            if (GUILayout.Button("세팅 문자열 적용"))
                modifier.FromSetting();

            EditorGUILayout.Space();

            if (GUILayout.Button("텍스쳐에 적용하기"))
            {
                var originalTexture = modifier.originalTexture;
                Texture2D texture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

                Camera cam = modifier.gameObject.AddComponent<Camera>();
                RenderTexture.active = modifier.modifiedTexture;
                cam.Render();
                Rect rt = new Rect(0, 0, originalTexture.width, originalTexture.height);
                texture.ReadPixels(rt, 0, 0);
                texture.Apply();
                RenderTexture.active = null;
                Object.DestroyImmediate(cam);

                var datas = texture.EncodeToPNG();
                Object.DestroyImmediate(texture);

                string originalTextureName = originalTexture.name;
                string filter = string.Concat(originalTextureName, " t:texture2D");
                string[] textureGUIDs = AssetDatabase.FindAssets(filter, new string[] { MapAssetPath });

                List<string> guids = new List<string>();
                for (int i = 0; i < textureGUIDs.Length; ++i)
                {
                    if (!Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(textureGUIDs[i])).Contains("Modified"))
                        guids.Add(textureGUIDs[i]);
                }

                if (guids.Count == 0)
                {
                    EditorUtility.DisplayDialog("ColorModifier", $"{originalTextureName} 를 찾지 못했습니다. 프로그래머에게 상담해주세요.", "ok");
                    return;
                }

                if (guids.Count > 1)
                {
                    EditorUtility.DisplayDialog("ColorModifier", $"{originalTextureName} 를 가진 텍스쳐가 여러개 있습니다. 유일한 이름을 갖도록 하여 적용해주세요.", "ok");
                    return;
                }

                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var fileName = Path.GetFileName(path);

                string newFileName = $"Modified_{modifier.material.name}_{fileName}";
                var filePath = Application.dataPath.Replace("Assets", path);
                filePath = filePath.Replace(fileName, newFileName);

                File.WriteAllBytes(filePath, datas);
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("ColorModifier", $"{newFileName} 를 생성하였습니다.", "ok");
            }
        }
    }
}
#endif