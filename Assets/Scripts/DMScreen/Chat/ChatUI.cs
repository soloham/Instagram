using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
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

        Picture.sprite = Chat.WithProfile.PictureBorderless;
        Username.text = Chat.WithProfile.Name;
        Status.text = Chat.GetStatus();

        ChatAreaManager.OnMessageAdded += ChatAreaManager_OnMessageAdded;
    }

    private void ChatAreaManager_OnMessageAdded(Chat toChat)
    {
        if (toChat != Chat)
        {
            return;
        }

        Status.text = Chat.GetStatus();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        NavigationManager.Instance.NavigateToChatbox(Chat);
    }

    private void OnDestroy()
    {
        ChatAreaManager.OnMessageAdded -= ChatAreaManager_OnMessageAdded;
    }
}
