using Assets.Scripts;
using Assets.Scripts.ChatScreen;

using System;
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
            Content = Message.Text
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
                Text = raw.Content
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
    }

    public void Initialise()
    {
        foreach (Transform child in MessagesHolder)
        {
            Destroy(child.gameObject);
        }

        allMessages = CurrentChat.ToChatMessages().Messages;
        TotalMessages = allMessages.Count;

        InitialiseChatObjectsTemplates();
    }

    private string GetMessageTimestamp(ChatMessage chatMessage)
    {
        var sameWeek = DateTime.Today.Subtract(chatMessage.Message.DeliveredAt.dateTime).Days <= 7;
        var sameYear = DateTime.Today.Year == chatMessage.Message.DeliveredAt.dateTime.Year;
        var today = sameWeek && chatMessage.Message.DeliveredAt.dateTime.DayOfWeek == DateTime.Today.DayOfWeek;
        var yesterday = sameWeek && chatMessage.Message.DeliveredAt.dateTime.DayOfWeek == DateTime.Today.DayOfWeek - 1;

        return (today ? "Today " : yesterday ? "Yesterday " : "") + chatMessage.Message.DeliveredAt.dateTime.ToString((today || yesterday) ? "h:mm tt" : sameWeek ? "ddd, h:mm tt" : !sameYear ? "d MMM yyyy, h:mm tt" : "d MMM, h:mm tt");
    }

    public List<ChatObjectTemplate> ChatObjectTemplates;
    public void InitialiseChatObjectsTemplates()
    {
        ChatObjectTemplates = new List<ChatObjectTemplate>();

        foreach (var chatMessage in allMessages)
        {
            InitialiseChatObjectTemplates(chatMessage);
        }
    }
    public void InitialiseChatObjectTemplates(ChatMessage chatMessage)
    {
        ChatMessage previousMessage;
        ChatMessage nextMessage;

        var prevMessageIndex = allMessages.IndexOf(chatMessage) - 1;
        if (prevMessageIndex < allMessages.Count && prevMessageIndex > 0)
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

        var messageObjectTemplate = new ChatMessageObjectTemplate
        {
            ChatMessage = chatMessage
        };
        ChatObjectTemplates.Add(messageObjectTemplate);

        var IsOurs = chatMessage.From == ProfileManager.Instance.LoggedInProfile;

        var currentIndex = allMessages.IndexOf(chatMessage);
        var isLast = currentIndex == allMessages.Count - 1;
        var nextMessageAddsBreak = AddsBreak(nextMessage, true);
        var isLocalLast = isLast || (previousMessage != null && previousMessage.From == chatMessage.From && (nextMessageAddsBreak || nextMessage.From != chatMessage.From));

        if (previousMessage != null)
        {
            var delaySpan = chatMessage.Message.DeliveredAt.dateTime.Subtract(previousMessage.Message.DeliveredAt.dateTime);

            if (delaySpan.TotalMinutes >= 10)
            {
                var timeBreakTemplate = new ChatTimeBreakObjectTemplate
                {
                    TimestampText = GetMessageTimestamp(chatMessage)
                };

                ChatObjectTemplates.Add(timeBreakTemplate);
            }
            else if (delaySpan.TotalMinutes >= 1 || previousMessage.From != chatMessage.From)
            {
                var delayObj = new ChatDelayObjectTemplate();
                ChatObjectTemplates.Add(delayObj);
            }
        }

        var addsBreak = AddsBreak(chatMessage);

        var isContinuation = !addsBreak && previousMessage != null && previousMessage.From == chatMessage.From && !isLocalLast;
        var isAlone = (addsBreak || previousMessage == null || previousMessage.From != chatMessage.From) && (isLast || nextMessageAddsBreak || nextMessage.From != chatMessage.From);

        messageObjectTemplate.IsContinuation = isContinuation;
        messageObjectTemplate.IsLocalLast = isLocalLast;
        messageObjectTemplate.IsLast = isLast;
        messageObjectTemplate.IsAlone = isAlone;

        messageObjectTemplate.EnableProfileImage = isLocalLast || isAlone;

        if (isLast && IsOurs)
        {
            var messageStatusObj = new ChatStatusObjectTemplate();
            ChatObjectTemplates.Add(messageStatusObj);
        }
    }

    public void InstantiateMessage(int objectTemplateIndex)
    {
        var chatObjectTemplate = ChatObjectTemplates[objectTemplateIndex];
        InstantiateMessage(chatObjectTemplate);
    }

    public void InstantiateMessage(ChatObjectTemplate chatObjectTemplate)
    {
        var objectTemplateIndex = ChatObjectTemplates.IndexOf(chatObjectTemplate);
        switch (chatObjectTemplate.Type)
        {
            case ChatObjectType.Message:
                var chatMessageObjectTemplate = chatObjectTemplate as ChatMessageObjectTemplate;
                var messageObject = Instantiate(MessagePrefab, MessagesHolder);
                messageObject.name = objectTemplateIndex.ToString();
                var messageUI = messageObject.GetComponent<MessageUI>();
                messageUI.Initialise(chatMessageObjectTemplate.ChatMessage);

                messageUI.ProfileAreaObject.GetComponentsInChildren<Image>().ToList().ForEach(x => x.enabled = chatMessageObjectTemplate.EnableProfileImage);

                var height = messageUI.MessageBackground.GetComponent<LayoutElement>().minHeight;

                var IsOurs = chatMessageObjectTemplate.ChatMessage.From == ProfileManager.Instance.LoggedInProfile;

                if (chatMessageObjectTemplate.IsAlone)
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
                        topRight = chatMessageObjectTemplate.IsContinuation ? 4 : chatMessageObjectTemplate.IsLocalLast ? 4 : height / 2;
                        bottomRight = chatMessageObjectTemplate.IsContinuation ? 4 : !chatMessageObjectTemplate.IsLocalLast ? 4 : height / 2;
                    }
                    else
                    {
                        topRight = bottomRight = height / 2;
                        topLeft = chatMessageObjectTemplate.IsContinuation ? 4 : chatMessageObjectTemplate.IsLocalLast ? 4 : height / 2;
                        bottomLeft = chatMessageObjectTemplate.IsContinuation ? 4 : !chatMessageObjectTemplate.IsLocalLast ? 4 : height / 2;
                    }

                    messageUI.IndependentRoundnessComponent.r.Set(topLeft, topRight, bottomRight, bottomLeft);
                }
                break;
            case ChatObjectType.Delay:
                var delayObj = Instantiate(DelayFillerPrefab, MessagesHolder);
                delayObj.name = objectTemplateIndex.ToString();
                break;
            case ChatObjectType.TimeBreak:
                var chatTimebreakObjectTemplate = chatObjectTemplate as ChatTimeBreakObjectTemplate;
                var timeBreakUIText = Instantiate(TimeBreakPrefab).GetComponent<TextMeshProUGUI>();
                timeBreakUIText.gameObject.name = objectTemplateIndex.ToString();
                timeBreakUIText.text = chatTimebreakObjectTemplate.TimestampText;
                break;
            case ChatObjectType.Status:
                var statusObj = Instantiate(MessageStatusPrefab, MessagesHolder);
                statusObj.name = objectTemplateIndex.ToString();
                break;
        }
    }

    bool AddsBreak(ChatMessage chatMessage, bool ignoreFromCase = false)
    {
        ChatMessage prevMessage;
        var prevMessageIndex = allMessages.IndexOf(chatMessage) - 1;
        if (prevMessageIndex < allMessages.Count && prevMessageIndex > 0)
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
