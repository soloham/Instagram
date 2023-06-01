using Assets.Scripts;
using Assets.Scripts.ChatScreen;

using Cysharp.Threading.Tasks;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ChatMessage
{
    public Message Message;
    public Profile From;

    public ChatMessageRaw ToRaw()
    {
        return new ChatMessageRaw
        {
            SenderName = From.Handle,
            TimestampMS = new DateTimeOffset(Message.DeliveredAt.dateTime).ToUnixTimeMilliseconds(),
            Content = Message.Text,
            Photos = Message.Photos
        };
    }

    public static ChatMessage FromRaw(ChatMessageRaw raw)
    {
        return new ChatMessage
        {
            Message = new Message
            {
                SentAt = new UDateTime
                {
                    dateTime = DateTimeOffset.FromUnixTimeMilliseconds(raw.TimestampMS).DateTime
                },
                Text = raw.Content,
                Photos = raw.Photos
            },
            From = ProfileManager.Instance.Profiles.Find(x => x.Handle == raw.SenderName)
        };
    }
}

public class ChatMessageRaw
{
    public string SenderName;
    public long TimestampMS;
    public string Content;
    public List<ChatMessagePhoto> Photos;
}

public class ChatMessagePhoto
{
    public string Uri { get; set; }
    public int Timestamp { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class ChatAreaManager : MonoBehaviour
{
    public GameObject MessagePrefab;
    public GameObject DelayFillerPrefab;
    public GameObject TimeBreakPrefab;
    public GameObject MessageStatusPrefab;
    public GameObject MessageLoaderPrefab;
    public Transform MessagesHolder;

    [Header("Page")]
    public int PageSize;
    public int CurrentPage = 1;

    public ChatScreenManager ChatScreenManager => FindObjectOfType<ChatScreenManager>();

    [Header("Right Slices")]
    public Sprite TopSliceRight;
    public Sprite BottomSliceRight;
    public Sprite BothSliceRight;

    [Space(10)]
    public Sprite CircleSlice;

    [Header("Left Slices")]
    public Sprite TopSliceLeft;
    public Sprite BottomSliceLeft;
    public Sprite BothSliceLeft;

    private Chat CurrentChat => ChatScreenManager?.Chat;

    private List<ChatMessage> allMessages;
    public int TotalMessages;

    public void DeInitialise()
    {
        foreach (Transform child in MessagesHolder)
        {
            Destroy(child.gameObject);
        }

        VirtualScrollRect.OnReachedEnd -= VirtualScrollRect_OnReachedEnd;
    }

    private void OnDestroy()
    {
        VirtualScrollRect.OnReachedEnd -= VirtualScrollRect_OnReachedEnd;
    }

    public void Initialise()
    {
        foreach (Transform child in MessagesHolder)
        {
            Destroy(child.gameObject);
        }

        CurrentPage = 1;

        allMessages = CurrentChat.ToChatMessages().Messages;
        TotalMessages = allMessages.Count;

        StartCoroutine(InstantiatePagedMessages(true));

        VirtualScrollRect.OnReachedEnd += VirtualScrollRect_OnReachedEnd;
    }

    public bool isLoadingPage = false;
    private void VirtualScrollRect_OnReachedEnd()
    {
        if (isLastPage)
        {
            Destroy(loaderObject);
        }

        if (isLoadingPage || isLastPage)
        {
            return;
        }

        StartCoroutine(LoadNextPage());
    }

    public bool isLastPage = false;
    GameObject loaderObject;
    private IEnumerator LoadNextPage()
    {
        isLoadingPage = true;

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.1f));
        Destroy(loaderObject);
        CurrentPage++;
        yield return StartCoroutine(InstantiatePagedMessages());
        isLoadingPage = false;
        FindObjectOfType<VirtualScrollRect>().UpdateVisibleMessages();
    }

    private string GetMessageTimestamp(ChatMessage chatMessage)
    {
        var sameWeek = DateTime.Today.Subtract(chatMessage.Message.DeliveredAt.dateTime).Days <= 7;
        var sameYear = DateTime.Today.Year == chatMessage.Message.DeliveredAt.dateTime.Year;
        var today = sameWeek && chatMessage.Message.DeliveredAt.dateTime.DayOfWeek == DateTime.Today.DayOfWeek;
        var yesterday = sameWeek && chatMessage.Message.DeliveredAt.dateTime.DayOfWeek == DateTime.Today.DayOfWeek - 1;

        return (today ? "Today " : yesterday ? "Yesterday " : "") + chatMessage.Message.DeliveredAt.dateTime.ToString((today || yesterday) ? "h:mm tt" : sameWeek ? "ddd, h:mm tt" : !sameYear ? "d MMM yyyy, h:mm tt" : "d MMM, h:mm tt");
    }

