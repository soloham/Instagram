using Assets.Scripts.ChatScreen;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[Serializable]
public class Profile
{
    public string Name;

    public string Handle;

    public string ProfileUID;

    [JsonIgnore]
    public Sprite Picture;
    [JsonIgnore]
    public Sprite PictureBorderless;

    [JsonIgnore]
    public List<Chat> Chats;

    public void Initialise()
    {
        Chats.ForEach(x => x.FromProfileHandle = this.Handle);
    }

    public void Hydrate(AllChatMessagesRaw allChatMessages)
    {
        try
        {
            var withHandles = allChatMessages.Conversations
                .Where(x => x.Participants.Contains(Handle))
                .SelectMany(x => x.Participants.Where(y => y != Handle))
                .ToList();

            var allMessagesByWithProfile = allChatMessages.Conversations
                .Where(x => x.Participants.Contains(Handle))
                .GroupBy(x => x.Participants.Single(y => y != Handle))
                .ToDictionary(x => x.Key, x =>
                {
                    try
                    {
                        var obj = x.Single();
                        return new { obj.Blocked, Messages = obj.Messages.Select(y => ChatMessage.FromRaw(y)).Where(y => y.From.Handle == Handle).ToList() };
                    }
                    catch(Exception ex)
                    {
                        throw;
                    }
                });

            var chats = withHandles
                .Select(withHandle => new Chat
                {
                    WithProfileHandle = withHandle,
                    FromProfileHandle = Handle,
                    Messages = allMessagesByWithProfile[withHandle].Messages.Select(x => x.Message).ToList(),
                    Blocked = allMessagesByWithProfile[withHandle].Blocked
                })
                .ToList();

            Chats = chats;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
