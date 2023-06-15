using Cysharp.Threading.Tasks;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public Color PointerDownColor;

    private DateTime? pressedTime;
    private bool isPrematureClick;

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

    public async void OnPointerDown(PointerEventData eventData)
    {
        isPrematureClick = true;
        pressedTime = DateTime.Now;
        await UniTask.Delay(50);

        if (pressedTime == null)
        {
            return;
        }
        isPrematureClick = false;

        GetComponent<Image>().color = PointerDownColor;

        var originalMousePosition = Input.mousePosition;

        await UniTask.Delay(400);

        if (pressedTime == null)
        {
            if (originalMousePosition == Input.mousePosition)
            {
                NavigationManager.Instance.NavigateToChatbox(Chat);
            }
            return;
        }

        Debug.Log(" Showing Context Menu");
        ChatContextMenuManager.Instance.Show(this);

        if (Application.platform == RuntimePlatform.Android)
        {
            Handheld.Vibrate();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressedTime = null;
        GetComponent<Image>().color = Color.black;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPrematureClick)
        {
            pressedTime = null;
            NavigationManager.Instance.NavigateToChatbox(Chat);
        }

        isPrematureClick = false;
    }

    private void OnDestroy()
    {
        ChatAreaManager.OnMessageAdded -= ChatAreaManager_OnMessageAdded;
    }
}
