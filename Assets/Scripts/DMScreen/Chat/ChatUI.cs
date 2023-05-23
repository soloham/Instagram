using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public Image Picture;
    public TextMeshProUGUI Username;
    public TextMeshProUGUI Status;

    [HideInInspector]
    public List<Profile> Profiles;

    public Chat Chat;

    private void Awake()
    {
        Profiles = ProfileManager.Instance?.Profiles;
    }

    public void Initialise(Chat chat)
    {
        Chat = chat;

        //Picture.sprite = Chat.WithProfile.PictureBorderless;
        Username.text = Chat.WithProfile.Name;
        Status.text = Chat.GetStatus();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OpenChat()
    {
        NavigationManager.Instance.NavigateToChatbox(Chat);
    }
}