    List<ChatMessage> pagedMessages;
    public IEnumerator InstantiatePagedMessages(bool initialising = false)
    {
        pagedMessages = allMessages
            .Skip(TotalMessages - (CurrentPage * PageSize))
            .Take(PageSize)
            .ToList();

        isLastPage = pagedMessages.Contains(allMessages.First());

        pagedMessages.Reverse();

        var oldChildCount = MessagesHolder.childCount;

        foreach (var chatMessage in pagedMessages)
        {
            InstantiateMessage(chatMessage);
        }

        yield return new WaitForEndOfFrame();
        loaderObject = Instantiate(MessageLoaderPrefab, MessagesHolder);

        loaderObject.GetComponent<MessagesLoader>().Initialise(GetMessageTimestamp(pagedMessages.Last()));

        for (int i = oldChildCount - 1; i < MessagesHolder.childCount; i++)
        {
            if (i < 0)
            {
                continue;
            }

            var child = MessagesHolder.GetChild(i);

            var messageUI = child.GetComponent<MessageUI>();
            var isVisible = !initialising || i < 30;
            var isMessage = messageUI != null;
            if (!isMessage)
            {
                child.gameObject.SetActive(isVisible);

                continue;
            }

            if (isMessage)
            {
                if (messageUI.ChatMessage.Message.Photos?.Count > 0)
                {
                    if (!isVisible)
                    {
                        ToggleLayoutComponents(child.gameObject);
                    }
                }
                else DestroyLayoutComponents(child.gameObject);
            }
            else DestroyLayoutComponents(child.gameObject);

            child.gameObject.SetActive(!initialising || i < 30);
        }

        FindObjectOfType<VirtualScrollRect>().UpdateVisibleMessages(false);

        if (initialising)
        {
            FindObjectOfType<VirtualScrollRect>().verticalNormalizedPosition = 0;
        }
    }

    void ToggleLayoutComponents(GameObject obj, bool enable = false)
    {
        obj.GetComponent<VerticalLayoutGroup>().enabled = enable;
        obj.GetComponent<LayoutElement>().enabled = enable;
        obj.GetComponent<ContentSizeFitter>().enabled = enable;

        obj.GetComponentsInChildren<VerticalLayoutGroup>().ToList().ForEach(x => x.enabled = enable);
        obj.GetComponentsInChildren<HorizontalLayoutGroup>().ToList().ForEach(x => x.enabled = enable);
        obj.GetComponentsInChildren<LayoutElement>().ToList().ForEach(x => x.enabled = enable);
        obj.GetComponentsInChildren<ContentSizeFitter>().ToList().ForEach(x => x.enabled = enable);
    }

    void DestroyLayoutComponents(GameObject obj)
    {
        Destroy(obj.GetComponent<VerticalLayoutGroup>());
        Destroy(obj.GetComponent<LayoutElement>());
        Destroy(obj.GetComponent<ContentSizeFitter>());

        obj.GetComponentsInChildren<VerticalLayoutGroup>().ToList().ForEach(x => Destroy(x));
        obj.GetComponentsInChildren<HorizontalLayoutGroup>().ToList().ForEach(x => Destroy(x));
        obj.GetComponentsInChildren<LayoutElement>().ToList().ForEach(x => Destroy(x));
        obj.GetComponentsInChildren<ContentSizeFitter>().ToList().ForEach(x => Destroy(x));
    }

    public void InstantiateMessage(int index)
    {
        var chatMessage = allMessages[index];
        InstantiateMessage(chatMessage);
    }

    public void InstantiateMessage(ChatMessage currentMessage, int indexOffset)
    {
        var chatMessage = allMessages[allMessages.IndexOf(currentMessage) + indexOffset];
        InstantiateMessage(chatMessage);
    }

