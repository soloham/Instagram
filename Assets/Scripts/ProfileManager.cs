using Assets.Scripts;
using Assets.Scripts.ChatScreen;

using Newtonsoft.Json;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.Networking;

using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Assets.Scripts.Helpers;
using System.Threading.Tasks;
using System.Threading;
using Cysharp.Threading.Tasks;

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

    private void Awake()
    {
        Instance = this;
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

    private async Task InitialiseData()
    {
        Settings = GetDownloadedSettings();

        await DriveHelper.DownloadFileByName("settings.json", GetSettingsFilePath(), SetStatusText);

        var latestSettings = GetDownloadedSettings();

        if (Settings == null || (latestSettings.Version > Settings.Version && latestSettings.ForceLoadMessage))
        {
            await DriveHelper.DownloadFileByName("allChatMessages.json", GetMessagesFilePath(), SetStatusText);
        }

        Settings = latestSettings;

        await ImportChatMessages();

        DMScreenHeaderManager.Instance.Initialise();
        DMSreenMessagesManager.Instance.Initialise();

        foreach (var feed in Settings.Feeds)
        {
            await MessagePhotoManager.EnsurePhotoExists(feed.FeedUID);
        }

        HomeScreenManager.Instance.InitialiseFeeds();

        SplashImageObject.SetActive(false);

        foreach (var profile in Profiles)
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
                        await MessagePhotoManager.EnsurePhotoExists(message.Photos.First().Uri);
                    }
                }
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

    async UniTask ImportChatMessages()
    {
        var json = File.ReadAllText(GetMessagesFilePath());
        var allChatMessages = JsonConvert.DeserializeObject<AllChatMessagesRaw>(json);

        var initialiseProfilesTasks = allChatMessages.Profiles.Select(async x =>
        {
            await MessagePhotoManager.EnsurePhotoExists(x.PictureUID);

            var profile = new Profile
            {
                Name = x.Name,
                Handle = x.Handle,

                Picture = MessagePhotoManager.LoadSprite(x.PictureUID)
            };
            profile.PictureBorderless = profile.Picture;

            return profile;
        }).ToArray();

        Profiles = (await UniTask.WhenAll(initialiseProfilesTasks)).ToList();

        Profiles.ForEach(x => x.Hydrate(allChatMessages));
    }

    void SetStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }

        Debug.Log(text);
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
    }
}
