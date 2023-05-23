using Assets.Scripts.ChatScreen;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[Serializable]
public class Chat
{
    public List<Message> Messages;

    [HideInInspector]
    public string FromProfileHandle;
    public string WithProfileHandle;

    public Profile FromProfile => ProfileManager.Instance.Profiles.Find(x => x.Handle == FromProfileHandle);
    public Profile WithProfile => ProfileManager.Instance.Profiles.SingleOrDefault(x => x.Handle == WithProfileHandle);

    public Chat TheirChat => WithProfile.Chats.Find(x => x.WithProfileHandle == FromProfileHandle);

    public bool Blocked;

    public string GetStatus()
    {
        var sortedMessages = ToChatMessages();

        var lastMessage = sortedMessages.Messages.LastOrDefault();
        if (lastMessage.From.Handle == WithProfileHandle)
        {
            var timeGoneSpan = DateTime.Now.Subtract(lastMessage.Message.ReceivedAt.dateTime);

            var messageText = lastMessage.Message.Text;
            return $"{messageText} · {timeGoneSpan.Days}d";
        }

        return "Sent";
    }

    public ChatMessagesRaw ToChatMessagesRaw()
    {
        var chatMessages = Messages.Select(x => new ChatMessage
        {
            Message = x,
            From = FromProfile
        });

        var theirChatMessages = TheirChat.Messages.Select(x => new ChatMessage
        {
            Message = x,
            From = WithProfile
        });

        var allChatMessages = chatMessages
            .Concat(theirChatMessages)
            .OrderBy(x => x.Message.DeliveredAt.dateTime)
            .ToList();

        return new ChatMessagesRaw
        {
            Participants = new List<string> { FromProfileHandle, WithProfileHandle },
            Messages = allChatMessages.Select(x => x.ToRaw()).ToList(),
            Blocked = Blocked
        };
    }

    public ChatMessages ToChatMessages()
    {
        var chatMessages = Messages.Select(x => new ChatMessage
        {
            Message = x,
            From = FromProfile
        });

        var theirChatMessages = TheirChat.Messages.Select(x => new ChatMessage
        {
            Message = x,
            From = WithProfile
        });

        var allChatMessages = chatMessages
            .Concat(theirChatMessages)
            .OrderBy(x => x.Message.DeliveredAt.dateTime)
            .ToList();

        return new ChatMessages
        {
            Handles = new List<string> { FromProfileHandle, WithProfileHandle },
            Messages = allChatMessages.ToList(),
            Blocked = Blocked
        };
    }
}
