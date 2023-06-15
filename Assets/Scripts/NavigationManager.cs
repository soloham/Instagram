using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;

    public GameObject HomeScreen;
    public GameObject DMScreen;
    public GameObject ChatScreen;

    public ChatScreenManager ChatScreenManager => FindObjectOfType<ChatScreenManager>();

    public UIScrollRectFaster LeftScroller;
    public UIScrollRectFaster RightScroller;

    public RectTransform ScrollContent;

    public TMP_InputField VelocityThresholdTMP;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChatScreen.SetActive(false);
        HomeScreen.SetActive(true);
        DMScreen.SetActive(true);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (LeftScroller.enabled && RightScroller.enabled)
        {
            if (ScrollContent.anchoredPosition.x == 0)
            {
                RightScroller.enabled = false;
            }
            else if (ScrollContent.anchoredPosition.x == -(ScrollContent.sizeDelta.x / 2))
            {
                LeftScroller.enabled = false;
            }
        }

        if (VelocityThresholdTMP == null)
        {
            return;
        }

        var isValid = float.TryParse(VelocityThresholdTMP.text, out float maxThreshold);

        if (isValid)
            LeftScroller.VelocityThreshold = RightScroller.VelocityThreshold = maxThreshold;
    }

    public void NavigateToDMs()
    {
        if (ChatScreenManager != null)
        {
            ChatScreenManager.DeInitialise();
        }

        ChatScreen.SetActive(false);
        //TouchScreenKeyboard.Android.consumesOutsideTouches = false;
    }

    public void NavigateToChatbox(Chat chat)
    {
        ChatScreen.SetActive(true);

        ChatScreenManager.Initialise(chat);
        //TouchScreenKeyboard.Android.consumesOutsideTouches = false;
    }

    public void NavigateToHomeScreen()
    {
        ChatScreen.SetActive(false);
        RightScroller.NavigateBackFromDMs();
        //TouchScreenKeyboard.Android.consumesOutsideTouches = false;
    }
}
