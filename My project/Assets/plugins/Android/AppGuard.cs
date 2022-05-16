#if UNITY_EDITOR
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

public class AppGuard : EditorWindow{
	private static string version = "2.3.2";
	private static string appGuardCli, productName, projectRootPath, level;
	private static string unprotectedApk, buildOutputFilePath, tempApk;
    private static string appGuardAppKey, keystore, keystorePassword, keystoreAlias, keystoreAliasPassword;
	private static bool isSelectKestore = false;
	private static Texture _logoTexture;
    private static bool protectionLevel1 = false;
    private static bool protectionLevel2 = false;
    private static bool protectionLevel3 = false;
	private static AppGuard window;
	private static bool checkWindow = false; 
	
	// Can modify this variable//
	public static BuildOptions buildOption = BuildOptions.None;

	// For Unity Editor
	[MenuItem ("AppGuard/Build and Protect")]
	public static void BuildSetting(MenuCommand menuCommand)
	{
		window = (AppGuard)EditorWindow.GetWindow(typeof(AppGuard));
		window.Show();
		checkWindow = true;
	}

	// For Command Line Build.
	public static void Build()
	{			
		string[] arguments = Environment.GetCommandLineArgs();
		int argNum = arguments.GetLength(0);
		for (int i = 0; i < argNum; i++)
		{
			if (arguments[i] == "-output")
			{
				buildOutputFilePath = arguments[i+1];
			}
		}
		if (IsNull(buildOutputFilePath))
		{
			return;
		}
		if (LoadData() == true)
		{
			UnityEditor.PlayerSettings.Android.keystoreName = keystore;
			UnityEditor.PlayerSettings.Android.keystorePass = keystorePassword;
			UnityEditor.PlayerSettings.Android.keyaliasName = keystoreAlias;
			UnityEditor.PlayerSettings.Android.keyaliasPass = keystoreAliasPassword;
			MakeAPK();
		}
	}

    public static void MakeAPK()
    {
        BuildAndroidProject(".apk");
        EditorUtility.DisplayProgressBar("Protect apk", "Wait few seconds...", (float)0.7);
        ProtectAppGuard(tempApk);
        DeleteTempFile();
        EditorUtility.ClearProgressBar();
        if (checkWindow != false)
        {
            window.Close();
        }
    }

    public static void MakeAAB()
    {
        BuildAndroidProject(".aab");
        EditorUtility.DisplayProgressBar("Protect aab", "Wait few seconds...", (float)0.7);
        ProtectAppGuard(tempApk);
        DeleteTempFile();
        EditorUtility.ClearProgressBar();
        if (checkWindow != false)
        {
            window.Close();
        }
    }

	void OnGUI()
	{
		InitPath();
        setOptions();

		appGuardCli = EditorGUILayout.TextField("AppGuard CLI path", appGuardCli);
		appGuardAppKey = EditorGUILayout.TextField("AppGuard AppKey", appGuardAppKey);
		if (IsNull(UnityEditor.PlayerSettings.Android.keystoreName) || IsNull(UnityEditor.PlayerSettings.Android.keyaliasName))
		{
			EditorGUILayout.LabelField("Keystore", "Please, input your keystore infomation.");
			EditorGUILayout.LabelField(" ", "Build Settings -> Player Settings -> Android Setting -> Publishing Settings");
			isSelectKestore = false;
		}
		else
		{
			EditorGUILayout.LabelField("Keystore", keystore);
			EditorGUILayout.LabelField("Keystore alias name", keystoreAlias);
			isSelectKestore = true;
		}
		SaveData();

		if (GUILayout.Button("Build APK"))
        {
            if (CheckSettings())
            {
                buildOutputFilePath = EditorUtility.SaveFilePanel("Build Android", "", productName + ".apk", "apk");
                if (buildOutputFilePath.Length != 0)
                {
                    MakeAPK();
                }
            }
        }

		if (GUILayout.Button("Build AAB"))
        {
            if (CheckSettings())
            {
                buildOutputFilePath = EditorUtility.SaveFilePanel("Build Android", "", productName + ".aab", "aab");
                if (buildOutputFilePath.Length != 0)
                {
                    MakeAAB();
                }
            }
        }

		// For Personal Edition
		/*
		if (GUILayout.Button("Protect"))
		{
			if (CheckSettings())
			{
				string unprotectedApk = EditorUtility.OpenFilePanel("Select Apk File", "", "apk");
				buildOutputFilePath = EditorUtility.SaveFilePanel("Protected(Output) Apk", "", productName + ".apk", "apk");
				if (buildOutputFilePath.Length != 0) {
					ProtectAppGuard(unprotectedApk);
				}
			}
		}
		*/
		EditorGUILayout.LabelField("", version);
	}

