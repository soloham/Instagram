using AdvancedInputFieldPlugin;

using Cysharp.Threading.Tasks;

using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ChatScreenManager : MonoBehaviour
{
    public AdvancedInputField SearchField;

    public Image ProfilePicture;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Handle;

    public GameObject BlockedUIObject;
    public TextMeshProUGUI BlockedUIDetailsText;
    public GameObject MessageUIObject;

    public Chat Chat;

    public GameObject ChatAreaObject;

    // Used internally - set by InputfieldFocused.cs
    public bool InputFieldActive = false;
    public RectTransform childRectTransform;

    public RectTransform movableRectTransform;

    public bool TestKeyboardHeight;
    public float KeybordHeightToTest;

    [Header("Saver")]
    public GameObject SaverBtn;
    public GameObject SaverLoaderObj;

    public delegate UniTask SaveMessagesDelegate();
    public static event SaveMessagesDelegate OnSave;

    public void SaveAll()
    {
        if (!ProfileManager.Instance.IsInEditMode)
        {
            return;
        }

        OnSaveAll();
    }

    public async UniTask OnSaveAll()
    {
        if (!ProfileManager.Instance.IsInEditMode)
        {
            return;
        }

        SaverBtn.SetActive(false);
        SaverLoaderObj.SetActive(true);

        if (OnSave != null)
        {
            await OnSave();
        }

        await UniTask.Delay(1000);

        SaverBtn.SetActive(true);
        SaverLoaderObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NavigationManager.Instance.NavigateToDMs();
        }

#if UNITY_ANDROID
        if (SearchField.Selected)
        {
            InputFieldActive = true;
            childRectTransform = SearchField.GetComponent<RectTransform>();
        }
#endif

        if (Chat == null || ProfilePicture == null)
        {
            return;
        }

        ProfilePicture.sprite = Chat.WithProfile.PictureBorderless;
        Name.text = Chat.WithProfile.Name;
        Handle.text = Chat.WithProfile.Handle;
    }

    public void Initialise(Chat chat)
    {
        Chat = chat;
        ChatAreaObject.GetComponent<ChatAreaManager>().Initialise();

        if (Chat.Blocked)
        {
            MessageUIObject.SetActive(false);
            BlockedUIObject.SetActive(true);
            BlockedUIObject.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(BlockedUIObject.transform.parent.GetComponent<RectTransform>().sizeDelta.x, 102);

            var chatAreaRect = ChatAreaObject.GetComponent<RectTransform>();
            chatAreaRect.offsetMin = new Vector2(chatAreaRect.offsetMax.x, 110);

            BlockedUIDetailsText.text = $"You can't message or video chat with {chat.WithProfile.Name}\r\n({chat.WithProfileHandle})";
        }
        else
        {
            MessageUIObject.SetActive(true);
            BlockedUIObject.SetActive(false);
            MessageUIObject.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(MessageUIObject.transform.parent.GetComponent<RectTransform>().sizeDelta.x, 52);

            var chatAreaRect = ChatAreaObject.GetComponent<RectTransform>();
            chatAreaRect.offsetMin = new Vector2(chatAreaRect.offsetMax.x, 52);
        }
    }

    public void DeInitialise()
    {
        Chat = null;
        ChatAreaObject.GetComponent<ChatAreaManager>().DeInitialise();
    }


    void LateUpdate()
    {
        if ((InputFieldActive && TouchScreenKeyboard.visible) || TestKeyboardHeight)
        {
            childRectTransform = SearchField.GetComponent<RectTransform>();

            movableRectTransform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            Rect rect = RectTransformExtension.GetScreenRect(childRectTransform, FindObjectOfType<Canvas>());
            float keyboardHeight = GetKeyboardHeight(false);

            float heightPercentOfKeyboard = keyboardHeight / Screen.height * 100f;
            float heightPercentOfInput = (Screen.height - (rect.y + rect.height)) / Screen.height * 100f;

            if ((heightPercentOfKeyboard > heightPercentOfInput) || TestKeyboardHeight)
            {
                // keyboard covers input field so move screen up to show keyboard
                float differenceHeightPercent = heightPercentOfKeyboard - heightPercentOfInput;
                float newYPos = TestKeyboardHeight ? KeybordHeightToTest : (movableRectTransform.GetComponent<RectTransform>().rect.height / 100f * differenceHeightPercent) + 19;

                Vector2 newAnchorPosition = Vector2.zero;
                newAnchorPosition.y = newYPos;
                movableRectTransform.GetComponent<RectTransform>().anchoredPosition = newAnchorPosition;
            }
            else
            {
                // Keyboard top is below the position of the input field, so leave screen anchored at zero
                movableRectTransform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            // No focus or touchscreen invisible, set screen anchor to zero
            movableRectTransform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        InputFieldActive = false;
    }

    private static int GetKeyboardHeight(bool includeInput)
    {
#if UNITY_EDITOR
        return 0;
#elif UNITY_ANDROID
            using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
                AndroidJavaObject view = unityPlayer.Call<AndroidJavaObject>("getView");
                AndroidJavaObject dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
                if (view == null || dialog == null)
                    return 0;
                var decorHeight = 0;
                if (includeInput)
                {
                    AndroidJavaObject decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
                    if (decorView != null)
                        decorHeight = decorView.Call<int>("getHeight");
                }
                using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    view.Call("getWindowVisibleDisplayFrame", rect);
                    return Screen.height - rect.Call<int>("height") + decorHeight;
                }
            }
#elif UNITY_IOS
            return (int)TouchScreenKeyboard.area.height;
#endif
    }

}



public static class RectTransformExtension
{

    public static Rect GetScreenRect(this RectTransform rectTransform, Canvas canvas)
    {

        Vector3[] corners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[2];

        rectTransform.GetWorldCorners(corners);

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
        }
        else
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
        }

        screenCorners[0].y = Screen.height - screenCorners[0].y;
        screenCorners[1].y = Screen.height - screenCorners[1].y;

        return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
    }

}