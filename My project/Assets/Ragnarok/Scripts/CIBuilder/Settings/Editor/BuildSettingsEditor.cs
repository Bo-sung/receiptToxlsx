#define TEST_BUILD

using CodeStage.AntiCheat.ObscuredTypes;
using Ragnarok.CIBuilder;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    [CustomEditor(typeof(BuildSettings))]
    public sealed class BuildSettingsEditor : Editor, IEqualityComparer<BuildType>
    {
        Dictionary<BuildType, StoreTypeConfigAttribute> storeTypeDic;
        Dictionary<BuildType, AuthServerConfigAttribute> authServerConfigDic;
        Dictionary<BuildType, AssetBundleConfigAttribute> assetBundleDic;

        void OnEnable()
        {
            storeTypeDic = new Dictionary<BuildType, StoreTypeConfigAttribute>(this);
            authServerConfigDic = new Dictionary<BuildType, AuthServerConfigAttribute>(this);
            assetBundleDic = new Dictionary<BuildType, AssetBundleConfigAttribute>(this);

            foreach (BuildType type in System.Enum.GetValues(typeof(BuildType)))
            {
                StoreTypeConfigAttribute storeTypeConfig = type.GetAttribute<StoreTypeConfigAttribute>();
                AuthServerConfigAttribute authServerConfig = type.GetAttribute<AuthServerConfigAttribute>();
                AssetBundleConfigAttribute assetBundleConfig = type.GetAttribute<AssetBundleConfigAttribute>();

                storeTypeDic.Add(type, storeTypeConfig ?? StoreTypeConfigAttribute.DEFAULT);
                authServerConfigDic.Add(type, authServerConfig ?? AuthServerConfigAttribute.DEFAULT);
                assetBundleDic.Add(type, assetBundleConfig ?? AssetBundleConfigAttribute.DEFAULT);
            }
        }

        void OnDisable()
        {
            if (storeTypeDic != null)
                storeTypeDic.Clear();

            if (authServerConfigDic != null)
                authServerConfigDic.Clear();

            if (assetBundleDic != null)
                assetBundleDic.Clear();
        }

        public override void OnInspectorGUI()
        {
            SerializedProperty script = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(script);

            serializedObject.Update();

            // 빌드버전
            SerializedProperty buildVersion = serializedObject.FindProperty("buildVersion");
            // AppVersion
            SerializedProperty appVersion = serializedObject.FindProperty("appVersion");
            // AssetVersion
            SerializedProperty assetVersion = serializedObject.FindProperty("assetVersion");
            // LogoName
            SerializedProperty logoName = serializedObject.FindProperty("logoName");
            // 개발자 모드
            SerializedProperty isDevelopment = serializedObject.FindProperty("isDevelopment");
            // 페이스북
            SerializedProperty isFaceBookFriend = serializedObject.FindProperty("isFaceBookFriend");
            // StoreType
            SerializedProperty storeType = serializedObject.FindProperty("storeType");
            SerializedProperty storeUrl = serializedObject.FindProperty("storeUrl");
            // AuthServerConfig
            SerializedProperty connectIP = serializedObject.FindProperty("serverConfig.connectIP");
            SerializedProperty connectPort = serializedObject.FindProperty("serverConfig.connectPort");
            SerializedProperty resourceUrl = serializedObject.FindProperty("serverConfig.resourceUrl");
            // AssetBundleSettings
            SerializedProperty baseURL = serializedObject.FindProperty("assetBundleSettings.baseURL");
            SerializedProperty mode = serializedObject.FindProperty("assetBundleSettings.mode");

            // 빌드버전
            EditorGUILayout.PropertyField(buildVersion);
            // AppVersion
            EditorGUILayout.PropertyField(appVersion);
            // AssetVersion
            EditorGUILayout.PropertyField(assetVersion);
            // LogoName
            EditorGUILayout.PropertyField(logoName);
            // 개발자모드
            EditorGUILayout.PropertyField(isDevelopment);
            // 페이스북
            EditorGUILayout.PropertyField(isFaceBookFriend);

            EditorGUI.BeginChangeCheck();

            BuildType oldType = Get
            (
                storeType.intValue, // StoreType
                connectIP.stringValue, connectPort.intValue, resourceUrl.stringValue,// AuthServerConfig
                baseURL.stringValue, mode.intValue // AssetBundleSettings
            );

            // Draw Build Type
            BuildType newType = (BuildType)EditorGUILayout.EnumPopup("Build Type", oldType);

            if (EditorGUI.EndChangeCheck())
            {
                StoreTypeConfigAttribute storeTypeConfig = storeTypeDic[newType];
                storeType.intValue = storeTypeConfig.storeType;

                AuthServerConfigAttribute authServerConfig = authServerConfigDic[newType];
                connectIP.stringValue = authServerConfig.connectIP;
                connectPort.intValue = authServerConfig.connectPort;
                resourceUrl.stringValue = authServerConfig.resourceUrl;

                AssetBundleConfigAttribute assetBundleConfig = assetBundleDic[newType];
                baseURL.stringValue = assetBundleConfig.baseURL;
                mode.intValue = assetBundleConfig.mode;

                ObscuredPrefs.DeleteKey(Config.SELECT_SERVER);
                ObscuredPrefs.Save();
            }

            Color backgroundColor = newType == BuildType.Custom ? Color.white : Color.gray;

            if (NGUIEditorTools.DrawHeader("StoreType Settings"))
            {
                NGUIEditorTools.BeginContents();
                {
                    GUI.backgroundColor = backgroundColor;
                    {
                        EditorGUILayout.PropertyField(storeType);
                        EditorGUILayout.PropertyField(storeUrl);
                    }
                    GUI.backgroundColor = Color.white;
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("Server Settings"))
            {
                NGUIEditorTools.BeginContents();
                {
                    GUI.backgroundColor = backgroundColor;
                    {
                        EditorGUILayout.PropertyField(connectIP);
                        EditorGUILayout.PropertyField(connectPort);
                        EditorGUILayout.PropertyField(resourceUrl);
                    }
                    GUI.backgroundColor = Color.white;
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("AssetBundle Settings"))
            {
                NGUIEditorTools.BeginContents();
                {
                    GUI.backgroundColor = backgroundColor;
                    {
                        EditorGUILayout.PropertyField(baseURL);
                        EditorGUILayout.PropertyField(mode);
                    }
                    GUI.backgroundColor = Color.white;
                }
                NGUIEditorTools.EndContents();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private BuildType Get
        (
            int storeType, // StoreType
            string connectIP, int connectPort, string resurceUrl,// AuthServerConfig
            string baseURL, int mode // AssetBundleSettings
        )
        {
            foreach (BuildType type in System.Enum.GetValues(typeof(BuildType)))
            {
                if (!storeTypeDic[type].Equals(storeType))
                    continue;

                if (!authServerConfigDic[type].Equals(connectIP, connectPort, resurceUrl))
                    continue;

                if (!assetBundleDic[type].Equals(baseURL, mode))
                    continue;

                return type;
            }

            return BuildType.Custom;
        }

        bool IEqualityComparer<BuildType>.Equals(BuildType x, BuildType y)
        {
            return x == y;
        }

        int IEqualityComparer<BuildType>.GetHashCode(BuildType obj)
        {
            return obj.GetHashCode();
        }
    }
}