	public static void InitPath()
	{
		LoadData();
		if (IsNull(UnityEditor.PlayerSettings.Android.keystoreName) == false)
		{
			keystore = UnityEditor.PlayerSettings.Android.keystoreName;
		}
		if (IsNull(UnityEditor.PlayerSettings.Android.keystorePass) == false)
		{
			keystorePassword = UnityEditor.PlayerSettings.Android.keystorePass;
		}
		if (IsNull(UnityEditor.PlayerSettings.Android.keyaliasName) == false)
		{
			keystoreAlias = UnityEditor.PlayerSettings.Android.keyaliasName;
		}
		if (IsNull(UnityEditor.PlayerSettings.Android.keyaliasPass) == false)
		{
			keystoreAliasPassword = UnityEditor.PlayerSettings.Android.keyaliasPass;
		}

		projectRootPath = Path.GetFullPath(Application.dataPath + "/..");

#if UNITY_EDITOR_WIN
        appGuardCli = Application.dataPath + "/AppGuard/windows/AppGuard.exe";

        // 키스토어가 상대경로로 주어지는 경우가 있음
        if (IsNull(keystore) == false)
        {
            if (keystore[1] != ':')
            {
                keystore = Path.Combine(projectRootPath, keystore);
            }
        }
#elif UNITY_EDITOR_OSX
		appGuardCli = Application.dataPath + "/AppGuard/mac/AppGuard";

        		if (IsNull(keystore) == false)
		{
			if (keystore[0] != '/')
			{
				keystore = Path.Combine(projectRootPath, keystore);
			}
		}
#endif
    }

    public static void setOptions()
    {
        _logoTexture = AssetDatabase.LoadAssetAtPath(@"Assets/Plugins/Android/AppGuard.png", typeof(Texture)) as Texture;
        GUI.DrawTexture(new Rect(190, 25, 290, 140), _logoTexture);

        GUI.Label(new Rect(130, 180, 170, 20), "AppGuard Protection Level");

        if (level == "--level1")
            protectionLevel1 = true;
        else if (level == "--level2")
            protectionLevel2 = true;
        else if (level == "--level3")
            protectionLevel3 = true;

        protectionLevel1 = GUI.Toggle(new Rect(320, 180, 70, 20), protectionLevel1, "Level 1");
        if (protectionLevel1 == true)
        {
            protectionLevel2 = false;
            protectionLevel3 = false;
            level = "--level1";
        }
        protectionLevel2 = GUI.Toggle(new Rect(390, 180, 70, 20), protectionLevel2, "Level 2");
        if (protectionLevel2 == true)
        {
            protectionLevel1 = false;
            protectionLevel3 = false;
            level = "--level2";
        }
        protectionLevel3 = GUI.Toggle(new Rect(460, 180, 70, 20), protectionLevel3, "Level 3");
        if (protectionLevel3 == true)
        {
            protectionLevel1 = false;
            protectionLevel2 = false;
            level = "--level3";
        }

        GUILayout.Space(200);
    }
  
	public static void BuildAndroidProject(string extension) {
		// add scenes
		List<string> sceneList = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (scene.enabled)
				sceneList.Add(scene.path);
		}
		string[] sceneArray = sceneList.ToArray();
		
		// Build Android Project
		tempApk = Path.Combine(Path.GetDirectoryName(buildOutputFilePath), Path.GetFileNameWithoutExtension(buildOutputFilePath) + "_appguard" + extension);
		string res = BuildPipeline.BuildPlayer(sceneArray, tempApk, BuildTarget.Android, buildOption).ToString();

