using Assets.Scripts;
using Assets.Scripts.ChatScreen;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using TMPro;

using UnityEngine;

using Assets.Scripts.Helpers;
using System.Threading.Tasks;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    public List<Profile> Profiles;

    public string LoggedInProfileHandle;

    public Settings Settings { get; set; }

    [HideInInspector]
    public Profile LoggedInProfile => Profiles.SingleOrDefault(x => x.Handle == LoggedInProfileHandle);

    public TextMeshProUGUI statusText; // Reference to the StatusText field in the UI
    public TextMeshProUGUI fpsText;

    public GameObject SplashImageObject;

    public TextAsset credentialsFile;

    CancellationTokenSource cancellationTokenSource;

    public bool IsInEditMode = false;

    public delegate void EditModeDelegate(bool isEditingAllowed);
    public static event EditModeDelegate OnEditModeChanged;

    private DateTime? editModeTogglePressedTime;
    private bool isEditModeTogglePressed;

    private void Awake()
    {
        Instance = this;

        ChatScreenManager.OnSave += ChatScreenManager_OnSave;
    }

    public void OnEditModeToggle_Pressed()
    {
        if (!Settings.AllowEditing)
        {
            return;
        }

        editModeTogglePressedTime = DateTime.Now;
        isEditModeTogglePressed = true;
    }

    public void OnEditModeToggle_Unpressed()
    {
        if (!Settings.AllowEditing)
        {
            return;
        }

        if (isEditModeTogglePressed)
        {
            var secondsElapsed = (DateTime.Now - editModeTogglePressedTime.Value).Seconds;
            if (secondsElapsed >= 3)
            {
                SetAllowEditing(!IsInEditMode);

                if (Application.platform == RuntimePlatform.Android)
                {
                    Handheld.Vibrate();
                }
            }
        }

        editModeTogglePressedTime = null;
        isEditModeTogglePressed = false;
    }

    private void SetAllowEditing(bool isAllowed)
    {
        IsInEditMode = isAllowed;

        if (OnEditModeChanged != null)
        {
            OnEditModeChanged.Invoke(IsInEditMode);
        }
    }

    // Start is called before the first frame update
    async UniTaskVoid Start()
    {
        SplashImageObject.SetActive(true);

        Application.targetFrameRate = Screen.currentResolution.refreshRate;

        //var json = GetAllChatMessagesAsJson();

        // Check for an internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            SetStatusText("No internet connection available. Skipping update.");
            return;
        }

        await InitialiseData();
    }

    private async UniTask ChatScreenManager_OnSave()
    {
        if (!Settings.AllowEditing)
        {
            return;
        }

        var allChatMessagesJson = GetAllChatMessagesAsJson();

        await DriveHelper.UploadFileByName("allChatMessages.json", allChatMessagesJson, SetStatusText);

        await DriveHelper.DownloadFileByName("settings.json", GetSettingsFilePath(), SetStatusText);
        var latestSettings = GetDownloadedSettings();

        Settings.Version++;
        var settingsJson = JsonConvert.SerializeObject(Settings);

        await DriveHelper.UploadFileByName("settings.json", settingsJson, SetStatusText);
    }

    private async Task InitialiseData()
    {
        Profiles = new List<Profile>();

        Settings = GetDownloadedSettings();

        try
        {
            SetStatusText("Downloading Settings...");
            await DriveHelper.DownloadFileByName("settings.json", GetSettingsFilePath(), SetStatusText);
        }
        catch
        {
            SetStatusText($"Failed to downloading Settings, {(Settings == null ? "connect to internet" : "using local version")}");
        }

        var latestSettings = GetDownloadedSettings();

        var resetProfiles = false;
        if (Settings == null || (latestSettings.Version > Settings.Version && latestSettings.ForceLoadMessage))
        {
            Settings = latestSettings;
            resetProfiles = true;

            SetStatusText("Downloading Messages...");
            await DriveHelper.DownloadFileByName("allChatMessages.json", GetMessagesFilePath(), SetStatusText);
        }

        Settings = latestSettings;

        await ImportChatMessages(resetProfiles);

        DMScreenHeaderManager.Instance.Initialise();
        DMSreenMessagesManager.Instance.Initialise();

        SetStatusText("Downloading Feed...");

        var feedsDownloadTasks = Settings.Feeds.Select(async feed => await MessagePhotoManager.EnsurePhotoExists(feed.FeedUID, SetStatusText)).ToArray();
        await UniTask.WhenAll(feedsDownloadTasks);

        HomeScreenManager.Instance.Initialise();

        SplashImageObject.SetActive(false);

        foreach (var profile in Profiles)
        {
            try
            {
                foreach (var chat in profile.Chats)
                {
                    var chatMessages = chat.ToChatMessages();
                    for (int i = chatMessages.Messages.Count - 1; i >= 0; i--)
                    {
                        if (!Application.isPlaying)
                        {
                            return;
                        }

                        var message = chatMessages.Messages[i].Message;

                        if (message.Photos?.Count > 0)
                        {
                            await MessagePhotoManager.EnsurePhotoExists(message.Photos.First().Uri, SetStatusText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    // Update is called once per frame
    private float deltaTime;
    void Update()
    {
        if (fpsText != null)
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = "FPS: " + Mathf.RoundToInt(fps);
        }
    }

    string GetAllChatMessagesAsJson()
    {
        var allChatMessages = new AllChatMessagesRaw();
        allChatMessages.Profiles = Profiles
            .Select(x => new ProfileRaw
            {
                Name = x.Name,
                Handle = x.Handle,
                PictureUID = x.ProfileUID
            })
            .ToList();

        var allChats = Profiles.SelectMany(x => x.Chats).ToList();
        foreach (var chat in allChats)
        {
            if (allChatMessages.Conversations.Any(x => x.Participants.Contains(chat.FromProfileHandle) && x.Participants.Contains(chat.WithProfileHandle)))
            {
                continue;
            }

            var chatMessages = chat.ToChatMessagesRaw();
            allChatMessages.Conversations.Add(chatMessages);
        }

        return JsonConvert.SerializeObject(allChatMessages);
    }

    string GetMessagesFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "messages.json");
    }

    string GetProfilePicFilePath(string name)
    {
        return Path.Combine(Application.persistentDataPath, name);
    }

    string GetSettingsFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "settings.json");
    }

    Settings GetDownloadedSettings()
    {
        var path = GetSettingsFilePath();
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<Settings>(json);
    }

    async UniTask ImportChatMessages(bool resetProfiles)
    {
        var json = File.ReadAllText(GetMessagesFilePath());
        var allChatMessages = JsonConvert.DeserializeObject<AllChatMessagesRaw>(json);

        if (resetProfiles)
        {
            allChatMessages.Profiles.ForEach(x =>
            {
                var profilePicPath = GetProfilePicFilePath(x.PictureUID);

                if (File.Exists(profilePicPath))
                {
                    File.Delete(profilePicPath);
                }
            });
        }

        var allProfileUIds = allChatMessages.Profiles.Select(x => x.PictureUID).ToList();

        var uniqueProfileTasks = allChatMessages.Profiles.Where(x => allProfileUIds.Count(y => y == x.PictureUID) == 1).ToList();
        var nonUniqueProfileTasks = allChatMessages.Profiles.Where(x => !uniqueProfileTasks.Contains(x)).ToList();

        var initialiseProfilesTasks = uniqueProfileTasks.Select(async x => await InitialiseProfile(x)).ToArray();

        Profiles = (await UniTask.WhenAll(initialiseProfilesTasks)).ToList();

        foreach (var profile in nonUniqueProfileTasks)
        {
            Profiles.Add(await InitialiseProfile(profile));
        }

        Profiles.ForEach(x => x.Hydrate(allChatMessages));
    }

    private async UniTask<Profile> InitialiseProfile(ProfileRaw rawProfile)
    {
        try
        {
            await MessagePhotoManager.EnsurePhotoExists(rawProfile.PictureUID, SetStatusText);
        }
        catch (Exception ex)
        {
            SetStatusText(ex.Message);
        }

        var profile = new Profile
        {
            Name = rawProfile.Name,
            Handle = rawProfile.Handle,

            Picture = MessagePhotoManager.LoadSprite(rawProfile.PictureUID),
            PictureBorderless = MessagePhotoManager.LoadSprite(rawProfile.PictureUID),
            ProfileUID = rawProfile.PictureUID
        };
        profile.PictureBorderless = profile.Picture;

        return profile;
    }

    void SetStatusText(string text)
    {
        if (statusText != null && (Settings?.VerboseSplashScreen ?? false))
        {
            statusText.text = text;
        }

        Debug.Log(text);
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        ChatScreenManager.OnSave -= ChatScreenManager_OnSave;
    }
}
