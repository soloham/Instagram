using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using System;

public class GameUpdater : MonoBehaviour
{
    public TextMeshProUGUI statusText; // Reference to the StatusText field in the UI

    // Replace with the download link for the latest APK
    private string apkDownloadLink = "https://drive.google.com/uc?export=download&id=1Xq4fxF7dbl_-fO3NUO2cD03iIsULP0hS";

    IEnumerator Start()
    {
        // Check for an internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            SetStatusText("No internet connection available. Skipping update.");
            yield break;
        }

        // Download the APK file using the download link
        yield return StartCoroutine(DownloadAPK(apkDownloadLink));

        // Install the APK file
        yield return StartCoroutine(InstallAPK());

        // Launch the updated game
        LaunchUpdatedGame();
    }

    IEnumerator DownloadAPK(string downloadLink)
    {
        SetStatusText("Downloading APK...");

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(downloadLink))
        {
            string filePath = GetAPKFilePath();
            unityWebRequest.downloadHandler = new DownloadHandlerFile(filePath);

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                SetStatusText("Failed to download APK: " + unityWebRequest.error);
                yield break;
            }

            SetStatusText("APK downloaded successfully!");
        }
    }

    string GetAPKFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "game.apk");
    }

    IEnumerator InstallAPK()
    {
        SetStatusText("Installing APK...");

        string apkFilePath = GetAPKFilePath();

        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {

                //Get Activity then Context
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

                //Get the package Name
                string packageName = unityContext.Call<string>("getPackageName");
                string authority = packageName + ".fileprovider";

                AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
                string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);


                int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
                int FLAG_GRANT_READ_URI_PERMISSION = intentObj.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");

                //File fileObj = new File(String pathname);
                AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkFilePath);
                //FileProvider object that will be used to call it static function
                AndroidJavaClass fileProvider = new AndroidJavaClass("android.support.v4.content.FileProvider");
                //getUriForFile(Context context, String authority, File file)
                AndroidJavaObject uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj);

                intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
                intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
                intent.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION);
                currentActivity.Call("startActivity", intent);
            }
            catch (Exception ex)
            {
                SetStatusText(ex.Message);
                yield break;
            }

            // Wait for the installation to complete
            while (IsInstallingAPK())
            {
                yield return null;
            }
        }
        else
        {
            SetStatusText("APK installation is only supported on Android.");
            yield break;
        }

        SetStatusText("APK installation complete!");
    }

    AndroidJavaObject GetCurrentActivity()
    {
        var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
    }

    bool IsInstallingAPK()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var packageManagerClass = new AndroidJavaClass("android.content.pm.PackageManager");
            var packageManager = GetCurrentActivity().Call<AndroidJavaObject>("getPackageManager");
            var packageName = GetCurrentActivity().Call<string>("getPackageName");

            const int INSTALLING = 1; // PackageManager.INSTALL_ONGOING
            int installStatus = packageManager.Call<int>("getPackageInstallationStatus", packageName);

            return installStatus == INSTALLING;
        }

        return false;
    }
    void LaunchUpdatedGame()
    {
        SetStatusText("Launching updated game..." + GetAPKFilePath());

        // Replace "com.yourgame.package" with your game's package identifier
        string packageName = "com.Stellarsoft.Instagram";

        try
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                // Launch the updated game using the package name
                string uri = $"package:{packageName}";
                Application.OpenURL(uri);
            }
            else
            {
                SetStatusText("Launching the updated game is only supported on Android.");
            }
        }
        catch (Exception ex)
        {
            SetStatusText(ex.Message);
        }
    }

    void SetStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }

        Debug.Log(text);
    }
}