    public void InstantiateMessage(ChatMessage chatMessage)
    {
        ChatMessage previousMessage = null;
        ChatMessage nextMessage = null;

        var prevMessageIndex = allMessages.IndexOf(chatMessage) - 1;
        if (prevMessageIndex < allMessages.Count && prevMessageIndex >= 0)
        {
            previousMessage = allMessages[prevMessageIndex];
        }
        else
        {
            previousMessage = null;
        }

        var nextMessageIndex = allMessages.IndexOf(chatMessage) + 1;
        if (nextMessageIndex < allMessages.Count)
        {
            nextMessage = allMessages[nextMessageIndex];
        }
        else
        {
            nextMessage = null;
        }

        var messageObject = Instantiate(MessagePrefab, MessagesHolder);

        var messageUI = messageObject.GetComponent<MessageUI>();
        messageUI.Initialise(chatMessage);

        var IsOurs = chatMessage.From == ProfileManager.Instance.LoggedInProfile;

        var currentIndex = allMessages.IndexOf(chatMessage);
        var isLast = currentIndex == allMessages.Count - 1;
        var nextMessageAddsBreak = AddsBreak(nextMessage, true);
        var isLocalLast = isLast || ((previousMessage == null || previousMessage.From == chatMessage.From) && (nextMessageAddsBreak || nextMessage.From != chatMessage.From));

        if (previousMessage != null)
        {
            var delaySpan = chatMessage.Message.DeliveredAt.dateTime.Subtract(previousMessage.Message.DeliveredAt.dateTime);

            if (delaySpan.TotalMinutes >= 10)
            {
                var timeBreakUIText = Instantiate(TimeBreakPrefab, MessagesHolder).GetComponent<TextMeshProUGUI>();

                timeBreakUIText.text = GetMessageTimestamp(chatMessage);
            }
            else if (delaySpan.TotalMinutes >= 1 || previousMessage.From != chatMessage.From)
            {
                var delayObj = Instantiate(DelayFillerPrefab, MessagesHolder);
            }
        }
        else
        {
            var timeBreakUIText = Instantiate(TimeBreakPrefab, MessagesHolder).GetComponent<TextMeshProUGUI>();

            timeBreakUIText.text = GetMessageTimestamp(chatMessage);
        }

        var addsBreak = AddsBreak(chatMessage);

        var isContinuation = !addsBreak && previousMessage != null && previousMessage.From == chatMessage.From && !isLocalLast;
        var isAlone = (addsBreak || previousMessage == null || previousMessage.From != chatMessage.From) && (isLast || nextMessageAddsBreak || nextMessage.From != chatMessage.From);

        messageUI.ProfileAreaObject.GetComponentsInChildren<Image>().ToList().ForEach(x => x.enabled = isLocalLast || isAlone);

        var height = messageUI.MessageBackground.GetComponent<LayoutElement>().minHeight;

        if (isAlone)
        {
            messageUI.IndependentRoundnessComponent.enabled = false;
            messageUI.RoundnessComponent.enabled = true;

            messageUI.RoundnessComponent.radius = height;
        }
        else
        {
            messageUI.RoundnessComponent.enabled = false;
            messageUI.IndependentRoundnessComponent.enabled = true;

            float topLeft = 0;
            float topRight = 0;
            float bottomLeft = 0;
            float bottomRight = 0;

            if (IsOurs)
            {
                topLeft = bottomLeft = height / 2;
                topRight = isContinuation ? 4 : isLocalLast ? 4 : height / 2;
                bottomRight = isContinuation ? 4 : !isLocalLast ? 4 : height / 2;
            }
            else
            {
                topRight = bottomRight = height / 2;
                topLeft = isContinuation ? 4 : isLocalLast ? 4 : height / 2;
                bottomLeft = isContinuation ? 4 : !isLocalLast ? 4 : height / 2;
            }

            messageUI.IndependentRoundnessComponent.r.Set(topLeft, topRight, bottomRight, bottomLeft);
        }

        if (isLast && IsOurs)
        {
            var messageStatusObj = Instantiate(MessageStatusPrefab, MessagesHolder);
            messageStatusObj.transform.SetAsFirstSibling();
        }
    }

    bool AddsBreak(ChatMessage chatMessage, bool ignoreFromCase = false)
    {
        ChatMessage prevMessage;
        var prevMessageIndex = allMessages.IndexOf(chatMessage) - 1;
        if (prevMessageIndex < allMessages.Count && prevMessageIndex >= 0)
        {
            prevMessage = allMessages[prevMessageIndex];
        }
        else
        {
            return false;
        }

        if (!ignoreFromCase && prevMessage.From != chatMessage.From)
        {
            return true;
        }

        var delaySpan = chatMessage.Message.DeliveredAt.dateTime.Subtract(prevMessage.Message.DeliveredAt.dateTime);

        return delaySpan.TotalMinutes >= 1;
    }

    public void Update()
    {
        if (MessagesHolder == null)
        {
            return;
        }

        LayoutRebuilder.MarkLayoutForRebuild(MessagesHolder.GetComponent<RectTransform>());
    }
}