		if (res.Length > 0 && res.Equals("New Report (UnityEngine.BuildReport)") != true)
		{
			throw new Exception("BuildPlayer failure: " + res);
		}
	}
    
	private static void ProtectAppGuard(string unprotectedApk)
	{
        string args = " -k \"" + keystore + "\" -a \"" + keystoreAlias + "\" -p \"" + keystorePassword + "\" -n \"" + unprotectedApk + "\" -o \"" + buildOutputFilePath + "\" -v " + appGuardAppKey + " " + level;
        UnityEngine.Debug.Log(appGuardCli + " " + args);
        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.FileName = appGuardCli;
        p.StartInfo.Arguments = args;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        string error = p.StandardError.ReadToEnd();
        p.WaitForExit();
        p.Close();
        if (output != "")
            UnityEngine.Debug.Log(output);
        if (error != "")
            UnityEngine.Debug.LogError(error);
	}

	private static void DeleteTempFile()
	{
		if(File.Exists(tempApk))
		{
			try
			{
				UnityEngine.Debug.Log("AppGuard Protection Complete.");
				File.Delete(tempApk);
			}
			catch (IOException e)
			{
				UnityEngine.Debug.Log(e.Message);
				return;
			}
		}
		else
		{
			UnityEngine.Debug.LogError("Build Fail");
		}
	}

	private bool CheckSettings()
	{
		if (isSelectKestore == false)
		{
			UnityEngine.Debug.LogError("Keystore not found.");
			EditorUtility.DisplayDialog("Input your keystore infomation.", "Build Settings -> Player Settings -> Android Setting -> Publishing Settings", "OK");
			return false;
		}

		if (IsNull(UnityEditor.PlayerSettings.Android.keystorePass))
		{
			UnityEngine.Debug.LogError("Input your keystore Password.");
			EditorUtility.DisplayDialog("Input your keystore Password.", "Build Settings -> Player Settings -> Android Setting -> Publishing Settings", "OK");
			return false;
		}

		if (IsNull(UnityEditor.PlayerSettings.Android.keyaliasPass))
		{
			UnityEngine.Debug.LogError("Input your keystore alias Password.");
			EditorUtility.DisplayDialog("Input your keystore alias Password.", "Build Settings -> Player Settings -> Android Setting -> Publishing Settings", "OK");
			return false;
		}

		if (IsNull(appGuardAppKey))
		{
			UnityEngine.Debug.LogError("Input AppKey.");
			EditorUtility.DisplayDialog("Input AppKey.", "Input your \''Toast App Guard\'s AppKey\'", "OK");
			return false;
		}

		if (IsValidFilePath(appGuardCli) && IsValidFilePath(keystore))
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	static private bool IsValidFilePath(string filePath)
	{
		if (File.Exists(filePath) || Directory.Exists(filePath))
		{
			return true;
		}
		else
		{
			UnityEngine.Debug.LogError("File not found " + filePath);
			EditorUtility.DisplayDialog("Please, Check path.", "File not found " + filePath, "OK");
			return false;
		}
	}
		
	public static bool IsNull(string input)
	{
		if ((input == null) || (input == ""))
			return true;
		else
			return false;
	}

	public static void SaveData()
	{
        EditorUserSettings.SetConfigValue("appGuardCli", appGuardCli);
        EditorUserSettings.SetConfigValue("projectRootPath", projectRootPath);
		EditorUserSettings.SetConfigValue("level", level);
		if (IsNull(appGuardAppKey) == false)
		{
            EditorUserSettings.SetConfigValue("appGuardAppKey", appGuardAppKey);
		}
		if (IsNull(keystore) == false)
		{
            EditorUserSettings.SetConfigValue("keystore", keystore);
		}
		if (IsNull(keystoreAlias) == false)
		{
            EditorUserSettings.SetConfigValue("keystoreAlias", keystoreAlias);
		}
		if (IsNull(keystorePassword) == false)
		{
            EditorUserSettings.SetConfigValue("keystorePassword", keystorePassword);
		}
		if (IsNull(keystoreAliasPassword) == false)
		{
            EditorUserSettings.SetConfigValue("keystoreAliasPassword", keystoreAliasPassword);
		}
	}

	public static bool LoadData()
	{
		productName = UnityEditor.PlayerSettings.productName;
		projectRootPath = EditorUserSettings.GetConfigValue("projectRootPath");
		appGuardCli = EditorUserSettings.GetConfigValue("appGuardCli");
		appGuardAppKey = EditorUserSettings.GetConfigValue("appGuardAppKey");
		keystore = EditorUserSettings.GetConfigValue("keystore");
		keystorePassword = EditorUserSettings.GetConfigValue("keystorePassword");
		keystoreAlias = EditorUserSettings.GetConfigValue("keystoreAlias");
		keystoreAliasPassword = EditorUserSettings.GetConfigValue("keystoreAliasPassword");
        level = EditorUserSettings.GetConfigValue("level");
        if (level == null)
        {
            level = "--level1";
        }
        if (IsNull(appGuardCli) || IsNull(appGuardAppKey) || IsNull(keystore)
		    || IsNull(keystorePassword) || IsNull(keystoreAlias) 
		    || IsNull(keystoreAliasPassword) || IsNull(projectRootPath))
		{
			return false;
		}
		else
		{
			return true;
		}
	}
}
#endif
