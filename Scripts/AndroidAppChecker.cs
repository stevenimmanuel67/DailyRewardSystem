using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidAppChecker : MonoBehaviour
{
    [Header("CONFIG")]
    public string targetPackageName = "com.example.targetgame";
    public string downloadUrl = "https://google.com";

    public bool IsAppInstalled()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");

            packageManager.Call<AndroidJavaObject>("getPackageInfo", targetPackageName, 0);
            
            Debug.Log($"[AppChecker] App DITEMUKAN: {targetPackageName}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.Log($"[AppChecker] App TIDAK ADA: {e.Message}");
            return false;
        }
#else
        Debug.Log("[AppChecker] Hanya berjalan di Android, Editor selalu return false");
        return false;
#endif
    }
}
