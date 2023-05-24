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

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    public List<Profile> Profiles;

    public string LoggedInProfileHandle;

    [HideInInspector]
    public Profile LoggedInProfile => Profiles.SingleOrDefault(x => x.Handle == LoggedInProfileHandle);

    public TextMeshProUGUI statusText; // Reference to the StatusText field in the UI
    public TextMeshProUGUI fpsText;

    public GameObject SplashImageObject;

    // Replace with the download link for the latest settings
    private string settingsDownloadLink =
        "https://drive.google.com/uc?export=download&id=12rcOzFkuT9sHuF5XwQz5szkaVSlufPVY";

    // Replace with the download link for the latest messages
    private string messagesDownloadLink =
        "https://drive.google.com/uc?export=download&id=1M8YZzGzl5TNBXXWyTTIslGzO4YD6dyYG";

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        SplashImageObject.SetActive(true);

        Application.targetFrameRate = Screen.currentResolution.refreshRate;

        //var json = GetAllChatMessagesAsJson();

        // Check for an internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            SetStatusText("No internet connection available. Skipping update.");
            yield break;
        }

        var currentSettings = GetDownloadedSettings();

        yield return StartCoroutine(DownloadJson(settingsDownloadLink, GetSettingsFilePath(), "Settings"));

        var latestSettings = GetDownloadedSettings();

        if (currentSettings == null || (latestSettings.Version > currentSettings.Version && latestSettings.ForceLoadMessage))
        {
            yield return StartCoroutine(DownloadJson(messagesDownloadLink, GetMessagesFilePath(), "Messages"));
        }

        ImportChatMessages();

        DMScreenHeaderManager.Instance.Initialise();
        DMSreenMessagesManager.Instance.Initialise();

        SplashImageObject.SetActive(false);
    }

    // Update is called once per frame
    private float deltaTime;
    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = "FPS: " + Mathf.RoundToInt(fps);
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

    IEnumerator DownloadJson(string downloadUrl, string filePath, string type)
    {
        SetStatusText($"Downloading {type}...");

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(downloadUrl))
        {
            unityWebRequest.downloadHandler = new DownloadHandlerFile(filePath);

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                SetStatusText($"Failed to download {type}: " + unityWebRequest.error);
                yield break;
            }

            SetStatusText($"{type} downloaded successfully!");
        }
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

    void ImportChatMessages()
    {
        var json = File.ReadAllText(GetMessagesFilePath());
        var allChatMessages = JsonConvert.DeserializeObject<AllChatMessagesRaw>(json);

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
}
