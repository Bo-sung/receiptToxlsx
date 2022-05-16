using UnityEngine;
using System;
using System.Collections;

public class PerftestManager : MonoBehaviour
{
#if UNITY_ANDROID
	static AndroidJavaObject appguardClass;

	static public void SetId(string userId) // set user id
	{
		try
        {
			if(appguardClass == null)
				appguardClass = new AndroidJavaClass("com.nhnent.appguard.Diresu");

			if (appguardClass != null){
                appguardClass.CallStatic("s", userId);
			}
		}
		catch(Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}
	}

    static public void SetCallback(string classPath, string functionName, bool useMsgBox) // set callback
	{
		try
        {
			if(appguardClass == null)
				appguardClass = new AndroidJavaClass("com.nhnent.appguard.Diresu");

			if (appguardClass != null){
                appguardClass.CallStatic("o", classPath, functionName, true, useMsgBox);
			}
		}
		catch(Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}
	}

	static public void ED(byte[] data) // set callback
	{
		try
		{
			if(appguardClass == null)
				appguardClass = new AndroidJavaClass("com.nhnent.appguard.Diresu");
			if (appguardClass != null) {
				appguardClass.CallStatic("e", data);
			}
		}
		catch(Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}
    }

    static public void B(int blockPolicy)
    {
        try
        {
            if(appguardClass == null)
                appguardClass = new AndroidJavaClass("com.nhnent.appguard.Diresu");
            if (appguardClass != null) {
                appguardClass.CallStatic("b", blockPolicy);
            }
        }
        catch(Exception e)
        {
            UnityEngine.Debug.Log(e.ToString());
        }
    }
#else
    static public void SetId(string userId) { }
    static public void SetCallback(string classPath, string functionName, bool useMsgBox) { }
	static public void ED(byte[] data) { }
    static public void B(int blockPolicy) { }
#endif
}
